using SoloAdventureSystem.ContentGenerator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Factory for creating SLM adapters based on configuration.
/// Supports MaIN.NET (embedded AI).
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
            "main.net" or "main" or "embedded" => CreateMaINAdapter(services, settings, loggerFactory),
            _ => throw new InvalidOperationException($"Unknown AI provider: {settings.Provider}. Use 'MaIN.NET'.")
        };

        return adapter;
    }

    private static MaINAdapter CreateMaINAdapter(IServiceProvider services, AISettings settings, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<MaINAdapter>();
        // Resolve optional structured output parser if registered
        var parser = services.GetService(typeof(SoloAdventureSystem.ContentGenerator.Parsing.IStructuredOutputParser)) as SoloAdventureSystem.ContentGenerator.Parsing.IStructuredOutputParser;
        var adapter = new MaINAdapter(Options.Create(settings), logger, null, parser);
        
        // Initialize asynchronously (will download model if needed)
        logger.LogInformation("Initializing MaIN.NET adapter...");
        adapter.InitializeAsync().GetAwaiter().GetResult();
        
        return adapter;
    }
}
