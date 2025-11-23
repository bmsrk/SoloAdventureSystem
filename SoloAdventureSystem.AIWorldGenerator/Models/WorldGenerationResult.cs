using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator.Models
{
    public class WorldGenerationResult
    {
        public WorldJsonModel World { get; set; } = new WorldJsonModel();
        public List<RoomModel> Rooms { get; set; } = new List<RoomModel>();
        public List<FactionModel> Factions { get; set; } = new List<FactionModel>();
        public List<NpcModel> Npcs { get; set; } = new List<NpcModel>();
        public List<StoryNodeModel> StoryNodes { get; set; } = new List<StoryNodeModel>();
        public List<string> LoreEntries { get; set; } = new List<string>();
    }
}
