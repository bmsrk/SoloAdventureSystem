using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;
using SoloAdventureSystem.ContentGenerator.Utils;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using Xunit;
using Xunit.Abstractions;

namespace SoloAdventureSystem.CLI.Tests;

/// <summary>
/// Integration tests for CLI-style world generation and validation.
/// Uses a shared fixture to avoid re-downloading the model.
/// </summary>
[Collection("Integration Test Collection")]
public class WorldGenerationIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly IntegrationTestFixture _fixture;
    private readonly string _testOutputDir;

    public WorldGenerationIntegrationTests(ITestOutputHelper output, IntegrationTestFixture fixture)
    {
        _output = output;
        _fixture = fixture;
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"CLITests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testOutputDir);
    }

    [Fact]
    public void GenerateWorld_WithLLamaSharp_CreatesValidWorld()
    {
        // Arrange
        var worldName = "CLITestWorld";
        var seed = 42069;
        var regions = 5;

        _output.WriteLine($"Starting world generation test: {worldName} (seed: {seed})");
        _output.WriteLine($"Output directory: {_testOutputDir}");
        _output.WriteLine($"Using shared adapter (Llama-3.2-1B)");

        // Create generator with shared adapter
        var generator = new SeededWorldGenerator(
            _fixture.Adapter,
            _fixture.ServiceProvider.GetRequiredService<IImageAdapter>(),
            _fixture.ServiceProvider.GetRequiredService<ILogger<SeededWorldGenerator>>());

        // Act - Generate world
        _output.WriteLine($"Generating world with {regions} regions...");
        var options = new WorldGenerationOptions
        {
            Name = worldName,
            Seed = seed,
            Theme = "Cyberpunk",
            Regions = regions,
            NpcDensity = "medium",
            Flavor = "Dark and gritty",
            Description = "A cyberpunk megacity",
            MainPlotPoint = "Uncover the conspiracy",
            TimePeriod = "2089",
            PowerStructure = "Corporations vs. Hackers"
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
        var validator = _fixture.ServiceProvider.GetRequiredService<WorldValidator>();
        
        validator.Validate(result);
        _output.WriteLine("? Structural validation passed");

        // Assert - Validate quality
        _output.WriteLine("Validating world quality...");
        var qualityResult = validator.ValidateQuality(result, options.Theme);
        
        _output.WriteLine($"Quality Metrics:");
        _output.WriteLine($"   Room Quality:    {qualityResult.Metrics.RoomQualityScore}/100");
        _output.WriteLine($"   NPC Quality:     {qualityResult.Metrics.NpcQualityScore}/100");
        _output.WriteLine($"   Faction Quality: {qualityResult.Metrics.FactionQualityScore}/100");
        _output.WriteLine($"   Consistency:     {qualityResult.Metrics.ConsistencyScore}/100");
        _output.WriteLine($"   Overall Score:   {qualityResult.Metrics.OverallScore}/100");

        if (qualityResult.Warnings.Count > 0)
        {
            _output.WriteLine($"Warnings ({qualityResult.Warnings.Count}):");
            foreach (var warning in qualityResult.Warnings)
            {
                _output.WriteLine($"   ?? {warning}");
            }
        }

        // Quality should be good with Llama-3.2
        Assert.True(qualityResult.IsValid, "Quality validation should pass");
        Assert.True(qualityResult.Metrics.OverallScore >= 60, 
            $"Overall score should be >= 60, got {qualityResult.Metrics.OverallScore}");

        // Assert - Export and verify
        _output.WriteLine("Exporting world...");
        var exporter = _fixture.ServiceProvider.GetRequiredService<WorldExporter>();
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

        _output.WriteLine("? All tests passed!");
    }

    [Fact]
    public void GenerateWorld_WithLLamaSharp_CreatesReproducibleWorld()
    {
        // Arrange
        var worldName = "ReproducibilityTest";
        var seed = 12345;
        var regions = 3;

        _output.WriteLine($"Testing reproducibility with seed: {seed}");
        _output.WriteLine("Using shared adapter (Llama-3.2-1B)");

        var generator = new SeededWorldGenerator(
            _fixture.Adapter,
            _fixture.ServiceProvider.GetRequiredService<IImageAdapter>(),
            _fixture.ServiceProvider.GetRequiredService<ILogger<SeededWorldGenerator>>());

        var options = new WorldGenerationOptions
        {
            Name = worldName,
            Seed = seed,
            Theme = "Cyberpunk",
            Regions = regions,
            NpcDensity = "medium",
            Flavor = "Dark cyberpunk",
            Description = "A cyberpunk world",
            MainPlotPoint = "Find the hacker",
            TimePeriod = "2089",
            PowerStructure = "Corps vs. Hackers"
        };

        // Act - Generate twice
        _output.WriteLine("Generating world (first time)...");
        var result1 = generator.Generate(options);
        
        _output.WriteLine("Generating world (second time with same seed)...");
        var result2 = generator.Generate(options);

        // Basic structural checks (pipeline expectations)
        Assert.True(result1.Factions.Count > 0, "At least one faction should be generated");
        Assert.True(result1.Rooms.Count > 0, "At least one room should be generated");
        Assert.True(result1.Npcs.Count > 0, "At least one NPC should be generated");

        // Reproducibility: counts should match between runs
        Assert.Equal(result1.Rooms.Count, result2.Rooms.Count);
        Assert.Equal(result1.Npcs.Count, result2.Npcs.Count);
        Assert.Equal(result1.Factions.Count, result2.Factions.Count);

        // Validate room content presence (non-empty descriptions and unique IDs)
        var roomIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var room in result1.Rooms)
        {
            Assert.False(string.IsNullOrWhiteSpace(room.Id), "Room must have an Id");
            Assert.False(string.IsNullOrWhiteSpace(room.BaseDescription), "Room must have a description");
            Assert.True(roomIds.Add(room.Id), "Room Ids must be unique");
        }

        // Validate NPCs presence
        var npcIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var npc in result1.Npcs)
        {
            Assert.False(string.IsNullOrWhiteSpace(npc.Id), "NPC must have an Id");
            Assert.False(string.IsNullOrWhiteSpace(npc.Name), "NPC must have a name");
            Assert.True(npcIds.Add(npc.Id), "NPC Ids must be unique");
        }

        // Story nodes: must exist and be quest/dialogue-linked without combat
        Assert.True(result1.StoryNodes.Count > 0, "Story nodes (dialogue) should be generated");

        bool hasSkillCheck = false;
        bool hasQuestEffect = false;
        foreach (var node in result1.StoryNodes)
        {
            // owner_npc_id or RawGeneratorJson referencing an npc is preferred
            // but at minimum choices should not include violent effects
            if (node.Choices != null)
            {
                foreach (var c in node.Choices)
                {
                    if (c.SkillCheck != null) hasSkillCheck = true;

                    if (c.Effects != null && c.Effects.Any())
                    {
                        foreach (var eff in c.Effects)
                        {
                            var leff = (eff ?? string.Empty).ToLowerInvariant();
                            // Disallow direct damage/combat tags in effects
                            Assert.DoesNotContain("damage", leff);
                            Assert.DoesNotContain("kill", leff);
                            Assert.DoesNotContain("attack", leff);
                            Assert.DoesNotContain("blood", leff);

                            if (leff.Contains("advance_quest") || leff.Contains("grant_reputation") || leff.Contains("record_opinion") || leff.Contains("unlock_location") || leff.Contains("reveal_clue"))
                            {
                                hasQuestEffect = true;
                            }
                        }
                    }
                }
            }
        }

        Assert.True(hasSkillCheck || hasQuestEffect, "Story nodes should include skill checks or quest-linked effects (non-violent resolutions)");

        _output.WriteLine("? Reproducibility and pipeline checks passed!");
    }

    public void Dispose()
    {
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
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Shared fixture for integration tests - initializes services and model once
/// </summary>
public class IntegrationTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public MaINAdapter Adapter { get; }

    public IntegrationTestFixture()
    {
        Console.WriteLine("??????????????????????????????????????????????????????????");
        Console.WriteLine("? Initializing Integration Test Fixture                   ?");
        Console.WriteLine("??????????????????????????????????????????????????????????");
        Console.WriteLine();
        Console.WriteLine("Using TinyLlama Q4 (600MB) for quality testing");
        Console.WriteLine("Services and model shared across all integration tests");
        Console.WriteLine();

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AI:Provider"] = "MaIN.NET",
                ["AI:Model"] = "tinyllama-q4",
                ["AI:LLamaModelKey"] = "tinyllama-q4",
                ["AI:ContextSize"] = "2048",
                ["AI:UseGPU"] = "false",
                ["AI:MaxInferenceThreads"] = "4"
            })
            .Build();

        // Build service collection
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<AISettings>(configuration.GetSection("AI"));
        services.AddSingleton<IImageAdapter, SimpleImageAdapter>();
        services.AddSingleton<WorldValidator>();
        services.AddSingleton<WorldExporter>();

        // Build a temporary provider to create the adapter (we need settings & logger)
        var tempProvider = services.BuildServiceProvider();

        var settings = tempProvider.GetRequiredService<IOptions<AISettings>>();
        var logger = tempProvider.GetRequiredService<ILogger<MaINAdapter>>();

        // Initialize adapter once
        Adapter = new MaINAdapter(settings, logger);

        Console.WriteLine("Downloading/loading model (this happens once for all tests)...");
        var progress = new Progress<DownloadProgress>(p =>
        {
            if (p.PercentComplete % 25 == 0 || p.PercentComplete == 100)
            {
                Console.WriteLine($"   ?? {p.PercentComplete}% - {p.FormattedETA} remaining");
            }
        });

        Adapter.InitializeAsync(progress).GetAwaiter().GetResult();

        // Dispose temporary provider
        if (tempProvider is IDisposable d) d.Dispose();

        // Now register the initialized adapter so other services (WorldValidator) get it
        services.AddSingleton<ILocalSLMAdapter>(Adapter);

        // Build final service provider with adapter registered
        ServiceProvider = services.BuildServiceProvider();

        Console.WriteLine();
        Console.WriteLine("? Model loaded and ready for all integration tests");
        Console.WriteLine();
    }

    public void Dispose()
    {
        Console.WriteLine();
        Console.WriteLine("Disposing integration test fixture...");
        Adapter?.Dispose();
        (ServiceProvider as IDisposable)?.Dispose();
        Console.WriteLine("? Cleanup complete");
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Collection definition for integration tests
/// </summary>
[CollectionDefinition("Integration Test Collection")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
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

    public void Dispose() 
    {
        GC.SuppressFinalize(this);
    }
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
