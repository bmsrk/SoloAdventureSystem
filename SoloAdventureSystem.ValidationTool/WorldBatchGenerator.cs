using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;
using SoloAdventureSystem.ContentGenerator.Utils;

namespace SoloAdventureSystem.ValidationTool;

/// <summary>
/// Batch world generator for testing different prompt configurations
/// </summary>
class WorldBatchGenerator
{
    public static async Task<int> GenerateWorlds(string[] args)
    {
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine("?  ?? Batch World Generator - Testing Prompt Variations   ?");
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine();

        var modelKey = args.Length > 0 ? args[0] : "tinyllama-q4";
        Console.WriteLine($"?? Using model: {modelKey}");
        Console.WriteLine();

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.Configure<AISettings>(options =>
        {
            options.Provider = "LLamaSharp";
            options.Model = modelKey;
            options.LLamaModelKey = modelKey;
            options.ContextSize = 2048;
            options.UseGPU = false;
            options.MaxInferenceThreads = 4;
        });

        // Register required services
        services.AddSingleton<IImageAdapter, SimpleImageAdapter>();
        services.AddSingleton<WorldValidator>();
        services.AddSingleton<WorldExporter>();

        var serviceProvider = services.BuildServiceProvider();

        // Initialize adapter once for all worlds
        Console.WriteLine("?? Initializing AI adapter...");
        var settings = serviceProvider.GetRequiredService<IOptions<AISettings>>();
        var logger = serviceProvider.GetRequiredService<ILogger<LLamaSharpAdapter>>();
        
        using var slmAdapter = new LLamaSharpAdapter(settings, logger);
        
        var progress = new Progress<DownloadProgress>(p =>
        {
            if (p.PercentComplete % 10 == 0 || p.PercentComplete == 100)
            {
                var downloadedMB = p.DownloadedBytes / 1024.0 / 1024.0;
                var totalMB = p.TotalBytes / 1024.0 / 1024.0;
                var speedMB = p.SpeedBytesPerSecond / 1024.0 / 1024.0;
                Console.WriteLine($"   ?? {downloadedMB:F0}/{totalMB:F0} MB ({p.PercentComplete}%) - {speedMB:F1} MB/s");
            }
        });

        await slmAdapter.InitializeAsync(progress);
        Console.WriteLine("? Adapter initialized!");
        Console.WriteLine();

        var imageAdapter = serviceProvider.GetRequiredService<IImageAdapter>();
        var generatorLogger = serviceProvider.GetRequiredService<ILogger<SeededWorldGenerator>>();
        var generator = new SeededWorldGenerator(slmAdapter, imageAdapter, generatorLogger);
        
        var validator = new WorldValidator();
        var exporter = new WorldExporter();

        // Define test configurations
        var configs = new[]
        {
            new WorldGenerationOptions
            {
                Name = "NeonCity",
                Seed = 10001,
                Regions = 5,
                Flavor = "Neon-soaked cyberpunk noir",
                Description = "A sprawling megacity where AI corporations control everything",
                MainPlotPoint = "Expose the corporation covering up illegal AI experiments",
                TimePeriod = "2089",
                PowerStructure = "Megacorporations, underground hackers, and street gangs"
            },
            new WorldGenerationOptions
            {
                Name = "WastelandHope",
                Seed = 10002,
                Regions = 5,
                Flavor = "Desperate but hopeful",
                Description = "Scattered communities rebuilding after the nuclear collapse",
                MainPlotPoint = "Unite the settlements against the warlord threat",
                TimePeriod = "47 years after the Fall",
                PowerStructure = "Tribal councils, roaming warlords, and scavenger clans"
            },
            new WorldGenerationOptions
            {
                Name = "StationOmega",
                Seed = 10003,
                Regions = 5,
                Flavor = "Claustrophobic paranoia",
                Description = "Isolated deep space station on the edge of known territory",
                MainPlotPoint = "Find the saboteur before life support fails completely",
                TimePeriod = "2156 - Deep space era",
                PowerStructure = "Station administration, rival departments, and hidden agendas"
            },
            new WorldGenerationOptions
            {
                Name = "DigitalPrison",
                Seed = 10004,
                Regions = 5,
                Flavor = "Surreal and disorienting",
                Description = "Virtual reality prison where human minds are trapped forever",
                MainPlotPoint = "Discover the glitch that reveals the truth of the simulation",
                TimePeriod = "Timeless virtual space",
                PowerStructure = "AI wardens, rebel hackers, and glitched anomalies"
            },
            new WorldGenerationOptions
            {
                Name = "RetroFuture",
                Seed = 10005,
                Regions = 5,
                Flavor = "Vibrant retro-futuristic",
                Description = "Alternative 1985 where AI emerged twenty years early",
                MainPlotPoint = "Solve the murder of the famous AI scientist",
                TimePeriod = "Alternative timeline 1985",
                PowerStructure = "Tech corporations, detective agencies, and AI rights activists"
            }
        };

        var generatedWorlds = new System.Collections.Generic.List<string>();

        // Generate each world
        for (int i = 0; i < configs.Length; i++)
        {
            var config = configs[i];
            
            Console.WriteLine($"????????????????????????????????????????????????????????????");
            Console.WriteLine($"? Generating World {i + 1}/{configs.Length}: {config.Name,-30} ?");
            Console.WriteLine($"????????????????????????????????????????????????????????????");
            Console.WriteLine();
            Console.WriteLine($"?? Flavor: {config.Flavor}");
            Console.WriteLine($"?? Setting: {config.Description}");
            Console.WriteLine($"?? Plot: {config.MainPlotPoint}");
            Console.WriteLine($"? Time: {config.TimePeriod}");
            Console.WriteLine();

            var startTime = DateTime.UtcNow;

            try
            {
                // Generate world
                var result = generator.Generate(config);
                
                // Validate
                validator.Validate(result);
                Console.WriteLine($"? Generated: {result.Rooms.Count} rooms, {result.Npcs.Count} NPCs, {result.Factions.Count} factions");

                // Export
                var tempDir = Path.Combine(Path.GetTempPath(), $"World_{config.Name}_{config.Seed}");
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                exporter.Export(result, config, tempDir);

                var zipPath = PathHelper.GetWorldZipPath(config.Name, config.Seed);
                exporter.Zip(tempDir, zipPath);

                var generationTime = DateTime.UtcNow - startTime;
                Console.WriteLine($"? Saved: {Path.GetFileName(zipPath)}");
                Console.WriteLine($"??  Generation time: {generationTime.TotalSeconds:F1}s");
                
                generatedWorlds.Add(zipPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Failed: {ex.Message}");
            }

            Console.WriteLine();
        }

        // Summary
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine("? Generation Complete                                      ?");
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine();
        Console.WriteLine($"? Successfully generated {generatedWorlds.Count}/{configs.Length} worlds");
        Console.WriteLine();
        Console.WriteLine("?? Now run quality analysis:");
        Console.WriteLine("   dotnet run -- analyze");
        Console.WriteLine();
        
        return 0;
    }
}
