using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator.Models
{
    public class FactionModel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Ideology { get; set; } = "";
        public Dictionary<string, int> Relations { get; set; } = new();
    }
}
