using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SoloAdventureSystem.ContentGenerator
{
    public class SeededWorldGenerator : IWorldGenerator
    {
        private readonly ILocalSLMAdapter _slm;
        private readonly IImageAdapter _image;
        private readonly ILogger<SeededWorldGenerator>? _logger;
        
        public SeededWorldGenerator(ILocalSLMAdapter slm, IImageAdapter image, ILogger<SeededWorldGenerator>? logger = null)
        {
            _slm = slm;
            _image = image;
            _logger = logger;
        }
        
        public WorldGenerationResult Generate(WorldGenerationOptions options)
        {
            _logger?.LogInformation("Starting world generation: {Name} (seed: {Seed}, regions: {Regions})", 
                options.Name, options.Seed, options.Regions);
            
            var rand = new Random(options.Seed);
            var result = new WorldGenerationResult();
            
            try
            {
                // Step 1: Generate rooms
                _logger?.LogInformation("Generating {Count} rooms...", Math.Max(3, options.Regions));
                for (int i = 0; i < Math.Max(3, options.Regions); i++)
                {
                    var roomId = $"room{i+1}";
                    
                    _logger?.LogDebug("Generating room {Index}/{Total}: {RoomId}", i + 1, options.Regions, roomId);
                    
                    string description;
                    try
                    {
                        description = _slm.GenerateRoomDescription(
                            $"Room {i+1} in {options.Name} ({options.Theme})", 
                            options.Seed + i);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to generate description for room {i+1} ({roomId}). " +
                            $"Error: {ex.Message}", ex);
                    }
                    
                    string visualPrompt;
                    try
                    {
                        visualPrompt = _image.GenerateImagePrompt(
                            $"Room {i+1} in {options.Name}", 
                            options.Seed + i);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning("Failed to generate image prompt for {RoomId}, using fallback: {Message}", 
                            roomId, ex.Message);
                        visualPrompt = $"A mysterious room in {options.Name}";
                    }
                    
                    var room = new RoomModel
                    {
                        Id = roomId,
                        Title = $"Room {i+1}",
                        BaseDescription = description,
                        Exits = new Dictionary<string, string>(),
                        Items = new List<string>(),
                        Npcs = new List<string>(),
                        VisualPrompt = visualPrompt,
                        UiPosition = new UiPosition { X = i % 3, Y = i / 3 }
                    };
                    result.Rooms.Add(room);
                }
                
                _logger?.LogInformation("Successfully generated {Count} rooms", result.Rooms.Count);
                
                // Connect rooms deterministically
                for (int i = 0; i < result.Rooms.Count - 1; i++)
                {
                    result.Rooms[i].Exits["east"] = result.Rooms[i + 1].Id;
                    result.Rooms[i + 1].Exits["west"] = result.Rooms[i].Id;
                }
                
                // Step 2: Generate faction
                _logger?.LogInformation("Generating faction...");
                string factionDescription;
                try
                {
                    factionDescription = _slm.GenerateFactionFlavor(
                        $"Faction One in {options.Name} ({options.Theme})", 
                        options.Seed);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to generate faction description. Error: {ex.Message}", ex);
                }
                
                var faction = new FactionModel
                {
                    Id = "faction1",
                    Name = "Faction One",
                    Description = factionDescription,
                    Ideology = "Neutral",
                    Relations = new Dictionary<string, int>()
                };
                result.Factions.Add(faction);
                
                // Step 3: Generate NPCs
                _logger?.LogInformation("Generating {Count} NPCs...", result.Rooms.Count);
                for (int i = 0; i < result.Rooms.Count; i++)
                {
                    var npcId = $"npc{i+1}";
                    
                    _logger?.LogDebug("Generating NPC {Index}/{Total}: {NpcId}", i + 1, result.Rooms.Count, npcId);
                    
                    string npcBio;
                    try
                    {
                        npcBio = _slm.GenerateNpcBio(
                            $"NPC {i+1} in {options.Name} ({options.Theme})", 
                            options.Seed + i);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to generate bio for NPC {i+1} ({npcId}). Error: {ex.Message}", ex);
                    }
                    
                    var npc = new NpcModel
                    {
                        Id = npcId,
                        Name = $"NPC {i+1}",
                        Description = npcBio,
                        FactionId = faction.Id,
                        Hostility = "Neutral",
                        Attributes = new NpcAttributes(),
                        Behavior = "Static",
                        Inventory = new List<string>()
                    };
                    result.Npcs.Add(npc);
                    result.Rooms[i].Npcs.Add(npcId);
                }
                
                _logger?.LogInformation("Successfully generated {Count} NPCs", result.Npcs.Count);
                
                // Step 4: Generate lore
                _logger?.LogInformation("Generating lore entries...");
                try
                {
                    result.LoreEntries = _slm.GenerateLoreEntries(
                        $"World {options.Name} ({options.Theme})", 
                        options.Seed, 
                        3);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to generate lore entries. Error: {ex.Message}", ex);
                }
                
                // Step 5: Create story nodes
                var storyNode = new StoryNodeModel
                {
                    Id = "story1",
                    Title = "The Beginning",
                    Text = "You awaken in a strange place.",
                    Choices = new List<StoryChoice>
                    {
                        new StoryChoice { Label = "Explore", Next = result.Rooms[0].Id, Effects = new List<string>() },
                        new StoryChoice { Label = "Wait", Next = "story2", Effects = new List<string>() }
                    }
                };
                result.StoryNodes.Add(storyNode);
                
                // Step 6: Create world metadata
                result.World = new WorldJsonModel
                {
                    Name = options.Name,
                    Description = $"World generated with seed {options.Seed}",
                    Version = "1.0.0",
                    Author = "SoloAdventureSystem.ContentGenerator",
                    CreatedAt = DateTime.UtcNow,
                    StartLocationId = result.Rooms[0].Id,
                    LocationIds = result.Rooms.ConvertAll(r => r.Id),
                    NpcIds = result.Npcs.ConvertAll(n => n.Id),
                    ItemIds = new List<string>(),
                    FactionIds = result.Factions.ConvertAll(f => f.Id),
                    StoryNodeIds = result.StoryNodes.ConvertAll(s => s.Id)
                };
                
                _logger?.LogInformation("World generation completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "World generation failed: {Message}", ex.Message);
                throw;
            }
        }
    }
}
