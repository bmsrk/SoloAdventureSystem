using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.LLM.Adapters;
using SoloAdventureSystem.LLM.Configuration;
using Xunit;

namespace SoloAdventureSystem.LLM.Tests
{
    /// <summary>
    /// Integration tests that exercise the real LLama engine + adapter.
    /// Tests are skipped automatically when a model file is not available.
    /// Set environment variable `LLAMA_MODEL_PATH` to point to a .gguf model file
    /// or place a model at '%APPDATA%/SoloAdventureSystem/models/tinyllama-q4.gguf'.
    /// </summary>
    public class LlamaAdapterIntegrationTests : IDisposable
    {
        private readonly LlamaEngine? _engine;
        private readonly LlamaAdapter? _adapter;

        public LlamaAdapterIntegrationTests()
        {
            var modelPath = Environment.GetEnvironmentVariable("LLAMA_MODEL_PATH");
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                modelPath = Path.Combine(appData, "SoloAdventureSystem", "models", "tinyllama-q4.gguf");
                if (!File.Exists(modelPath) && !Directory.Exists(modelPath))
                {
                    // No model present; tests will be no-ops in each test method
                    _engine = null;
                    _adapter = null;
                    return;
                }
            }

            // If model exists, initialize engine and adapter
            _engine = new LlamaEngine();
            try
            {
                // Initialize engine (may be slow)
                _engine.InitializeAsync(modelPath!, 2048, useGpu: false, maxThreads: 1).GetAwaiter().GetResult();

                var settings = Options.Create(new AISettings
                {
                    LLamaModelKey = modelPath,
                    Model = "tinyllama-q4",
                    ContextSize = 2048,
                    UseGPU = false,
                    MaxInferenceThreads = 1
                });

                _adapter = new LlamaAdapter(settings, _engine, logger: null, parser: null);
                _adapter.InitializeAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // ensure cleanup on initialization failure
                _adapter?.Dispose();
                _engine?.Dispose();
                throw;
            }
        }

        private bool HasModel() => _engine != null && _adapter != null;

        [Fact]
        public void Integration_GenerateRoomDescription_NotEmpty()
        {
            if (!HasModel())
            {
                Console.WriteLine("Skipping integration test: LLama model not found. Set LLAMA_MODEL_PATH or place tinyllama-q4.gguf in AppData to run integration tests.");
                return;
            }

            var prompt = "A dimly lit cyberpunk bar with neon signs and humming servers";
            var result = _adapter!.GenerateRoomDescription(prompt);

            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void Integration_GenerateNpcBio_NotEmpty()
        {
            if (!HasModel())
            {
                Console.WriteLine("Skipping integration test: LLama model not found. Set LLAMA_MODEL_PATH or place tinyllama-q4.gguf in AppData to run integration tests.");
                return;
            }

            var prompt = "Generate a 2-sentence bio for a skilled hacker named Zero";
            var result = _adapter!.GenerateNpcBio(prompt);

            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void Integration_GenerateRaw_ReturnsText()
        {
            if (!HasModel())
            {
                Console.WriteLine("Skipping integration test: LLama model not found. Set LLAMA_MODEL_PATH or place tinyllama-q4.gguf in AppData to run integration tests.");
                return;
            }

            var raw = _adapter!.GenerateRaw("Say hello to the test runner", 50);
            Assert.False(string.IsNullOrWhiteSpace(raw));
        }

        public void Dispose()
        {
            try { _adapter?.Dispose(); } catch { }
            try { _engine?.Dispose(); } catch { }
            GC.SuppressFinalize(this);
        }
    }
}
