using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator.Models
{
    // DTOs for parsing model output JSON into intermediate types
    public class ChoiceDto
    {
        public string? label { get; set; }
        public string? text { get; set; }
        public string? option { get; set; }
        public string? next { get; set; }
        public string? nextSuffix { get; set; }
        public SkillCheckDto? skill_check { get; set; }
        public List<string>? effects { get; set; }
    }

    public class SkillCheckDto
    {
        public string? attribute { get; set; }
        public string? skill { get; set; }
        public int target_number { get; set; }
    }
}
