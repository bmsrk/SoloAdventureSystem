using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;

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

        string npcBio;
        try
        {
            var npcPrompt = PromptTemplates.BuildNpcPrompt(
                npcName,
                context.Options,
                room.Title,
                faction.Name);
            npcBio = _slm.GenerateNpcBio(npcPrompt, npcSeed);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to generate bio for NPC {index + 1} ({npcName}). Error: {ex.Message}", ex);
        }

        var npc = new NpcModel
        {
            Id = npcId,
            Name = npcName,
            Description = npcBio,
            FactionId = faction.Id,
            Hostility = "Neutral",
            Attributes = new NpcAttributes(),
            Behavior = "Static",
            Inventory = new List<string>()
        };

        _logger?.LogDebug("? NPC {Index} generated: {NpcName}", index + 1, npcName);
        return npc;
    }
}
