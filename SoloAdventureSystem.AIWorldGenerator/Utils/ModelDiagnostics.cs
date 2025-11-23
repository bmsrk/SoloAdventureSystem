using System;
using System.IO;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;

namespace SoloAdventureSystem.ContentGenerator.Utils;

/// <summary>
/// Diagnostic utility for troubleshooting model persistence and caching issues.
/// </summary>
public static class ModelDiagnostics
{
    /// <summary>
    /// Performs comprehensive diagnostics on the model cache.
    /// </summary>
    public static string RunDiagnostics(ILogger? logger = null)
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("?????????????????????????????????????????????????????????");
        report.AppendLine("?         MODEL CACHE DIAGNOSTIC REPORT                 ?");
        report.AppendLine("?????????????????????????????????????????????????????????");
        report.AppendLine();

        // 1. Check cache directory
        var cacheDir = GGUFModelDownloader.GetModelCacheDirectory();
        report.AppendLine($"?? Cache Directory: {cacheDir}");
        
        if (!Directory.Exists(cacheDir))
        {
            report.AppendLine("? ISSUE: Cache directory does not exist!");
            report.AppendLine($"?? Creating directory: {cacheDir}");
            try
            {
                Directory.CreateDirectory(cacheDir);
                report.AppendLine("? Directory created successfully");
            }
            catch (Exception ex)
            {
                report.AppendLine($"? Failed to create directory: {ex.Message}");
                return report.ToString();
            }
        }
        else
        {
            report.AppendLine("? Cache directory exists");
        }
        
        report.AppendLine();

        // 2. Check directory permissions
        try
        {
            var testFile = Path.Combine(cacheDir, $"test_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            report.AppendLine("? Directory is writable");
        }
        catch (Exception ex)
        {
            report.AppendLine($"? ISSUE: Directory is not writable!");
            report.AppendLine($"   Error: {ex.Message}");
            report.AppendLine("?? Check file permissions on the cache directory");
        }
        
        report.AppendLine();

        // 3. List all files in cache directory
        var files = Directory.GetFiles(cacheDir, "*.*", SearchOption.AllDirectories);
        report.AppendLine($"?? Files in cache: {files.Length}");
        
        if (files.Length > 0)
        {
            report.AppendLine();
            report.AppendLine("Files found:");
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var sizeMB = fileInfo.Length / 1024.0 / 1024.0;
                var age = DateTime.Now - fileInfo.LastWriteTime;
                report.AppendLine($"   • {Path.GetFileName(file)}");
                report.AppendLine($"     Size: {sizeMB:F1} MB | Age: {age.Days}d {age.Hours}h");
            }
        }
        
        report.AppendLine();

        // 4. Check cached models
        var cachedModels = ModelCacheInfo.GetAllCachedModels();
        report.AppendLine($"?? Cached Models: {cachedModels.Count}");
        
        if (cachedModels.Count > 0)
        {
            report.AppendLine();
            foreach (var model in cachedModels)
            {
                var status = model.IsValid ? "? Valid" : "? Corrupted";
                report.AppendLine($"   • {model.ModelKey} - {status}");
                report.AppendLine($"     Size: {model.SizeMB:F1} MB");
                report.AppendLine($"     Path: {model.Path}");
                report.AppendLine($"     Last Modified: {model.LastModified:yyyy-MM-dd HH:mm:ss}");
                
                if (!model.IsValid)
                {
                    report.AppendLine($"     ?? Model appears corrupted - delete and re-download");
                }
                
                report.AppendLine();
            }
        }
        else
        {
            report.AppendLine("??  No models cached yet. Models will be downloaded on first use.");
            report.AppendLine();
        }

        // 5. Check disk space
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(cacheDir)!);
            var freeSpaceGB = drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0;
            report.AppendLine($"?? Available Disk Space: {freeSpaceGB:F1} GB");
            
            if (freeSpaceGB < 3)
            {
                report.AppendLine("??  WARNING: Low disk space! Recommend at least 3GB free for model downloads.");
            }
            else
            {
                report.AppendLine("? Sufficient disk space available");
            }
        }
        catch (Exception ex)
        {
            report.AppendLine($"??  Could not check disk space: {ex.Message}");
        }
        
        report.AppendLine();

        // 6. Check for temporary files
        var tempFiles = Directory.GetFiles(cacheDir, "*.tmp", SearchOption.AllDirectories);
        if (tempFiles.Length > 0)
        {
            report.AppendLine($"??  Found {tempFiles.Length} temporary file(s) - may indicate interrupted downloads:");
            foreach (var tmpFile in tempFiles)
            {
                var fileInfo = new FileInfo(tmpFile);
                report.AppendLine($"   • {Path.GetFileName(tmpFile)} ({fileInfo.Length / 1024.0 / 1024.0:F1} MB)");
                report.AppendLine($"     ?? Safe to delete if download completed or failed");
            }
            report.AppendLine();
        }

        // 7. Summary
        report.AppendLine("???????????????????????????????????????????????????????");
        report.AppendLine("SUMMARY:");
        
        var issues = new System.Collections.Generic.List<string>();
        if (!Directory.Exists(cacheDir)) issues.Add("Cache directory missing");
        if (cachedModels.Any(m => !m.IsValid)) issues.Add("Corrupted models detected");
        if (tempFiles.Length > 0) issues.Add("Temporary files present");
        
        if (issues.Count == 0)
        {
            report.AppendLine("? No issues detected - model cache is healthy");
        }
        else
        {
            report.AppendLine($"??  {issues.Count} issue(s) detected:");
            foreach (var issue in issues)
            {
                report.AppendLine($"   • {issue}");
            }
        }
        
        report.AppendLine("???????????????????????????????????????????????????????");
        
        logger?.LogInformation("Model diagnostics completed");
        
        return report.ToString();
    }

    /// <summary>
    /// Cleans up temporary files and corrupted models.
    /// </summary>
    public static int CleanupCache(ILogger? logger = null)
    {
        logger?.LogInformation("?? Starting cache cleanup...");
        
        var cacheDir = GGUFModelDownloader.GetModelCacheDirectory();
        if (!Directory.Exists(cacheDir))
        {
            logger?.LogWarning("Cache directory does not exist, nothing to clean");
            return 0;
        }

        int cleanedCount = 0;

        // Remove temporary files
        var tempFiles = Directory.GetFiles(cacheDir, "*.tmp", SearchOption.AllDirectories);
        foreach (var tmpFile in tempFiles)
        {
            try
            {
                File.Delete(tmpFile);
                logger?.LogInformation("???  Deleted temporary file: {File}", Path.GetFileName(tmpFile));
                cleanedCount++;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to delete temporary file: {File}", tmpFile);
            }
        }

        // Remove corrupted models
        var cachedModels = ModelCacheInfo.GetAllCachedModels();
        foreach (var model in cachedModels.Where(m => !m.IsValid))
        {
            try
            {
                File.Delete(model.Path);
                logger?.LogInformation("???  Deleted corrupted model: {ModelKey}", model.ModelKey);
                cleanedCount++;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to delete corrupted model: {ModelKey}", model.ModelKey);
            }
        }

        logger?.LogInformation("? Cleanup complete - removed {Count} file(s)", cleanedCount);
        return cleanedCount;
    }
}
