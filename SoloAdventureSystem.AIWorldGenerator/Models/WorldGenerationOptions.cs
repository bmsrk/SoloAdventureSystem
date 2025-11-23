namespace SoloAdventureSystem.ContentGenerator.Models
{
    public class WorldGenerationOptions
    {
        public string Name { get; set; } = "Test";
        public int Seed { get; set; } = 12345;
        public string Theme { get; set; } = "Default";
        public int Regions { get; set; } = 3;
        public string NpcDensity { get; set; } = "medium";
        public bool RenderImages { get; set; } = false;
        
        // New customization parameters for more diverse world generation
        
        /// <summary>
        /// Atmospheric flavor/mood of the world (e.g., "dark and gritty", "hopeful rebellion", "neon-soaked mystery")
        /// </summary>
        public string Flavor { get; set; } = "Atmospheric and mysterious";
        
        /// <summary>
        /// Brief description of the world setting (e.g., "A sprawling megacity where corporations rule", "Post-apocalyptic wasteland")
        /// </summary>
        public string Description { get; set; } = "A cyberpunk world where technology and humanity collide";
        
        /// <summary>
        /// Main plot point or central conflict (e.g., "A rogue AI threatens the city", "Search for a legendary artifact")
        /// </summary>
        public string MainPlotPoint { get; set; } = "Uncover the conspiracy behind recent disappearances";
        
        /// <summary>
        /// Setting time period or era (e.g., "2089", "Far future", "Alternative 1980s")
        /// </summary>
        public string TimePeriod { get; set; } = "Near future (2077)";
        
        /// <summary>
        /// Dominant factions or power structures (e.g., "Megacorps vs. Street Gangs", "AI Overlords and Human Rebels")
        /// </summary>
        public string PowerStructure { get; set; } = "Corporations, hackers, and underground resistance";
    }
}
