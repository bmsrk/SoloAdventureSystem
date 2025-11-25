using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;
using SoloAdventureSystem.Engine.Rules;

namespace SoloAdventureSystem.ContentGenerator.Generation
{
    /// <summary>
    /// Orchestrator for world generation using specialized content generators.
    /// Refactored to use pipeline pattern with dedicated generators for each content type.
    /// </summary>
    public class SeededWorldGenerator : IWorldGenerator
    {
        private readonly IContentGenerator<List<FactionModel>> _factionGenerator;
        private readonly IContentGenerator<List<RoomModel>> _roomGenerator;
        private readonly RoomConnector _roomConnector;
        private readonly IContentGenerator<List<NpcModel>> _npcGenerator;
        private readonly ILogger<SeededWorldGenerator>? _logger;
        private readonly ILocalSLMAdapter? _slm; // store adapter when provided
        
        /// <summary>
        /// Constructor for dependency injection with specialized generators
        /// </summary>
        public SeededWorldGenerator(
            IContentGenerator<List<FactionModel>> factionGenerator,
            IContentGenerator<List<RoomModel>> roomGenerator,
            RoomConnector roomConnector,
            IContentGenerator<List<NpcModel>> npcGenerator,
            ILogger<SeededWorldGenerator>? logger = null)
        {
            _factionGenerator = factionGenerator ?? throw new ArgumentNullException(nameof(factionGenerator));
            _roomGenerator = roomGenerator ?? throw new ArgumentNullException(nameof(roomGenerator));
            _roomConnector = roomConnector ?? throw new ArgumentNullException(nameof(roomConnector));
            _npcGenerator = npcGenerator ?? throw new ArgumentNullException(nameof(npcGenerator));
            _logger = logger;
        }

        /// <summary>
        /// Legacy constructor for backward compatibility
        /// Creates generators inline
        /// </summary>
        public SeededWorldGenerator(ILocalSLMAdapter slm, IImageAdapter image, ILogger<SeededWorldGenerator>? logger = null)
        {
            if (slm == null) throw new ArgumentNullException(nameof(slm));
            if (image == null) throw new ArgumentNullException(nameof(image));

            _logger = logger;
            _slm = slm;
            
            // Create specialized generators
            _factionGenerator = new FactionGenerator(slm, logger as ILogger<FactionGenerator>);
            _roomGenerator = new RoomGenerator(slm, image, logger as ILogger<RoomGenerator>);
            _roomConnector = new RoomConnector(logger as ILogger<RoomConnector>);
            _npcGenerator = new NpcGenerator(slm, logger as ILogger<NpcGenerator>);
            // Lore generation removed temporarily
        }
        
        public WorldGenerationResult Generate(WorldGenerationOptions options)
        {
            _logger?.LogInformation("Starting enhanced world generation: {Name} (seed: {Seed}, regions: {Regions})", 
                options.Name, options.Seed, options.Regions);
            
            _logger?.LogInformation("World Parameters - Flavor: '{Flavor}', Setting: '{Description}'", 
                options.Flavor, options.Description);
            
            var context = new WorldGenerationContext(options);
            var result = new WorldGenerationResult();
            
            try
            {
                // Pipeline: Generate content in dependency order
                result.Factions = _factionGenerator.Generate(context);
                context.Factions = result.Factions;
                
                result.Rooms = _roomGenerator.Generate(context);
                context.Rooms = result.Rooms;
                
                result.Rooms = _roomConnector.Generate(context);
                
                result.Npcs = _npcGenerator.Generate(context);
                context.Npcs = result.Npcs;
                
                // Lore generation disabled: provide empty list for now
                result.LoreEntries = new List<string>();
                context.LoreEntries = result.LoreEntries;
                
                // Create story nodes based on main plot point
                result.StoryNodes = GenerateStoryNodes(context);
                context.StoryNodes = result.StoryNodes;
                
                // Create world metadata
                result.World = CreateWorldMetadata(result, options);
                
                _logger?.LogInformation("Enhanced world generation completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "World generation failed: {Message}", ex.Message);
                throw;
            }
        }

        private List<StoryNodeModel> GenerateStoryNodes(WorldGenerationContext context)
        {
            _logger?.LogInformation("?? Generating story nodes...");

            // Use DialogueGenerator to create dialogue/story nodes. Pass stored adapter if available so Web UI generation uses LLM-enhanced dialogue
            var dialogueGen = new DialogueGenerator(context, _logger as ILogger<DialogueGenerator>, _slm);

            // Provide a skillSelector that chooses skills based on NPC attributes and behavior
            List<StoryNodeModel> dialogueNodes = dialogueGen.Generate(npc =>
            {
                if (npc == null) return null;
                var skills = new List<Skill> { Skill.Social };
                // High charisma -> Social
                if (npc.Attributes != null && npc.Attributes.Charisma > 12)
                {
                    if (!skills.Contains(Skill.Social)) skills.Insert(0, Skill.Social);
                }
                // Aggressive behavior -> Combat
                if (!string.IsNullOrEmpty(npc.Behavior) && npc.Behavior.Equals("Aggressive", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (!skills.Contains(Skill.Combat)) skills.Add(Skill.Combat);
                }
                // Intelligent NPCs get Knowledge checks
                if (npc.Attributes != null && npc.Attributes.Intelligence > 12)
                {
                    if (!skills.Contains(Skill.Knowledge)) skills.Add(Skill.Knowledge);
                }
                return skills;
            });

             // Return generated nodes
             return dialogueNodes;
         }

        private WorldJsonModel CreateWorldMetadata(WorldGenerationResult result, WorldGenerationOptions options)
        {
            _logger?.LogInformation("?? Creating world metadata...");

            return new WorldJsonModel
            {
                Name = options.Name,
                Description = $"{options.Description} ({options.Flavor}). {options.MainPlotPoint}",
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
        }
    }
}
