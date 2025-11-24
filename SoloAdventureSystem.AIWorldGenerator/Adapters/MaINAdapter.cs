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

    public string GenerateRoomDescription(string context, int seed)
    {
        EnsureInitialized();
        
        return _policy.Execute(
            () => Generate(context, seed, GenerationLimits.RoomDescriptionTokens),
            "RoomDescription");
    }

    public string GenerateNpcBio(string context, int seed)
    {
        EnsureInitialized();
        
        return _policy.Execute(
            () => Generate(context, seed, GenerationLimits.NpcBioTokens),
            "NpcBio");
    }

    public string GenerateFactionFlavor(string context, int seed)
    {
        EnsureInitialized();
        
        return _policy.Execute(
            () => Generate(context, seed, GenerationLimits.FactionLoreTokens),
            "FactionFlavor");
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

                var entry = _policy.Execute(
                    () => Generate(context, seed + i, GenerationLimits.LoreEntryTokens),
                    $"LoreEntry{i + 1}");
                
                entries.Add(entry);
                _logger?.LogDebug("? Lore entry {Index} generated ({Length} chars)", i + 1, entry.Length);
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