using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;
using SoloAdventureSystem.ContentGenerator.Parsing;
using SoloAdventureSystem.LLM.Adapters;

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

            var modelConfig = _settings.GetModelConfiguration();
            var hardware = _settings.GetHardwareConfiguration();

            var downloader = new SoloAdventureSystem.AIWorldGenerator.EmbeddedModel.GGUFModelDownloader(_logger as ILogger<SoloAdventureSystem.AIWorldGenerator.EmbeddedModel.GGUFModelDownloader>);
            var path = await downloader.EnsureModelAvailableAsync(modelConfig.ModelKey, null);

            await _engine.InitializeAsync(path, modelConfig.ContextSize, hardware.UseGPU, hardware.MaxThreads, progress);

            _initialized = true;
        }

        private void EnsureInit()
        {
            if (!_initialized)
                throw new InvalidOperationException("Adapter not initialized");
        }

        public string GenerateRaw(string prompt, int seed, int maxTokens = 150)
        {
            EnsureInit();
            return _engine.Generate(prompt, seed, maxTokens);
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

        public string GenerateRoomDescription(string context, int seed)
        {
            EnsureInit();
            if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context, seed), out var parsed))
            {
                if (parsed != null && parsed.TryGetValue("description", out var d) && d != null)
                    return d.ToString() ?? string.Empty;
            }

            var raw = _engine.Generate(context, seed, SoloAdventureSystem.AIWorldGenerator.Generation.GenerationLimits.RoomDescriptionTokens);
            return Clean(raw);
        }

        public string GenerateNpcBio(string context, int seed)
        {
            EnsureInit();
            if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context, seed), out var parsed))
            {
                if (parsed != null && parsed.TryGetValue("bio", out var b) && b != null)
                    return b.ToString() ?? string.Empty;
            }
            var raw = _engine.Generate(context, seed, SoloAdventureSystem.AIWorldGenerator.Generation.GenerationLimits.NpcBioTokens);
            return Clean(raw);
        }

        public string GenerateFactionFlavor(string context, int seed)
        {
            EnsureInit();
            if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context, seed), out var parsed))
            {
                if (parsed != null && parsed.TryGetValue("description", out var d) && d != null)
                    return d.ToString() ?? string.Empty;
            }
            var raw = _engine.Generate(context, seed, SoloAdventureSystem.AIWorldGenerator.Generation.GenerationLimits.FactionLoreTokens);
            return Clean(raw);
        }

        public List<string> GenerateLoreEntries(string context, int seed, int count)
        {
            EnsureInit();
            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var itemSeed = seed + i;
                if (TryStructured<Dictionary<string, object>>(() => GenerateRaw(context, itemSeed), out var parsed))
                {
                    if (parsed != null && parsed.TryGetValue("text", out var t) && t != null)
                    {
                        list.Add(t.ToString() ?? string.Empty);
                        continue;
                    }
                }
                var raw = _engine.Generate(context, itemSeed, SoloAdventureSystem.AIWorldGenerator.Generation.GenerationLimits.LoreEntryTokens);
                list.Add(Clean(raw));
            }
            return list;
        }

        public string GenerateDialogue(string prompt, int seed)
        {
            EnsureInit();
            if (TryStructured<List<Dictionary<string, object>>>(() => GenerateRaw(prompt, seed), out var parsedList))
            {
                if (parsedList != null)
                {
                    return JsonSerializer.Serialize(parsedList);
                }
            }
            var raw = _engine.Generate(prompt, seed, SoloAdventureSystem.AIWorldGenerator.Generation.GenerationLimits.DialogueTokens);
            return Clean(raw);
        }

        private string Clean(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            // simple placeholder: trim and collapse whitespace
            var trimmed = input.Trim();
            trimmed = System.Text.RegularExpressions.Regex.Replace(trimmed, "\\s+", " ");
            return trimmed;
        }

        public void Dispose()
        {
            (_engine as IDisposable)?.Dispose();
        }
    }
}
