namespace SoloAdventureSystem.LLM.Configuration;

public class AISettings
{
    public string Provider { get; set; } = "LLamaSharp";
    public string Model { get; set; } = "tinyllama-q4";
    public string LLamaModelKey { get; set; } = "tinyllama-q4";
    public int? ContextSize { get; set; } = 2048;
    public bool UseGPU { get; set; }
    public int MaxInferenceThreads { get; set; } = 4;
    public bool SuppressNativeConsoleOutput { get; set; } = true;
}
