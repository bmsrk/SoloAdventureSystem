using System;
using System.Collections.Generic;
using System.Linq;
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

        string descriptionRaw;
        try
        {
            var roomPrompt = PromptTemplates.BuildRoomPrompt(
                roomName,
                context.Options,
                atmosphere,
                index,
                total);
            descriptionRaw = _slm.GenerateRoomDescription(roomPrompt, roomSeed);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to generate description for room {index + 1} ({roomName}). " +
                $"Error: {ex.Message}", ex);
        }

        // Try to parse structured TOON/JSON output
        string finalDescription = string.Empty;
        var items = new List<string>();
        var exits = new Dictionary<string, string>();

        try
        {
            // Try as object
            var parsedObj = ToonCodec.Deserialize<Dictionary<string, object>>(descriptionRaw);
            Dictionary<string, object>? source = parsedObj;

            // If not an object, try parsing as list and take first element
            if (source == null)
            {
                var parsedList = ToonCodec.Deserialize<List<Dictionary<string, object>>>(descriptionRaw);
                if (parsedList != null && parsedList.Count > 0)
                {
                    source = parsedList[0];
                }
            }

            if (source != null)
            {
                // Title override
                if (source.TryGetValue("title", out var t) && t != null)
                {
                    var tstr = t.ToString();
                    if (!string.IsNullOrWhiteSpace(tstr)) roomName = tstr.Trim();
                }

                // Description fields (support multiple possible keys)
                if (source.TryGetValue("description", out var d) && d != null)
                {
                    finalDescription = d.ToString() ?? string.Empty;
                }
                else if (source.TryGetValue("base_description", out var bd) && bd != null)
                {
                    finalDescription = bd.ToString() ?? string.Empty;
                }
                else if (source.TryGetValue("text", out var tx) && tx != null)
                {
                    finalDescription = tx.ToString() ?? string.Empty;
                }

                // Items array
                if (source.TryGetValue("objects", out var objs) && objs is System.Text.Json.JsonElement jeObjs)
                {
                    try
                    {
                        if (jeObjs.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var el in jeObjs.EnumerateArray())
                            {
                                if (el.ValueKind == System.Text.Json.JsonValueKind.String)
                                    items.Add(el.GetString()!);
                                else
                                    items.Add(el.ToString());
                            }
                        }
                    }
                    catch { }
                }
                else if (source.TryGetValue("items", out var itms) && itms is System.Text.Json.JsonElement jeItems)
                {
                    try
                    {
                        if (jeItems.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var el in jeItems.EnumerateArray())
                            {
                                if (el.ValueKind == System.Text.Json.JsonValueKind.String)
                                    items.Add(el.GetString()!);
                                else
                                    items.Add(el.ToString());
                            }
                        }
                    }
                    catch { }
                }

                // Exits map
                if (source.TryGetValue("exits", out var exs) && exs is System.Text.Json.JsonElement jeExs && jeExs.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    foreach (var prop in jeExs.EnumerateObject())
                    {
                        exits[prop.Name] = prop.Value.GetString() ?? string.Empty;
                    }
                }
                else if (source.TryGetValue("exits", out exs) && exs is Dictionary<string, object> dictExs)
                {
                    foreach (var kv in dictExs)
                    {
                        if (kv.Value != null) exits[kv.Key] = kv.Value.ToString() ?? string.Empty;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Structured parse failed for room output; falling back to raw text");
        }

        // If structured parse didn't yield a description, use legacy cleaned text
        if (string.IsNullOrWhiteSpace(finalDescription))
        {
            finalDescription = descriptionRaw ?? string.Empty;
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
            BaseDescription = finalDescription,
            Exits = exits,
            Items = items,
            Npcs = new List<string>(),
            VisualPrompt = visualPrompt,
            UiPosition = new UiPosition { X = index % 3, Y = index / 3 }
        };

        _logger?.LogDebug("? Room {Index} generated: {RoomName}", index + 1, roomName);
        return room;
    }
}
