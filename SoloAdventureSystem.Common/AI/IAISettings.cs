namespace SoloAdventureSystem.Common.AI
{
    public interface IAISettings
    {
        string Provider { get; }
        string Model { get; }
        string LLamaModelKey { get; }
        int ContextSize { get; }
        bool UseGPU { get; }
        int MaxInferenceThreads { get; }
        string? CacheDirectory { get; }
    }
