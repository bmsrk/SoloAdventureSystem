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
/// These tests require the model to be downloaded and may take several minutes.
/// </summary>
public class LLamaSharpAdapterTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly LLamaSharpAdapter? _adapter;
    private readonly bool _adapterInitialized;

    public LLamaSharpAdapterTests(ITestOutputHelper output)
    {
        _output = output;
        
        try
        {
            var settings = Options.Create(new AISettings
            {
                Provider = "LLamaSharp",
                Model = "phi-3-mini-q4",
                LLamaModelKey = "phi-3-mini-q4",
                ContextSize = 2048,
                UseGPU = false,
                MaxInferenceThreads = 4
            });

            var logger = new XunitLogger<LLamaSharpAdapter>(_output);
            _adapter = new LLamaSharpAdapter(settings, logger);
            
            _output.WriteLine("Initializing LLamaSharp adapter...");
            _adapter.InitializeAsync().GetAwaiter().GetResult();
            _adapterInitialized = true;
            _output.WriteLine("? Adapter initialized successfully");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"?? Could not initialize adapter: {ex.Message}");
            _adapterInitialized = false;
        }
    }

    [Fact(Skip = "Long-running test - requires model download and initialization (run manually)")]
    public void GenerateRoomDescription_WithValidPrompt_ReturnsNonEmptyText()
    {
        // Skip if adapter not initialized
        if (!_adapterInitialized || _adapter == null)
        {
            _output.WriteLine("?? Skipping: Adapter not initialized");
            return;
        }

        // Arrange
        var prompt = "Describe a neon-lit cyberpunk bar with holographic displays.";
        var seed = 42;

        _output.WriteLine($"Prompt: \"{prompt}\"");
        _output.WriteLine($"Seed: {seed}");

        // Act
        var startTime = DateTime.UtcNow;
        var result = _adapter.GenerateRoomDescription(prompt, seed);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length > 20, $"Result too short: {result.Length} characters");

        _output.WriteLine($"? Generated in {duration.TotalSeconds:F1}s");
        _output.WriteLine($"Result ({result.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        _output.WriteLine(result);
        _output.WriteLine("???????????????????????????????????????????????????????????");
    }

    [Fact(Skip = "Long-running test - requires model download and initialization (run manually)")]
    public void GenerateNpcBio_WithValidPrompt_ReturnsNonEmptyText()
    {
        // Skip if adapter not initialized
        if (!_adapterInitialized || _adapter == null)
        {
            _output.WriteLine("?? Skipping: Adapter not initialized");
            return;
        }

        // Arrange
        var prompt = "Create a bio for a street samurai named Blade who operates in the shadows.";
        var seed = 123;

        _output.WriteLine($"Prompt: \"{prompt}\"");
        _output.WriteLine($"Seed: {seed}");

        // Act
        var startTime = DateTime.UtcNow;
        var result = _adapter.GenerateNpcBio(prompt, seed);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length > 20, $"Result too short: {result.Length} characters");

        _output.WriteLine($"? Generated in {duration.TotalSeconds:F1}s");
        _output.WriteLine($"Result ({result.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        _output.WriteLine(result);
        _output.WriteLine("???????????????????????????????????????????????????????????");
    }

    [Fact(Skip = "Long-running test - requires model download and initialization (run manually)")]
    public void GenerateFactionFlavor_WithValidPrompt_ReturnsNonEmptyText()
    {
        // Skip if adapter not initialized
        if (!_adapterInitialized || _adapter == null)
        {
            _output.WriteLine("?? Skipping: Adapter not initialized");
            return;
        }

        // Arrange
        var prompt = "Describe the Chrome Dragons, a powerful cyberpunk gang controlling the neon district.";
        var seed = 456;

        _output.WriteLine($"Prompt: \"{prompt}\"");
        _output.WriteLine($"Seed: {seed}");

        // Act
        var startTime = DateTime.UtcNow;
        var result = _adapter.GenerateFactionFlavor(prompt, seed);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length > 20, $"Result too short: {result.Length} characters");

        _output.WriteLine($"? Generated in {duration.TotalSeconds:F1}s");
        _output.WriteLine($"Result ({result.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        _output.WriteLine(result);
        _output.WriteLine("???????????????????????????????????????????????????????????");
    }

    [Fact(Skip = "Long-running test - requires model download and initialization (run manually)")]
    public void GenerateLoreEntries_WithValidPrompt_ReturnsCorrectCount()
    {
        // Skip if adapter not initialized
        if (!_adapterInitialized || _adapter == null)
        {
            _output.WriteLine("?? Skipping: Adapter not initialized");
            return;
        }

        // Arrange
        var prompt = "Generate lore about the history of the Neon City megacity.";
        var seed = 789;
        var count = 3;

        _output.WriteLine($"Prompt: \"{prompt}\"");
        _output.WriteLine($"Seed: {seed}");
        _output.WriteLine($"Count: {count}");

        // Act
        var startTime = DateTime.UtcNow;
        var results = _adapter.GenerateLoreEntries(prompt, seed, count);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.NotNull(results);
        Assert.Equal(count, results.Count);
        
        foreach (var entry in results)
        {
            Assert.NotNull(entry);
            Assert.NotEmpty(entry);
            Assert.True(entry.Length > 10, $"Lore entry too short: {entry.Length} characters");
        }

        _output.WriteLine($"? Generated {count} entries in {duration.TotalSeconds:F1}s");
        
        for (int i = 0; i < results.Count; i++)
        {
            _output.WriteLine($"\nEntry {i + 1} ({results[i].Length} chars):");
            _output.WriteLine("???????????????????????????????????????????????????????????");
            _output.WriteLine(results[i]);
        }
        _output.WriteLine("???????????????????????????????????????????????????????????");
    }

    [Fact(Skip = "Long-running test - requires model download and initialization (run manually)")]
    public void GenerateRoomDescription_SameSeed_ProducesSameOutput()
    {
        // Skip if adapter not initialized
        if (!_adapterInitialized || _adapter == null)
        {
            _output.WriteLine("?? Skipping: Adapter not initialized");
            return;
        }

        // Arrange
        var prompt = "A dimly lit server room with rows of blinking machines.";
        var seed = 999;

        _output.WriteLine($"Testing determinism with seed: {seed}");

        // Act
        var result1 = _adapter.GenerateRoomDescription(prompt, seed);
        var result2 = _adapter.GenerateRoomDescription(prompt, seed);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(result1, result2);

        _output.WriteLine("? Same seed produces identical output");
        _output.WriteLine($"Output ({result1.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        _output.WriteLine(result1);
        _output.WriteLine("???????????????????????????????????????????????????????????");
    }

    [Fact(Skip = "Long-running test - requires model download and initialization (run manually)")]
    public void GenerateRoomDescription_DifferentSeeds_ProducesDifferentOutput()
    {
        // Skip if adapter not initialized
        if (!_adapterInitialized || _adapter == null)
        {
            _output.WriteLine("?? Skipping: Adapter not initialized");
            return;
        }

        // Arrange
        var prompt = "A high-tech laboratory with experimental equipment.";
        var seed1 = 111;
        var seed2 = 222;

        _output.WriteLine($"Testing variance with seeds: {seed1} vs {seed2}");

        // Act
        var result1 = _adapter.GenerateRoomDescription(prompt, seed1);
        var result2 = _adapter.GenerateRoomDescription(prompt, seed2);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotEqual(result1, result2);

        _output.WriteLine("? Different seeds produce different output");
        _output.WriteLine($"\nSeed {seed1} ({result1.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        _output.WriteLine(result1);
        _output.WriteLine("???????????????????????????????????????????????????????????");
        _output.WriteLine($"\nSeed {seed2} ({result2.Length} chars):");
        _output.WriteLine("???????????????????????????????????????????????????????????");
        _output.WriteLine(result2);
        _output.WriteLine("???????????????????????????????????????????????????????????");
    }

    [Fact(Skip = "Long-running test - requires model download and initialization (run manually)")]
    public void GenerateRoomDescription_MultipleTypes_AllSucceed()
    {
        // Skip if adapter not initialized
        if (!_adapterInitialized || _adapter == null)
        {
            _output.WriteLine("?? Skipping: Adapter not initialized");
            return;
        }

        // Arrange & Act & Assert
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
                "Room" => _adapter.GenerateRoomDescription(prompt, seed),
                "NPC" => _adapter.GenerateNpcBio(prompt, seed),
                "Faction" => _adapter.GenerateFactionFlavor(prompt, seed),
                _ => throw new InvalidOperationException()
            };

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result.Length > 20, $"{type} result too short: {result.Length} characters");
            
            _output.WriteLine($"? {type}: {result.Length} chars");
            _output.WriteLine(result.Substring(0, Math.Min(100, result.Length)) + "...");
        }

        _output.WriteLine("\n? All generation types successful!");
    }

    public void Dispose()
    {
        _adapter?.Dispose();
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
