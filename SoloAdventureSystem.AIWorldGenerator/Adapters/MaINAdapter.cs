using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.LLM.Adapters;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// MaINAdapter now acts as a thin bridge to the LLM project. All model/loading
/// and generation logic lives in SoloAdventureSystem.LLM. This class exposes the
/// original ILocalSLMAdapter interface used by the world generator and delegates
/// calls to an injected ILLMAdapter implementation.
/// </summary>
public class MaINAdapter : ILocalSLMAdapter, IDisposable
{
    private readonly ILogger<MaINAdapter>? _logger;
    private readonly ILLMAdapter _llmAdapter;
    private readonly IGenerationPolicy _policy;
    private bool _isInitialized;

    // Primary constructor: accept an ILLMAdapter (preferred for DI)
    public MaINAdapter(ILLMAdapter llmAdapter, IGenerationPolicy? policy = null, ILogger<MaINAdapter>? logger = null)
    {
        _llmAdapter = llmAdapter ?? throw new ArgumentNullException(nameof(llmAdapter));
        _policy = policy ?? new ResilienceGenerationPolicy(logger as ILogger<ResilienceGenerationPolicy>);
        _logger = logger;
    }

    // Backwards-compatible constructor: build internal LlamaEngine + LlamaAdapter from settings
    public MaINAdapter(IOptions<AISettings> settings, ILogger<MaINAdapter>? logger = null, IGenerationPolicy? policy = null, SoloAdventureSystem.ContentGenerator.Parsing.IStructuredOutputParser? parser = null)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));
        _logger = logger;
        _policy = policy ?? new ResilienceGenerationPolicy(logger as ILogger<ResilienceGenerationPolicy>);

        // Create engine and adapter from the LLM project
        var engine = new LlamaEngine(logger: null);
        var llmAdapter = new LlamaAdapter(settings, engine, logger: null, parser: parser);
        _llmAdapter = llmAdapter;
    }

    public async System.Threading.Tasks.Task InitializeAsync(IProgress<SoloAdventureSystem.ContentGenerator.EmbeddedModel.DownloadProgress>? progress = null)
    {
        if (_isInitialized)
        {
            _logger?.LogDebug("? MaIN adapter already initialized (delegating to LLM)");
            return;
        }

        try
        {
            _logger?.LogInformation("?? Initializing MaIN adapter (delegating to LLM)...");
            await _llmAdapter.InitializeAsync(progress);
            _isInitialized = true;
            _logger?.LogInformation("? MaIN adapter (LLM) initialized successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "? Failed to initialize delegated LLM adapter");
            throw;
        }
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            _logger?.LogError("? Adapter not initialized - InitializeAsync() must be called first");
            throw new InvalidOperationException("Adapter not initialized. Call InitializeAsync() before using.");
        }
    }

    private string ExecuteWithPolicy(Func<string> work, string operationName)
    {
        EnsureInitialized();
        try
        {
            return _policy.Execute(work, operationName) ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Generation failed for {OperationName}", operationName);
            // Fall back to direct call
            try
            {
                return work() ?? string.Empty;
            }
            catch (Exception inner)
            {
                _logger?.LogError(inner, "Direct generation fallback failed for {OperationName}", operationName);
                return string.Empty;
            }
        }
    }

    public string GenerateRoomDescription(string context, int seed)
    {
        return ExecuteWithPolicy(() => _llmAdapter.GenerateRoomDescription(context, seed), "RoomDescription");
    }

    public string GenerateNpcBio(string context, int seed)
    {
        return ExecuteWithPolicy(() => _llmAdapter.GenerateNpcBio(context, seed), "NpcBio");
    }

    public string GenerateFactionFlavor(string context, int seed)
    {
        return ExecuteWithPolicy(() => _llmAdapter.GenerateFactionFlavor(context, seed), "FactionFlavor");
    }

    public List<string> GenerateLoreEntries(string context, int seed, int count)
    {
        EnsureInitialized();
        try
        {
            return _llmAdapter.GenerateLoreEntries(context, seed, count);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to generate lore entries via delegated LLM adapter");
            // Best-effort fallback: return list of empty entries
            var fallback = new List<string>();
            for (int i = 0; i < count; i++) fallback.Add(string.Empty);
            return fallback;
        }
    }

    public string GenerateDialogue(string prompt, int seed)
    {
        return ExecuteWithPolicy(() => _llmAdapter.GenerateDialogue(prompt, seed), "Dialogue");
    }

    public string GenerateRaw(string prompt, int seed, int maxTokens = 150)
    {
        EnsureInitialized();
        try
        {
            return _llmAdapter.GenerateRaw(prompt, seed, maxTokens);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to get raw generation from delegated LLM adapter");
            return string.Empty;
        }
    }

    public void Dispose()
    {
        _logger?.LogInformation("?? Disposing MaINAdapter (delegating to LLM)...");
        try
        {
            (_llmAdapter as IDisposable)?.Dispose();
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Error disposing underlying LLM adapter");
        }
        GC.SuppressFinalize(this);
    }
}