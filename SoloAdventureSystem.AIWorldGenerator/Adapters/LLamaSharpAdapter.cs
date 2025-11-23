using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using SoloAdventureSystem.ContentGenerator.Generation;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// SLM adapter using LLamaSharp for truly embedded, offline AI inference.
/// Uses GGUF models with llama.cpp backend - 100% local, no internet required after model download.
/// </summary>
public class LLamaSharpAdapter : ILocalSLMAdapter, IDisposable
{
    private readonly ILogger<LLamaSharpAdapter>? _logger;
    private readonly AISettings _settings;
    private readonly LLamaInferenceEngine _engine;
    private readonly PromptOptimizer _optimizer;
    private bool _isInitialized;

    public LLamaSharpAdapter(IOptions<AISettings> settings, ILogger<LLamaSharpAdapter>? logger = null)
    {
        _logger = logger;
        _settings = settings.Value;
        _engine = new LLamaInferenceEngine(logger as ILogger<LLamaInferenceEngine>);
        _optimizer = new PromptOptimizer();
    }

    /// <summary>
    /// Initializes the adapter by ensuring model is downloaded and loaded.
    /// </summary>
    public async System.Threading.Tasks.Task InitializeAsync(IProgress<DownloadProgress>? progress = null)
    {
        if (_isInitialized)
        {
            _logger?.LogDebug("? LLamaSharp adapter already initialized");
            return;
        }

        try
        {
            _logger?.LogInformation("?? Initializing LLamaSharp adapter...");

            // Ensure model is downloaded
            _logger?.LogDebug("?? Creating model downloader...");
            var downloader = new GGUFModelDownloader(_logger as ILogger<GGUFModelDownloader>);
            var modelKey = _settings.LLamaModelKey ?? "phi-3-mini-q4";
            
            _logger?.LogInformation("?? Target model: {ModelKey}", modelKey);
            
            var modelPath = await downloader.EnsureModelAvailableAsync(modelKey, progress);
            _logger?.LogInformation("? Model available at: {Path}", modelPath);

            // Load model into memory
            var contextSize = _settings.ContextSize ?? 2048;
            var gpuLayers = _settings.UseGPU ? 35 : 0;
            var threads = _settings.MaxInferenceThreads;
            var seed = _settings.Seed.HasValue ? (uint)_settings.Seed.Value : (uint?)null;

            _logger?.LogInformation("?? Model configuration:");
            _logger?.LogInformation("   Context Size: {Context} tokens", contextSize);
            _logger?.LogInformation("   GPU Acceleration: {GPU} (layers: {Layers})", _settings.UseGPU, gpuLayers);
            _logger?.LogInformation("   CPU Threads: {Threads}", threads);
            _logger?.LogInformation("   Random Seed: {Seed}", seed?.ToString() ?? "random");

            _logger?.LogDebug("?? Loading model into memory...");
            _engine.LoadModel(modelPath, contextSize, gpuLayers, threads, seed);

            _isInitialized = true;
            _logger?.LogInformation("? LLamaSharp adapter initialized successfully and ready for inference");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Failed to initialize LLamaSharp adapter");
            _logger?.LogError("?? Common causes:");
            _logger?.LogError("   1. Model download failed - check internet connection");
            _logger?.LogError("   2. Insufficient RAM - try a smaller model (TinyLlama)");
            _logger?.LogError("   3. Corrupted model file - delete and re-download");
            _logger?.LogError("   4. Incompatible model format - ensure it's a GGUF file");
            
            throw new InvalidOperationException(
                "Failed to initialize embedded AI model. Check the logs for details.", ex);
        }
    }

    public string GenerateRoomDescription(string context, int seed)
    {
        EnsureInitialized();

        try
        {
            _logger?.LogDebug("?? Generating room description (seed: {Seed})", seed);
            
            var systemPrompt = _optimizer.OptimizeSystemPrompt(PromptTemplates.RoomDescriptionSystem);
            var fullPrompt = FormatInstructPrompt(systemPrompt, context);

            _logger?.LogDebug("?? Prompt size: System={SystemLen} chars, User={UserLen} chars, Total={TotalLen} chars",
                systemPrompt.Length, context.Length, fullPrompt.Length);

            var output = _engine.Generate(
                fullPrompt,
                maxTokens: 180,      // Reduced for tighter control
                temperature: 0.5f,   // Lower for more consistent formatting
                seed: seed,
                timeout: TimeSpan.FromMinutes(3));

            var cleaned = CleanOutput(output);
            _logger?.LogDebug("? Room description generated ({Length} chars)", cleaned.Length);
            
            return cleaned;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Failed to generate room description for seed {Seed}", seed);
            throw new InvalidOperationException(
                $"AI generation failed for room description. Error: {ex.Message}", ex);
        }
    }

    public string GenerateNpcBio(string context, int seed)
    {
        EnsureInitialized();

        try
        {
            _logger?.LogDebug("?? Generating NPC bio (seed: {Seed})", seed);
            
            var systemPrompt = _optimizer.OptimizeSystemPrompt(PromptTemplates.NpcBioSystem);
            var fullPrompt = FormatInstructPrompt(systemPrompt, context);

            var output = _engine.Generate(
                fullPrompt,
                maxTokens: 150,      // Reduced for 2-sentence target
                temperature: 0.6f,   // Balanced for personality
                seed: seed,
                timeout: TimeSpan.FromMinutes(3));

            var cleaned = CleanOutput(output);
            
            // If output is too short, it likely hit a stop token too early - return what we have
            if (string.IsNullOrWhiteSpace(cleaned) || cleaned.Length < 10)
            {
                _logger?.LogWarning("?? NPC bio generation produced short/empty output, using raw output");
                cleaned = output.Trim();
            }
            
            _logger?.LogDebug("? NPC bio generated ({Length} chars)", cleaned.Length);
            
            return cleaned;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Failed to generate NPC bio for seed {Seed}", seed);
            throw new InvalidOperationException(
                $"AI generation failed for NPC bio. Error: {ex.Message}", ex);
        }
    }

    public string GenerateFactionFlavor(string context, int seed)
    {
        EnsureInitialized();

        try
        {
            _logger?.LogDebug("?? Generating faction flavor (seed: {Seed})", seed);
            
            var systemPrompt = _optimizer.OptimizeSystemPrompt(PromptTemplates.FactionLoreSystem);
            var fullPrompt = FormatInstructPrompt(systemPrompt, context);

            var output = _engine.Generate(
                fullPrompt,
                maxTokens: 180,      // 3 sentences target
                temperature: 0.6f,   // Balanced for faction personality
                seed: seed,
                timeout: TimeSpan.FromMinutes(3));

            var cleaned = CleanOutput(output);
            
            // If output is too short, it likely hit a stop token too early - return what we have
            if (string.IsNullOrWhiteSpace(cleaned) || cleaned.Length < 10)
            {
                _logger?.LogWarning("?? Faction flavor generation produced short/empty output, using raw output");
                cleaned = output.Trim();
            }
            
            _logger?.LogDebug("? Faction flavor generated ({Length} chars)", cleaned.Length);
            
            return cleaned;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Failed to generate faction flavor for seed {Seed}", seed);
            throw new InvalidOperationException(
                $"AI generation failed for faction. Error: {ex.Message}", ex);
        }
    }

    public List<string> GenerateLoreEntries(string context, int seed, int count)
    {
        EnsureInitialized();

        _logger?.LogDebug("?? Generating {Count} lore entries (seed: {Seed})", count, seed);
        
        var entries = new List<string>();
        var systemPrompt = _optimizer.OptimizeSystemPrompt(PromptTemplates.WorldLoreSystem);

        for (int i = 0; i < count; i++)
        {
            try
            {
                _logger?.LogDebug("?? Generating lore entry {Index}/{Total}", i + 1, count);
                
                var userPrompt = $"{context} (Entry #{i + 1})";
                var fullPrompt = FormatInstructPrompt(systemPrompt, userPrompt);

                var output = _engine.Generate(
                    fullPrompt,
                    maxTokens: 100,      // 1-2 sentences target
                    temperature: 0.65f,  // Slightly higher for variety
                    seed: seed + i,
                    timeout: TimeSpan.FromMinutes(2));

                var cleaned = CleanOutput(output);
                entries.Add(cleaned);
                _logger?.LogDebug("? Lore entry {Index} generated ({Length} chars)", i + 1, cleaned.Length);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "?? Failed to generate lore entry {Index}, using fallback", i + 1);
                entries.Add($"Lore entry {i + 1} (generation failed)");
            }
        }

        _logger?.LogInformation("? Generated {Count} lore entries", entries.Count);
        return entries;
    }

    /// <summary>
    /// Formats the prompt using the appropriate instruction template for the model.
    /// Different models use different chat templates.
    /// </summary>
    private string FormatInstructPrompt(string system, string user)
    {
        var modelKey = _settings.LLamaModelKey ?? "phi-3-mini-q4";
        
        _logger?.LogDebug("?? Formatting prompt for model: {ModelKey}", modelKey);
        
        // Phi-3 uses Microsoft's special tokens
        if (modelKey == "phi-3-mini-q4")
        {
            return $"<|system|>\n{system}<|end|>\n<|user|>\n{user}<|end|>\n<|assistant|>\n";
        }
        
        // TinyLlama uses ChatML format
        if (modelKey == "tinyllama-q4")
        {
            return $"<|im_start|>system\n{system}<|im_end|>\n<|im_start|>user\n{user}<|im_end|>\n<|im_start|>assistant\n";
        }
        
        // Llama 3.2 uses Meta's instruction format
        if (modelKey == "llama-3.2-1b-q4")
        {
            return $"<|begin_of_text|><|start_header_id|>system<|end_header_id|>\n\n{system}<|eot_id|>" +
                   $"<|start_header_id|>user<|end_header_id|>\n\n{user}<|eot_id|>" +
                   $"<|start_header_id|>assistant<|end_header_id|>\n\n";
        }
        
        // Fallback to generic format for unknown models
        _logger?.LogWarning("?? Unknown model '{ModelKey}', using generic prompt format", modelKey);
        return $"System: {system}\n\nUser: {user}\n\nAssistant:";
    }

    /// <summary>
    /// Cleans up the model output (removes artifacts, extra whitespace).
    /// Only removes markers that appear at the start of new lines to avoid over-aggressive truncation.
    /// </summary>
    private string CleanOutput(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return string.Empty;

        // Remove common artifacts
        output = output.Trim();

        // Remove instruction markers ONLY if they appear at the START of a new line
        // This prevents truncating valid content like "assistant: role" or "user: action"
        var markers = new[] { "<|system|>", "<|user|>", "<|assistant|>", "<|end|>", 
                             "<|im_start|>", "<|im_end|>", "<|eot_id|>", "<|begin_of_text|>",
                             "<|start_header_id|>", "<|end_header_id|>" };
        
        foreach (var marker in markers)
        {
            // Only match if marker is at the start of a line
            var pattern = $@"(^|\n){Regex.Escape(marker)}";
            var match = Regex.Match(output, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                output = output.Substring(0, match.Index).Trim();
                _logger?.LogDebug("?? Cleaned marker '{Marker}' from output", marker);
            }
        }
        
        // Remove role markers ONLY if they're at line start followed by colon
        // Pattern: "\nUser:" or "\nAssistant:" or "\nSystem:" at start of line
        var rolePattern = @"\n(User|Human|Assistant|System)\s*:";
        var roleMatch = Regex.Match(output, rolePattern, RegexOptions.IgnoreCase);
        if (roleMatch.Success)
        {
            output = output.Substring(0, roleMatch.Index).Trim();
            _logger?.LogDebug("?? Cleaned role marker from output");
        }

        // Remove any trailing incomplete sentences (rare edge case)
        // Only if output is very long and ends mid-word
        if (output.Length > 200 && !output.EndsWith(".") && !output.EndsWith("!") && !output.EndsWith("?"))
        {
            var lastSentenceEnd = Math.Max(
                Math.Max(output.LastIndexOf('.'), output.LastIndexOf('!')), 
                output.LastIndexOf('?'));
            
            if (lastSentenceEnd > output.Length * 0.7) // Only trim if we're not losing too much
            {
                output = output.Substring(0, lastSentenceEnd + 1).Trim();
                _logger?.LogDebug("?? Trimmed incomplete sentence from output");
            }
        }

        return output;
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

    public void Dispose()
    {
        _logger?.LogInformation("??? Disposing LLamaSharpAdapter...");
        _engine?.Dispose();
        _logger?.LogDebug("? LLamaSharpAdapter disposed");
        GC.SuppressFinalize(this);
    }
}
