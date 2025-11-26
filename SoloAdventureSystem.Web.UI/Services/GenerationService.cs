using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoloAdventureSystem.Web.UI.Services
{
    public class GenerationService
    {
        public async IAsyncEnumerable<string> RunGenerationAsync(CancellationToken cancellationToken = default)
        {
            var steps = new[] {
                "Initializing AI Models",
                "Loading World Templates",
                "Generating Base Structure",
                "Creating Room Layouts",
                "Populating NPCs",
                "Establishing Factions",
                "Adding Lore Elements",
                "Connecting Story Nodes",
                "Validating World Integrity",
                "Finalizing World Package"
            };

            foreach (var step in steps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return step;
            }
        }
    }
}