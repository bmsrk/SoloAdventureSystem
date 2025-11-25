using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using SoloAdventureSystem.ContentGenerator.Generation;
using LLama;
using LLama.Common;
using System.Text.Json;
using SoloAdventureSystem.ContentGenerator.Generation;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// SLM adapter using MaIN.NET/LLamaSharp for truly embedded, offline AI inference.
/// MaIN.NET is built on top of LLamaSharp, providing GGUF model support.
/// Refactored to use ResilienceGenerationPolicy for consistent error handling.
/// </summary>
public class MaINAdapter : ILocalSLMAdapter, IDisposable
{
    private readonly ILogger<MaINAdapter>? _logger;
    private readonly AISettings _settings;
    private readonly IGenerationPolicy _policy;
    private bool _isInitialized;
    private LLamaWeights? _weights;
    private LLamaContext? _context;
    // Prevent repeated fallback attempts
    private bool _attemptedTinyLlamaFallback = false;

    public MaINAdapter(
        IOptions<AISettings> settings, 
        ILogger<MaINAdapter>? logger = null,
        IGenerationPolicy? policy = null)
    {
        _logger = logger;
        _settings = settings.Value;
        _policy = policy ?? new ResilienceGenerationPolicy(logger as ILogger<ResilienceGenerationPolicy>);
    }

    /// <summary>
    /// Initializes the adapter by ensuring model is downloaded and loaded.
    /// </summary>
    public async System.Threading.Tasks.Task InitializeAsync(IProgress<DownloadProgress>? progress = null)
    {
        if (_isInitialized)
        {
            _logger?.LogDebug("? MaIN adapter already initialized");
            return;
        }

        try
        {
            _logger?.LogInformation("?? Initializing MaIN.NET/LLamaSharp adapter...");

            var modelConfig = _settings.GetModelConfiguration();
            _logger?.LogInformation("?? Target model: {ModelKey}", modelConfig.ModelKey);

            // Download model if needed
            var downloader = new GGUFModelDownloader(_logger as ILogger<GGUFModelDownloader>);
            var modelPath = await downloader.EnsureModelAvailableAsync(modelConfig.ModelKey, progress);

            _logger?.LogInformation("?? Loading model from: {ModelPath}", modelPath);

            var hardwareConfig = _settings.GetHardwareConfiguration();

            // Configure model parameters
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = (uint)modelConfig.ContextSize,
                GpuLayerCount = hardwareConfig.UseGPU ? 99 : 0,
                UseMemorymap = true,
                UseMemoryLock = false
            };

            _logger?.LogDebug("Using {Threads} threads for inference", hardwareConfig.MaxThreads);

            // Load model weights
            _weights = LLamaWeights.LoadFromFile(parameters);
            _logger?.LogInformation("? Model weights loaded");

            // Create context
            _context = _weights.CreateContext(parameters);
            _logger?.LogInformation("? Context created");

            _isInitialized = true;
            _logger?.LogInformation("? MaIN.NET adapter initialized successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Failed to initialize MaIN.NET adapter");
            throw new InvalidOperationException(
                "Failed to initialize embedded AI model. Check the logs for details.", ex);
        }
    }

    private bool TryGenerateStructured<T>(Func<string> genFunc, out T? result)
    {
        result = default;

        // First attempt: model asked to return TOON; genFunc should use prompt builders that request TOON
        string raw = string.Empty;
        try
        {
            raw = genFunc() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Generation failed for structured attempt");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(raw))
        {
            // Try TOON decode
            try
            {
                var parsed = ToonCodec.Deserialize<T>(raw);
                if (parsed != null)
                {
                    result = parsed;
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "TOON decode failed, will try JSON fallback");
            }

            // If TOON failed, try to detect embedded JSON
            if (raw.StartsWith("#json\n", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var json = raw.Substring(6);
                    result = JsonSerializer.Deserialize<T>(json);
                    if (result != null) return true;
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "Embedded JSON parse failed");
                }
            }

            // As a repair attempt, try to extract first JSON object or array from the text
            try
            {
                var firstJson = ExtractJsonFromText(raw);
                if (!string.IsNullOrWhiteSpace(firstJson))
                {
                    result = JsonSerializer.Deserialize<T>(firstJson);
                    if (result != null) return true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Heuristic JSON extraction failed");
            }
        }

        return false;
    }

    private static string? ExtractJsonFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        int start = text.IndexOf('{');
        int startArr = text.IndexOf('[');
        if (start == -1 && startArr == -1) return null;

        if (start == -1 || (startArr >= 0 && startArr < start)) start = startArr;

        int depth = 0;
        bool inString = false;
        for (int i = start; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '"') inString = !inString;
            if (inString) continue;
            if (c == '{' || c == '[') depth++;
            if (c == '}' || c == ']')
            {
                depth--;
                if (depth == 0)
                {
                    return text.Substring(start, i - start + 1);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Clean up generated text
    /// </summary>
    private string CleanOutput(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Remove common artifacts
        text = Regex.Replace(text, @"<\|.*?\|>", ""); // Remove special tokens
        text = Regex.Replace(text, @"\[INST\].*?\[/INST\]", ""); // Remove instruction markers
        text = Regex.Replace(text, @"USER:.*?ASSISTANT:", ""); // Remove chat markers

        // Trim and normalize whitespace
        text = text.Trim();
        text = Regex.Replace(text, @"\s+", " ");

        return SanitizeGeneratedText(text);
    }

    /// <summary>
    /// Sanitize generated text by normalizing Unicode, removing control/unprintable chars,
    /// and stripping decorative lines or long repeated punctuation (like many question marks).
    /// </summary>
    private string SanitizeGeneratedText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // Normalize unicode where possible
        try { text = text.Normalize(System.Text.NormalizationForm.FormC); } catch { }

        // Replace unicode replacement char and some common problematic characters with space
        text = text.Replace('\uFFFD', ' ');

        // Replace most control characters (except newline/tab) with a single space
        var cleanedChars = new System.Text.StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
            {
                cleanedChars.Append(' ');
            }
            else
            {
                cleanedChars.Append(c);
            }
        }
        text = cleanedChars.ToString();

        // Split into lines and process each line conservatively
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var kept = new System.Collections.Generic.List<string>();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // If a line is overwhelmingly punctuation/symbols (very likely decorative), skip it.
            var totalChars = line.Length;
            var punctCount = line.Count(ch => char.IsPunctuation(ch) || char.IsSymbol(ch));
            if (totalChars > 0 && (punctCount / (double)totalChars) > 0.85)
            {
                // skip extreme decorative lines like "????? ----- *****"
                continue;
            }

            // Remove up to a few leading/trailing punctuation/symbol characters (to drop decorative edges)
            // but don't strip inner punctuation
            line = Regex.Replace(line, "^[\\p{P}\\p{S}\\s]{1,3}", "");
            line = Regex.Replace(line, "[\\p{P}\\p{S}\\s]{1,3}$", "");

            if (string.IsNullOrWhiteSpace(line)) continue;

            // Collapse very long runs of the same punctuation down to a small run (e.g., '!!!!!' -> '!!!')
            line = Regex.Replace(line, "([\\p{P}])\\1{4,}", "$1$1$1");

            // Normalize internal whitespace
            line = Regex.Replace(line, "\\s+", " ").Trim();

            kept.Add(line);
        }

        // Join kept lines into a single paragraph but preserve sentence boundaries where possible
        var result = string.Join(" ", kept);

        // Final normalization: collapse excessive whitespace and trim
        result = Regex.Replace(result, "\\s+", " ").Trim();

        return result;
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            _logger?.LogError("? Adapter not initialized - InitializeAsync() must be called first");
            throw new InvalidOperationException(
                "Adapter not initialized. Call InitializeAsync() before using.");
        }
    }

    /// <summary>
    /// Core generation method using LLamaSharp
    /// </summary>
    private async System.Threading.Tasks.Task<string> GenerateAsync(string prompt, int seed, int maxTokens = 150)
    {
        if (_context == null || _weights == null)
        {
            throw new InvalidOperationException("Model not initialized");
        }

        try
        {
            // Create executor for text generation
            var executor = new StatelessExecutor(_weights, _context.Params);

            // Set up inference parameters
            var inferenceParams = new InferenceParams
            {
                MaxTokens = maxTokens,
                AntiPrompts = new List<string> { "\n\n", "###", "<|end|>", "USER:", "ASSISTANT:" }
            };

            // Generate text
            var result = new System.Text.StringBuilder();
            
            await foreach (var text in executor.InferAsync(prompt, inferenceParams))
            {
                result.Append(text);
            }

            var generated = result.ToString().Trim();

            // Keep raw output for diagnostics
            var rawGenerated = generated;

            // Clean up the output
            var cleaned = CleanOutput(generated);

            // If cleaning removed all content but raw had useful text, fall back to a lighter cleanup
            if (string.IsNullOrWhiteSpace(cleaned) && !string.IsNullOrWhiteSpace(rawGenerated))
            {
                _logger?.LogWarning("? Cleaned generation was empty; falling back to raw output (will apply light cleanup)");
                // Light cleanup: remove control chars and normalize whitespace, but don't strip lines
                var light = Regex.Replace(rawGenerated, "\\p{C}", " ");
                light = Regex.Replace(light, "\\s+", " ").Trim();
                cleaned = light;
            }

            return cleaned;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Generation failed");
            throw;
        }
    }
    
    private string Generate(string prompt, int seed, int maxTokens = 150)
    {
        return GenerateAsync(prompt, seed, maxTokens).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Try generation via policy and, if result is empty, retry once with seed+1 bypassing the policy to avoid policy-level consecutive empty detection.
    /// </summary>
    private string GenerateWithRetry(string prompt, int seed, int maxTokens, string operationName)
    {
        EnsureInitialized();

        string text = string.Empty;
        try
        {
            // Primary generation through the resilient policy
            text = _policy.Execute(() => Generate(prompt, seed, maxTokens), operationName);
        }
        catch (GenerationException ex)
        {
            _logger?.LogWarning(ex, "Primary generation via policy failed for {OperationName}; will attempt one direct retry.", operationName);
            // fall through to direct retry
        }

        // If policy returned empty or failed, attempt one direct retry with altered seed
        if (string.IsNullOrWhiteSpace(text))
        {
            try
            {
                _logger?.LogDebug("Attempting direct retry for {OperationName} with seed {SeedPlus}", operationName, seed + 1);
                var direct = Generate(prompt, seed + 1, maxTokens);
                if (!string.IsNullOrWhiteSpace(direct))
                    return direct;
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Direct retry failed for {OperationName}", operationName);
            }
        }
        
        // If still empty, attempt an automatic recovery: switch to tinyllama if not already tried
        if (string.IsNullOrWhiteSpace(text) && !_attemptedTinyLlamaFallback)
        {
            try
            {
                // Only attempt if current model isn't tinyllama
                if (!string.Equals(_settings.LLamaModelKey, "tinyllama-q4", StringComparison.OrdinalIgnoreCase))
                {
                    _attemptedTinyLlamaFallback = true;
                    _logger?.LogWarning("Empty generation detected for {OperationName}. Attempting auto-recovery by switching to TinyLlama and reinitializing.", operationName);

                    // Dispose current context and weights to force reload
                    DisposeContextAndWeights();

                    // Update settings to tinyllama key
                    try
                    {
                        _settings.LLamaModelKey = "tinyllama-q4";
                        _settings.Model = "tinyllama-q4";
                    }
                    catch { /* best-effort update */ }

                    // Reinitialize (will download/load tinyllama if needed)
                    InitializeAsync().GetAwaiter().GetResult();

                    // Try a direct generation once more
                    _logger?.LogInformation("Retrying generation with TinyLlama for {OperationName}", operationName);
                    var recovery = Generate(prompt, seed + 2, maxTokens);
                    if (!string.IsNullOrWhiteSpace(recovery))
                    {
                        return recovery;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Auto-recovery attempt failed");
            }
        }

        return text ?? string.Empty;
    }

    public string GenerateRoomDescription(string context, int seed)
    {
        EnsureInitialized();

        // Attempt to generate structured TOON/JSON Room object
        if (TryGenerateStructured<Dictionary<string, object>>( () => Generate(context, seed, GenerationLimits.RoomDescriptionTokens), out var parsed))
        {
            // Map parsed dictionary to a readable description string
            try
            {
                if (parsed != null && parsed.TryGetValue("description", out var descObj) && descObj != null)
                {
                    var desc = descObj.ToString() ?? string.Empty;
                    return desc;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Failed to map parsed room object to string");
            }
        }

        // Legacy fallback: generate text via helper that retries once
        var text = GenerateWithRetry(context, seed, GenerationLimits.RoomDescriptionTokens, "RoomDescription-Legacy");

        // Apply sanitization
        var cleaned = CleanOutput(text);
        if (string.IsNullOrWhiteSpace(cleaned) && !string.IsNullOrWhiteSpace(text))
        {
            _logger?.LogWarning("? Cleaned generation was empty; falling back to raw output (light cleanup)");
            var light = System.Text.RegularExpressions.Regex.Replace(text, "\\p{C}", " ");
            light = System.Text.RegularExpressions.Regex.Replace(light, "\\s+", " ").Trim();
            cleaned = light;
        }

        return cleaned;
    }

    public string GenerateNpcBio(string context, int seed)
    {
        EnsureInitialized();

        if (TryGenerateStructured<Dictionary<string, object>>(() => Generate(context, seed, GenerationLimits.NpcBioTokens), out var parsed))
        {
            try
            {
                if (parsed != null && parsed.TryGetValue("bio", out var bioObj) && bioObj != null)
                    return bioObj.ToString() ?? string.Empty;
            }
            catch { }
        }

        var text = GenerateWithRetry(context, seed, GenerationLimits.NpcBioTokens, "NpcBio-Legacy");
        var cleaned = CleanOutput(text);
        if (string.IsNullOrWhiteSpace(cleaned) && !string.IsNullOrWhiteSpace(text))
        {
            var light = System.Text.RegularExpressions.Regex.Replace(text, "\\p{C}", " ");
            light = System.Text.RegularExpressions.Regex.Replace(light, "\\s+", " ").Trim();
            cleaned = light;
        }
        return cleaned;
    }

    public string GenerateFactionFlavor(string context, int seed)
    {
        EnsureInitialized();

        if (TryGenerateStructured<Dictionary<string, object>>(() => Generate(context, seed, GenerationLimits.FactionLoreTokens), out var parsed))
        {
            try
            {
                if (parsed != null && parsed.TryGetValue("description", out var d) && d != null)
                    return d.ToString() ?? string.Empty;
            }
            catch { }
        }

        var text = GenerateWithRetry(context, seed, GenerationLimits.FactionLoreTokens, "FactionFlavor-Legacy");
        var cleaned = CleanOutput(text);
        if (string.IsNullOrWhiteSpace(cleaned) && !string.IsNullOrWhiteSpace(text))
        {
            var light = System.Text.RegularExpressions.Regex.Replace(text, "\\p{C}", " ");
            light = System.Text.RegularExpressions.Regex.Replace(light, "\\s+", " ").Trim();
            cleaned = light;
        }
        return cleaned;
    }

    public List<string> GenerateLoreEntries(string context, int seed, int count)
    {
        EnsureInitialized();

        var entries = new List<string>();

        for (int i = 0; i < count; i++)
        {
            var itemSeed = seed + i;

            // Try structured decode for single lore entry
            if (TryGenerateStructured<Dictionary<string, object>>(() => Generate(context, itemSeed, GenerationLimits.LoreEntryTokens), out var parsed))
            {
                if (parsed != null && parsed.TryGetValue("text", out var t) && t != null)
                {
                    entries.Add(t.ToString() ?? string.Empty);
                    continue;
                }
            }

            // Legacy fallback with retry
            try
            {
                var raw = GenerateWithRetry(context, itemSeed, GenerationLimits.LoreEntryTokens, $"LoreEntry{i + 1}");
                var cleaned = CleanOutput(raw);
                if (string.IsNullOrWhiteSpace(cleaned) && !string.IsNullOrWhiteSpace(raw))
                {
                    var light = System.Text.RegularExpressions.Regex.Replace(raw, "\\p{C}", " ");
                    light = System.Text.RegularExpressions.Regex.Replace(light, "\\s+", " ").Trim();
                    cleaned = light;
                }
                entries.Add(cleaned);
            }
            catch (GenerationException ex)
            {
                _logger?.LogWarning(ex, "? Failed to generate lore entry {Index}, using fallback", i + 1);
                entries.Add($"Lore entry {i + 1} (generation failed)");
            }
        }

        _logger?.LogInformation("? Generated {Count} lore entries", entries.Count);
        return entries;
    }

    /// <summary>
    /// Generate dialogue with the AI model, attempting to parse structured JSON output.
    /// </summary>
    public string GenerateDialogue(string prompt, int seed)
    {
        EnsureInitialized();

        // Try structured parse as a list of choice objects
        if (TryGenerateStructured<List<Dictionary<string, object>>>(() => Generate(prompt, seed, GenerationLimits.DialogueTokens), out var parsedList))
        {
            try
            {
                if (parsedList != null)
                {
                    // Return compact JSON representation for downstream consumers
                    return JsonSerializer.Serialize(parsedList);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Failed to serialize parsed dialogue list");
            }
        }

        // Legacy fallback
        var text = GenerateWithRetry(prompt, seed, GenerationLimits.DialogueTokens, "Dialogue-Legacy");
        var cleaned = CleanOutput(text);
        if (string.IsNullOrWhiteSpace(cleaned) && !string.IsNullOrWhiteSpace(text))
        {
            var light = System.Text.RegularExpressions.Regex.Replace(text, "\\p{C}", " ");
            light = System.Text.RegularExpressions.Regex.Replace(light, "\\s+", " ").Trim();
            cleaned = light;
        }

        return cleaned;
    }

    private void DisposeContextAndWeights()
    {
        _context?.Dispose();
        _context = null;
        
        _weights?.Dispose();
        _weights = null;
        
        _isInitialized = false;
    }

    public void Dispose()
    {
        _logger?.LogInformation("?? Disposing MaINAdapter...");
        
        DisposeContextAndWeights();
        
        _logger?.LogDebug("? MaINAdapter disposed");
        GC.SuppressFinalize(this);
    }
}