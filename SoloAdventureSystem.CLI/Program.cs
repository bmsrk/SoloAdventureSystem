using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Utils;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;

namespace SoloAdventureSystem.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Define root command
        var rootCommand = new RootCommand("SoloAdventureSystem - AI World Generator CLI");

        // Define generate command
        var generateCommand = new Command("generate", "Generate a new world using AI");

        // Add options
        var nameOption = new Option<string>(
            name: "--name",
            description: "World name",
            getDefaultValue: () => "CLIWorld");
        nameOption.AddAlias("-n");

        var seedOption = new Option<int>(
            name: "--seed",
            description: "Random seed for generation",
            getDefaultValue: () => new Random().Next(1, 999999));
        seedOption.AddAlias("-s");

        var themeOption = new Option<string>(
            name: "--theme",
            description: "World theme",
            getDefaultValue: () => "Cyberpunk");
        themeOption.AddAlias("-t");

        var regionsOption = new Option<int>(
            name: "--regions",
            description: "Number of regions/rooms to generate",
            getDefaultValue: () => 5);
        regionsOption.AddAlias("-r");

        var providerOption = new Option<string>(
            name: "--provider",
            description: "AI provider (only LLamaSharp supported)",
            getDefaultValue: () => "LLamaSharp");
        providerOption.AddAlias("-p");

        var modelOption = new Option<string>(
            name: "--model",
            description: "Model: phi-3-mini-q4, tinyllama-q4, or llama-3.2-1b-q4",
            getDefaultValue: () => "phi-3-mini-q4");
        modelOption.AddAlias("-m");

        var outputOption = new Option<string?>(
            name: "--output",
            description: "Output directory (default: content/worlds)",
            getDefaultValue: () => null);
        outputOption.AddAlias("-o");

        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose logging",
            getDefaultValue: () => false);
        verboseOption.AddAlias("-v");

        generateCommand.AddOption(nameOption);
        generateCommand.AddOption(seedOption);
        generateCommand.AddOption(themeOption);
        generateCommand.AddOption(regionsOption);
        generateCommand.AddOption(providerOption);
        generateCommand.AddOption(modelOption);
        generateCommand.AddOption(outputOption);
        generateCommand.AddOption(verboseOption);

        // Set handler
        generateCommand.SetHandler(async (string name, int seed, string theme, int regions, string provider, string model, string? output, bool verbose) =>
        {
            await GenerateWorldAsync(name, seed, theme, regions, provider, model, output, verbose);
        }, nameOption, seedOption, themeOption, regionsOption, providerOption, modelOption, outputOption, verboseOption);

        rootCommand.AddCommand(generateCommand);

        // Add list command
        var listCommand = new Command("list", "List generated worlds");
        listCommand.SetHandler(ListWorlds);
        rootCommand.AddCommand(listCommand);

        // Add info command
        var infoCommand = new Command("info", "Show system information");
        infoCommand.SetHandler(ShowInfo);
        rootCommand.AddCommand(infoCommand);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task GenerateWorldAsync(string name, int seed, string theme, int regions, string provider, string model, string? outputDir, bool verbose)
    {
        Console.WriteLine("??????????????????????????????????????????????????????????????????");
        Console.WriteLine("?          SoloAdventureSystem - World Generator CLI            ?");
        Console.WriteLine("??????????????????????????????????????????????????????????????????");
        Console.WriteLine();

        // Display configuration
        Console.WriteLine("?? Configuration:");
        Console.WriteLine($"   Name:     {name}");
        Console.WriteLine($"   Seed:     {seed}");
        Console.WriteLine($"   Theme:    {theme}");
        Console.WriteLine($"   Regions:  {regions}");
        Console.WriteLine($"   Provider: {provider}");
        Console.WriteLine($"   Model:    {model}");
        Console.WriteLine();

        try
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Build service container
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
            });

            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);
            services.Configure<AISettings>(options =>
            {
                options.Provider = provider;
                options.Model = model;
                options.LLamaModelKey = model;
                options.ContextSize = 2048;
                options.UseGPU = false;
                options.MaxInferenceThreads = Environment.ProcessorCount;
            });

            // Add services
            services.AddSingleton<IImageAdapter, SimpleImageAdapter>();
            services.AddSingleton<WorldValidator>();
            services.AddSingleton<WorldExporter>();

            var serviceProvider = services.BuildServiceProvider();

            // Initialize adapter
            Console.WriteLine("??  Initializing AI adapter...");
            ILocalSLMAdapter slmAdapter;

            if (provider.Equals("LLamaSharp", StringComparison.OrdinalIgnoreCase))
            {
                var settings = serviceProvider.GetRequiredService<IOptions<AISettings>>();
                var logger = serviceProvider.GetRequiredService<ILogger<LLamaSharpAdapter>>();
                var adapter = new LLamaSharpAdapter(settings, logger);

                Console.WriteLine("?? Checking model availability...");
                var progress = new Progress<DownloadProgress>(p =>
                {
                    if (p.PercentComplete % 10 == 0 || p.PercentComplete == 100)
                    {
                        var downloadedMB = p.DownloadedBytes / 1024.0 / 1024.0;
                        var totalMB = p.TotalBytes / 1024.0 / 1024.0;
                        var speedMB = p.SpeedBytesPerSecond / 1024.0 / 1024.0;
                        Console.WriteLine($"   Downloading: {downloadedMB:F0}/{totalMB:F0} MB ({p.PercentComplete}%) - {speedMB:F1} MB/s - ETA: {p.FormattedETA}");
                    }
                });

                await adapter.InitializeAsync(progress);
                Console.WriteLine("? Model loaded successfully!");
                slmAdapter = adapter;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported provider: {provider}. Only 'LLamaSharp' is supported.");
            }

            // Create generator
            var imageAdapter = serviceProvider.GetRequiredService<IImageAdapter>();
            var generatorLogger = serviceProvider.GetRequiredService<ILogger<SeededWorldGenerator>>();
            var generator = new SeededWorldGenerator(slmAdapter, imageAdapter, generatorLogger);

            // Generate world
            Console.WriteLine();
            Console.WriteLine("?? Generating world...");
            var options = new WorldGenerationOptions
            {
                Name = name,
                Seed = seed,
                Theme = theme,
                Regions = regions,
                NpcDensity = "medium"
            };

            var startTime = DateTime.UtcNow;
            var result = generator.Generate(options);
            var generationTime = DateTime.UtcNow - startTime;

            Console.WriteLine($"? World generated in {generationTime.TotalSeconds:F1}s");
            Console.WriteLine($"   Rooms:    {result.Rooms.Count}");
            Console.WriteLine($"   NPCs:     {result.Npcs.Count}");
            Console.WriteLine($"   Factions: {result.Factions.Count}");
            Console.WriteLine($"   Lore:     {result.LoreEntries.Count} entries");
            Console.WriteLine();

            // Validate
            Console.WriteLine("?? Validating world structure...");
            var validator = serviceProvider.GetRequiredService<WorldValidator>();
            validator.Validate(result);
            Console.WriteLine("? Validation passed!");
            Console.WriteLine();

            // Export
            Console.WriteLine("?? Exporting world...");
            var exporter = serviceProvider.GetRequiredService<WorldExporter>();
            var tempDir = Path.Combine(Path.GetTempPath(), $"World_{name}_{seed}");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            exporter.Export(result, options, tempDir);

            // Zip
            var zipPath = outputDir != null
                ? Path.Combine(outputDir, $"World_{name}_{seed}.zip")
                : PathHelper.GetWorldZipPath(name, seed);

            var zipDir = Path.GetDirectoryName(zipPath);
            if (!string.IsNullOrEmpty(zipDir) && !Directory.Exists(zipDir))
            {
                Directory.CreateDirectory(zipDir);
            }

            exporter.Zip(tempDir, zipPath);

            var fileInfo = new FileInfo(zipPath);
            Console.WriteLine("? World exported successfully!");
            Console.WriteLine();
            Console.WriteLine("?? Output:");
            Console.WriteLine($"   Path: {zipPath}");
            Console.WriteLine($"   Size: {PathHelper.FormatFileSize(fileInfo.Length)}");
            Console.WriteLine();

            // Show sample content
            Console.WriteLine("?? Sample Content:");
            Console.WriteLine();
            Console.WriteLine($"   ?? First Room: {result.Rooms[0].Title}");
            var desc = result.Rooms[0].BaseDescription;
            if (desc.Length > 150) desc = desc.Substring(0, 150) + "...";
            Console.WriteLine($"      {desc}");
            Console.WriteLine();

            if (result.Npcs.Count > 0)
            {
                Console.WriteLine($"   ?? First NPC: {result.Npcs[0].Name}");
                var bio = result.Npcs[0].Description;
                if (bio.Length > 150) bio = bio.Substring(0, 150) + "...";
                Console.WriteLine($"      {bio}");
                Console.WriteLine();
            }

            Console.WriteLine("?? World generation complete!");
            Console.WriteLine();

            // Cleanup
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("? Error during world generation:");
            Console.WriteLine($"   {ex.Message}");
            if (verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
            }
            Console.ResetColor();
            Environment.Exit(1);
        }
    }

    static void ListWorlds()
    {
        Console.WriteLine("?? Generated Worlds:");
        Console.WriteLine();

        var worldsDir = PathHelper.GetSharedWorldsDirectory();
        if (!Directory.Exists(worldsDir))
        {
            Console.WriteLine("   No worlds directory found.");
            return;
        }

        var worldFiles = Directory.GetFiles(worldsDir, "*.zip");
        if (worldFiles.Length == 0)
        {
            Console.WriteLine("   No worlds generated yet.");
            Console.WriteLine();
            Console.WriteLine("   Generate one with: dotnet run generate --name MyWorld");
            return;
        }

        foreach (var file in worldFiles)
        {
            var fileInfo = new FileInfo(file);
            var fileName = Path.GetFileNameWithoutExtension(file);
            Console.WriteLine($"   ?? {fileName}");
            Console.WriteLine($"      Size: {PathHelper.FormatFileSize(fileInfo.Length)}");
            Console.WriteLine($"      Created: {fileInfo.CreationTime:yyyy-MM-dd HH:mm}");
            Console.WriteLine();
        }

        Console.WriteLine($"Total: {worldFiles.Length} world(s)");
    }

    static void ShowInfo()
    {
        Console.WriteLine("??  System Information:");
        Console.WriteLine();
        Console.WriteLine($"   .NET Version:      {Environment.Version}");
        Console.WriteLine($"   OS:                {Environment.OSVersion}");
        Console.WriteLine($"   CPU Cores:         {Environment.ProcessorCount}");
        Console.WriteLine($"   Working Directory: {Directory.GetCurrentDirectory()}");
        Console.WriteLine();

        var worldsDir = PathHelper.GetSharedWorldsDirectory();
        Console.WriteLine($"   Worlds Directory:  {worldsDir}");

        var modelsDir = GGUFModelDownloader.GetModelCacheDirectory();
        Console.WriteLine($"   Models Directory:  {modelsDir}");
        Console.WriteLine();

        // Check cached models
        var cachedModels = GGUFModelDownloader.GetCachedModels();
        if (cachedModels.Count > 0)
        {
            Console.WriteLine("?? Cached Models:");
            foreach (var model in cachedModels)
            {
                Console.WriteLine($"   ? {model.Key}");
                Console.WriteLine($"      Size: {model.Value.SizeMB:F1} MB");
                Console.WriteLine($"      Path: {model.Value.Path}");
                Console.WriteLine();
            }

            var totalSize = GGUFModelDownloader.GetTotalCacheSize();
            Console.WriteLine($"Total cache size: {PathHelper.FormatFileSize(totalSize)}");
        }
        else
        {
            Console.WriteLine("?? No cached models found.");
            Console.WriteLine("   Models will be downloaded on first use.");
        }
    }
}
