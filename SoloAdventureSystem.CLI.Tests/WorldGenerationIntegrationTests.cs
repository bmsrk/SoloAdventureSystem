using System;
using System.IO;
using System.IO.Compression;
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
using Xunit;
using Xunit.Abstractions;

namespace SoloAdventureSystem.CLI.Tests;

/// <summary>
/// Integration tests for CLI-style world generation and validation.
/// These tests simulate the full world generation workflow as used by the CLI.
/// </summary>
public class WorldGenerationIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _testOutputDir;

    public WorldGenerationIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"CLITests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testOutputDir);

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AI:Provider"] = "LLamaSharp",
                ["AI:Model"] = "phi-3-mini-q4",
                ["AI:LLamaModelKey"] = "phi-3-mini-q4",
                ["AI:ContextSize"] = "2048",
                ["AI:UseGPU"] = "false",
                ["AI:MaxInferenceThreads"] = "4"
            })
            .Build();

        // Build service container
        var services = new ServiceCollection();

        // Add logging that outputs to xUnit
        services.AddLogging(builder =>
        {
            builder.AddProvider(new XunitLoggerProvider(output));
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add configuration
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<AISettings>(configuration.GetSection("AI"));

        // Add services
        services.AddSingleton<IImageAdapter, SimpleImageAdapter>();
        services.AddSingleton<WorldValidator>();
        services.AddSingleton<WorldExporter>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact(Skip = "Long-running test - requires model download and generation (2-5 minutes)")]
    public async Task GenerateWorld_WithLLamaSharp_CreatesValidWorld()
    {
        // Arrange
        var worldName = "CLITestWorld";
        var seed = 42069;
        var theme = "Cyberpunk";
        var regions = 5;

        _output.WriteLine($"Starting world generation test: {worldName} (seed: {seed})");
        _output.WriteLine($"Output directory: {_testOutputDir}");

        // Initialize LLamaSharp adapter
        var settings = _serviceProvider.GetRequiredService<IOptions<AISettings>>();
        var logger = _serviceProvider.GetRequiredService<ILogger<LLamaSharpAdapter>>();
        var slmAdapter = new LLamaSharpAdapter(settings, logger);

        _output.WriteLine("Initializing LLamaSharp adapter (this may download the model)...");
        var progress = new Progress<DownloadProgress>(p =>
        {
            if (p.PercentComplete % 25 == 0 || p.PercentComplete == 100)
            {
                _output.WriteLine($"Download progress: {p.PercentComplete}% - {p.FormattedETA} remaining");
            }
        });

        await slmAdapter.InitializeAsync(progress);
        _output.WriteLine("? Model initialized successfully");

        // Create generator
        var imageAdapter = _serviceProvider.GetRequiredService<IImageAdapter>();
        var generatorLogger = _serviceProvider.GetRequiredService<ILogger<SeededWorldGenerator>>();
        var generator = new SeededWorldGenerator(slmAdapter, imageAdapter, generatorLogger);

        // Act - Generate world
        _output.WriteLine($"Generating world with {regions} regions...");
        var options = new WorldGenerationOptions
        {
            Name = worldName,
            Seed = seed,
            Theme = theme,
            Regions = regions,
            NpcDensity = "medium"
        };

        var startTime = DateTime.UtcNow;
        var result = generator.Generate(options);
        var generationTime = DateTime.UtcNow - startTime;

        _output.WriteLine($"? World generated in {generationTime.TotalSeconds:F1}s");
        _output.WriteLine($"   Rooms:    {result.Rooms.Count}");
        _output.WriteLine($"   NPCs:     {result.Npcs.Count}");
        _output.WriteLine($"   Factions: {result.Factions.Count}");
        _output.WriteLine($"   Lore:     {result.LoreEntries.Count} entries");

        // Assert - Validate structure
        _output.WriteLine("Validating world structure...");
        var validator = _serviceProvider.GetRequiredService<WorldValidator>();
        
        // This will throw if validation fails
        validator.Validate(result);
        _output.WriteLine("? Structural validation passed");

        // Assert - Validate quality
        _output.WriteLine("Validating world quality...");
        var qualityResult = validator.ValidateQuality(result, theme);
        
        _output.WriteLine($"Quality Metrics:");
        _output.WriteLine($"   Room Quality:    {qualityResult.Metrics.RoomQualityScore}/100");
        _output.WriteLine($"   NPC Quality:     {qualityResult.Metrics.NpcQualityScore}/100");
        _output.WriteLine($"   Faction Quality: {qualityResult.Metrics.FactionQualityScore}/100");
        _output.WriteLine($"   Consistency:     {qualityResult.Metrics.ConsistencyScore}/100");
        _output.WriteLine($"   Overall Score:   {qualityResult.Metrics.OverallScore}/100");

        if (qualityResult.Warnings.Any())
        {
            _output.WriteLine($"Warnings ({qualityResult.Warnings.Count}):");
            foreach (var warning in qualityResult.Warnings)
            {
                _output.WriteLine($"   ?? {warning}");
            }
        }

        // Quality validation should pass or have acceptable warnings
        Assert.True(qualityResult.IsValid || qualityResult.Warnings.Count < 5, 
            $"Quality validation failed with {qualityResult.Errors.Count} errors");

        // Assert - Export and verify
        _output.WriteLine("Exporting world...");
        var exporter = _serviceProvider.GetRequiredService<WorldExporter>();
        var tempDir = Path.Combine(_testOutputDir, $"World_{worldName}_{seed}");
        Directory.CreateDirectory(tempDir);

        exporter.Export(result, options, tempDir);

        // Verify exported files
        Assert.True(File.Exists(Path.Combine(tempDir, "world.json")), "world.json should exist");
        Assert.True(Directory.Exists(Path.Combine(tempDir, "rooms")), "rooms directory should exist");
        Assert.True(Directory.Exists(Path.Combine(tempDir, "npcs")), "npcs directory should exist");
        Assert.True(Directory.Exists(Path.Combine(tempDir, "factions")), "factions directory should exist");
        
        var roomFiles = Directory.GetFiles(Path.Combine(tempDir, "rooms"), "*.json");
        Assert.Equal(result.Rooms.Count, roomFiles.Length);
        _output.WriteLine($"? Exported {roomFiles.Length} room files");

        // Create ZIP
        var zipPath = Path.Combine(_testOutputDir, $"World_{worldName}_{seed}.zip");
        exporter.Zip(tempDir, zipPath);

        Assert.True(File.Exists(zipPath), "ZIP file should be created");
        var fileInfo = new FileInfo(zipPath);
        _output.WriteLine($"? Created ZIP file: {PathHelper.FormatFileSize(fileInfo.Length)}");

        // Verify ZIP contents
        using (var zip = ZipFile.OpenRead(zipPath))
        {
            Assert.True(zip.Entries.Count > 0, "ZIP should contain files");
            _output.WriteLine($"? ZIP contains {zip.Entries.Count} files");
        }

        // Dispose adapter
        slmAdapter.Dispose();
        _output.WriteLine("? All tests passed!");
    }

    [Fact]
    public async Task GenerateWorld_WithLLamaSharp_CreatesReproducibleWorld()
    {
        // This is a faster test that verifies the same seed produces the same world structure
        // without doing full LLM validation
        
        // Arrange
        var worldName = "ReproducibilityTest";
        var seed = 12345;
        var theme = "Cyberpunk";
        var regions = 3; // Smaller for faster test

        _output.WriteLine($"Testing reproducibility with seed: {seed}");

        // Initialize adapter
        var settings = _serviceProvider.GetRequiredService<IOptions<AISettings>>();
        var logger = _serviceProvider.GetRequiredService<ILogger<LLamaSharpAdapter>>();
        var slmAdapter = new LLamaSharpAdapter(settings, logger);

        _output.WriteLine("Initializing adapter...");
        await slmAdapter.InitializeAsync();

        var imageAdapter = _serviceProvider.GetRequiredService<IImageAdapter>();
        var generatorLogger = _serviceProvider.GetRequiredService<ILogger<SeededWorldGenerator>>();
        var generator = new SeededWorldGenerator(slmAdapter, imageAdapter, generatorLogger);

        var options = new WorldGenerationOptions
        {
            Name = worldName,
            Seed = seed,
            Theme = theme,
            Regions = regions,
            NpcDensity = "medium"
        };

        // Act - Generate twice
        _output.WriteLine("Generating world (first time)...");
        var result1 = generator.Generate(options);
        
        _output.WriteLine("Generating world (second time with same seed)...");
        var result2 = generator.Generate(options);

        // Assert - Structure should be identical
        Assert.Equal(result1.Rooms.Count, result2.Rooms.Count);
        Assert.Equal(result1.Npcs.Count, result2.Npcs.Count);
        Assert.Equal(result1.Factions.Count, result2.Factions.Count);

        // Room IDs and titles should be the same
        for (int i = 0; i < result1.Rooms.Count; i++)
        {
            Assert.Equal(result1.Rooms[i].Id, result2.Rooms[i].Id);
            Assert.Equal(result1.Rooms[i].Title, result2.Rooms[i].Title);
            _output.WriteLine($"? Room {i + 1} reproduced: {result1.Rooms[i].Title}");
        }

        // NPC names should be the same
        for (int i = 0; i < result1.Npcs.Count; i++)
        {
            Assert.Equal(result1.Npcs[i].Id, result2.Npcs[i].Id);
            Assert.Equal(result1.Npcs[i].Name, result2.Npcs[i].Name);
            _output.WriteLine($"? NPC {i + 1} reproduced: {result1.Npcs[i].Name}");
        }

        slmAdapter.Dispose();
        _output.WriteLine("? Reproducibility test passed!");
    }

    public void Dispose()
    {
        // Cleanup test output directory
        try
        {
            if (Directory.Exists(_testOutputDir))
            {
                Directory.Delete(_testOutputDir, true);
                _output.WriteLine($"Cleaned up test directory: {_testOutputDir}");
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Warning: Could not cleanup test directory: {ex.Message}");
        }
    }
}

/// <summary>
/// Custom logger provider that outputs to xUnit test output
/// </summary>
public class XunitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;

    public XunitLoggerProvider(ITestOutputHelper output)
    {
        _output = output;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(_output, categoryName);
    }

    public void Dispose() { }
}

/// <summary>
/// Custom logger that writes to xUnit test output
/// </summary>
public class XunitLogger : ILogger
{
    private readonly ITestOutputHelper _output;
    private readonly string _categoryName;

    public XunitLogger(ITestOutputHelper output, string categoryName)
    {
        _output = output;
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        try
        {
            var message = formatter(state, exception);
            _output.WriteLine($"[{logLevel}] {_categoryName}: {message}");
            
            if (exception != null)
            {
                _output.WriteLine($"Exception: {exception}");
            }
        }
        catch
        {
            // Ignore logging errors
        }
    }
}
