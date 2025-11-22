using Azure.AI.Inference;
using Azure;
using SoloAdventureSystem.ContentGenerator.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Adapter that uses GitHub Models API (or Azure OpenAI) for text generation.
/// Supports deterministic generation via temperature=0.
/// </summary>
public class GitHubModelsAdapter : ILocalSLMAdapter
{
    private readonly ChatCompletionsClient _client;
    private readonly AISettings _settings;
    private readonly ILogger<GitHubModelsAdapter> _logger;

    public GitHubModelsAdapter(IOptions<AISettings> settings, ILogger<GitHubModelsAdapter> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrEmpty(_settings.Token))
        {
            throw new InvalidOperationException(
                "GitHub/Azure token is required. Please enter your API key in the yellow field.");
        }

        var credential = new AzureKeyCredential(_settings.Token);
        _client = new ChatCompletionsClient(new Uri(_settings.Endpoint), credential);
        
        _logger.LogInformation(
            "Initialized GitHub Models adapter with model {Model} at {Endpoint}", 
            _settings.Model, 
            _settings.Endpoint);
    }

    public string GenerateRoomDescription(string context, int seed)
    {
        var systemPrompt = $"You are a creative game world designer. Generate a vivid room description for a text-based adventure game. Keep it concise (2-3 sentences). Seed: {seed}";
        var userPrompt = $"Create a room description for: {context}";
        
        return GenerateText(systemPrompt, userPrompt, seed);
    }

    public string GenerateNpcBio(string context, int seed)
    {
        var systemPrompt = $"You are a creative game world designer. Generate a short NPC biography for a text-based adventure game. Keep it concise (1-2 sentences). Seed: {seed}";
        var userPrompt = $"Create an NPC bio for: {context}";
        
        return GenerateText(systemPrompt, userPrompt, seed);
    }

    public string GenerateFactionFlavor(string context, int seed)
    {
        var systemPrompt = $"You are a creative game world designer. Generate faction lore and flavor text for a text-based adventure game. Keep it concise (2-3 sentences). Seed: {seed}";
        var userPrompt = $"Create faction flavor for: {context}";
        
        return GenerateText(systemPrompt, userPrompt, seed);
    }

    public List<string> GenerateLoreEntries(string context, int seed, int count)
    {
        var entries = new List<string>();
        var systemPrompt = $"You are a creative game world designer. Generate world lore entries for a text-based adventure game. Each entry should be 1-2 sentences. Seed: {seed}";
        
        for (int i = 0; i < count; i++)
        {
            var userPrompt = $"Create lore entry #{i + 1} for: {context}";
            entries.Add(GenerateText(systemPrompt, userPrompt, seed + i));
        }
        
        return entries;
    }

    private string GenerateText(string systemPrompt, string userPrompt, int seed)
    {
        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= _settings.MaxRetries; attempt++)
        {
            try
            {
                var requestOptions = new ChatCompletionsOptions
                {
                    Messages =
                    {
                        new ChatRequestSystemMessage(systemPrompt),
                        new ChatRequestUserMessage(userPrompt)
                    },
                    Model = _settings.Model,
                    Temperature = (float)_settings.Temperature,
                    MaxTokens = 200,
                    // Use seed for deterministic generation (if supported by the model)
                    Seed = seed
                };

                _logger.LogDebug("Generating text with model {Model}, seed {Seed} (attempt {Attempt}/{MaxRetries})", 
                    _settings.Model, seed, attempt, _settings.MaxRetries);
                
                var response = _client.Complete(requestOptions);
                var chatCompletion = response.Value;
                var result = chatCompletion.Content;
                
                _logger.LogDebug("Successfully generated {Length} characters", result.Length);
                
                return result;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 401)
            {
                // Authentication error - don't retry
                _logger.LogError(ex, "Authentication failed. Check your API key.");
                throw new InvalidOperationException(
                    "Authentication failed. Your API key appears to be invalid or expired. Please check your API key.", ex);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 429)
            {
                // Rate limit - retry with backoff
                lastException = ex;
                _logger.LogWarning("Rate limited (attempt {Attempt}/{MaxRetries}). Waiting before retry...", 
                    attempt, _settings.MaxRetries);
                
                if (attempt < _settings.MaxRetries)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
                }
            }
            catch (Azure.RequestFailedException ex) when (ex.Status >= 500)
            {
                // Server error - retry
                lastException = ex;
                _logger.LogWarning("Server error {StatusCode} (attempt {Attempt}/{MaxRetries})", 
                    ex.Status, attempt, _settings.MaxRetries);
                
                if (attempt < _settings.MaxRetries)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(attempt)); // Linear backoff
                }
            }
            catch (Exception ex)
            {
                // Other errors - log and throw immediately
                _logger.LogError(ex, "Failed to generate text: {Message}", ex.Message);
                throw new InvalidOperationException(
                    $"AI generation failed: {ex.Message}. Check your API key and network connection.", ex);
            }
        }
        
        // All retries exhausted
        throw new InvalidOperationException(
            $"Failed to generate text after {_settings.MaxRetries} attempts. Last error: {lastException?.Message}", 
            lastException);
    }
}
