using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SoloAdventureSystem.ContentGenerator.EmbeddedModel;

/// <summary>
/// Downloads and manages embedded AI models for offline world generation.
/// Uses Phi-3 Mini ONNX model for fast, local inference.
/// </summary>
public class ModelDownloader
{
    private readonly ILogger<ModelDownloader>? _logger;
    private readonly HttpClient _httpClient;
    
    // Phi-3 Mini 4K Instruct ONNX (INT4 quantized for efficiency)
    private const string DEFAULT_MODEL_URL = 
        "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-onnx/resolve/main/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4/phi-3-mini-4k-instruct-cpu-int4-rtn-block-32-acc-level-4.onnx";
    
    // Alternative URL using HF CDN
    private const string FALLBACK_MODEL_URL =
        "https://cdn-lfs-us-1.huggingface.co/repos/96/a8/96a81f55e0d4c2e0e53b2f5e3b6f7c6f/phi-3-mini-4k-instruct-cpu-int4-rtn-block-32-acc-level-4.onnx";
    
    private const string DEFAULT_MODEL_DIR = "models";
    private const string DEFAULT_MODEL_FILENAME = "phi-3-mini-int4.onnx";
    private const long EXPECTED_MODEL_SIZE = 2_300_000_000; // ~2.3 GB
    
    public ModelDownloader(ILogger<ModelDownloader>? logger = null)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(30) // Large model download
        };
    }
    
    /// <summary>
    /// Ensures the model is downloaded and ready to use.
    /// Downloads if missing or corrupted.
    /// </summary>
    public async Task<string> EnsureModelAvailableAsync(
        string? customUrl = null,
        IProgress<DownloadProgress>? progress = null)
    {
        var modelPath = GetModelPath();
        
        // Check if model already exists and is valid
        if (File.Exists(modelPath))
        {
            var fileInfo = new FileInfo(modelPath);
            if (fileInfo.Length > EXPECTED_MODEL_SIZE * 0.9) // Within 10% of expected size
            {
                _logger?.LogInformation("Model already exists at {Path} ({Size} MB)", 
                    modelPath, fileInfo.Length / 1024 / 1024);
                return modelPath;
            }
            else
            {
                _logger?.LogWarning("Existing model appears corrupted. Re-downloading...");
                File.Delete(modelPath);
            }
        }
        
        // Download model
        var url = customUrl ?? DEFAULT_MODEL_URL;
        _logger?.LogInformation("Downloading model from {Url}...", url);
        
        await DownloadModelAsync(url, modelPath, progress);
        
        _logger?.LogInformation("Model downloaded successfully to {Path}", modelPath);
        return modelPath;
    }
    
    /// <summary>
    /// Gets the local path where the model should be stored.
    /// </summary>
    public static string GetModelPath()
    {
        var modelDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SoloAdventureSystem",
            DEFAULT_MODEL_DIR);
        
        Directory.CreateDirectory(modelDir);
        return Path.Combine(modelDir, DEFAULT_MODEL_FILENAME);
    }
    
    /// <summary>
    /// Downloads the model file with progress reporting.
    /// </summary>
    private async Task DownloadModelAsync(
        string url, 
        string destinationPath, 
        IProgress<DownloadProgress>? progress)
    {
        var tempPath = destinationPath + ".tmp";
        
        try
        {
            _logger?.LogInformation("Starting download from {Url}", url);
            _logger?.LogInformation("Destination: {Path}", destinationPath);
            _logger?.LogInformation("Temp file: {TempPath}", tempPath);
            
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            
            _logger?.LogInformation("Response status: {StatusCode}", response.StatusCode);
            
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            _logger?.LogInformation("Content length: {TotalBytes} bytes ({TotalMB} MB)", 
                totalBytes, totalBytes / 1024 / 1024);
            
            var downloadedBytes = 0L;
            var buffer = new byte[8192];
            
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            var startTime = DateTime.UtcNow;
            var lastReportTime = DateTime.UtcNow;
            
            while (true)
            {
                var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                
                await fileStream.WriteAsync(buffer, 0, bytesRead);
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
            
            _logger?.LogInformation("Download complete. Moving temp file to destination...");
            
            // Move temp file to final location
            if (File.Exists(destinationPath))
            {
                _logger?.LogWarning("Destination file exists, deleting...");
                File.Delete(destinationPath);
            }
            File.Move(tempPath, destinationPath);
            
            _logger?.LogInformation("Model file successfully saved to {Path}", destinationPath);
        }
        catch (HttpRequestException httpEx)
        {
            _logger?.LogError(httpEx, "HTTP request failed");
            
            // Cleanup temp file
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            
            throw new InvalidOperationException(
                $"Network error downloading model. Status: {httpEx.StatusCode}. " +
                $"Check your internet connection and firewall settings.", httpEx);
        }
        catch (IOException ioEx)
        {
            _logger?.LogError(ioEx, "I/O error during download");
            
            // Cleanup temp file
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            
            throw new InvalidOperationException(
                $"File system error: {ioEx.Message}. " +
                $"Check disk space and permissions.", ioEx);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to download model");
            
            // Cleanup temp file
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            
            throw new InvalidOperationException(
                $"Failed to download model from {url}. " +
                $"Error: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Checks if the model is already downloaded.
    /// </summary>
    public static bool IsModelDownloaded()
    {
        var modelPath = GetModelPath();
        if (!File.Exists(modelPath))
            return false;
        
        var fileInfo = new FileInfo(modelPath);
        return fileInfo.Length > EXPECTED_MODEL_SIZE * 0.9;
    }
    
    /// <summary>
    /// Deletes the downloaded model (for cleanup/reset).
    /// </summary>
    public static void DeleteModel()
    {
        var modelPath = GetModelPath();
        if (File.Exists(modelPath))
        {
            File.Delete(modelPath);
        }
    }
}

/// <summary>
/// Progress information for model download.
/// </summary>
public class DownloadProgress
{
    public long DownloadedBytes { get; set; }
    public long TotalBytes { get; set; }
    public int PercentComplete { get; set; }
    public long SpeedBytesPerSecond { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    
    public string GetSpeedFormatted()
    {
        var speedMB = SpeedBytesPerSecond / 1024.0 / 1024.0;
        return $"{speedMB:F2} MB/s";
    }
    
    public string GetProgressFormatted()
    {
        var downloadedMB = DownloadedBytes / 1024.0 / 1024.0;
        var totalMB = TotalBytes / 1024.0 / 1024.0;
        return $"{downloadedMB:F1} MB / {totalMB:F1} MB ({PercentComplete}%)";
    }
}
