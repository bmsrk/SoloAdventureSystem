using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;

namespace SoloAdventureSystem.LLM.Adapters
{
    public class LlamaEngine : ILLMEngine, IDisposable
    {
        private readonly ILogger<LlamaEngine>? _logger;
        private LLamaWeights? _weights;
        private LLamaContext? _context;

        public LlamaEngine(ILogger<LlamaEngine>? logger = null)
        {
            _logger = logger;
        }

        public async Task InitializeAsync(string modelPath, int contextSize, bool useGpu, int maxThreads, IProgress<int>? progress = null)
        {
            try
            {
                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = (uint)contextSize,
                    GpuLayerCount = useGpu ? 99 : 0,
                    UseMemorymap = true,
                    UseMemoryLock = false
                };

                _weights = LLamaWeights.LoadFromFile(parameters);
                _context = _weights.CreateContext(parameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize LLama engine");
                throw;
            }

            await Task.CompletedTask;
        }

        public async Task<string> GenerateAsync(string prompt, int seed, int maxTokens, CancellationToken cancellationToken = default)
        {
            if (_context == null || _weights == null)
                throw new InvalidOperationException("Engine not initialized");

            var executor = new StatelessExecutor(_weights, _context.Params);
            var inferenceParams = new InferenceParams
            {
                MaxTokens = maxTokens,
                AntiPrompts = new List<string> { "\n\n", "###", "<|end|>", "USER:", "ASSISTANT:" }
            };

            var result = new StringBuilder();
            await foreach (var text in executor.InferAsync(prompt, inferenceParams))
            {
                result.Append(text);
            }

            return result.ToString().Trim();
        }

        public string Generate(string prompt, int seed, int maxTokens = 150)
        {
            return GenerateAsync(prompt, seed, maxTokens).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _context?.Dispose();
            _context = null;
            _weights?.Dispose();
            _weights = null;
            GC.SuppressFinalize(this);
        }
    }
}
