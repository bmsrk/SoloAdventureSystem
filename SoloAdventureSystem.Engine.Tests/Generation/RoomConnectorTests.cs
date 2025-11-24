using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for RoomConnector
/// </summary>
public class RoomConnectorTests
{
    private readonly Mock<ILogger<RoomConnector>> _mockLogger;
    private readonly RoomConnector _connector;

    public RoomConnectorTests()
    {
        _mockLogger = new Mock<ILogger<RoomConnector>>();
        _connector = new RoomConnector(_mockLogger.Object);
    }

    [Fact]
    public void Generate_WithLessThanTwoRooms_ReturnsUnchanged()
    {
        // Arrange
        var context = CreateTestContext();
        context.Rooms.Add(CreateRoom("room1"));

        // Act
        var result = _connector.Generate(context);

        // Assert
        Assert.Single(result);
        Assert.Empty(result[0].Exits);
    }

    [Fact]
    public void Generate_WithTwoRooms_CreatesBasicConnection()
    {
        // Arrange
        var context = CreateTestContext();
        context.Rooms.Add(CreateRoom("room1"));
        context.Rooms.Add(CreateRoom("room2"));

        // Act
        var result = _connector.Generate(context);

        // Assert
        Assert.Equal("room2", result[0].Exits["east"]);
        Assert.Equal("room1", result[1].Exits["west"]);
    }

    [Fact]
    public void Generate_WithMultipleRooms_CreatesLinearChain()
    {
        // Arrange
        var context = CreateTestContext();
        for (int i = 1; i <= 5; i++)
        {
            context.Rooms.Add(CreateRoom($"room{i}"));
        }

        // Act
        var result = _connector.Generate(context);

        // Assert - Verify linear chain
        for (int i = 0; i < 4; i++)
        {
            Assert.True(result[i].Exits.ContainsKey("east"));
            Assert.Equal(result[i + 1].Id, result[i].Exits["east"]);
        }

        for (int i = 1; i < 5; i++)
        {
            Assert.True(result[i].Exits.ContainsKey("west"));
            Assert.Equal(result[i - 1].Id, result[i].Exits["west"]);
        }
    }

    [Fact]
    public void Generate_AllRoomsAreReachable()
    {
        // Arrange
        var context = CreateTestContext(seed: 42); // Deterministic for testing
        for (int i = 1; i <= 10; i++)
        {
            context.Rooms.Add(CreateRoom($"room{i}"));
        }

        // Act
        var result = _connector.Generate(context);

        // Assert - Use BFS to verify all rooms reachable from room1
        var reachable = new HashSet<string>();
        var queue = new Queue<string>();
        queue.Enqueue("room1");
        reachable.Add("room1");

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var room = result.First(r => r.Id == current);

            foreach (var exit in room.Exits.Values)
            {
                if (!reachable.Contains(exit))
                {
                    reachable.Add(exit);
                    queue.Enqueue(exit);
                }
            }
        }

        Assert.Equal(result.Count, reachable.Count);
    }

    [Fact]
    public void Generate_MayCreateAdditionalConnections()
    {
        // Arrange
        var context = CreateTestContext(seed: 12345);
        for (int i = 1; i <= 10; i++)
        {
            context.Rooms.Add(CreateRoom($"room{i}"));
        }

        // Act
        var result = _connector.Generate(context);

        // Assert - Count total connections (should be more than just linear chain)
        var totalConnections = result.Sum(r => r.Exits.Count);
        var minimumLinearConnections = (result.Count - 1) * 2; // Each link counted twice

        // May have additional connections (this is probabilistic, so just check >= minimum)
        Assert.True(totalConnections >= minimumLinearConnections);
    }

    [Fact]
    public void Generate_BidirectionalConnections()
    {
        // Arrange
        var context = CreateTestContext();
        for (int i = 1; i <= 5; i++)
        {
            context.Rooms.Add(CreateRoom($"room{i}"));
        }

        // Act
        var result = _connector.Generate(context);

        // Assert - Verify all connections are bidirectional
        foreach (var room in result)
        {
            foreach (var exit in room.Exits)
            {
                var targetRoom = result.First(r => r.Id == exit.Value);
                
                // Find reverse direction
                var reverseDirection = GetReverseDirection(exit.Key);
                if (reverseDirection != null)
                {
                    Assert.True(targetRoom.Exits.ContainsKey(reverseDirection),
                        $"Expected bidirectional connection from {room.Id} to {targetRoom.Id}");
                    Assert.Equal(room.Id, targetRoom.Exits[reverseDirection]);
                }
            }
        }
    }

    [Fact]
    public void Generate_DoesNotModifyRoomCount()
    {
        // Arrange
        var context = CreateTestContext();
        for (int i = 1; i <= 5; i++)
        {
            context.Rooms.Add(CreateRoom($"room{i}"));
        }
        var originalCount = context.Rooms.Count;

        // Act
        var result = _connector.Generate(context);

        // Assert
        Assert.Equal(originalCount, result.Count);
    }

    [Fact]
    public void Generate_PreservesRoomIds()
    {
        // Arrange
        var context = CreateTestContext();
        var expectedIds = new List<string>();
        for (int i = 1; i <= 5; i++)
        {
            var id = $"room{i}";
            expectedIds.Add(id);
            context.Rooms.Add(CreateRoom(id));
        }

        // Act
        var result = _connector.Generate(context);

        // Assert
        var resultIds = result.Select(r => r.Id).OrderBy(id => id).ToList();
        var expectedOrdered = expectedIds.OrderBy(id => id).ToList();
        Assert.Equal(expectedOrdered, resultIds);
    }

    [Fact]
    public void Generate_LogsProgress()
    {
        // Arrange
        var context = CreateTestContext();
        context.Rooms.Add(CreateRoom("room1"));
        context.Rooms.Add(CreateRoom("room2"));

        // Act
        _connector.Generate(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Connecting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private static WorldGenerationContext CreateTestContext(int seed = 123)
    {
        return new WorldGenerationContext(new WorldGenerationOptions
        {
            Name = "TestWorld",
            Seed = seed,
            Theme = "TestTheme",
            Regions = 5
        });
    }

    private static RoomModel CreateRoom(string id)
    {
        return new RoomModel
        {
            Id = id,
            Title = $"Test Room {id}",
            BaseDescription = "Test description",
            Exits = new Dictionary<string, string>(),
            Items = new List<string>(),
            Npcs = new List<string>(),
            VisualPrompt = "Test prompt",
            UiPosition = new UiPosition { X = 0, Y = 0 }
        };
    }

    private static string? GetReverseDirection(string direction)
    {
        return direction switch
        {
            "north" => "south",
            "south" => "north",
            "east" => "west",
            "west" => "east",
            _ => null
        };
    }
}
