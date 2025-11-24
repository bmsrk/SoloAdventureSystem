using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for RoomGenerator
/// </summary>
public class RoomGeneratorTests
{
    private readonly Mock<ILocalSLMAdapter> _mockSlm;
    private readonly Mock<IImageAdapter> _mockImage;
    private readonly Mock<ILogger<RoomGenerator>> _mockLogger;
    private readonly RoomGenerator _generator;

    public RoomGeneratorTests()
    {
        _mockSlm = new Mock<ILocalSLMAdapter>();
        _mockImage = new Mock<IImageAdapter>();
        _mockLogger = new Mock<ILogger<RoomGenerator>>();
        _generator = new RoomGenerator(_mockSlm.Object, _mockImage.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullSlm_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new RoomGenerator(null!, _mockImage.Object));
    }

    [Fact]
    public void Constructor_WithNullImageAdapter_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new RoomGenerator(_mockSlm.Object, null!));
    }

    [Fact]
    public void Generate_WithValidContext_ReturnsCorrectNumberOfRooms()
    {
        // Arrange
        var context = CreateTestContext(regions: 5);
        SetupMocks();

        // Act
        var result = _generator.Generate(context);

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void Generate_RespectsMinimumRoomCount()
    {
        // Arrange
        var context = CreateTestContext(regions: 2); // Below minimum
        SetupMocks();

        // Act
        var result = _generator.Generate(context);

        // Assert
        Assert.True(result.Count >= GenerationLimits.MinimumRooms);
    }

    [Fact]
    public void Generate_CreatesRoomsWithUniqueIds()
    {
        // Arrange
        var context = CreateTestContext(regions: 5);
        SetupMocks();

        // Act
        var result = _generator.Generate(context);

        // Assert
        var ids = result.Select(r => r.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Generate_CreatesRoomsWithSequentialIds()
    {
        // Arrange
        var context = CreateTestContext(regions: 3);
        SetupMocks();

        // Act
        var result = _generator.Generate(context);

        // Assert
        Assert.Equal("room1", result[0].Id);
        Assert.Equal("room2", result[1].Id);
        Assert.Equal("room3", result[2].Id);
    }

    [Fact]
    public void Generate_SetsUiPositions()
    {
        // Arrange
        var context = CreateTestContext(regions: 5);
        SetupMocks();

        // Act
        var result = _generator.Generate(context);

        // Assert
        foreach (var room in result)
        {
            Assert.NotNull(room.UiPosition);
            Assert.True(room.UiPosition.X >= 0);
            Assert.True(room.UiPosition.Y >= 0);
        }
    }

    [Fact]
    public void Generate_CallsSlmForEachRoom()
    {
        // Arrange
        var context = CreateTestContext(regions: 3);
        SetupMocks();

        // Act
        _generator.Generate(context);

        // Assert
        _mockSlm.Verify(
            s => s.GenerateRoomDescription(It.IsAny<string>(), It.IsAny<int>()),
            Times.Exactly(3));
    }

    [Fact]
    public void Generate_CallsImageAdapterForEachRoom()
    {
        // Arrange
        var context = CreateTestContext(regions: 3);
        SetupMocks();

        // Act
        _generator.Generate(context);

        // Assert
        _mockImage.Verify(
            i => i.GenerateImagePrompt(It.IsAny<string>(), It.IsAny<int>()),
            Times.Exactly(3));
    }

    [Fact]
    public void Generate_SlmThrowsException_WrapsInInvalidOperationException()
    {
        // Arrange
        var context = CreateTestContext(regions: 1);
        _mockSlm
            .Setup(s => s.GenerateRoomDescription(It.IsAny<string>(), It.IsAny<int>()))
            .Throws(new Exception("SLM error"));

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _generator.Generate(context));
        Assert.Contains("Failed to generate description for room", ex.Message);
    }

    [Fact]
    public void Generate_ImageAdapterThrowsException_UsesFallbackPrompt()
    {
        // Arrange
        var context = CreateTestContext(regions: 3); // Use minimum to avoid extra rooms
        _mockSlm
            .Setup(s => s.GenerateRoomDescription(It.IsAny<string>(), It.IsAny<int>()))
            .Returns("Room description");
        _mockImage
            .Setup(i => i.GenerateImagePrompt(It.IsAny<string>(), It.IsAny<int>()))
            .Throws(new Exception("Image error"));

        // Act
        var result = _generator.Generate(context);

        // Assert
        Assert.Equal(3, result.Count); // Should have 3 rooms (minimum)
        foreach (var room in result)
        {
            Assert.NotNull(room.VisualPrompt);
            Assert.Contains("TestWorld", room.VisualPrompt); // Fallback contains world name
        }
    }

    [Fact]
    public void Generate_InitializesEmptyCollections()
    {
        // Arrange
        var context = CreateTestContext(regions: 1);
        SetupMocks();

        // Act
        var result = _generator.Generate(context);

        // Assert
        var room = result[0];
        Assert.NotNull(room.Exits);
        Assert.Empty(room.Exits);
        Assert.NotNull(room.Items);
        Assert.Empty(room.Items);
        Assert.NotNull(room.Npcs);
        Assert.Empty(room.Npcs);
    }

    [Fact]
    public void Generate_LogsProgress()
    {
        // Arrange
        var context = CreateTestContext(regions: 2);
        SetupMocks();

        // Act
        _generator.Generate(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private static WorldGenerationContext CreateTestContext(int regions = 5, int seed = 123)
    {
        return new WorldGenerationContext(new WorldGenerationOptions
        {
            Name = "TestWorld",
            Seed = seed,
            Theme = "TestTheme",
            Regions = regions
        });
    }

    private void SetupMocks()
    {
        _mockSlm
            .Setup(s => s.GenerateRoomDescription(It.IsAny<string>(), It.IsAny<int>()))
            .Returns("Test room description");
        
        _mockImage
            .Setup(i => i.GenerateImagePrompt(It.IsAny<string>(), It.IsAny<int>()))
            .Returns("Test visual prompt");
    }
}
