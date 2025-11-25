using System;
using System.Collections.Generic;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Context object that carries state through the world generation pipeline.
/// Replaces passing multiple parameters repeatedly and enables pipeline pattern.
/// </summary>
public class WorldGenerationContext
{
    public WorldGenerationOptions Options { get; }
    public Random Random { get; }

    // Generated content (populated during pipeline execution)
    public List<FactionModel> Factions { get; set; } = new();
    public List<RoomModel> Rooms { get; set; } = new();
    public List<NpcModel> Npcs { get; set; } = new();
    public List<string> LoreEntries { get; set; } = new();
    public List<StoryNodeModel> StoryNodes { get; set; } = new();

    public WorldGenerationContext(WorldGenerationOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Random = new Random();
    }

    /// <summary>
    /// Get room count based on options
    /// </summary>
    public int GetRoomCount()
    {
        return Math.Max(GenerationLimits.MinimumRooms, Options.Regions);
    }
}
