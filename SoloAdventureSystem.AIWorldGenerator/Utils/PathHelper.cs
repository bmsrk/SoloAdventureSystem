using System;
using System.IO;

namespace SoloAdventureSystem.ContentGenerator.Utils;

/// <summary>
/// Helper for finding and managing the shared content/worlds directory.
/// Ensures all generated worlds are saved to a consistent location.
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// Finds the solution root directory by looking for .sln or .slnx file.
    /// </summary>
    public static string? FindSolutionRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var dir = currentDir;
        
        // Walk up directories looking for .sln or .slnx file
        for (int i = 0; i < 10; i++)
        {
            var slnFiles = Directory.GetFiles(dir, "*.sln");
            var slnxFiles = Directory.GetFiles(dir, "*.slnx");
            
            if (slnFiles.Length > 0 || slnxFiles.Length > 0)
            {
                return dir;
            }
            
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        
        return null;
    }

    /// <summary>
    /// Gets the shared worlds directory path and ensures it exists.
    /// Creates the directory if it doesn't exist.
    /// </summary>
    /// <returns>Full path to the shared content/worlds directory</returns>
    public static string GetSharedWorldsDirectory()
    {
        var solutionRoot = FindSolutionRoot();
        
        string worldsPath;
        if (solutionRoot != null)
        {
            // Use solution-level shared content folder
            worldsPath = Path.Combine(solutionRoot, "content", "worlds");
        }
        else
        {
            // Fallback to local content folder
            worldsPath = Path.Combine(Directory.GetCurrentDirectory(), "content", "worlds");
        }
        
        // Ensure directory exists
        Directory.CreateDirectory(worldsPath);
        
        return worldsPath;
    }

    /// <summary>
    /// Gets the full path for a world zip file in the shared worlds directory.
    /// Sanitizes world name to prevent path traversal and handle invalid characters.
    /// Uses a provided id (timestamp or GUID) for uniqueness.
    /// </summary>
    /// <param name="worldName">The name of the world</param>
    /// <param name="id">An identifier (timestamp/GUID) to make filename unique</param>
    /// <returns>Full path to the world ZIP file</returns>
    public static string GetWorldZipPath(string worldName, string id)
    {
        // Sanitize world name - remove invalid path characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitizedName = string.Join("_", worldName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        
        // Handle empty name after sanitization
        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            sanitizedName = "Untitled";
        }
        
        // Limit length to prevent "path too long" errors (Windows MAX_PATH = 260)
        const int MaxNameLength = 50;
        if (sanitizedName.Length > MaxNameLength)
        {
            sanitizedName = sanitizedName.Substring(0, MaxNameLength);
        }
        
        // Remove leading/trailing whitespace and dots (invalid in Windows)
        sanitizedName = sanitizedName.Trim().Trim('.');
        
        var worldsDir = GetSharedWorldsDirectory();
        return Path.Combine(worldsDir, $"World_{sanitizedName}_{id}.zip");
    }

    /// <summary>
    /// Compatibility overload which accepts an integer seed/id.
    /// </summary>
    public static string GetWorldZipPath(string worldName, int seed)
    {
        return GetWorldZipPath(worldName, seed.ToString());
    }
    
    /// <summary>
    /// Validates that a path is safe and doesn't attempt path traversal.
    /// </summary>
    /// <param name="path">Path to validate</param>
    /// <returns>True if path is safe</returns>
    public static bool IsPathSafe(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        // Check for path traversal attempts
        if (path.Contains(".."))
            return false;

        return true;
    }

    /// <summary>
    /// Formats file size in human-readable format.
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
