using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoloAdventureSystem.LLM.Adapters;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Thin adapter implementing `ILocalSLMAdapter` that delegates to an `ILLMAdapter`.
/// This replaces MaINAdapter in the AIWorldGenerator project to avoid cross-project coupling.
/// </summary>
public class LlamaSLMAdapter : ILocalSLMAdapter, IDisposable
{
    private readonly ILLMAdapter _llmAdapter;
    private readonly ILogger<LlamaSLMAdapter>? _logger;
    private bool _initialized;

    public LlamaSLMAdapter(ILLMAdapter llmAdapter, ILogger<LlamaSLMAdapter>? logger = null)
    {
        _llmAdapter = llmAdapter ?? throw new ArgumentNullException(nameof(llmAdapter));
        _logger = logger;
    }

    public async System.Threading.Tasks.Task InitializeAsync(IProgress<DownloadProgress>? progress = null)
    {
        if (_initialized) return;
        _logger?.LogInformation("Initializing LlamaSLMAdapter (delegating to LLM)");
        // ILLMAdapter uses IProgress<int>? for progress. We don't have a compatible translator here,
        // so pass null to the underlying adapter to avoid type mismatch and keep behavior simple.
        await _llmAdapter.InitializeAsync(null);
        _initialized = true;
    }

    private void EnsureInitialized()
    {
        if (!_initialized) throw new InvalidOperationException("Adapter not initialized. Call InitializeAsync() before use.");
    }

    public string GenerateRoomDescription(string context, int seed)
    {
        EnsureInitialized();
        return _llmAdapter.GenerateRoomDescription(context, seed);
    }

    public string GenerateNpcBio(string context, int seed)
    {
        EnsureInitialized();
        return _llmAdapter.GenerateNpcBio(context, seed);
    }

    public string GenerateFactionFlavor(string context, int seed)
    {
        EnsureInitialized();
        return _llmAdapter.GenerateFactionFlavor(context, seed);
    }

    public List<string> GenerateLoreEntries(string context, int seed, int count)
    {
        EnsureInitialized();
        return _llmAdapter.GenerateLoreEntries(context, seed, count);
    }

    public string GenerateDialogue(string prompt, int seed)
    {
        EnsureInitialized();
        return _llmAdapter.GenerateDialogue(prompt, seed);
    }

    public string GenerateRaw(string prompt, int seed, int maxTokens = 150)
    {
        EnsureInitialized();
        return _llmAdapter.GenerateRaw(prompt, seed, maxTokens);
    }

    public void Dispose()
    {
        (_llmAdapter as IDisposable)?.Dispose();
        GC.SuppressFinalize(this);
    }
}
