using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator.Models
{
    public class WorldProfileModel
    {
        public string Name { get; set; } = "";
        public string Theme { get; set; } = "";
        public string Tone { get; set; } = "";
        public string CoreConflict { get; set; } = "";
    }

    public class HighLevelConceptModel
    {
        public string MainForces { get; set; } = "";
        public string Atmosphere { get; set; } = "";
        public string TechOrMagicLevel { get; set; } = "";
        public string BigIdeas { get; set; } = "";
    }

    public class RegionModel
    {
        public string Name { get; set; } = "";
        public string Biome { get; set; } = "";
        public string Threat { get; set; } = "";
        public string UniqueDetail { get; set; } = "";
        public string AdventureHook { get; set; } = "";
    }

    public class KeyLocationModel
    {
        public string Name { get; set; } = "";
        public string Purpose { get; set; } = "";
        public string Mood { get; set; } = "";
        public string SpecialDetail { get; set; } = "";
        public string PlayerActions { get; set; } = "";
    }

    public class DynamicElementModel
    {
        public string Type { get; set; } = ""; // rumor, event, shift, env
        public string Description { get; set; } = "";
    }

    public class AdventureSeedModel
    {
        public string Premise { get; set; } = "";
        public string Location { get; set; } = "";
        public string Stakes { get; set; } = "";
        public string OpposingForce { get; set; } = "";
        public string Complication { get; set; } = "";
    }
}
