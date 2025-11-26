using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Models;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;

namespace SoloAdventureSystem.AIWorldGeneration.Tests;

public class WorldGenerationTests
{
    [Fact]
    public void SeededGenerator_WithStubAdapter_GeneratesWorldStructure()
    {
        var services = new ServiceCollection();
        services.Configure<AISettings>(opts => {
            opts.Provider = "Stub";
            opts.Model = "stub";
            opts.LLamaModelKey = "stub";
        });

        var provider = services.BuildServiceProvider();
        var settings = provider.GetRequiredService<IOptions<AISettings>>();

        var stubAdapter = new TestStubAdapter();
        var generator = new SeededWorldGenerator(stubAdapter, null, null);

        var options = new WorldGenerationOptions { Name = "T", Regions = 1 };

        var world = generator.Generate(options);

        Assert.NotNull(world);
        Assert.NotNull(world.Rooms);
    }
}

// Local deterministic test stub implementing ILocalSLMAdapter to avoid cross-project test dependencies
internal class TestStubAdapter : ILocalSLMAdapter, IDisposable
{
    public System.Threading.Tasks.Task InitializeAsync(IProgress<DownloadProgress>? progress = null)
    {
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public string GenerateRoomDescription(string context, int seed) => $"[MOCK ROOM {seed}] {context}";
    public string GenerateNpcBio(string context, int seed) => $"[MOCK NPC {seed}] {context}";
    public string GenerateFactionFlavor(string context, int seed) => $"[MOCK FACTION {seed}] {context}";
    public System.Collections.Generic.List<string> GenerateLoreEntries(string context, int seed, int count)
    {
        var list = new System.Collections.Generic.List<string>();
        for (int i = 0; i < count; i++) list.Add($"[MOCK LORE {seed + i}] {context}");
        return list;
    }

    public string GenerateDialogue(string prompt, int seed) => $"[MOCK DIALOGUE {seed}] {prompt}";

    public string GenerateRaw(string prompt, int seed, int maxTokens = 150) => $"[MOCK RAW {seed}] {prompt}";

    public void Dispose() { }
}
