using System.Threading.Tasks;
using System.Collections.Generic;

namespace SoloAdventureSystem.LLM.Adapters
{
    public interface ILLMAdapter
    {
        System.Threading.Tasks.Task InitializeAsync(System.IProgress<int>? progress = null);

        string GenerateRoomDescription(string context);

        string GenerateNpcBio(string context);

        string GenerateFactionFlavor(string context);

        List<string> GenerateLoreEntries(string context, int count);

        string GenerateDialogue(string prompt);

        string GenerateRaw(string prompt, int maxTokens = 150);
    }
}
