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
            Seed = 12345,
            Theme = "TestTheme",
            Regions = 5
        };

        // Act
        var context = new WorldGenerationContext(options);

        // Assert
        Assert.Same(options, context.Options);
        Assert.Equal(12345, context.Seed);
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
    public void Random_IsDeterministicWithSameSeed()
    {
        // Arrange
        var options1 = CreateTestOptions(seed: 42);
        var options2 = CreateTestOptions(seed: 42);

        // Act
        var context1 = new WorldGenerationContext(options1);
        var context2 = new WorldGenerationContext(options2);

        var random1Values = new[] { context1.Random.Next(), context1.Random.Next(), context1.Random.Next() };
        var random2Values = new[] { context2.Random.Next(), context2.Random.Next(), context2.Random.Next() };

        // Assert
        Assert.Equal(random1Values, random2Values);
    }

    [Fact]
    public void GetSeedFor_WithContentType_ReturnsDifferentSeeds()
    {
        // Arrange
        var context = new WorldGenerationContext(CreateTestOptions(seed: 100));

        // Act
        var roomSeed = context.GetSeedFor("Room", 0);
        var npcSeed = context.GetSeedFor("NPC", 0);
        var loreSeed = context.GetSeedFor("Lore", 0);

        // Assert
        Assert.NotEqual(roomSeed, npcSeed);
        Assert.NotEqual(npcSeed, loreSeed);
        Assert.NotEqual(roomSeed, loreSeed);
    }

    [Fact]
    public void GetSeedFor_WithSameTypeAndIndex_ReturnsSameSeed()
    {
        // Arrange
        var context = new WorldGenerationContext(CreateTestOptions(seed: 100));

        // Act
        var seed1 = context.GetSeedFor("Room", 5);
        var seed2 = context.GetSeedFor("Room", 5);

        // Assert
        Assert.Equal(seed1, seed2);
    }

    [Fact]
    public void GetSeedFor_WithDifferentIndices_ReturnsDifferentSeeds()
    {
        // Arrange
        var context = new WorldGenerationContext(CreateTestOptions(seed: 100));

        // Act
        var seed1 = context.GetSeedFor("Room", 0);
        var seed2 = context.GetSeedFor("Room", 1);
        var seed3 = context.GetSeedFor("Room", 2);

        // Assert
        Assert.NotEqual(seed1, seed2);
        Assert.NotEqual(seed2, seed3);
        Assert.NotEqual(seed1, seed3);
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

    [Fact]
    public void Seed_MatchesOptionsSeed()
    {
        // Arrange
        var options = CreateTestOptions(seed: 99999);

        // Act
        var context = new WorldGenerationContext(options);

        // Assert
        Assert.Equal(99999, context.Seed);
        Assert.Equal(options.Seed, context.Seed);
    }

    private static WorldGenerationOptions CreateTestOptions(int seed = 123, int regions = 5)
    {
        return new WorldGenerationOptions
        {
            Name = "TestWorld",
            Seed = seed,
            Theme = "TestTheme",
            Regions = regions
        };
    }
}
