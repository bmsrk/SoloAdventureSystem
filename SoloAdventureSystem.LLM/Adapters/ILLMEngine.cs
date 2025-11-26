using System.Threading;
using System.Threading.Tasks;

namespace SoloAdventureSystem.LLM.Adapters
{
    public interface ILLMEngine
    {
        Task InitializeAsync(string modelPath, int contextSize, bool useGpu, int maxThreads, IProgress<int>? progress = null);
        Task<string> GenerateAsync(string prompt, int maxTokens, CancellationToken cancellationToken = default);
        string Generate(string prompt, int maxTokens = 150);
        void Dispose();
    }
}
