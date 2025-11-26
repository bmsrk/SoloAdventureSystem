using SoloAdventureSystem.ContentGenerator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoloAdventureSystem.ContentGenerator.Parsing;
using SoloAdventureSystem.LLM.Adapters;
using SoloAdventureSystem.ContentGenerator.EmbeddedModel;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Factory for creating SLM adapters based on configuration.
/// Supports LLamaSharp (embedded AI) via LlamaAdapter from the LLM project.
/// </summary>
public class SLMAdapterFactory
{
    public static ILocalSLMAdapter Create(IServiceProvider services)
    {
        var settings = services.GetRequiredService<IOptions<AISettings>>().Value;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("SLMAdapterFactory");

        logger.LogInformation("Creating SLM adapter for provider: {Provider}", settings.Provider);

        return settings.Provider.ToLowerInvariant() switch
        {
            "llamasharp" or "llama" or "embedded" => CreateLlamaWrappedAdapter(services, settings, loggerFactory),
            _ => throw new InvalidOperationException($"Unknown AI provider: {settings.Provider}. Use 'LLamaSharp'.")
        };
    }

    private static ILocalSLMAdapter CreateLlamaWrappedAdapter(IServiceProvider services, AISettings settings, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("SLMAdapterFactory");

        // Map AISettings from ContentGenerator to LLM.Configuration
        var mapped = new SoloAdventureSystem.LLM.Configuration.AISettings
        {
            Provider = settings.Provider,
            Model = settings.Model,
            LLamaModelKey = settings.LLamaModelKey,
            ContextSize = settings.ContextSize ?? 2048,
            UseGPU = settings.UseGPU,
            MaxInferenceThreads = settings.MaxInferenceThreads
        };

        // If a model key is provided, ensure the model is available locally and update mapped key
        if (!string.IsNullOrWhiteSpace(settings.LLamaModelKey))
        {
            try
            {
                var downloader = new GGUFModelDownloader(loggerFactory.CreateLogger<GGUFModelDownloader>());
                var modelPath = downloader.EnsureModelAvailableAsync(settings.LLamaModelKey, null).GetAwaiter().GetResult();
                if (!string.IsNullOrWhiteSpace(modelPath)) mapped.LLamaModelKey = modelPath;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to ensure model available via GGUFModelDownloader - will attempt to proceed with configured key");
            }
        }

        var engineLogger = loggerFactory.CreateLogger<SoloAdventureSystem.LLM.Adapters.LlamaEngine>();
        var engine = new SoloAdventureSystem.LLM.Adapters.LlamaEngine(engineLogger);

        var llmOptions = Options.Create(mapped);
        var llmLogger = loggerFactory.CreateLogger<SoloAdventureSystem.LLM.Adapters.LlamaAdapter>();

        var llmAdapter = new SoloAdventureSystem.LLM.Adapters.LlamaAdapter(llmOptions, engine, llmLogger, parser: null);

        // Wrap into a local adapter implementation that delegates to the LLM adapter
        var adapter = new LlamaSLMAdapter(llmAdapter, loggerFactory.CreateLogger<LlamaSLMAdapter>());

        // Initialize synchronously for factory consumers
        logger.LogInformation("Initializing Llama-based SLM adapter...");
        adapter.InitializeAsync(null).GetAwaiter().GetResult();

        return adapter;
    }
}
