using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;

namespace SoloAdventureSystem.ContentGenerator.EmbeddedModel;

/// <summary>
/// LLamaSharp-based inference engine for truly embedded AI.
/// Uses llama.cpp backend with GGUF models for 100% offline generation.
/// </summary>
public class LLamaInferenceEngine : IDisposable
{
    private readonly ILogger<LLamaInferenceEngine>? _logger;
    private LLamaWeights? _weights;
    private LLamaContext? _context;
    private InteractiveExecutor? _executor;
    private bool _isLoaded;
    private readonly object _lockObject = new();

    public LLamaInferenceEngine(ILogger<LLamaInferenceEngine>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Loads the GGUF model from the specified path.
    /// Model remains in memory for fast inference.
    /// </summary>
    public void LoadModel(string modelPath, int contextSize = 2048, int gpuLayers = 0, int threads = 4, uint? seed = null)
    {
        lock (_lockObject)
        {
            if (_isLoaded)
            {
                _logger?.LogDebug("Model already loaded, skipping reload");
                return;
            }

            try
            {
                _logger?.LogInformation("Loading GGUF model from {Path}...", modelPath);
                var startTime = DateTime.UtcNow;

                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = (uint)contextSize,
                    GpuLayerCount = gpuLayers,
                    Seed = seed ?? 1337u,
                    MainGpu = 0,
                    SplitMode = LLama.Native.GPUSplitMode.None
                };

                _weights = LLamaWeights.LoadFromFile(parameters);
                _context = _weights.CreateContext(parameters);
                _executor = new InteractiveExecutor(_context);

                var loadTime = DateTime.UtcNow - startTime;
                _logger?.LogInformation("Model loaded successfully in {Time:F1}s", loadTime.TotalSeconds);

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load model from {Path}", modelPath);
                throw new InvalidOperationException(
                    $"Failed to load AI model from {modelPath}. The model file may be corrupted or incompatible.", ex);
            }
        }
    }

    /// <summary>
    /// Generates text based on the input prompt.
    /// </summary>
    public string Generate(
        string prompt,
        int maxTokens = 150,
        float temperature = 0.7f,
        int seed = -1)
    {
        if (!_isLoaded || _executor == null)
        {
            throw new InvalidOperationException("Model not loaded. Call LoadModel() first.");
        }

        try
        {
            _logger?.LogDebug("Generating text (max tokens: {MaxTokens}, temp: {Temp})", maxTokens, temperature);

            var startTime = DateTime.UtcNow;

            var inferenceParams = new InferenceParams
            {
                MaxTokens = maxTokens,
                Temperature = temperature,
                AntiPrompts = new List<string> { "\nUser:", "\nHuman:", "\n\n\n" }
            };

            var tokens = new List<string>();
            
            // Use synchronous foreach on the async enumerable
            var task = Task.Run(async () =>
            {
                await foreach (var output in _executor.InferAsync(prompt, inferenceParams))
                {
                    tokens.Add(output);
                    if (tokens.Count >= maxTokens)
                        break;
                }
            });
            
            task.Wait();

            var result = string.Join("", tokens).Trim();

            var generationTime = DateTime.UtcNow - startTime;
            var tokensPerSecond = tokens.Count / Math.Max(generationTime.TotalSeconds, 0.001);

            _logger?.LogDebug("Generated {Tokens} tokens in {Time:F1}s ({TPS:F1} tok/s)",
                tokens.Count, generationTime.TotalSeconds, tokensPerSecond);

            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to generate text");
            throw new InvalidOperationException("AI generation failed. The model may have encountered an error.", ex);
        }
    }

    /// <summary>
    /// Unloads the model from memory.
    /// </summary>
    public void UnloadModel()
    {
        lock (_lockObject)
        {
            if (!_isLoaded)
                return;

            _logger?.LogInformation("Unloading model from memory");

            _executor = null;
            _context?.Dispose();
            _weights?.Dispose();

            _context = null;
            _weights = null;
            _isLoaded = false;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    public void Dispose()
    {
        UnloadModel();
    }
}
