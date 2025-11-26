using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.LLM.Adapters;
using SoloAdventureSystem.LLM.Configuration;
using SoloAdventureSystem.LLM.Parsing;

namespace SoloAdventureSystem.LLM.Adapters
{
    public class LlamaAdapter : ILLMAdapter, IDisposable
    {
        private readonly ILogger<LlamaAdapter>? _logger;
        private readonly AISettings _settings;
        private readonly ILLMEngine _engine;
        private readonly IStructuredOutputParser _parser;
        private bool _initialized;

        public LlamaAdapter(IOptions<AISettings> settings, ILLMEngine engine, ILogger<LlamaAdapter>? logger = null, IStructuredOutputParser? parser = null)
        {
            _logger = logger;
            _settings = settings.Value;
            _engine = engine;
            _parser = parser ?? new JsonStructuredOutputParser();
        }

        public async System.Threading.Tasks.Task InitializeAsync(System.IProgress<int>? progress = null)
        {
            if (_initialized) return;

            // Expect settings.LLamaModelKey to be the local model path or model key.
            var modelPath = _settings.LLamaModelKey ?? string.Empty;
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                throw new InvalidOperationException("LLama model path/key not configured in AISettings.LLamaModelKey");
            }

            // Initialize engine with provided model path and hardware settings
            var contextSize = _settings.ContextSize ?? 2048;
            await _engine.InitializeAsync(modelPath, contextSize, _settings.UseGPU, _settings.MaxInferenceThreads, progress);

            _initialized = true;
        }

        private void EnsureInit()
        {
            if (!_initialized)
                throw new InvalidOperationException("Adapter not initialized");
        }

        public string GenerateRaw(string prompt, int maxTokens = 150)
        {
            EnsureInit();
            return _engine.Generate(prompt, maxTokens);
        }

        private bool TryStructured<T>(Func<string> genFunc, out T? parsed)
        {
            parsed = default;
            try
            {
                var raw = genFunc();
                if (string.IsNullOrWhiteSpace(raw)) return false;
                if (_parser.TryParse<T>(raw, out var p))
                {
                    parsed = p;
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Structured parse attempt failed");
            }
            return false;
        }

        // Token limits used directly to avoid dependency on AIWorldGenerator project
        private const int RoomDescriptionTokens = 180;
        private const int NpcBioTokens = 150;
        private const int FactionLoreTokens = 160;
        private const int LoreEntryTokens = 120;
        private const int DialogueTokens = 200;

        public string GenerateRoomDescription(string context)
        {
            EnsureInit();
            if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context), out var parsed))
            {
                if (parsed != null && parsed.TryGetValue("description", out var d) && d != null)
                    return d.ToString() ?? string.Empty;
            }

            var raw = _engine.Generate(context, RoomDescriptionTokens);
            return Clean(raw);
        }

        public string GenerateNpcBio(string context)
        {
            EnsureInit();
            if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context), out var parsed))
            {
                if (parsed != null && parsed.TryGetValue("bio", out var b) && b != null)
                    return b.ToString() ?? string.Empty;
            }
            var raw = _engine.Generate(context, NpcBioTokens);
            return Clean(raw);
        }

        public string GenerateFactionFlavor(string context)
        {
            EnsureInit();
            if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context), out var parsed))
            {
                if (parsed != null && parsed.TryGetValue("description", out var d) && d != null)
                    return d.ToString() ?? string.Empty;
            }
            var raw = _engine.Generate(context, FactionLoreTokens);
            return Clean(raw);
        }

        public List<string> GenerateLoreEntries(string context, int count)
        {
            EnsureInit();
            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context), out var parsed))
                {
                    if (parsed != null && parsed.TryGetValue("text", out var t) && t != null)
                    {
                        list.Add(t.ToString() ?? string.Empty);
                        continue;
                    }
                }
                var raw = _engine.Generate(context, LoreEntryTokens);
                list.Add(Clean(raw));
            }
            return list;
        }

        public string GenerateDialogue(string prompt)
        {
            EnsureInit();
            if (TryStructured<List<Dictionary<string, object>>>(() => GenerateRaw(prompt), out var parsedList))
            {
                if (parsedList != null)
                {
                    return JsonSerializer.Serialize(parsedList);
                }
            }
            var raw = _engine.Generate(prompt, DialogueTokens);
            return Clean(raw);
        }

        private static string Clean(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();
            trimmed = System.Text.RegularExpressions.Regex.Replace(trimmed, "\\s+", " ");
            return trimmed;
        }

        public void Dispose()
        {
            (_engine as IDisposable)?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
