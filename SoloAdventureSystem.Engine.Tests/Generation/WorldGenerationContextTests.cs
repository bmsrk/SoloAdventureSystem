using System;
using Xunit;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for WorldGenerationContext
/// </summary>
public class WorldGenerationContextTests
{
    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new WorldGenerationContext(null!));
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange
        var options = new WorldGenerationOptions
        {
            Name = "TestWorld",
            Theme = "TestTheme",
            Regions = 5
        };

        // Act
        var context = new WorldGenerationContext(options);

        // Assert
        Assert.Same(options, context.Options);
        Assert.NotNull(context.Random);
        Assert.NotNull(context.Factions);
        Assert.NotNull(context.Rooms);
        Assert.NotNull(context.Npcs);
        Assert.NotNull(context.LoreEntries);
        Assert.NotNull(context.StoryNodes);
    }

    [Fact]
    public void Constructor_InitializesEmptyCollections()
    {
        // Arrange
        var options = CreateTestOptions();

        // Act
        var context = new WorldGenerationContext(options);

        // Assert
        Assert.Empty(context.Factions);
        Assert.Empty(context.Rooms);
        Assert.Empty(context.Npcs);
        Assert.Empty(context.LoreEntries);
        Assert.Empty(context.StoryNodes);
    }

    [Fact]
    public void GetRoomCount_RespectsMinimumRooms()
    {
        // Arrange
        var context1 = new WorldGenerationContext(CreateTestOptions(regions: 1));
        var context2 = new WorldGenerationContext(CreateTestOptions(regions: 2));

        // Act
        var count1 = context1.GetRoomCount();
        var count2 = context2.GetRoomCount();

        // Assert
        Assert.True(count1 >= GenerationLimits.MinimumRooms);
        Assert.True(count2 >= GenerationLimits.MinimumRooms);
    }

    [Fact]
    public void GetRoomCount_ReturnsRegionsWhenAboveMinimum()
    {
        // Arrange
        var context = new WorldGenerationContext(CreateTestOptions(regions: 10));

        // Act
        var count = context.GetRoomCount();

        // Assert
        Assert.Equal(10, count);
    }

    [Fact]
    public void GetRoomCount_ReturnsMinimumWhenRegionsBelowMinimum()
    {
        // Arrange
        var context = new WorldGenerationContext(CreateTestOptions(regions: 1));

        // Act
        var count = context.GetRoomCount();

        // Assert
        Assert.Equal(GenerationLimits.MinimumRooms, count);
    }

    [Fact]
    public void Collections_CanBeModified()
    {
        // Arrange
        var context = new WorldGenerationContext(CreateTestOptions());
        var faction = new FactionModel { Id = "faction1", Name = "Test Faction" };
        var room = new RoomModel { Id = "room1", Title = "Test Room" };

        // Act
        context.Factions.Add(faction);
        context.Rooms.Add(room);

        // Assert
        Assert.Single(context.Factions);
        Assert.Single(context.Rooms);
        Assert.Same(faction, context.Factions[0]);
        Assert.Same(room, context.Rooms[0]);
    }

    private static WorldGenerationOptions CreateTestOptions(int regions = 5)
    {
        return new WorldGenerationOptions
        {
            Name = "TestWorld",
            Theme = "TestTheme",
            Regions = regions
        };
    }
}
