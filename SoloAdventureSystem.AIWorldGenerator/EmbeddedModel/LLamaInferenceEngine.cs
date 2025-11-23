using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Logging;

namespace SoloAdventureSystem.ContentGenerator.EmbeddedModel;

/// <summary>
/// LLamaSharp-based inference engine for truly embedded AI.
/// Uses llama.cpp backend with GGUF models for 100% offline generation.
/// Thread-safe with mutex protection for model file access.
/// </summary>
public class LLamaInferenceEngine : IDisposable
{
    private readonly ILogger<LLamaInferenceEngine>? _logger;
    private LLamaWeights? _weights;
    private LLamaContext? _context;
    private InteractiveExecutor? _executor;
    private bool _isLoaded;
    private readonly object _lockObject = new();
    private string? _loadedModelPath;

    public LLamaInferenceEngine(ILogger<LLamaInferenceEngine>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Loads the GGUF model from the specified path.
    /// Model remains in memory for fast inference.
    /// Thread-safe with file-level mutex protection.
    /// </summary>
    public void LoadModel(string modelPath, int contextSize = 2048, int gpuLayers = 0, int threads = 4, uint? seed = null)
    {
        lock (_lockObject)
        {
            if (_isLoaded && _loadedModelPath == modelPath)
            {
                _logger?.LogDebug("? Model already loaded in memory, skipping reload");
                return;
            }

            // Get mutex name based on model path (use hash to avoid path length issues)
            var mutexName = $"Global\\SoloAdventureSystem_Model_{GetStableHash(modelPath):X8}";
            
            Mutex? fileMutex = null;
            bool mutexAcquired = false;
            bool createdNew = false;
            
            try
            {
                _logger?.LogDebug("?? Acquiring model file mutex: {MutexName}", mutexName);
                
                // Try to open existing mutex or create new one
                fileMutex = new Mutex(false, mutexName, out createdNew);
                
                if (createdNew)
                {
                    _logger?.LogDebug("?? Created new file mutex");
                }
                else
                {
                    _logger?.LogDebug("?? Opened existing file mutex");
                }
                
                // Wait up to 2 minutes for the mutex
                mutexAcquired = fileMutex.WaitOne(TimeSpan.FromMinutes(2));
                
                if (!mutexAcquired)
                {
                    _logger?.LogWarning("?? Could not acquire model file mutex within timeout");
                    throw new TimeoutException($"Could not acquire lock for model file {modelPath}. Another process may be loading the model.");
                }
                
                _logger?.LogDebug("? Model file mutex acquired");
                _logger?.LogInformation("?? Loading GGUF model from {Path}...", modelPath);
                
                // Verify file exists before attempting to load
                if (!File.Exists(modelPath))
                {
                    _logger?.LogError("? Model file not found at {Path}", modelPath);
                    throw new FileNotFoundException($"Model file not found: {modelPath}", modelPath);
                }
                
                var fileInfo = new FileInfo(modelPath);
                _logger?.LogInformation("?? Model file size: {SizeMB:F1} MB", fileInfo.Length / 1024.0 / 1024.0);
                _logger?.LogInformation("?? Model parameters: ContextSize={Context}, GpuLayers={Gpu}, Threads={Threads}, Seed={Seed}",
                    contextSize, gpuLayers, threads, seed ?? 1337u);
                
                var startTime = DateTime.UtcNow;

                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = (uint)contextSize,
                    GpuLayerCount = gpuLayers,
                    Seed = seed ?? 1337u,
                    MainGpu = 0,
                    SplitMode = LLama.Native.GPUSplitMode.None
                };

                _logger?.LogDebug("?? Loading model weights...");
                _weights = LLamaWeights.LoadFromFile(parameters);
                _logger?.LogDebug("? Model weights loaded");
                
                _logger?.LogDebug("?? Creating model context...");
                _context = _weights.CreateContext(parameters);
                _logger?.LogDebug("? Model context created");
                
                _logger?.LogDebug("? Initializing executor...");
                _executor = new InteractiveExecutor(_context);
                _logger?.LogDebug("? Executor initialized");

                var loadTime = DateTime.UtcNow - startTime;
                _logger?.LogInformation("? Model loaded successfully in {Time:F1}s", loadTime.TotalSeconds);
                _logger?.LogInformation("?? Model ready for inference (context size: {Context} tokens)", contextSize);

                _isLoaded = true;
                _loadedModelPath = modelPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "? Failed to load model from {Path}", modelPath);
                _logger?.LogError("?? Troubleshooting tips:");
                _logger?.LogError("   1. Verify the model file exists and is not corrupted");
                _logger?.LogError("   2. Ensure you have enough RAM (model size + context buffer)");
                _logger?.LogError("   3. Check file permissions on the model directory");
                _logger?.LogError("   4. Try deleting the model and re-downloading it");
                
                throw new InvalidOperationException(
                    $"Failed to load AI model from {modelPath}. The model file may be corrupted or incompatible. " +
                    $"Error: {ex.Message}", ex);
            }
            finally
            {
                if (mutexAcquired && fileMutex != null)
                {
                    try
                    {
                        _logger?.LogDebug("?? Releasing model file mutex");
                        fileMutex.ReleaseMutex();
                    }
                    catch (ApplicationException ex)
                    {
                        _logger?.LogWarning(ex, "?? Failed to release file mutex (may have been abandoned)");
                    }
                }
                
                fileMutex?.Dispose();
            }
        }
    }

    /// <summary>
    /// Generates text based on the input prompt.
    /// Automatically clears context after generation to prevent KV cache overflow.
    /// </summary>
    public string Generate(
        string prompt,
        int maxTokens = 150,
        float temperature = 0.7f,
        int seed = -1,
        TimeSpan? timeout = null)
    {
        if (!_isLoaded || _executor == null || _context == null)
        {
            _logger?.LogError("? Cannot generate: Model not loaded");
            throw new InvalidOperationException("Model not loaded. Call LoadModel() first.");
        }

        var actualTimeout = timeout ?? TimeSpan.FromMinutes(5); // Default 5 min timeout

        try
        {
            var promptPreview = prompt.Length > 100 ? prompt.Substring(0, 100) + "..." : prompt;
            _logger?.LogDebug("?? Generating text - MaxTokens: {MaxTokens}, Temp: {Temp}, Timeout: {Timeout}, Prompt: \"{Preview}\"", 
                maxTokens, temperature, actualTimeout, promptPreview);

            var startTime = DateTime.UtcNow;

            // Clear context before each generation to prevent KV cache overflow
            _logger?.LogDebug("?? Clearing context to prevent KV cache overflow...");
            _context.NativeHandle.KvCacheClear();
            
            var inferenceParams = new InferenceParams
            {
                MaxTokens = maxTokens,
                // Use SamplingPipeline instead of obsolete Temperature property
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = temperature
                },
                // Enhanced anti-prompts to stop at natural boundaries
                AntiPrompts = new List<string> 
                { 
                    "\nUser:", 
                    "\nHuman:", 
                    "\nAssistant:",
                    "\n\n\n",      // Triple newline (common in bad output)
                    "Example:",    // Prevent it from generating examples
                    "BAD Example:", 
                    "GOOD Example:",
                    "\nWrite",     // Stop before meta-instructions
                    "\nSentence",  // Stop before structural instructions
                }
            };

            var tokens = new List<string>();
            var lastLogTime = DateTime.UtcNow;
            
            var task = Task.Run(async () =>
            {
                await foreach (var output in _executor.InferAsync(prompt, inferenceParams))
                {
                    // Check timeout during generation
                    if ((DateTime.UtcNow - startTime) > actualTimeout)
                    {
                        _logger?.LogWarning("?? Generation timeout after {Timeout}", actualTimeout);
                        break;
                    }
                    
                    tokens.Add(output);
                    
                    // Log progress every 5 seconds for long generations
                    var now = DateTime.UtcNow;
                    if ((now - lastLogTime).TotalSeconds >= 5)
                    {
                        lastLogTime = now;
                        var elapsed = now - startTime;
                        var tokensPerSecond = tokens.Count / Math.Max(elapsed.TotalSeconds, 0.001);
                        _logger?.LogDebug("? Generation in progress: {Tokens}/{MaxTokens} tokens ({TPS:F1} tok/s)",
                            tokens.Count, maxTokens, tokensPerSecond);
                    }
                    
                    if (tokens.Count >= maxTokens)
                        break;
                }
            });
            
            // Wait with timeout
            if (!task.Wait(actualTimeout))
            {
                _logger?.LogError("? Generation timed out after {Timeout}", actualTimeout);
                
                // Clear context after timeout to ensure clean state
                try
                {
                    _context.NativeHandle.KvCacheClear();
                    _logger?.LogDebug("? Context cleared after timeout");
                }
                catch (Exception clearEx)
                {
                    _logger?.LogWarning(clearEx, "?? Failed to clear context after timeout");
                }
                
                throw new TimeoutException($"Text generation timed out after {actualTimeout}");
            }

            var result = string.Join("", tokens).Trim();

            var generationTime = DateTime.UtcNow - startTime;
            var tokensPerSecond = tokens.Count / Math.Max(generationTime.TotalSeconds, 0.001);

            _logger?.LogInformation("? Generated {Tokens} tokens in {Time:F1}s ({TPS:F1} tok/s)",
                tokens.Count, generationTime.TotalSeconds, tokensPerSecond);
                
            var resultPreview = result.Length > 100 ? result.Substring(0, 100) + "..." : result;
            _logger?.LogDebug("?? Result preview: \"{Preview}\"", resultPreview);

            return result;
        }
        catch (TimeoutException)
        {
            throw; // Re-throw timeout
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Failed to generate text");
            _logger?.LogError("?? This may indicate:");
            _logger?.LogError("   1. The model encountered an internal error");
            _logger?.LogError("   2. The prompt may be too long or malformed");
            _logger?.LogError("   3. System resources (RAM/CPU) may be exhausted");
            _logger?.LogError("   4. KV cache overflow - try reducing context size or max tokens");
            
            // Try to clear context for recovery
            try
            {
                if (_context != null)
                {
                    _context.NativeHandle.KvCacheClear();
                    _logger?.LogDebug("? Context cleared for error recovery");
                }
            }
            catch (Exception clearEx)
            {
                _logger?.LogWarning(clearEx, "?? Failed to clear context during error recovery");
            }
            
            throw new InvalidOperationException(
                $"AI generation failed. The model may have encountered an error. " +
                $"Error: {ex.Message}", ex);
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
            {
                _logger?.LogDebug("?? Model not loaded, nothing to unload");
                return;
            }

            _logger?.LogInformation("?? Unloading model from memory...");

            _executor = null;
            _logger?.LogDebug("? Executor disposed");
            
            _context?.Dispose();
            _logger?.LogDebug("? Context disposed");
            
            _weights?.Dispose();
            _logger?.LogDebug("? Weights disposed");

            _context = null;
            _weights = null;
            _isLoaded = false;
            _loadedModelPath = null;

            _logger?.LogDebug("??? Forcing garbage collection...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            _logger?.LogInformation("? Model unloaded successfully, memory freed");
        }
    }

    public void Dispose()
    {
        UnloadModel();
    }

    /// <summary>
    /// Gets a stable hash code for a file path to use in mutex names.
    /// </summary>
    private static int GetStableHash(string input)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in input.ToLowerInvariant())
            {
                hash = hash * 31 + c;
            }
            return hash;
        }
    }
}
