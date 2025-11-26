using System;
using Xunit;
using SoloAdventureSystem.LLM.Adapters;
using Microsoft.Extensions.Options;

namespace SoloAdventureSystem.LLM.Tests;

public class LlamaAdapterTests
{
    [Fact]
    public void StubAdapter_ProducesPredictableOutput()
    {
        var settings = Options.Create(new SoloAdventureSystem.LLM.Configuration.AISettings
        {
            Provider = "LLamaSharp",
            Model = "tinyllama-q4",
            LLamaModelKey = "tinyllama-q4",
            ContextSize = 2048,
            UseGPU = false,
            MaxInferenceThreads = 1
        });

        var stub = new TestStubAdapter();
        stub.InitializeAsync().GetAwaiter().GetResult();

        var room = stub.GenerateRoomDescription("A test room");
        Assert.Contains("MOCK", room);
    }
}

public class TestStubAdapter : ILLMAdapter, IDisposable
{
    public System.Threading.Tasks.Task InitializeAsync(System.IProgress<int>? progress = null) => System.Threading.Tasks.Task.CompletedTask;
    public string GenerateRoomDescription(string context) => $"[MOCK ROOM] {context}";
    public string GenerateNpcBio(string context) => $"[MOCK NPC] {context}";
    public string GenerateFactionFlavor(string context) => $"[MOCK FACTION] {context}";
    public System.Collections.Generic.List<string> GenerateLoreEntries(string context, int count) { var list = new System.Collections.Generic.List<string>(); for (int i =0;i<count;i++) list.Add($"[MOCK LORE {i}] {context}"); return list; }
    public string GenerateDialogue(string prompt) => $"[MOCK DIALOGUE] {prompt}";
    public string GenerateRaw(string prompt, int maxTokens = 150) => $"[MOCK RAW] {prompt}";
    public void Dispose() {}
}
