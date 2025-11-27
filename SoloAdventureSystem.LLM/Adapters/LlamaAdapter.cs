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

        private bool TryStructured<T>(Func<string> genFunc, out T? parsed, int attempts = 3)
        {
            parsed = default;
            if (attempts < 1) attempts = 1;

            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    var raw = genFunc();
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    if (_parser.TryParse<T>(raw, out var p))
                    {
                        parsed = p;
                        if (i > 0) _logger?.LogDebug("Structured parse succeeded after {Attempt} attempts", i + 1);
                        return true;
                    }

                    // If parser failed, log debug and retry
                    _logger?.LogDebug("Structured parse attempt {Attempt} failed. Raw length={Len}", i + 1, raw.Length);
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "Structured parse attempt threw");
                }
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

            if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context, RoomDescriptionTokens), out var parsed, attempts: 3))
            {
                if (parsed != null && parsed.TryGetValue("description", out var d) && d != null)
                    return d.ToString() ?? string.Empty;
            }

            // If structured failed, retry a few times still but return empty if no structured output
            for (int i = 0; i < 2; i++)
            {
                var rawTry = GenerateRaw(context, RoomDescriptionTokens);
                if (TryStructured<Dictionary<string, object>>(() => rawTry, out parsed, attempts:1))
                {
                    if (parsed != null && parsed.TryGetValue("description", out var d) && d != null)
                        return d.ToString() ?? string.Empty;
                }
            }

            _logger?.LogWarning("Structured room generation failed after retries; returning cleaned free-form text.");
            var raw = _engine.Generate(context, RoomDescriptionTokens);
            return Clean(raw);
        }

        public string GenerateNpcBio(string context)
        {
            EnsureInit();

            if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context, NpcBioTokens), out var parsed, attempts: 3))
            {
                if (parsed != null && parsed.TryGetValue("bio", out var b) && b != null)
                    return b.ToString() ?? string.Empty;
            }

            for (int i = 0; i < 2; i++)
            {
                var rawTry = GenerateRaw(context, NpcBioTokens);
                if (TryStructured<Dictionary<string, object>>(() => rawTry, out parsed, attempts:1))
                {
                    if (parsed != null && parsed.TryGetValue("bio", out var b) && b != null)
                        return b.ToString() ?? string.Empty;
                }
            }

            _logger?.LogWarning("Structured NPC generation failed after retries; returning cleaned free-form text.");
            var raw = _engine.Generate(context, NpcBioTokens);
            return Clean(raw);
        }

        public string GenerateFactionFlavor(string context)
        {
            EnsureInit();

            if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context, FactionLoreTokens), out var parsed, attempts: 3))
            {
                if (parsed != null && parsed.TryGetValue("description", out var d) && d != null)
                    return d.ToString() ?? string.Empty;
            }

            for (int i = 0; i < 2; i++)
            {
                var rawTry = GenerateRaw(context, FactionLoreTokens);
                if (TryStructured<Dictionary<string, object>>(() => rawTry, out parsed, attempts:1))
                {
                    if (parsed != null && parsed.TryGetValue("description", out var d) && d != null)
                        return d.ToString() ?? string.Empty;
                }
            }

            _logger?.LogWarning("Structured faction generation failed after retries; returning cleaned free-form text.");
            var raw = _engine.Generate(context, FactionLoreTokens);
            return Clean(raw);
        }

        public List<string> GenerateLoreEntries(string context, int count)
        {
            EnsureInit();
            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context, LoreEntryTokens), out var parsed, attempts: 3))
                {
                    if (parsed != null && parsed.TryGetValue("text", out var t) && t != null)
                    {
                        list.Add(t.ToString() ?? string.Empty);
                        continue;
                    }
                }

                var success = false;
                for (int r = 0; r < 2; r++)
                {
                    var rawTry = GenerateRaw(context, LoreEntryTokens);
                    if (TryStructured<Dictionary<string, object>>(() => rawTry, out parsed, attempts:1))
                    {
                        if (parsed != null && parsed.TryGetValue("text", out var t) && t != null)
                        {
                            list.Add(t.ToString() ?? string.Empty);
                            success = true;
                            break;
                        }
                    }
                }

                if (success) continue;

                _logger?.LogWarning("Structured lore entry generation failed for entry {Index}; using cleaned fallback.", i);
                var raw = _engine.Generate(context, LoreEntryTokens);
                list.Add(Clean(raw));
            }
            return list;
        }

        public string GenerateDialogue(string prompt)
        {
            EnsureInit();
            if (TryStructured<List<Dictionary<string, object>>>(() => GenerateRaw(prompt, DialogueTokens), out var parsedList, attempts: 3))
            {
                if (parsedList != null)
                {
                    return JsonSerializer.Serialize(parsedList);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                var rawTry = GenerateRaw(prompt, DialogueTokens);
                if (TryStructured<List<Dictionary<string, object>>>(() => rawTry, out parsedList, attempts:1))
                {
                    if (parsedList != null)
                    {
                        return JsonSerializer.Serialize(parsedList);
                    }
                }
            }

            _logger?.LogWarning("Structured dialogue generation failed after retries; returning cleaned free-form text.");
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
