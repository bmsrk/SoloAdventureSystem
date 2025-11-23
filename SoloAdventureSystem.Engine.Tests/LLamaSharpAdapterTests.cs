using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for LLamaSharp adapter to verify AI text generation is working correctly.
/// Uses a shared fixture to avoid downloading/loading the model multiple times.
/// </summary>
[Collection("LLamaSharp Collection")]
public class LLamaSharpAdapterTests
{
    private readonly ITestOutputHelper _output;
    private readonly LLamaSharpFixture _fixture;

    public LLamaSharpAdapterTests(ITestOutputHelper output, LLamaSharpFixture fixture)
    {
        _output = output;
        _fixture = fixture;
    }

    [Fact]
    public void GenerateRoomDescription_WithValidPrompt_ReturnsNonEmptyText()
    {
        // Arrange
        var adapter = _fixture.Adapter;
        var prompt = "Describe a neon-lit cyberpunk bar with holographic displays.";
        var seed = 42;

        _output.WriteLine($"Prompt: \"{prompt}\"");
        _output.WriteLine($"Seed: {seed}");

        // Act
        var startTime = DateTime.UtcNow;
        var result = adapter.GenerateRoomDescription(prompt, seed);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length > 20, $"Result too short: {result.Length} characters");

        _output.WriteLine($"? Generated in {duration.TotalSeconds:F1}s");
        _output.WriteLine($"Result ({result.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        var preview = result.Length > 55 ? result.Substring(0, 55) : result.PadRight(55);
        _output.WriteLine($"? {preview} ?");
        _output.WriteLine("???????????????????????????????????????????????????????????");
    }

    [Fact]
    public void GenerateNpcBio_WithValidPrompt_ReturnsNonEmptyText()
    {
        // Arrange
        var adapter = _fixture.Adapter;
        var prompt = "Create a bio for a street samurai named Blade who operates in the shadows.";
        var seed = 123;

        _output.WriteLine($"Prompt: \"{prompt}\"");
        _output.WriteLine($"Seed: {seed}");

        // Act
        var startTime = DateTime.UtcNow;
        var result = adapter.GenerateNpcBio(prompt, seed);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length > 20, $"Result too short: {result.Length} characters");

        _output.WriteLine($"? Generated in {duration.TotalSeconds:F1}s");
        _output.WriteLine($"Result ({result.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        var preview = result.Length > 55 ? result.Substring(0, 55) : result.PadRight(55);
        _output.WriteLine($"? {preview} ?");
        _output.WriteLine("???????????????????????????????????????????????????????????");
    }

    [Fact]
    public void GenerateFactionFlavor_WithValidPrompt_ReturnsNonEmptyText()
    {
        // Arrange
        var adapter = _fixture.Adapter;
        var prompt = "Describe the Chrome Dragons, a powerful cyberpunk gang controlling the neon district.";
        var seed = 456;

        _output.WriteLine($"Prompt: \"{prompt}\"");
        _output.WriteLine($"Seed: {seed}");

        // Act
        var startTime = DateTime.UtcNow;
        var result = adapter.GenerateFactionFlavor(prompt, seed);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length > 20, $"Result too short: {result.Length} characters");

        _output.WriteLine($"? Generated in {duration.TotalSeconds:F1}s");
        _output.WriteLine($"Result ({result.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        var preview = result.Length > 55 ? result.Substring(0, 55) : result.PadRight(55);
        _output.WriteLine($"? {preview} ?");
        _output.WriteLine("???????????????????????????????????????????????????????????");
    }

    [Fact]
    public void GenerateLoreEntries_WithValidPrompt_ReturnsCorrectCount()
    {
        // Arrange
        var adapter = _fixture.Adapter;
        var prompt = "Generate lore about the history of the Neon City megacity.";
        var seed = 789;
        var count = 3;

        _output.WriteLine($"Prompt: \"{prompt}\"");
        _output.WriteLine($"Seed: {seed}");
        _output.WriteLine($"Count: {count}");

        // Act
        var startTime = DateTime.UtcNow;
        var results = adapter.GenerateLoreEntries(prompt, seed, count);
        var duration = DateTime.UtcNow - startTime;

        // Assert - Should return correct count
        Assert.NotNull(results);
        Assert.Equal(count, results.Count);
        
        _output.WriteLine($"? Generated {count} entries in {duration.TotalSeconds:F1}s");
        
        var validEntries = 0;
        for (int i = 0; i < results.Count; i++)
        {
            Assert.NotNull(results[i]); // Each entry should not be null
            
            var entry = results[i];
            var isValid = !string.IsNullOrWhiteSpace(entry) && entry.Length > 10;
            
            if (isValid)
            {
                validEntries++;
                _output.WriteLine($"\nEntry {i + 1} ({entry.Length} chars): ?");
                _output.WriteLine("???????????????????????????????????????????????????????????");
                var preview = entry.Length > 55 ? entry.Substring(0, 52) + "..." : entry;
                _output.WriteLine($"? {preview.PadRight(55)} ?");
                _output.WriteLine("???????????????????????????????????????????????????????????");
            }
            else
            {
                _output.WriteLine($"\nEntry {i + 1}: ?? Empty or too short");
            }
        }
        
        _output.WriteLine($"\nValid entries: {validEntries}/{count}");
        
        // With a good model, most entries should be valid
        Assert.True(validEntries >= count * 0.7, 
            $"Expected at least 70% valid entries, got {validEntries}/{count}");
    }

    [Fact]
    public void GenerateRoomDescription_SameSeed_ProducesConsistentOutput()
    {
        // Arrange
        var adapter = _fixture.Adapter;
        var prompt = "A dimly lit server room with rows of blinking machines.";
        var seed = 999;

        _output.WriteLine($"Testing consistency with seed: {seed}");

        // Act
        var result1 = adapter.GenerateRoomDescription(prompt, seed);
        var result2 = adapter.GenerateRoomDescription(prompt, seed);

        // Assert - Both should produce valid output
        Assert.NotNull(result1);
        Assert.NotEmpty(result1);
        Assert.True(result1.Length > 20, $"First result too short: {result1.Length} characters");
        
        Assert.NotNull(result2);
        Assert.NotEmpty(result2);
        Assert.True(result2.Length > 20, $"Second result too short: {result2.Length} characters");
        
        _output.WriteLine($"First output ({result1.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        var preview1 = result1.Length > 55 ? result1.Substring(0, 55) : result1.PadRight(55);
        _output.WriteLine($"? {preview1} ?");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        
        _output.WriteLine($"Second output ({result2.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        var preview2 = result2.Length > 55 ? result2.Substring(0, 55) : result2.PadRight(55);
        _output.WriteLine($"? {preview2} ?");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        
        _output.WriteLine("? Both generations succeeded with valid output");
    }

    [Fact]
    public void GenerateRoomDescription_DifferentSeeds_ProducesDifferentOutput()
    {
        // Arrange
        var adapter = _fixture.Adapter;
        var prompt = "A high-tech laboratory with experimental equipment.";
        var seed1 = 111;
        var seed2 = 222;

        _output.WriteLine($"Testing variance with seeds: {seed1} vs {seed2}");

        // Act
        var result1 = adapter.GenerateRoomDescription(prompt, seed1);
        var result2 = adapter.GenerateRoomDescription(prompt, seed2);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotEqual(result1, result2);

        _output.WriteLine("? Different seeds produce different output");
        _output.WriteLine($"\nSeed {seed1} ({result1.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        var preview1 = result1.Length > 55 ? result1.Substring(0, 55) : result1.PadRight(55);
        _output.WriteLine($"? {preview1} ?");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        
        _output.WriteLine($"\nSeed {seed2} ({result2.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        var preview2 = result2.Length > 55 ? result2.Substring(0, 55) : result2.PadRight(55);
        _output.WriteLine($"? {preview2} ?");
        _output.WriteLine("???????????????????????????????????????????????????????????");
    }

    [Fact]
    public void GenerateRoomDescription_MultipleTypes_AllSucceed()
    {
        // Arrange
        var adapter = _fixture.Adapter;
        var tests = new[]
        {
            ("Room", "Describe a cyberpunk apartment", 100),
            ("NPC", "Create a bio for a netrunner", 200),
            ("Faction", "Describe a corporate faction", 300)
        };

        _output.WriteLine("Testing multiple generation types...");
        
        foreach (var (type, prompt, seed) in tests)
        {
            _output.WriteLine($"\n{type}: \"{prompt}\" (seed: {seed})");
            
            string result = type switch
            {
                "Room" => adapter.GenerateRoomDescription(prompt, seed),
                "NPC" => adapter.GenerateNpcBio(prompt, seed),
                "Faction" => adapter.GenerateFactionFlavor(prompt, seed),
                _ => throw new InvalidOperationException()
            };

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result.Length > 20, $"{type} result too short: {result.Length} characters");
            
            _output.WriteLine($"? {type}: {result.Length} chars");
            var preview = result.Length > 100 ? result.Substring(0, 100) + "..." : result;
            _output.WriteLine($"   {preview}");
        }

        _output.WriteLine("\n? All generation types successful!");
    }
}

/// <summary>
/// Shared fixture for LLamaSharp tests - initializes model once for all tests
/// </summary>
public class LLamaSharpFixture : IDisposable
{
    public LLamaSharpAdapter Adapter { get; }
    
    public LLamaSharpFixture()
    {
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine("? Initializing LLamaSharp Test Fixture                    ?");
        Console.WriteLine("????????????????????????????????????????????????????????????");
        Console.WriteLine();
        Console.WriteLine("Using TinyLlama Q4 (600MB) for better quality");
        Console.WriteLine("Model will be shared across all tests");
        Console.WriteLine();
        
        var settings = Options.Create(new AISettings
        {
            Provider = "LLamaSharp",
            Model = "tinyllama-q4",
            LLamaModelKey = "tinyllama-q4",
            ContextSize = 2048,
            UseGPU = false,
            MaxInferenceThreads = 4
        });

        var logger = new XunitLogger<LLamaSharpAdapter>(new ConsoleTestOutputHelper());
        Adapter = new LLamaSharpAdapter(settings, logger);
        
        Console.WriteLine("Downloading/loading model (this happens once)...");
        Adapter.InitializeAsync().GetAwaiter().GetResult();
        
        Console.WriteLine();
        Console.WriteLine("? Model loaded and ready for all tests");
        Console.WriteLine();
    }

    public void Dispose()
    {
        Console.WriteLine();
        Console.WriteLine("Disposing shared LLamaSharp adapter...");
        Adapter?.Dispose();
        Console.WriteLine("? Cleanup complete");
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Collection definition for LLamaSharp tests
/// </summary>
[CollectionDefinition("LLamaSharp Collection")]
public class LLamaSharpCollection : ICollectionFixture<LLamaSharpFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

/// <summary>
/// Simple console output helper for fixture initialization
/// </summary>
public class ConsoleTestOutputHelper : ITestOutputHelper
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        Console.WriteLine(format, args);
    }
}

/// <summary>
/// Simple xUnit logger adapter
/// </summary>
public class XunitLogger<T> : ILogger<T>
{
    private readonly ITestOutputHelper _output;

    public XunitLogger(ITestOutputHelper output)
    {
        _output = output;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        try
        {
            var message = formatter(state, exception);
            _output.WriteLine($"[{logLevel}] {message}");
            
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
