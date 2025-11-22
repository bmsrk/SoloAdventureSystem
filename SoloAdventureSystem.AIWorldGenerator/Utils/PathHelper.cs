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
    /// </summary>
    public static string GetWorldZipPath(string worldName, int seed)
    {
        var worldsDir = GetSharedWorldsDirectory();
        return Path.Combine(worldsDir, $"World_{worldName}_{seed}.zip");
    }
}
