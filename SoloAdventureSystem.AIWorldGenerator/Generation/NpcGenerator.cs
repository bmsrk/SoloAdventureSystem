using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;
using System.Text.Json;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Specialized generator for NPC content.
/// Creates NPCs with AI-generated personalities and assigns them to rooms.
/// </summary>
public class NpcGenerator : IContentGenerator<List<NpcModel>>
{
    private readonly ILocalSLMAdapter _slm;
    private readonly ILogger<NpcGenerator>? _logger;

    public NpcGenerator(ILocalSLMAdapter slm, ILogger<NpcGenerator>? logger = null)
    {
        _slm = slm ?? throw new ArgumentNullException(nameof(slm));
        _logger = logger;
    }

    public List<NpcModel> Generate(WorldGenerationContext context)
    {
        if (context.Rooms.Count == 0)
        {
            _logger?.LogWarning("No rooms available for NPC placement");
            return new List<NpcModel>();
        }

        if (context.Factions.Count == 0)
        {
            _logger?.LogWarning("No factions available for NPC assignment");
            return new List<NpcModel>();
        }

        var npcCount = context.Rooms.Count;
        _logger?.LogInformation("?? Generating {Count} NPCs with enhanced personalities...", npcCount);

        var npcs = new List<NpcModel>();
        var faction = context.Factions[0]; // Use first faction

        for (int i = 0; i < npcCount; i++)
        {
            var npc = GenerateNpc(context, i, faction, context.Rooms[i]);
            npcs.Add(npc);
            
            // Assign NPC to room
            context.Rooms[i].Npcs.Add(npc.Id);
        }

        _logger?.LogInformation("? Successfully generated {Count} NPCs", npcs.Count);
        return npcs;
    }

    private NpcModel GenerateNpc(
        WorldGenerationContext context, 
        int index, 
        FactionModel faction,
        RoomModel room)
    {
        var npcId = $"npc{index + 1}";
        var npcSeed = context.GetSeedFor("NPC", index);
        var npcName = ProceduralNames.GenerateNpcName(npcSeed);

        _logger?.LogDebug("Generating NPC {Index}/{Total}: {NpcName}", 
            index + 1, context.Rooms.Count, npcName);

        string npcBioRaw;
        try
        {
            var npcPrompt = PromptTemplates.BuildNpcPrompt(
                npcName,
                context.Options,
                room.Title,
                faction.Name);
            npcBioRaw = _slm.GenerateNpcBio(npcPrompt, npcSeed);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to generate bio for NPC {index + 1} ({npcName}). Error: {ex.Message}", ex);
        }

        string finalBio = string.Empty;
        string role = string.Empty;
        string trait = string.Empty;

        try
        {
            var parsed = ToonCodec.Deserialize<Dictionary<string, object>>(npcBioRaw);
            if (parsed == null)
            {
                var parsedList = ToonCodec.Deserialize<List<Dictionary<string, object>>>(npcBioRaw);
                if (parsedList != null && parsedList.Count > 0) parsed = parsedList[0];
            }

            if (parsed != null)
            {
                if (parsed.TryGetValue("bio", out var bio) && bio != null) finalBio = bio.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(finalBio) && parsed.TryGetValue("description", out var desc) && desc != null) finalBio = desc.ToString() ?? string.Empty;
                if (parsed.TryGetValue("role", out var r) && r != null) role = r.ToString() ?? string.Empty;
                if (parsed.TryGetValue("trait", out var tr) && tr != null) trait = tr.ToString() ?? string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Structured parse failed for NPC output; falling back to raw text");
        }

        if (string.IsNullOrWhiteSpace(finalBio)) finalBio = npcBioRaw;

        var npc = new NpcModel
        {
            Id = npcId,
            Name = npcName,
            Description = finalBio,
            FactionId = faction.Id,
            Hostility = "Neutral",
            Attributes = new NpcAttributes(),
            Behavior = "Static",
            Inventory = new List<string>()
        };

        if (!string.IsNullOrWhiteSpace(role)) npc.Behavior = role;
        if (!string.IsNullOrWhiteSpace(trait)) npc.Description = npc.Description + " " + trait;

        _logger?.LogDebug("? NPC {Index} generated: {NpcName}", index + 1, npcName);
        return npc;
    }
}
