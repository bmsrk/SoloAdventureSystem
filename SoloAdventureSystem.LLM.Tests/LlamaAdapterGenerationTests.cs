using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.LLM.Adapters;
using SoloAdventureSystem.LLM.Configuration;
using Xunit;

namespace SoloAdventureSystem.LLM.Tests
{
    public class LlamaAdapterGenerationTests
    {
        private class FakeEngine : ILLMEngine
        {
            public bool Initialized { get; private set; }

            public Task InitializeAsync(string modelPath, int contextSize, bool useGpu, int maxThreads, IProgress<int>? progress = null)
            {
                Initialized = true;
                return Task.CompletedTask;
            }

            public Task<string> GenerateAsync(string prompt, int maxTokens, CancellationToken cancellationToken = default)
            {
                // Not used by adapter in these tests
                return Task.FromResult(Generate(prompt, maxTokens));
            }

            public string Generate(string prompt, int maxTokens = 150)
            {
                // Return JSON when prompt contains special marker to exercise structured parsing
                if (prompt != null && prompt.Contains("STRUCTURED_ROOM", StringComparison.OrdinalIgnoreCase))
                    return "{\"description\":\"Structured Room Description\"}";

                if (prompt != null && prompt.Contains("STRUCTURED_DIALOGUE", StringComparison.OrdinalIgnoreCase))
                    return "[{\"text\":\"Choice A\",\"next\":\"dialogue_npc1_a\"}]";

                // Return unstructured text for fallback path
                if (prompt != null && prompt.Contains("NPC_FALLBACK", StringComparison.OrdinalIgnoreCase))
                    return "  Marcus Chen rose from street hacker to security chief.  ";

                return "Fallback output";
            }

            public void Dispose() { }
        }

        private static LlamaAdapter CreateAdapterWithFakeEngine(FakeEngine engine)
        {
            var settings = Options.Create(new AISettings
            {
                LLamaModelKey = "fake-model-path",
                Model = "tinyllama-q4",
                ContextSize = 2048,
                UseGPU = false,
                MaxInferenceThreads = 1
            });

            var adapter = new LlamaAdapter(settings, engine, logger: null, parser: null);
            adapter.InitializeAsync().GetAwaiter().GetResult();
            return adapter;
        }

        [Fact]
        public void GenerateRoomDescription_ParsesStructuredJson_FromEngineRaw()
        {
            var engine = new FakeEngine();
            var adapter = CreateAdapterWithFakeEngine(engine);

            var result = adapter.GenerateRoomDescription("STRUCTURED_ROOM: provide JSON with description field");

            Assert.Equal("Structured Room Description", result);
        }

        [Fact]
        public void GenerateNpcBio_UsesFallbackAndCleansWhitespace_WhenNotStructured()
        {
            var engine = new FakeEngine();
            var adapter = CreateAdapterWithFakeEngine(engine);

            var result = adapter.GenerateNpcBio("NPC_FALLBACK prompt");

            // Clean should trim and normalize whitespace
            Assert.Equal("Marcus Chen rose from street hacker to security chief.", result);
        }

        [Fact]
        public void GenerateDialogue_ReturnsSerializedList_WhenStructured()
        {
            var engine = new FakeEngine();
            var adapter = CreateAdapterWithFakeEngine(engine);

            var result = adapter.GenerateDialogue("STRUCTURED_DIALOGUE request");

            // Should be JSON string containing the dialogue choice text
            Assert.Contains("Choice A", result);
            Assert.Contains("dialogue_npc1_a", result);
        }
    }
}
