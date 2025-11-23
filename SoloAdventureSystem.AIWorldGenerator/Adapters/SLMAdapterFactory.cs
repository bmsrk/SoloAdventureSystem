using SoloAdventureSystem.ContentGenerator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Factory for creating SLM adapters based on configuration.
/// Supports LLamaSharp (embedded AI).
/// </summary>
public class SLMAdapterFactory
{
    public static ILocalSLMAdapter Create(IServiceProvider services)
    {
        var settings = services.GetRequiredService<IOptions<AISettings>>().Value;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("SLMAdapterFactory");
        
        logger.LogInformation("Creating SLM adapter for provider: {Provider}", settings.Provider);

        ILocalSLMAdapter adapter = settings.Provider.ToLowerInvariant() switch
        {
            "llamasharp" or "llama" or "embedded" => CreateLLamaSharpAdapter(services, settings, loggerFactory),
            _ => throw new InvalidOperationException($"Unknown AI provider: {settings.Provider}. Use 'LLamaSharp'.")
        };

        return adapter;
    }

    private static LLamaSharpAdapter CreateLLamaSharpAdapter(IServiceProvider services, AISettings settings, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<LLamaSharpAdapter>();
        var adapter = new LLamaSharpAdapter(Options.Create(settings), logger);
        
        // Initialize asynchronously (will download model if needed)
        logger.LogInformation("Initializing LLamaSharp adapter...");
        adapter.InitializeAsync().GetAwaiter().GetResult();
        
        return adapter;
    }
}
