using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Generation;

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
            _logger?.LogInformation("Starting enhanced world generation: {Name} (seed: {Seed}, regions: {Regions})", 
                options.Name, options.Seed, options.Regions);
            
            var rand = new Random(options.Seed);
            var result = new WorldGenerationResult();
            
            try
            {
                // Step 1: Generate faction first (needed for NPCs)
                _logger?.LogInformation("Generating faction...");
                var factionName = ProceduralNames.GenerateFactionName(options.Seed);
                string factionDescription;
                try
                {
                    var factionPrompt = PromptTemplates.BuildFactionPrompt(factionName, options.Theme, options.Seed);
                    factionDescription = _slm.GenerateFactionFlavor(
                        factionPrompt, 
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
                    Name = factionName,
                    Description = factionDescription,
                    Ideology = "Neutral",
                    Relations = new Dictionary<string, int>()
                };
                result.Factions.Add(faction);
                
                // Step 2: Generate rooms with enhanced names and descriptions
                var roomCount = Math.Max(3, options.Regions);
                _logger?.LogInformation("Generating {Count} rooms with enhanced quality...", roomCount);
                
                for (int i = 0; i < roomCount; i++)
                {
                    var roomId = $"room{i+1}";
                    var roomName = ProceduralNames.GenerateRoomName(options.Seed + i);
                    var atmosphere = ProceduralNames.GenerateAtmosphere(options.Seed + i);
                    
                    _logger?.LogDebug("Generating room {Index}/{Total}: {RoomName}", i + 1, roomCount, roomName);
                    
                    string description;
                    try
                    {
                        var roomPrompt = PromptTemplates.BuildRoomPrompt(
                            roomName, 
                            options.Theme, 
                            atmosphere, 
                            i, 
                            roomCount);
                        description = _slm.GenerateRoomDescription(
                            roomPrompt, 
                            options.Seed + i);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to generate description for room {i+1} ({roomName}). " +
                            $"Error: {ex.Message}", ex);
                    }
                    
                    string visualPrompt;
                    try
                    {
                        visualPrompt = _image.GenerateImagePrompt(
                            $"{roomName} in {options.Name} - {options.Theme}", 
                            options.Seed + i);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning("Failed to generate image prompt for {RoomName}, using fallback: {Message}", 
                            roomName, ex.Message);
                        visualPrompt = $"A {roomName.ToLower()} in {options.Name}";
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
                        UiPosition = new UiPosition { X = i % 3, Y = i / 3 }
                    };
                    result.Rooms.Add(room);
                }
                
                _logger?.LogInformation("Successfully generated {Count} rooms", result.Rooms.Count);
                
                // Step 3: Create better room connections (not just linear)
                ConnectRooms(result.Rooms, rand);
                
                // Step 4: Generate NPCs with enhanced names and bios
                _logger?.LogInformation("Generating {Count} NPCs with enhanced personalities...", result.Rooms.Count);
                for (int i = 0; i < result.Rooms.Count; i++)
                {
                    var npcId = $"npc{i+1}";
                    var npcName = ProceduralNames.GenerateNpcName(options.Seed + i + 1000);
                    
                    _logger?.LogDebug("Generating NPC {Index}/{Total}: {NpcName}", i + 1, result.Rooms.Count, npcName);
                    
                    string npcBio;
                    try
                    {
                        var npcPrompt = PromptTemplates.BuildNpcPrompt(
                            npcName,
                            options.Theme,
                            result.Rooms[i].Title,
                            faction.Name);
                        npcBio = _slm.GenerateNpcBio(
                            npcPrompt, 
                            options.Seed + i + 1000);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to generate bio for NPC {i+1} ({npcName}). Error: {ex.Message}", ex);
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
                    result.Npcs.Add(npc);
                    result.Rooms[i].Npcs.Add(npcId);
                }
                
                _logger?.LogInformation("Successfully generated {Count} NPCs", result.Npcs.Count);
                
                // Step 5: Generate enhanced lore entries
                _logger?.LogInformation("Generating lore entries with improved quality...");
                try
                {
                    var loreEntries = new List<string>();
                    for (int i = 0; i < 3; i++)
                    {
                        var lorePrompt = PromptTemplates.BuildLorePrompt(options.Name, options.Theme, i + 1);
                        var entry = _slm.GenerateLoreEntries(lorePrompt, options.Seed + i, 1)[0];
                        loreEntries.Add(entry);
                    }
                    result.LoreEntries = loreEntries;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to generate lore entries. Error: {ex.Message}", ex);
                }
                
                // Step 6: Create story nodes
                var storyNode = new StoryNodeModel
                {
                    Id = "story1",
                    Title = "The Beginning",
                    Text = $"You awaken in {result.Rooms[0].Title}, disoriented and uncertain how you arrived here.",
                    Choices = new List<StoryChoice>
                    {
                        new StoryChoice { Label = "Look around", Next = result.Rooms[0].Id, Effects = new List<string>() },
                        new StoryChoice { Label = "Stay still and listen", Next = "story2", Effects = new List<string>() }
                    }
                };
                result.StoryNodes.Add(storyNode);
                
                // Step 7: Create world metadata
                result.World = new WorldJsonModel
                {
                    Name = options.Name,
                    Description = $"A {options.Theme} world generated with seed {options.Seed}",
                    Version = "1.0.0",
                    Author = "SoloAdventureSystem (Enhanced Generator)",
                    CreatedAt = DateTime.UtcNow,
                    StartLocationId = result.Rooms[0].Id,
                    LocationIds = result.Rooms.ConvertAll(r => r.Id),
                    NpcIds = result.Npcs.ConvertAll(n => n.Id),
                    ItemIds = new List<string>(),
                    FactionIds = result.Factions.ConvertAll(f => f.Id),
                    StoryNodeIds = result.StoryNodes.ConvertAll(s => s.Id)
                };
                
                _logger?.LogInformation("Enhanced world generation completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "World generation failed: {Message}", ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Creates better room connections than simple linear chain.
        /// Creates a more interconnected graph structure.
        /// </summary>
        private void ConnectRooms(List<RoomModel> rooms, Random rand)
        {
            if (rooms.Count < 2) return;
            
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
                }
                
                // Try to add a shortcut connection
                if (rand.Next(100) < 20 && i + 2 < rooms.Count && !rooms[i].Exits.ContainsKey("south"))
                {
                    rooms[i].Exits["south"] = rooms[i + 2].Id;
                    rooms[i + 2].Exits["north"] = rooms[i].Id;
                }
            }
        }
    }
}
