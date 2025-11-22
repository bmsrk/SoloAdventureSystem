using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoloAdventureSystem.ContentGenerator.EmbeddedModel;

/// <summary>
/// Downloads and manages GGUF models from HuggingFace.
/// Provides access to small, efficient models for local inference.
/// Models are persisted in user's AppData folder and reused across sessions.
/// </summary>
public class GGUFModelDownloader
{
    private readonly ILogger<GGUFModelDownloader>? _logger;
    private readonly HttpClient _httpClient;

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
    /// </summary>
    public async Task<string> EnsureModelAvailableAsync(
        string modelKey = "phi-3-mini-q4",
        IProgress<DownloadProgress>? progress = null)
    {
        if (!MODELS.TryGetValue(modelKey, out var modelInfo))
        {
            throw new ArgumentException($"Unknown model: {modelKey}. Available: {string.Join(", ", MODELS.Keys)}");
        }

        var modelPath = GetModelPath(modelKey);

        // Check if model already exists and is valid (PERSISTED)
        if (File.Exists(modelPath))
        {
            var fileInfo = new FileInfo(modelPath);
            if (fileInfo.Length > modelInfo.ExpectedSize * 0.9) // Within 10% of expected size
            {
                _logger?.LogInformation("? Using cached model at {Path} ({SizeMB} MB) - No download needed!",
                    modelPath, fileInfo.Length / 1024 / 1024);
                return modelPath;
            }
            else
            {
                _logger?.LogWarning("Existing model appears corrupted (size: {Size}). Re-downloading...", fileInfo.Length);
                File.Delete(modelPath);
            }
        }

        // Download model (will be persisted for future use)
        var url = $"https://huggingface.co/{modelInfo.Repo}/resolve/main/{modelInfo.Filename}";
        _logger?.LogInformation("Downloading model {ModelKey} from {Url}...", modelKey, url);
        _logger?.LogInformation("Model will be cached at: {Path}", modelPath);

        await DownloadModelAsync(url, modelPath, modelInfo.ExpectedSize, progress);

        _logger?.LogInformation("? Model downloaded and cached successfully at {Path}", modelPath);
        _logger?.LogInformation("Future generations will use the cached model - no re-download needed!");
        return modelPath;
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

        try
        {
            _logger?.LogInformation("Starting download to {TempPath}", tempPath);

            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? expectedSize;
            _logger?.LogInformation("Content length: {TotalMB} MB", totalBytes / 1024 / 1024);

            var downloadedBytes = 0L;
            var buffer = new byte[8192];

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var startTime = DateTime.UtcNow;
            var lastReportTime = DateTime.UtcNow;

            while (true)
            {
                var bytesRead = await contentStream.ReadAsync(buffer);
                if (bytesRead == 0) break;

                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                downloadedBytes += bytesRead;

                // Report progress every 500ms
                var now = DateTime.UtcNow;
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
            }

            _logger?.LogInformation("Download complete. Moving to final location...");

            // Move temp file to final location
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }
            File.Move(tempPath, destinationPath);

            _logger?.LogInformation("Model file saved to {Path}", destinationPath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to download model");

            // Cleanup temp file
            if (File.Exists(tempPath))
                File.Delete(tempPath);

            throw new InvalidOperationException(
                $"Failed to download model: {ex.Message}. Check your internet connection.", ex);
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
