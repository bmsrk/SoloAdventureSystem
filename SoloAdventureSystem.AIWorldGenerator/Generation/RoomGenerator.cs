using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Specialized generator for room/location content.
/// Handles room creation with AI-generated descriptions and visual prompts.
/// </summary>
public class RoomGenerator : IContentGenerator<List<RoomModel>>
{
    private readonly ILocalSLMAdapter _slm;
    private readonly IImageAdapter _image;
    private readonly ILogger<RoomGenerator>? _logger;

    public RoomGenerator(
        ILocalSLMAdapter slm, 
        IImageAdapter image,
        ILogger<RoomGenerator>? logger = null)
    {
        _slm = slm ?? throw new ArgumentNullException(nameof(slm));
        _image = image ?? throw new ArgumentNullException(nameof(image));
        _logger = logger;
    }

    public List<RoomModel> Generate(WorldGenerationContext context)
    {
        var roomCount = context.GetRoomCount();
        _logger?.LogInformation("?? Generating {Count} rooms with enhanced quality...", roomCount);

        var rooms = new List<RoomModel>();

        for (int i = 0; i < roomCount; i++)
        {
            rooms.Add(GenerateRoom(context, i, roomCount));
        }

        _logger?.LogInformation("? Successfully generated {Count} rooms", rooms.Count);
        return rooms;
    }

    private RoomModel GenerateRoom(WorldGenerationContext context, int index, int total)
    {
        var roomId = $"room{index + 1}";
        var roomSeed = context.GetSeedFor("Room", index);
        var roomName = ProceduralNames.GenerateRoomName(roomSeed);
        var atmosphere = ProceduralNames.GenerateAtmosphere(roomSeed);

        _logger?.LogDebug("Generating room {Index}/{Total}: {RoomName}", index + 1, total, roomName);

        string description;
        try
        {
            var roomPrompt = PromptTemplates.BuildRoomPrompt(
                roomName,
                context.Options,
                atmosphere,
                index,
                total);
            description = _slm.GenerateRoomDescription(roomPrompt, roomSeed);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to generate description for room {index + 1} ({roomName}). " +
                $"Error: {ex.Message}", ex);
        }

        string visualPrompt;
        try
        {
            visualPrompt = _image.GenerateImagePrompt(
                $"{roomName} in {context.Options.Name} - {context.Options.Theme}",
                roomSeed);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning("Failed to generate image prompt for {RoomName}, using fallback: {Message}",
                roomName, ex.Message);
            visualPrompt = $"A {roomName.ToLower()} in {context.Options.Name}";
        }

        var room = new RoomModel
        {
            Id = roomId,
            Title = roomName,
            BaseDescription = description,
            Exits = new Dictionary<string, string>(),
            Items = new List<string>(),
            Npcs = new List<string>(),
            VisualPrompt = visualPrompt,
            UiPosition = new UiPosition { X = index % 3, Y = index / 3 }
        };

        _logger?.LogDebug("? Room {Index} generated: {RoomName}", index + 1, roomName);
        return room;
    }
}
