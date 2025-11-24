using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;

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
        private readonly IContentGenerator<List<string>> _loreGenerator;
        private readonly ILogger<SeededWorldGenerator>? _logger;
        
        /// <summary>
        /// Constructor for dependency injection with specialized generators
        /// </summary>
        public SeededWorldGenerator(
            IContentGenerator<List<FactionModel>> factionGenerator,
            IContentGenerator<List<RoomModel>> roomGenerator,
            RoomConnector roomConnector,
            IContentGenerator<List<NpcModel>> npcGenerator,
            IContentGenerator<List<string>> loreGenerator,
            ILogger<SeededWorldGenerator>? logger = null)
        {
            _factionGenerator = factionGenerator ?? throw new ArgumentNullException(nameof(factionGenerator));
            _roomGenerator = roomGenerator ?? throw new ArgumentNullException(nameof(roomGenerator));
            _roomConnector = roomConnector ?? throw new ArgumentNullException(nameof(roomConnector));
            _npcGenerator = npcGenerator ?? throw new ArgumentNullException(nameof(npcGenerator));
            _loreGenerator = loreGenerator ?? throw new ArgumentNullException(nameof(loreGenerator));
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
            
            // Create specialized generators
            _factionGenerator = new FactionGenerator(slm, logger as ILogger<FactionGenerator>);
            _roomGenerator = new RoomGenerator(slm, image, logger as ILogger<RoomGenerator>);
            _roomConnector = new RoomConnector(logger as ILogger<RoomConnector>);
            _npcGenerator = new NpcGenerator(slm, logger as ILogger<NpcGenerator>);
            _loreGenerator = new LoreGenerator(slm, logger as ILogger<LoreGenerator>);
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
                
                result.LoreEntries = _loreGenerator.Generate(context);
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

            var storyNode = new StoryNodeModel
            {
                Id = "story1",
                Title = "The Beginning",
                Text = $"You awaken in {context.Rooms[0].Title}, disoriented and uncertain how you arrived here. {context.Options.MainPlotPoint}",
                Choices = new List<StoryChoice>
                {
                    new StoryChoice { Label = "Look around", Next = context.Rooms[0].Id, Effects = new List<string>() },
                    new StoryChoice { Label = "Stay still and listen", Next = "story2", Effects = new List<string>() }
                }
            };

            _logger?.LogInformation("? Story nodes generated");
            return new List<StoryNodeModel> { storyNode };
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
