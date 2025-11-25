using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator.Models
{
    public class NpcModel
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string FactionId { get; set; } = "";
        public string Hostility { get; set; } = "Neutral";
        public NpcAttributes Attributes { get; set; } = new NpcAttributes();
        public string Behavior { get; set; } = "Static";
        public List<string> Inventory { get; set; } = new();

        // New: guidance for dialogue generation
        public string Alignment { get; set; } = "Neutral"; // e.g., Ambitious, Loyal, Paranoid
        public string Motivation { get; set; } = "Maintain status quo"; // short description of driving motive
    }
    public class NpcAttributes
    {
        public int Strength { get; set; } = 10;
        public int Dexterity { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public int Constitution { get; set; } = 10;
        public int Wisdom { get; set; } = 10;
        public int Charisma { get; set; } = 10;
    }
}
