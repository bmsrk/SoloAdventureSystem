using Xunit;
using SoloAdventureSystem.ContentGenerator;
using Microsoft.Extensions.Logging.Abstractions;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for Stub adapter and world generation to diagnose null reference errors
/// </summary>
public class StubGeneratorTests
{
    [Fact]
    public void StubAdapter_GenerateRoomDescription_ReturnsNonNull()
    {
        // Arrange
        var adapter = new StubSLMAdapter();
        
        // Act
        var result = adapter.GenerateRoomDescription("Test Room in Cyberpunk World", 12345);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("Test Room in Cyberpunk World", result);
        Assert.Contains("12345", result);
    }

    [Fact]
    public void StubGenerator_CompleteWorkflow_FromOptionsToResult()
    {
        // This test simulates the exact workflow used in the UI
        
        // Arrange
        var slmAdapter = new StubSLMAdapter();
        var imageAdapter = new StubImageAdapter();
        var logger = NullLogger<SeededWorldGenerator>.Instance;
        var generator = new SeededWorldGenerator(slmAdapter, imageAdapter, logger);
        
        var options = new WorldGenerationOptions
        {
            Name = "NeonCity",
            Seed = 42069,
            Theme = "Cyberpunk",
            Regions = 5,
            NpcDensity = "medium",
            RenderImages = false
        };
        
        // Act
        WorldGenerationResult? result = null;
        Exception? caughtException = null;
        
        try
        {
            result = generator.Generate(options);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }
        
        // Assert
        if (caughtException != null)
        {
            Assert.Fail($"Exception: {caughtException.GetType().Name}: {caughtException.Message}\n{caughtException.StackTrace}");
        }
        
        Assert.NotNull(result);
        Assert.NotNull(result.World);
        Assert.NotNull(result.Rooms);
        Assert.NotNull(result.Npcs);
        Assert.NotEmpty(result.Rooms);
    }
}
