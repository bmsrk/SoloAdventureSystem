using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoloAdventureSystem.ContentGenerator.EmbeddedModel;

/// <summary>
/// Downloads and manages GGUF models from HuggingFace.
/// Provides access to small, efficient models for local inference.
/// Models are persisted in user's AppData folder and reused across sessions.
/// Thread-safe with mutex protection for concurrent access.
/// </summary>
public class GGUFModelDownloader
{
    private readonly ILogger<GGUFModelDownloader>? _logger;
    private readonly HttpClient _httpClient;
    
    // Global mutex names for each model to prevent concurrent access across processes
    private static readonly Dictionary<string, string> MODEL_MUTEX_NAMES = new()
    {
        ["phi-3-mini-q4"] = "Global\\SoloAdventureSystem_Phi3Mini_Mutex",
        ["tinyllama-q4"] = "Global\\SoloAdventureSystem_TinyLlama_Mutex",
        ["llama-3.2-1b-q4"] = "Global\\SoloAdventureSystem_Llama32_Mutex"
    };

    // Default: Phi-3-mini Q4_K_M (2GB, excellent quality/speed balance)
    private static readonly Dictionary<string, ModelInfo> MODELS = new()
    {
        ["phi-3-mini-q4"] = new ModelInfo(
            "microsoft/Phi-3-mini-4k-instruct-gguf",
            "Phi-3-mini-4k-instruct-q4.gguf",
            2_100_000_000
        ),
        ["tinyllama-q4"] = new ModelInfo(
            "TheBloke/TinyLlama-1.1B-Chat-v1.0-GGUF",
            "tinyllama-1.1b-chat-v1.0.Q4_K_M.gguf",
            600_000_000
        ),
        ["llama-3.2-1b-q4"] = new ModelInfo(
            "bartowski/Llama-3.2-1B-Instruct-GGUF",
            "Llama-3.2-1B-Instruct-Q4_K_M.gguf",
            800_000_000
        )
    };

    public GGUFModelDownloader(ILogger<GGUFModelDownloader>? logger = null)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(30)
        };
    }

    /// <summary>
    /// Ensures the specified model is downloaded and ready to use.
    /// Downloads if missing or corrupted. Models are cached and reused.
    /// Thread-safe with mutex protection.
    /// </summary>
    public async Task<string> EnsureModelAvailableAsync(
        string modelKey = "phi-3-mini-q4",
        IProgress<DownloadProgress>? progress = null)
    {
        _logger?.LogInformation("?? Checking model availability: {ModelKey}", modelKey);
        
        if (!MODELS.TryGetValue(modelKey, out var modelInfo))
        {
            var availableModels = string.Join(", ", MODELS.Keys);
            _logger?.LogError("? Unknown model key: {ModelKey}. Available models: {Available}", modelKey, availableModels);
            throw new ArgumentException($"Unknown model: {modelKey}. Available: {string.Join(", ", MODELS.Keys)}");
        }

        var modelPath = GetModelPath(modelKey);
        _logger?.LogDebug("?? Model path: {Path}", modelPath);

        // Get mutex name for this model
        if (!MODEL_MUTEX_NAMES.TryGetValue(modelKey, out var mutexName))
        {
            _logger?.LogWarning("?? No mutex defined for model {ModelKey}, using default", modelKey);
            mutexName = $"Global\\SoloAdventureSystem_{modelKey}_Mutex";
        }

        // Use mutex to ensure only one process/thread downloads or accesses the model at a time
        _logger?.LogDebug("?? Acquiring mutex: {MutexName}", mutexName);
        
        Mutex? mutex = null;
        bool mutexAcquired = false;
        bool createdNew = false;
        
        try
        {
            // Try to open existing mutex or create new one
            mutex = new Mutex(false, mutexName, out createdNew);
            
            if (createdNew)
            {
                _logger?.LogDebug("?? Created new mutex: {MutexName}", mutexName);
            }
            else
            {
                _logger?.LogDebug("?? Opened existing mutex: {MutexName}", mutexName);
            }
            
            // Wait up to 5 minutes for the mutex (in case another process is downloading)
            mutexAcquired = mutex.WaitOne(TimeSpan.FromMinutes(5));
            
            if (!mutexAcquired)
            {
                _logger?.LogError("? Could not acquire mutex within timeout. Another process may be stuck.");
                throw new TimeoutException($"Could not acquire model lock for {modelKey}. Another process may be using or downloading the model.");
            }
            
            _logger?.LogDebug("? Mutex acquired successfully");

            // Check if model already exists and is valid (PERSISTED)
            if (File.Exists(modelPath))
            {
                var fileInfo = new FileInfo(modelPath);
                var expectedSize = modelInfo.ExpectedSize;
                var sizeRatio = (double)fileInfo.Length / expectedSize;
                
                _logger?.LogDebug("?? Model file found - Size: {ActualMB:F1} MB / Expected: {ExpectedMB:F1} MB (Ratio: {Ratio:P0})",
                    fileInfo.Length / 1024.0 / 1024.0,
                    expectedSize / 1024.0 / 1024.0,
                    sizeRatio);
                
                if (fileInfo.Length > modelInfo.ExpectedSize * 0.9) // Within 10% of expected size
                {
                    _logger?.LogInformation("? Using cached model at {Path} ({SizeMB:F1} MB) - No download needed!",
                        modelPath, fileInfo.Length / 1024.0 / 1024.0);
                    return modelPath;
                }
                else
                {
                    _logger?.LogWarning("?? Existing model appears corrupted (size: {ActualBytes} bytes, expected ~{ExpectedBytes} bytes). Re-downloading...", 
                        fileInfo.Length, expectedSize);
                    try
                    {
                        File.Delete(modelPath);
                        _logger?.LogInformation("??? Deleted corrupted model file");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "? Failed to delete corrupted model file");
                        throw new InvalidOperationException($"Failed to delete corrupted model at {modelPath}", ex);
                    }
                }
            }
            else
            {
                _logger?.LogInformation("?? Model not cached. Will download from HuggingFace.");
            }

            // Download model (will be persisted for future use)
            var url = $"https://huggingface.co/{modelInfo.Repo}/resolve/main/{modelInfo.Filename}";
            _logger?.LogInformation("?? Downloading model {ModelKey} from {Url}...", modelKey, url);
            _logger?.LogInformation("?? Model will be cached at: {Path}", modelPath);
            _logger?.LogInformation("?? Expected size: {SizeMB:F1} MB", modelInfo.ExpectedSize / 1024.0 / 1024.0);

            await DownloadModelAsync(url, modelPath, modelInfo.ExpectedSize, progress);

            _logger?.LogInformation("? Model downloaded and cached successfully at {Path}", modelPath);
            _logger?.LogInformation("?? Future generations will use the cached model - no re-download needed!");
            return modelPath;
        }
        finally
        {
            // Always release the mutex if we acquired it
            if (mutexAcquired && mutex != null)
            {
                try
                {
                    _logger?.LogDebug("?? Releasing mutex");
                    mutex.ReleaseMutex();
                }
                catch (ApplicationException ex)
                {
                    _logger?.LogWarning(ex, "?? Failed to release mutex (may have been abandoned)");
                }
            }
            
            // Dispose mutex
            mutex?.Dispose();
        }
    }

    /// <summary>
    /// Gets the local path where the model should be stored (persistent cache).
    /// Location: %APPDATA%\SoloAdventureSystem\models\
    /// </summary>
    public static string GetModelPath(string modelKey)
    {
        var modelDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SoloAdventureSystem",
            "models");

        Directory.CreateDirectory(modelDir);
        return Path.Combine(modelDir, $"{modelKey}.gguf");
    }

    /// <summary>
    /// Gets the directory where all models are cached.
    /// </summary>
    public static string GetModelCacheDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SoloAdventureSystem",
            "models");
    }

    /// <summary>
    /// Gets information about all cached models.
    /// </summary>
    public static Dictionary<string, CachedModelInfo> GetCachedModels()
    {
        var result = new Dictionary<string, CachedModelInfo>();
        
        foreach (var (modelKey, modelInfo) in MODELS)
        {
            var path = GetModelPath(modelKey);
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                result[modelKey] = new CachedModelInfo
                {
                    ModelKey = modelKey,
                    Path = path,
                    SizeBytes = fileInfo.Length,
                    SizeMB = fileInfo.Length / 1024.0 / 1024.0,
                    LastModified = fileInfo.LastWriteTime,
                    IsValid = fileInfo.Length > modelInfo.ExpectedSize * 0.9
                };
            }
        }
        
        return result;
    }

    /// <summary>
    /// Gets total size of all cached models.
    /// </summary>
    public static long GetTotalCacheSize()
    {
        var cached = GetCachedModels();
        return cached.Values.Sum(m => m.SizeBytes);
    }

    /// <summary>
    /// Downloads the model file with progress reporting.
    /// </summary>
    private async Task DownloadModelAsync(
        string url,
        string destinationPath,
        long expectedSize,
        IProgress<DownloadProgress>? progress)
    {
        var tempPath = destinationPath + ".tmp";
        _logger?.LogDebug("?? Temporary download path: {TempPath}", tempPath);

        try
        {
            _logger?.LogInformation("?? Starting download to {TempPath}", tempPath);

            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? expectedSize;
            _logger?.LogInformation("?? Content length: {TotalMB:F1} MB", totalBytes / 1024.0 / 1024.0);

            var downloadedBytes = 0L;
            var buffer = new byte[8192];

            using (var contentStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                var startTime = DateTime.UtcNow;
                var lastReportTime = DateTime.UtcNow;
                var lastLogTime = DateTime.UtcNow;

                while (true)
                {
                    var bytesRead = await contentStream.ReadAsync(buffer);
                    if (bytesRead == 0) break;

                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    downloadedBytes += bytesRead;

                    var now = DateTime.UtcNow;
                    
                    // Report progress every 500ms
                    if (progress != null && totalBytes > 0 && (now - lastReportTime).TotalMilliseconds >= 500)
                    {
                        lastReportTime = now;
                        var percent = (int)((downloadedBytes * 100) / totalBytes);
                        var elapsed = now - startTime;
                        var speed = elapsed.TotalSeconds > 0 ? downloadedBytes / elapsed.TotalSeconds : 0;
                        var remaining = totalBytes - downloadedBytes;
                        var estimatedTimeRemaining = speed > 0
                            ? TimeSpan.FromSeconds(remaining / speed)
                            : TimeSpan.Zero;

                        progress.Report(new DownloadProgress
                        {
                            DownloadedBytes = downloadedBytes,
                            TotalBytes = totalBytes,
                            PercentComplete = percent,
                            SpeedBytesPerSecond = (long)speed,
                            EstimatedTimeRemaining = estimatedTimeRemaining
                        });
                    }
                    
                    // Log progress every 10 seconds
                    if ((now - lastLogTime).TotalSeconds >= 10)
                    {
                        lastLogTime = now;
                        var percent = (int)((downloadedBytes * 100) / totalBytes);
                        var elapsed = now - startTime;
                        var speed = elapsed.TotalSeconds > 0 ? downloadedBytes / elapsed.TotalSeconds : 0;
                        var speedMB = speed / 1024.0 / 1024.0;
                        
                        _logger?.LogInformation("?? Download progress: {Percent}% ({DownloadedMB:F1}/{TotalMB:F1} MB) at {SpeedMB:F1} MB/s",
                            percent,
                            downloadedBytes / 1024.0 / 1024.0,
                            totalBytes / 1024.0 / 1024.0,
                            speedMB);
                    }
                }
                
                // Ensure everything is written to disk before closing
                await fileStream.FlushAsync();
            } // File stream is now closed and disposed

            _logger?.LogInformation("? Download complete. Moving to final location...");
            
            // Small delay to ensure file system has released all handles
            await Task.Delay(100);

            // Ensure destination directory exists
            var destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                _logger?.LogDebug("?? Creating directory: {Directory}", destDir);
                Directory.CreateDirectory(destDir);
            }

            // Move temp file to final location with retry logic
            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // Remove existing file if present
                    if (File.Exists(destinationPath))
                    {
                        _logger?.LogDebug("??? Removing existing file at destination (attempt {Attempt})", attempt);
                        File.Delete(destinationPath);
                        await Task.Delay(100); // Brief delay after deletion
                    }
                    
                    _logger?.LogDebug("?? Moving temp file to final location (attempt {Attempt})...", attempt);
                    File.Move(tempPath, destinationPath);
                    _logger?.LogInformation("? Model file saved to {Path}", destinationPath);
                    break; // Success!
                }
                catch (IOException ioEx) when (attempt < maxRetries)
                {
                    _logger?.LogWarning("?? File move attempt {Attempt} failed: {Message}. Retrying...", 
                        attempt, ioEx.Message);
                    await Task.Delay(1000 * attempt); // Exponential backoff: 1s, 2s, 3s
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Failed to download model from {Url}", url);

            // Cleanup temp file
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                    _logger?.LogDebug("??? Cleaned up temporary file");
                }
                catch (Exception cleanupEx)
                {
                    _logger?.LogWarning(cleanupEx, "?? Failed to cleanup temporary file: {TempPath}", tempPath);
                }
            }

            throw new InvalidOperationException(
                $"Failed to download model: {ex.Message}. Check your internet connection and ensure {url} is accessible.", ex);
        }
    }

    /// <summary>
    /// Checks if the model is already downloaded.
    /// </summary>
    public static bool IsModelDownloaded(string modelKey)
    {
        if (!MODELS.ContainsKey(modelKey))
            return false;

        var modelPath = GetModelPath(modelKey);
        if (!File.Exists(modelPath))
            return false;

        var fileInfo = new FileInfo(modelPath);
        var expectedSize = MODELS[modelKey].ExpectedSize;
        return fileInfo.Length > expectedSize * 0.9;
    }

    /// <summary>
    /// Deletes the downloaded model (for cleanup/reset).
    /// </summary>
    public static void DeleteModel(string modelKey)
    {
        var modelPath = GetModelPath(modelKey);
        if (File.Exists(modelPath))
        {
            File.Delete(modelPath);
        }
    }

    private record ModelInfo(string Repo, string Filename, long ExpectedSize);
}

/// <summary>
/// Information about a cached model file.
/// </summary>
public class CachedModelInfo
{
    public string ModelKey { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public double SizeMB { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsValid { get; set; }
}
