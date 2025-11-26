using System.Threading.Tasks;
using System.Collections.Generic;

namespace SoloAdventureSystem.LLM.Adapters
{
    public interface ILLMAdapter
    {
        System.Threading.Tasks.Task InitializeAsync(System.IProgress<int>? progress = null);

        string GenerateRoomDescription(string context, int seed);

        string GenerateNpcBio(string context, int seed);

        string GenerateFactionFlavor(string context, int seed);

        List<string> GenerateLoreEntries(string context, int seed, int count);

        string GenerateDialogue(string prompt, int seed);

        string GenerateRaw(string prompt, int seed, int maxTokens = 150);
    }
}
