using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator.Models
{
    public class RoomModel
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string BaseDescription { get; set; } = "";
        public Dictionary<string, string> Exits { get; set; } = new();
        public List<string> Items { get; set; } = new();
        public List<string> Npcs { get; set; } = new();
        public string VisualPrompt { get; set; } = "";
        public UiPosition UiPosition { get; set; } = new UiPosition();
    }
    public class UiPosition
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
