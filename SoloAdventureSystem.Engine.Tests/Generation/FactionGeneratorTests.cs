using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for FactionGenerator
/// </summary>
public class FactionGeneratorTests
{
    private readonly Mock<ILocalSLMAdapter> _mockSlm;
    private readonly Mock<ILogger<FactionGenerator>> _mockLogger;
    private readonly FactionGenerator _generator;

    public FactionGeneratorTests()
    {
        _mockSlm = new Mock<ILocalSLMAdapter>();
        _mockLogger = new Mock<ILogger<FactionGenerator>>();
        _generator = new FactionGenerator(_mockSlm.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullAdapter_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FactionGenerator(null!));
    }

    [Fact]
    public void Generate_WithValidContext_ReturnsSingleFaction()
    {
        // Arrange
        var context = CreateTestContext();
        _mockSlm
            .Setup(s => s.GenerateFactionFlavor(It.IsAny<string>(), It.IsAny<int>()))
            .Returns("Test faction description");

        // Act
        var result = _generator.Generate(context);

        // Assert
        Assert.Single(result);
        Assert.Equal("faction1", result[0].Id);
        Assert.NotNull(result[0].Name);
        Assert.Equal("Test faction description", result[0].Description);
        Assert.Equal("Neutral", result[0].Ideology);
        Assert.NotNull(result[0].Relations);
    }

    [Fact]
    public void Generate_CallsSlmWithSomeSeed()
    {
        // Arrange
        var context = CreateTestContext();
        _mockSlm
            .Setup(s => s.GenerateFactionFlavor(It.IsAny<string>(), It.IsAny<int>()))
            .Returns("Test description");

        // Act
        _generator.Generate(context);

        // Assert
        _mockSlm.Verify(
            s => s.GenerateFactionFlavor(It.IsAny<string>(), It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public void Generate_SlmThrowsException_WrapsInInvalidOperationException()
    {
        // Arrange
        var context = CreateTestContext();
        _mockSlm
            .Setup(s => s.GenerateFactionFlavor(It.IsAny<string>(), It.IsAny<int>()))
            .Throws(new Exception("SLM error"));

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _generator.Generate(context));
        Assert.Contains("Failed to generate faction description", ex.Message);
        Assert.Contains("SLM error", ex.Message);
    }

    [Fact]
    public void Generate_UsesProceduralNameGeneration()
    {
        // Arrange
        var context = CreateTestContext();
        _mockSlm
            .Setup(s => s.GenerateFactionFlavor(It.IsAny<string>(), It.IsAny<int>()))
            .Returns("Description");

        // Act
        var result = _generator.Generate(context);

        // Assert - Name should be procedurally generated based on randomness
        Assert.NotEmpty(result[0].Name);
        Assert.NotEqual("faction1", result[0].Name); // Should not be ID
    }

    [Fact]
    public void Generate_LogsProgress()
    {
        // Arrange
        var context = CreateTestContext();
        _mockSlm
            .Setup(s => s.GenerateFactionFlavor(It.IsAny<string>(), It.IsAny<int>()))
            .Returns("Description");

        // Act
        _generator.Generate(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating faction")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private static WorldGenerationContext CreateTestContext()
    {
        return new WorldGenerationContext(new WorldGenerationOptions
        {
            Name = "TestWorld",
            Theme = "TestTheme",
            Regions = 5
        });
    }
}
