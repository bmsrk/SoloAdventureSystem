using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Utils;
using SoloAdventureSystem.UI.Game;
using SoloAdventureSystem.UI.WorldGenerator;
using SoloAdventureSystem.UI.Themes;
using MudVision.WorldLoader;
using Terminal.Gui;

namespace SoloAdventureSystem.UI;

class Program
{
    static void Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        // Build service container
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // Add configuration
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<AISettings>(configuration.GetSection("AI"));
        
        // Add world loader services
        services.AddSingleton<IWorldLoader, WorldLoaderService>();
        services.AddSingleton<IWorldState, WorldState>();
        
        // Add world generator services
        services.AddSingleton<IImageAdapter, SimpleImageAdapter>();
        services.AddSingleton<WorldValidator>();
        services.AddSingleton<WorldExporter>();
        
        // Add UI services
        services.AddTransient<WorldGeneratorUI>();
        services.AddTransient<GameUI>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Initialize Terminal.Gui ONCE at application start
        Application.Init();
        try
        {
            ThemeManager.ApplyCurrentTheme();
            
            // Main menu loop (skip title screen for now to make it work)
            var mainMenu = new MainMenuUI();
            bool exit = false;
            
            while (!exit)
            {
                var choice = mainMenu.ShowMenu();
                
                switch (choice)
                {
                    case MainMenuUI.MenuChoice.GenerateWorld:
                        GenerateWorld(serviceProvider);
                        break;
                    case MainMenuUI.MenuChoice.PlayWorld:
                        PlayWorld(serviceProvider);
                        break;
                    case MainMenuUI.MenuChoice.Settings:
                        // Settings removed - single theme only
                        break;
                    case MainMenuUI.MenuChoice.Exit:
                        exit = true;
                        break;
                }
            }
        }
        finally
        {
            // Shutdown ONCE at application end
            Application.Shutdown();
        }
    }
    
    static void GenerateWorld(IServiceProvider serviceProvider)
    {
        try
        {
            // Use 'using' to ensure proper disposal of WorldGeneratorUI and prevent memory leaks
            using var worldGeneratorUI = serviceProvider.GetRequiredService<WorldGeneratorUI>();
            var worldPath = worldGeneratorUI.GenerateWorld();
            
            if (worldPath != null)
            {
                ShowMessage("Success", $"World generated successfully!\n\n{worldPath}");
            }
        }
        catch (Exception ex)
        {
            ShowError("Error", $"Error generating world:\n{ex.Message}");
        }
    }
    
    static void PlayWorld(IServiceProvider serviceProvider)
    {
        try
        {
            var worldsPath = PathHelper.GetSharedWorldsDirectory();
            
            if (!Directory.Exists(worldsPath))
            {
                MessageBox.ErrorQuery("Error", "No worlds directory found!\n\nGenerate a world first.", "OK");
                return;
            }
            
            var worldFiles = Directory.GetFiles(worldsPath, "*.zip");
            if (worldFiles.Length == 0)
            {
                MessageBox.ErrorQuery("Error", "No world files found!\n\nGenerate a world first.", "OK");
                return;
            }
            
            var worldSelector = new WorldSelectorUI(worldsPath);
            var worldPath = worldSelector.SelectWorldInternal(); // Use internal method
            
            if (worldPath == null)
            {
                return; // User cancelled
            }
            
            // Load the world
            var worldLoader = serviceProvider.GetRequiredService<IWorldLoader>();
            var worldState = serviceProvider.GetRequiredService<IWorldState>();
            
            WorldModel? world = null;
            try
            {
                using var fileStream = File.OpenRead(worldPath);
                world = worldLoader.LoadFromZipAsync(fileStream).GetAwaiter().GetResult();
            }
            catch (InvalidDataException ex)
            {
                MessageBox.ErrorQuery("Invalid World File", 
                    $"The world file is corrupted or invalid:\n\n{ex.Message}\n\nTry generating a new world.", "OK");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Load Error", 
                    $"Failed to load world file:\n\n{ex.Message}", "OK");
                return;
            }
            
            if (world == null || world.WorldDefinition == null)
            {
                MessageBox.ErrorQuery("Error", "Failed to load world: World data is null", "OK");
                return;
            }
            
            if (world.Rooms == null || world.Rooms.Count == 0)
            {
                MessageBox.ErrorQuery("Error", "Failed to load world: No rooms found in world", "OK");
                return;
            }
            
            // Verify start location exists
            var startLocation = world.Rooms.FirstOrDefault(r => r.Id == world.WorldDefinition.StartLocationId);
            if (startLocation == null)
            {
                MessageBox.ErrorQuery("Error", 
                    $"Start location '{world.WorldDefinition.StartLocationId}' not found in world.\n\nThe world file may be corrupted.", "OK");
                return;
            }
            
            worldState.SetWorld(world);
            
            // Start the game
            var gameUI = serviceProvider.GetRequiredService<GameUI>();
            try
            {
                gameUI.StartGameInternal(); // Use internal method
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.ErrorQuery("Game Error", $"Failed to start game:\n\n{ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery("Unexpected Error", 
                    $"An unexpected error occurred:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}", "OK");
            }
        }
        catch (FileNotFoundException ex)
        {
            MessageBox.ErrorQuery("Error", $"World file not found:\n{ex.Message}", "OK");
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Error", $"Unexpected error:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}", "OK");
        }
    }

    static void ShowMessage(string title, string message)
    {
        // Application is already initialized
        MessageBox.Query(title, message, "OK");
    }

    static void ShowError(string title, string message)
    {
        // Application is already initialized
        MessageBox.ErrorQuery(title, message, "OK");
    }
}
