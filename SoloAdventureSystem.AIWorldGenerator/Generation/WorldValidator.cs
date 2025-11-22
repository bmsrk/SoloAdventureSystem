using System;
using System.Linq;

namespace SoloAdventureSystem.ContentGenerator
{
    public class WorldValidator
    {
        public void Validate(WorldGenerationResult result)
        {
            if (result.World == null)
                throw new Exception("Missing world.json");
            if (result.Rooms == null || result.Rooms.Count < 3)
                throw new Exception("At least 3 rooms are required.");
            if (result.Factions == null || result.Factions.Count < 1)
                throw new Exception("At least 1 faction is required.");
            if (result.StoryNodes == null || result.StoryNodes.Count < 1)
                throw new Exception("At least 1 story node is required.");
        }
    }
}
