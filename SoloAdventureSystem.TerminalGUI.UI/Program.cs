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

        // Find worlds directory
        var worldsPath = FindWorldsDirectory();
        if (worldsPath == null)
        {
            Console.WriteLine("❌ ERROR: Could not find worlds directory!");
            Console.WriteLine("   Expected: ../SoloAdventureSystem.AIWorldGenerator/content/worlds");
            Console.WriteLine();
            Console.WriteLine("   Generate some worlds first using the AI World Generator!");
            Console.WriteLine("   Press any key to exit...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"✓ Found worlds directory: {worldsPath}");
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
        
        // Try going up to solution root
        var solutionDir = currentDir;
        int levelsUp = 0;
        while (solutionDir != null && levelsUp < 5 && !File.Exists(Path.Combine(solutionDir, "SoloAdventureSystem.sln")))
        {
            solutionDir = Directory.GetParent(solutionDir)?.FullName;
            levelsUp++;
        }

        if (solutionDir != null && File.Exists(Path.Combine(solutionDir, "SoloAdventureSystem.sln")))
        {
            // Priority 1: Solution-level shared content folder
            var sharedWorldsPath = Path.Combine(solutionDir, "content", "worlds");
            if (Directory.Exists(sharedWorldsPath) && Directory.GetFiles(sharedWorldsPath, "*.zip").Length > 0)
            {
                Console.WriteLine($"✓ Found worlds in shared folder: {sharedWorldsPath}");
                return sharedWorldsPath;
            }

            // Priority 2: Generator's content folder
            var generatorWorldsPath = Path.Combine(solutionDir, "SoloAdventureSystem.AIWorldGenerator", "content", "worlds");
            if (Directory.Exists(generatorWorldsPath) && Directory.GetFiles(generatorWorldsPath, "*.zip").Length > 0)
            {
                Console.WriteLine($"✓ Found worlds in generator folder: {generatorWorldsPath}");
                return generatorWorldsPath;
            }
        }

        // Priority 3: Current directory
        var localPath = Path.Combine(currentDir, "content", "worlds");
        if (Directory.Exists(localPath) && Directory.GetFiles(localPath, "*.zip").Length > 0)
        {
            Console.WriteLine($"✓ Found worlds in local folder: {localPath}");
            return localPath;
        }

        // Priority 4: Parent directory (when running from project folder)
        var parentPath = Path.Combine(currentDir, "..", "SoloAdventureSystem.AIWorldGenerator", "content", "worlds");
        var fullParentPath = Path.GetFullPath(parentPath);
        if (Directory.Exists(fullParentPath) && Directory.GetFiles(fullParentPath, "*.zip").Length > 0)
        {
            Console.WriteLine($"✓ Found worlds in parent folder: {fullParentPath}");
            return fullParentPath;
        }

        // Priority 5: Bin output directory (when running from bin/Debug)
        var binWorldsPath = Path.Combine(currentDir, "worlds");
        if (Directory.Exists(binWorldsPath) && Directory.GetFiles(binWorldsPath, "*.zip").Length > 0)
        {
            Console.WriteLine($"✓ Found worlds in bin folder: {binWorldsPath}");
            return binWorldsPath;
        }

        return null;
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
