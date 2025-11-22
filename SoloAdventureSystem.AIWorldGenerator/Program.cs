using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.UI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Check if user wants interactive UI mode
        var useUI = args.Length == 0 || args.Contains("--ui") || args.Contains("--interactive");

        var builder = Host.CreateApplicationBuilder(args);

        // Configure settings
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddCommandLine(args);

        builder.Services.Configure<AISettings>(builder.Configuration.GetSection(AISettings.SectionName));

        // Register adapters
        builder.Services.AddSingleton<GitHubModelsAdapter>();
        builder.Services.AddSingleton<OpenAIAdapter>();
        builder.Services.AddSingleton<IImageAdapter, StubImageAdapter>();
        builder.Services.AddSingleton<ILocalSLMAdapter>(SLMAdapterFactory.Create);

        // Register generator and services
        builder.Services.AddSingleton<IWorldGenerator, SeededWorldGenerator>();
        builder.Services.AddSingleton<WorldValidator>();
        builder.Services.AddSingleton<WorldExporter>();
        builder.Services.AddSingleton<WorldGeneratorApp>();
        builder.Services.AddSingleton<WorldGeneratorUI>();

        // Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var host = builder.Build();

        if (useUI)
        {
            // Run Terminal.GUI interface
            var ui = host.Services.GetRequiredService<WorldGeneratorUI>();
            ui.Run();
            return 0;
        }
        else
        {
            // Run CLI mode
            var app = host.Services.GetRequiredService<WorldGeneratorApp>();
            return await app.RunAsync(args);
        }
    }
}

/// <summary>
/// Main application logic for the world generator (CLI mode).
/// </summary>
class WorldGeneratorApp
{
    private readonly IWorldGenerator _generator;
    private readonly WorldValidator _validator;
    private readonly WorldExporter _exporter;
    private readonly ILogger<WorldGeneratorApp> _logger;

    public WorldGeneratorApp(
        IWorldGenerator generator,
        WorldValidator validator,
        WorldExporter exporter,
        ILogger<WorldGeneratorApp> logger)
    {
        _generator = generator;
        _validator = validator;
        _exporter = exporter;
        _logger = logger;
    }

    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            var options = ParseArguments(args);
            
            _logger.LogInformation("Generating world: {Name}", options.Name);
            _logger.LogInformation("Seed: {Seed}, Regions: {Regions}, NPC Density: {NpcDensity}", 
                options.Seed, options.Regions, options.NpcDensity);

            var result = _generator.Generate(options);

            _logger.LogInformation("Validating world...");
            _validator.Validate(result);

            var tempDir = Path.Combine(Path.GetTempPath(), $"SoloAdventureWorld_{options.Name}_{options.Seed}");
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);

            _logger.LogInformation("Exporting world to {TempDir}...", tempDir);
            _exporter.Export(result, options, tempDir);

            var zipPath = Path.Combine("content/worlds", $"SoloAdventureWorld_{options.Name}_{options.Seed}.zip");
            Directory.CreateDirectory("content/worlds");
            
            _logger.LogInformation("Creating ZIP archive...");
            _exporter.Zip(tempDir, zipPath);

            _logger.LogInformation("✅ World ZIP generated: {ZipPath}", zipPath);
            _logger.LogInformation("📊 Stats: {Rooms} rooms, {NPCs} NPCs, {Factions} factions, {StoryNodes} story nodes",
                result.Rooms.Count, result.Npcs.Count, result.Factions.Count, result.StoryNodes.Count);

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ World generation failed: {Message}", ex.Message);
            return 1;
        }
    }

    private WorldGenerationOptions ParseArguments(string[] args)
    {
        var options = new WorldGenerationOptions();
        
        foreach (var arg in args)
        {
            if (arg.StartsWith("--name=")) options.Name = arg.Split('=', 2)[1];
            if (arg.StartsWith("--seed=")) options.Seed = int.Parse(arg.Split('=', 2)[1]);
            if (arg.StartsWith("--theme=")) options.Theme = arg.Split('=', 2)[1];
            if (arg.StartsWith("--regions=")) options.Regions = int.Parse(arg.Split('=', 2)[1]);
            if (arg.StartsWith("--npc-density=")) options.NpcDensity = arg.Split('=', 2)[1];
            if (arg.StartsWith("--render-images=")) options.RenderImages = bool.Parse(arg.Split('=', 2)[1]);
        }

        return options;
    }
}
