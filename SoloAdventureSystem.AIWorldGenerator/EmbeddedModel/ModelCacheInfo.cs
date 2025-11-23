using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SoloAdventureSystem.ContentGenerator.Utils;

namespace SoloAdventureSystem.ContentGenerator.EmbeddedModel;

/// <summary>
/// Utilities for managing and inspecting the model cache.
/// Provides information about downloaded models and cache management.
/// </summary>
public static class ModelCacheInfo
{
    /// <summary>
    /// Gets detailed information about all cached models.
    /// </summary>
    public static List<CachedModelInfo> GetAllCachedModels()
    {
        return GGUFModelDownloader.GetCachedModels().Values.ToList();
    }

    /// <summary>
    /// Gets the total size of all cached models.
    /// </summary>
    public static long GetTotalCacheSize()
    {
        return GGUFModelDownloader.GetTotalCacheSize();
    }

    /// <summary>
    /// Gets a human-readable cache summary.
    /// </summary>
    public static string GetCacheSummary()
    {
        var models = GetAllCachedModels();
        var totalSize = GetTotalCacheSize();
        
        if (models.Count == 0)
        {
            return "No models cached. Models will be downloaded on first use.";
        }

        var sizeFormatted = PathHelper.FormatFileSize(totalSize);
        var modelNames = string.Join(", ", models.Select(m => m.ModelKey));
        
        return $"{models.Count} model(s) cached ({sizeFormatted}): {modelNames}";
    }

    /// <summary>
    /// Checks if a specific model is already cached.
    /// </summary>
    public static bool IsModelCached(string modelKey)
    {
        return GGUFModelDownloader.IsModelDownloaded(modelKey);
    }

    /// <summary>
    /// Gets the cache directory path.
    /// </summary>
    public static string GetCacheDirectory()
    {
        return GGUFModelDownloader.GetModelCacheDirectory();
    }

    /// <summary>
    /// Deletes a specific cached model to free up space.
    /// </summary>
    /// <param name="modelKey">The model key to delete</param>
    /// <returns>True if model was deleted, false if it wasn't cached</returns>
    public static bool DeleteCachedModel(string modelKey)
    {
        if (!IsModelCached(modelKey))
            return false;

        try
        {
            GGUFModelDownloader.DeleteModel(modelKey);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Clears all cached models to free up disk space.
    /// </summary>
    /// <returns>Number of models deleted</returns>
    public static int ClearAllCache()
    {
        var models = GetAllCachedModels();
        int deletedCount = 0;

        foreach (var model in models)
        {
            try
            {
                GGUFModelDownloader.DeleteModel(model.ModelKey);
                deletedCount++;
            }
            catch
            {
                // Continue with next model
            }
        }

        return deletedCount;
    }

    /// <summary>
    /// Gets detailed information about a specific model.
    /// </summary>
    public static CachedModelInfo? GetModelInfo(string modelKey)
    {
        var models = GGUFModelDownloader.GetCachedModels();
        return models.TryGetValue(modelKey, out var info) ? info : null;
    }

    /// <summary>
    /// Validates the integrity of all cached models.
    /// </summary>
    /// <returns>List of model keys that are corrupted or invalid</returns>
    public static List<string> ValidateCache()
    {
        var models = GetAllCachedModels();
        return models.Where(m => !m.IsValid).Select(m => m.ModelKey).ToList();
    }

    /// <summary>
    /// Gets a formatted list of cached models for display.
    /// </summary>
    public static string GetCacheReport()
    {
        var models = GetAllCachedModels();
        if (models.Count == 0)
        {
            return "No models cached.\n\nModels will be automatically downloaded when you generate a world using LLamaSharp.";
        }

        var report = "Cached Models:\n\n";
        foreach (var model in models.OrderBy(m => m.ModelKey))
        {
            var sizeFormatted = PathHelper.FormatFileSize(model.SizeBytes);
            var status = model.IsValid ? "? Valid" : "? Corrupted";
            var lastModified = model.LastModified.ToString("yyyy-MM-dd HH:mm");
            
            report += $"• {model.ModelKey}\n";
            report += $"  Size: {sizeFormatted}\n";
            report += $"  Status: {status}\n";
            report += $"  Last Modified: {lastModified}\n";
            report += $"  Path: {model.Path}\n\n";
        }

        var totalSize = PathHelper.FormatFileSize(GetTotalCacheSize());
        report += $"Total cache size: {totalSize}";

        return report;
    }
}
