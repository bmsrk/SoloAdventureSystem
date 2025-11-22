using SoloAdventureSystem.ContentGenerator.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Factory for creating SLM adapters based on configuration.
/// Creates fresh adapters on each call to support runtime settings changes.
/// </summary>
public class SLMAdapterFactory
{
    public static ILocalSLMAdapter Create(IServiceProvider services)
    {
        var settings = services.GetRequiredService<IOptions<AISettings>>().Value;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("SLMAdapterFactory");
        
        logger.LogInformation("Creating SLM adapter for provider: {Provider} with model: {Model}", 
            settings.Provider, settings.Model);

        ILocalSLMAdapter baseAdapter = settings.Provider.ToLowerInvariant() switch
        {
            "stub" => new StubSLMAdapter(),
            "githubmodels" or "github" => CreateGitHubAdapter(services, settings, loggerFactory),
            "azureopenai" or "azure" => CreateGitHubAdapter(services, settings, loggerFactory), // Uses GitHub Models endpoint
            "openai" => CreateOpenAIAdapter(services, settings, loggerFactory),
            "groq" => CreateGroqAdapter(services, settings, loggerFactory),
            _ => throw new InvalidOperationException($"Unknown AI provider: {settings.Provider}")
        };

        // Wrap with caching if enabled
        if (settings.EnableCaching && settings.Provider.ToLowerInvariant() != "stub")
        {
            logger.LogInformation("Enabling caching for adapter");
            var cachedAdapter = new CachedSLMAdapter(
                baseAdapter,
                services.GetRequiredService<IOptions<AISettings>>(),
                services.GetRequiredService<ILogger<CachedSLMAdapter>>());
            return cachedAdapter;
        }

        return baseAdapter;
    }

    private static GitHubModelsAdapter CreateGitHubAdapter(IServiceProvider services, AISettings settings, ILoggerFactory loggerFactory)
    {
        // Create fresh instance with current settings
        var logger = loggerFactory.CreateLogger<GitHubModelsAdapter>();
        var options = Options.Create(settings);
        return new GitHubModelsAdapter(options, logger);
    }

    private static OpenAIAdapter CreateOpenAIAdapter(IServiceProvider services, AISettings settings, ILoggerFactory loggerFactory)
    {
        // Create fresh instance with current settings
        var logger = loggerFactory.CreateLogger<OpenAIAdapter>();
        var options = Options.Create(settings);
        return new OpenAIAdapter(options, logger);
    }

    private static GroqAdapter CreateGroqAdapter(IServiceProvider services, AISettings settings, ILoggerFactory loggerFactory)
    {
        // Create fresh instance with current settings
        var logger = loggerFactory.CreateLogger<GroqAdapter>();
        var options = Options.Create(settings);
        return new GroqAdapter(options, logger);
    }
}
