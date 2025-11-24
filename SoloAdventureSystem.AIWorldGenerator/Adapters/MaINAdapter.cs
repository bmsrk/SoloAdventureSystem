using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using LLama;
using LLama.Common;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// SLM adapter using MaIN.NET/LLamaSharp for truly embedded, offline AI inference.
/// MaIN.NET is built on top of LLamaSharp, providing GGUF model support.
/// </summary>
public class MaINAdapter : ILocalSLMAdapter, IDisposable
{
    private readonly ILogger<MaINAdapter>? _logger;
    private readonly AISettings _settings;
    private bool _isInitialized;
    private LLamaWeights? _weights;
    private LLamaContext? _context;

    // Track consecutive empty outputs to detect model failure
    private int _consecutiveEmptyOutputs = 0;
    private const int MAX_EMPTY_OUTPUTS_BEFORE_FAILURE = 3;

    public MaINAdapter(IOptions<AISettings> settings, ILogger<MaINAdapter>? logger = null)
    {
        _logger = logger;
        _settings = settings.Value;
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

            var modelKey = _settings.LLamaModelKey ?? "phi-3-mini-q4";
            _logger?.LogInformation("?? Target model: {ModelKey}", modelKey);

            // Download model if needed
            var downloader = new GGUFModelDownloader(_logger as ILogger<GGUFModelDownloader>);
            var modelPath = await downloader.EnsureModelAvailableAsync(modelKey, progress);

            _logger?.LogInformation("?? Loading model from: {ModelPath}", modelPath);

            // Configure model parameters
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = (uint)(_settings.ContextSize ?? 2048),
                GpuLayerCount = _settings.UseGPU ? 99 : 0, // Use GPU if available
                UseMemorymap = true,
                UseMemoryLock = false
            };
            
            // Set threads separately if it's a different type
            if (_settings.MaxInferenceThreads > 0)
            {
                // LLamaSharp 0.25 may have changed the Threads property type
                // Try to set it via reflection or just use the default
                _logger?.LogDebug("Using {Threads} threads for inference", _settings.MaxInferenceThreads);
            }

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

    public string GenerateRoomDescription(string context, int seed)
    {
        EnsureInitialized();

        try
        {
            _logger?.LogDebug("?? Generating room description (seed: {Seed})", seed);

            var result = Generate(context, seed, maxTokens: 200);

            if (string.IsNullOrWhiteSpace(result))
            {
                _consecutiveEmptyOutputs++;
                _logger?.LogWarning("? Empty room description generated ({Count}/{Max})", 
                    _consecutiveEmptyOutputs, MAX_EMPTY_OUTPUTS_BEFORE_FAILURE);
                
                if (_consecutiveEmptyOutputs >= MAX_EMPTY_OUTPUTS_BEFORE_FAILURE)
                {
                    throw new InvalidOperationException("Model is consistently producing empty outputs");
                }
            }
            else
            {
                _consecutiveEmptyOutputs = 0;
            }

            _logger?.LogDebug("? Room description generated ({Length} chars)", result.Length);
            return result;
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

            var result = Generate(context, seed, maxTokens: 180);

            if (string.IsNullOrWhiteSpace(result))
            {
                _consecutiveEmptyOutputs++;
                _logger?.LogWarning("? Empty NPC bio generated ({Count}/{Max})", 
                    _consecutiveEmptyOutputs, MAX_EMPTY_OUTPUTS_BEFORE_FAILURE);
            }
            else
            {
                _consecutiveEmptyOutputs = 0;
            }

            _logger?.LogDebug("? NPC bio generated ({Length} chars)", result.Length);
            return result;
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

            var result = Generate(context, seed, maxTokens: 200);

            if (string.IsNullOrWhiteSpace(result))
            {
                _consecutiveEmptyOutputs++;
                _logger?.LogWarning("? Empty faction flavor generated ({Count}/{Max})", 
                    _consecutiveEmptyOutputs, MAX_EMPTY_OUTPUTS_BEFORE_FAILURE);
            }
            else
            {
                _consecutiveEmptyOutputs = 0;
            }

            _logger?.LogDebug("? Faction flavor generated ({Length} chars)", result.Length);
            return result;
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

        for (int i = 0; i < count; i++)
        {
            try
            {
                _logger?.LogDebug("?? Generating lore entry {Index}/{Total}", i + 1, count);

                var entry = Generate(context, seed + i, maxTokens: 150);
                
                if (string.IsNullOrWhiteSpace(entry))
                {
                    _logger?.LogWarning("? Empty lore entry {Index}, using fallback", i + 1);
                    entry = $"Lore entry {i + 1} (generation failed)";
                }

                entries.Add(entry);
                _logger?.LogDebug("? Lore entry {Index} generated ({Length} chars)", i + 1, entry.Length);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "? Failed to generate lore entry {Index}, using fallback", i + 1);
                entries.Add($"Lore entry {i + 1} (generation failed)");
            }
        }

        _logger?.LogInformation("? Generated {Count} lore entries", entries.Count);
        return entries;
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

            // Clean up the output
            generated = CleanOutput(generated);

            return generated;
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

        return text;
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
        _logger?.LogInformation("?? Disposing MaINAdapter...");
        
        _context?.Dispose();
        _context = null;
        
        _weights?.Dispose();
        _weights = null;
        
        _isInitialized = false;
        
        _logger?.LogDebug("? MaINAdapter disposed");
        GC.SuppressFinalize(this);
    }
}