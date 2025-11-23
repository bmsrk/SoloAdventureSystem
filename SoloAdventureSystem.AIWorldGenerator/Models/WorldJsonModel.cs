using System;
using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator.Models
{
    public class WorldJsonModel
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Version { get; set; } = "1.0.0";
        public string Author { get; set; } = "Generator";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string StartLocationId { get; set; } = "";
        public List<string> LocationIds { get; set; } = new List<string>();
        public List<string> NpcIds { get; set; } = new List<string>();
        public List<string> ItemIds { get; set; } = new List<string>();
        public List<string> FactionIds { get; set; } = new List<string>();
        public List<string> StoryNodeIds { get; set; } = new List<string>();
    }
}
