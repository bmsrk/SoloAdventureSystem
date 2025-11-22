using System.Collections.Generic;

namespace SoloAdventureSystem.ContentGenerator
{
    public class StubSLMAdapter : ILocalSLMAdapter
    {
        public string GenerateRoomDescription(string context, int seed)
        {
            return $"Room description for '{context}' (seed {seed})";
        }
        public string GenerateNpcBio(string context, int seed)
        {
            return $"NPC bio for '{context}' (seed {seed})";
        }
        public string GenerateFactionFlavor(string context, int seed)
        {
            return $"Faction flavor for '{context}' (seed {seed})";
        }
        public List<string> GenerateLoreEntries(string context, int seed, int count)
        {
            var entries = new List<string>();
            for (int i = 0; i < count; i++)
                entries.Add($"Lore entry {i+1} for '{context}' (seed {seed})");
            return entries;
        }
    }
}
