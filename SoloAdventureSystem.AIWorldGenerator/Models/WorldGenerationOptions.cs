namespace SoloAdventureSystem.ContentGenerator
{
    public class WorldGenerationOptions
    {
        public string Name { get; set; } = "Test";
        public int Seed { get; set; } = 12345;
        public string Theme { get; set; } = "Default";
        public int Regions { get; set; } = 3;
        public string NpcDensity { get; set; } = "medium";
        public bool RenderImages { get; set; } = false;
    }
}
