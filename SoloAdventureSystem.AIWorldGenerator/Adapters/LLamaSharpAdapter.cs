using System;
using System.Collections.Generic;
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
            _logger?.LogDebug("Adapter already initialized");
            return;
        }

        try
        {
            _logger?.LogInformation("Initializing LLamaSharp adapter...");

            // Ensure model is downloaded
            var downloader = new GGUFModelDownloader(_logger as ILogger<GGUFModelDownloader>);
            var modelKey = _settings.LLamaModelKey ?? "phi-3-mini-q4";
            var modelPath = await downloader.EnsureModelAvailableAsync(modelKey, progress);

            // Load model into memory
            var contextSize = _settings.ContextSize ?? 2048;
            var gpuLayers = _settings.UseGPU ? 35 : 0;
            var threads = _settings.MaxInferenceThreads;
            var seed = _settings.Seed.HasValue ? (uint)_settings.Seed.Value : (uint?)null;

            _logger?.LogInformation("Loading model with context={Context}, GPU={GPU}, threads={Threads}",
                contextSize, _settings.UseGPU, threads);

            _engine.LoadModel(modelPath, contextSize, gpuLayers, threads, seed);

            _isInitialized = true;
            _logger?.LogInformation("LLamaSharp adapter initialized successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize LLamaSharp adapter");
            throw new InvalidOperationException(
                "Failed to initialize embedded AI model. Check the logs for details.", ex);
        }
    }

    public string GenerateRoomDescription(string context, int seed)
    {
        EnsureInitialized();

        try
        {
            var systemPrompt = _optimizer.OptimizeSystemPrompt(PromptTemplates.RoomDescriptionSystem);
            var fullPrompt = FormatInstructPrompt(systemPrompt, context);

            var output = _engine.Generate(
                fullPrompt,
                maxTokens: 200,
                temperature: 0.7f,
                seed: seed);

            return CleanOutput(output);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate room description");
            throw new InvalidOperationException(
                $"AI generation failed for room description. Error: {ex.Message}", ex);
        }
    }

    public string GenerateNpcBio(string context, int seed)
    {
        EnsureInitialized();

        try
        {
            var systemPrompt = _optimizer.OptimizeSystemPrompt(PromptTemplates.NpcBioSystem);
            var fullPrompt = FormatInstructPrompt(systemPrompt, context);

            var output = _engine.Generate(
                fullPrompt,
                maxTokens: 150,
                temperature: 0.7f,
                seed: seed);

            return CleanOutput(output);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate NPC bio");
            throw new InvalidOperationException(
                $"AI generation failed for NPC bio. Error: {ex.Message}", ex);
        }
    }

    public string GenerateFactionFlavor(string context, int seed)
    {
        EnsureInitialized();

        try
        {
            var systemPrompt = _optimizer.OptimizeSystemPrompt(PromptTemplates.FactionLoreSystem);
            var fullPrompt = FormatInstructPrompt(systemPrompt, context);

            var output = _engine.Generate(
                fullPrompt,
                maxTokens: 200,
                temperature: 0.7f,
                seed: seed);

            return CleanOutput(output);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate faction flavor");
            throw new InvalidOperationException(
                $"AI generation failed for faction. Error: {ex.Message}", ex);
        }
    }

    public List<string> GenerateLoreEntries(string context, int seed, int count)
    {
        EnsureInitialized();

        var entries = new List<string>();
        var systemPrompt = _optimizer.OptimizeSystemPrompt(PromptTemplates.WorldLoreSystem);

        for (int i = 0; i < count; i++)
        {
            try
            {
                var userPrompt = $"{context} (Entry #{i + 1})";
                var fullPrompt = FormatInstructPrompt(systemPrompt, userPrompt);

                var output = _engine.Generate(
                    fullPrompt,
                    maxTokens: 100,
                    temperature: 0.8f, // Higher temp for variety
                    seed: seed + i);

                entries.Add(CleanOutput(output));
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to generate lore entry {Index}", i + 1);
                entries.Add($"Lore entry {i + 1} (generation failed)");
            }
        }

        return entries;
    }

    /// <summary>
    /// Formats the prompt using Phi-3 instruct template.
    /// </summary>
    private string FormatInstructPrompt(string system, string user)
    {
        // Phi-3 instruction template
        return $"<|system|>\n{system}<|end|>\n<|user|>\n{user}<|end|>\n<|assistant|>\n";
    }

    /// <summary>
    /// Cleans up the model output (removes artifacts, extra whitespace).
    /// </summary>
    private string CleanOutput(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return string.Empty;

        // Remove common artifacts
        output = output.Trim();

        // Remove instruction markers if they appear in output
        var markers = new[] { "<|system|>", "<|user|>", "<|assistant|>", "<|end|>", "User:", "Assistant:" };
        foreach (var marker in markers)
        {
            var index = output.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                output = output.Substring(0, index).Trim();
            }
        }

        return output;
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException(
                "Adapter not initialized. Call InitializeAsync() before using.");
        }
    }

    public void Dispose()
    {
        _engine?.Dispose();
    }
}
