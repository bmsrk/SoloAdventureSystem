using SoloAdventureSystem.TerminalGUI.GameEngine;
using MudVision.WorldLoader;

namespace SoloAdventureSystem.TerminalGUI;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  ███████╗ ██████╗ ██╗      ██████╗      █████╗ ██████╗ ██╗   ██╗     ║");
        Console.WriteLine("║  ██╔════╝██╔═══██╗██║     ██╔═══██╗    ██╔══██╗██╔══██╗██║   ██║     ║");
        Console.WriteLine("║  ███████╗██║   ██║██║     ██║   ██║    ███████║██║  ██║██║   ██║     ║");
        Console.WriteLine("║  ╚════██║██║   ██║██║     ██║   ██║    ██╔══██║██║  ██║╚██╗ ██╔╝     ║");
        Console.WriteLine("║  ███████║╚██████╔╝███████╗╚██████╔╝    ██║  ██║██████╔╝ ╚████╔╝      ║");
        Console.WriteLine("║  ╚══════╝ ╚═════╝ ╚══════╝ ╚═════╝     ╚═╝  ╚═╝╚═════╝   ╚═══╝       ║");
        Console.WriteLine("║                    ⚡ SOLO ADVENTURE SYSTEM v2.0 ⚡                    ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Find worlds directory - use shared content/worlds folder
        var worldsPath = FindWorldsDirectory();
        if (worldsPath == null)
        {
            Console.WriteLine("❌ ERROR: Could not find worlds directory!");
            Console.WriteLine("   Expected: {solution}/content/worlds");
            Console.WriteLine();
            Console.WriteLine("   Generate some worlds first using the AI World Generator!");
            Console.WriteLine("   Press any key to exit...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"✓ Found worlds directory: {worldsPath}");
        
        // Check if any worlds exist
        var worldFiles = Directory.GetFiles(worldsPath, "*.zip");
        if (worldFiles.Length == 0)
        {
            Console.WriteLine("⚠ No world files found in the directory!");
            Console.WriteLine("   Generate worlds using: SoloAdventureSystem.AIWorldGenerator");
            Console.WriteLine("   Press any key to exit...");
            Console.ReadKey();
            return;
        }
        
        Console.WriteLine($"✓ Found {worldFiles.Length} world(s)");
        Console.WriteLine();

        // World selection
        var selector = new WorldSelectorUI(worldsPath);
        var selectedWorldPath = selector.SelectWorld();

        if (selectedWorldPath == null)
        {
            Console.WriteLine("No world selected. Exiting...");
            return;
        }

        Console.WriteLine($"Loading world: {Path.GetFileName(selectedWorldPath)}...");

        // Load world
        var worldLoader = new WorldLoaderService();
        var world = await LoadWorld(worldLoader, selectedWorldPath);

        if (world == null)
        {
            Console.WriteLine("❌ Failed to load world!");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"✓ Loaded: {world.WorldDefinition!.Name}");
        Console.WriteLine($"  Rooms: {world.Rooms!.Count}");
        Console.WriteLine($"  NPCs: {world.Npcs!.Count}");
        Console.WriteLine($"  Factions: {world.Factions!.Count}");
        Console.WriteLine();
        Console.WriteLine("Press any key to start adventure...");
        Console.ReadKey();

        // Initialize game state
        var gameState = new GameState
        {
            World = world,
            CurrentLocation = world.Rooms.FirstOrDefault(r => r.Id == world.WorldDefinition.StartLocationId)
                ?? world.Rooms.First()
        };

        // Run game
        var gameUI = new GameUI(gameState);
        gameUI.Run();

        Console.WriteLine();
        Console.WriteLine("Thanks for playing!");
    }

    static string? FindWorldsDirectory()
    {
        var currentDir = Directory.GetCurrentDirectory();
        Console.WriteLine($"🔍 Current directory: {currentDir}");
        
        // Try going up to solution root
        var solutionDir = currentDir;
        int levelsUp = 0;
        
        while (solutionDir != null && levelsUp < 5)
        {
            Console.WriteLine($"   Checking: {solutionDir}");
            
            // Check for both .sln and .slnx files
            var slnFiles = Directory.GetFiles(solutionDir, "*.sln");
            var slnxFiles = Directory.GetFiles(solutionDir, "*.slnx");
            
            if (slnFiles.Length > 0 || slnxFiles.Length > 0)
            {
                var foundFile = slnFiles.Length > 0 ? slnFiles[0] : slnxFiles[0];
                Console.WriteLine($"   ✓ Found solution file: {Path.GetFileName(foundFile)}");
                break;
            }
            
            solutionDir = Directory.GetParent(solutionDir)?.FullName;
            levelsUp++;
        }

        if (solutionDir != null)
        {
            Console.WriteLine($"✓ Found solution root: {solutionDir}");
            
            // Use solution-level shared content folder
            var sharedWorldsPath = Path.Combine(solutionDir, "content", "worlds");
            Console.WriteLine($"🎯 Worlds path: {sharedWorldsPath}");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(sharedWorldsPath))
            {
                Console.WriteLine($"📁 Creating shared worlds directory: {sharedWorldsPath}");
                Directory.CreateDirectory(sharedWorldsPath);
            }
            else
            {
                Console.WriteLine($"✓ Directory exists");
                
                // List files for debugging
                var files = Directory.GetFiles(sharedWorldsPath, "*.zip");
                if (files.Length > 0)
                {
                    Console.WriteLine($"📦 Files found:");
                    foreach (var file in files)
                    {
                        Console.WriteLine($"   - {Path.GetFileName(file)}");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠ No .zip files in directory");
                }
            }
            
            return sharedWorldsPath;
        }

        // Fallback: create local content/worlds directory
        Console.WriteLine("⚠ Could not find solution root (no .sln or .slnx file found)");
        Console.WriteLine("   Using local content/worlds directory");
        var localPath = Path.Combine(currentDir, "content", "worlds");
        Directory.CreateDirectory(localPath);
        return localPath;
    }

    static async Task<WorldModel?> LoadWorld(WorldLoaderService loader, string zipPath)
    {
        try
        {
            using var fs = new FileStream(zipPath, FileMode.Open, FileAccess.Read);
            return await loader.LoadFromZipAsync(fs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading world: {ex.Message}");
            return null;
        }
    }
}
