using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Specialized service for connecting rooms intelligently.
/// Creates more interesting topology than simple linear chains.
/// </summary>
public class RoomConnector : IContentGenerator<List<RoomModel>>
{
    private readonly ILogger<RoomConnector>? _logger;

    public RoomConnector(ILogger<RoomConnector>? logger = null)
    {
        _logger = logger;
    }

    public List<RoomModel> Generate(WorldGenerationContext context)
    {
        var rooms = context.Rooms;
        
        if (rooms.Count < 2)
        {
            _logger?.LogWarning("Not enough rooms to connect ({Count})", rooms.Count);
            return rooms;
        }

        _logger?.LogInformation("?? Connecting {Count} rooms...", rooms.Count);

        ConnectRoomsIntelligently(rooms, context.Random);

        _logger?.LogInformation("? Room connections created");
        return rooms;
    }

    /// <summary>
    /// Creates better room connections than simple linear chain.
    /// Creates a more interconnected graph structure.
    /// </summary>
    private void ConnectRoomsIntelligently(List<RoomModel> rooms, Random rand)
    {
        // First, create a main path (linear chain) so world is always traversable
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            rooms[i].Exits["east"] = rooms[i + 1].Id;
            rooms[i + 1].Exits["west"] = rooms[i].Id;
        }

        // Then add some additional connections for variety (30% chance per room)
        for (int i = 0; i < rooms.Count; i++)
        {
            // Try to add a north/south connection
            if (rand.Next(100) < 30 && i + 3 < rooms.Count)
            {
                rooms[i].Exits["south"] = rooms[i + 3].Id;
                rooms[i + 3].Exits["north"] = rooms[i].Id;
                _logger?.LogDebug("Added north/south connection: {Room1} <-> {Room2}", 
                    rooms[i].Id, rooms[i + 3].Id);
            }

            // Try to add a shortcut connection
            if (rand.Next(100) < 20 && i + 2 < rooms.Count && !rooms[i].Exits.ContainsKey("south"))
            {
                rooms[i].Exits["south"] = rooms[i + 2].Id;
                rooms[i + 2].Exits["north"] = rooms[i].Id;
                _logger?.LogDebug("Added shortcut connection: {Room1} <-> {Room2}", 
                    rooms[i].Id, rooms[i + 2].Id);
            }
        }
    }
}
