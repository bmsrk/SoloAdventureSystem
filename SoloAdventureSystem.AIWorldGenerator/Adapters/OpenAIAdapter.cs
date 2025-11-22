using SoloAdventureSystem.ContentGenerator.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using OpenAI;
using System.ClientModel;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Adapter for OpenAI API (direct, not via GitHub/Azure).
/// Supports free tier and paid accounts.
/// </summary>
public class OpenAIAdapter : ILocalSLMAdapter
{
    private readonly OpenAIClient _client;
    private readonly ChatClient _chatClient;
    private readonly AISettings _settings;
    private readonly ILogger<OpenAIAdapter> _logger;

    public OpenAIAdapter(IOptions<AISettings> settings, ILogger<OpenAIAdapter> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrEmpty(_settings.Token))
        {
            throw new InvalidOperationException(
                "OpenAI API key is required. Please enter your API key in the yellow field.");
        }

        // Create OpenAI client (NOT AzureOpenAIClient!)
        _client = new OpenAIClient(_settings.Token);
        _chatClient = _client.GetChatClient(_settings.Model);
        
        _logger.LogInformation(
            "Initialized OpenAI adapter with model {Model}", 
            _settings.Model);
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
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                };

                var options = new ChatCompletionOptions
                {
                    Temperature = (float)_settings.Temperature,
                    MaxOutputTokenCount = 200
#pragma warning disable OPENAI001 // Experimental feature
                    ,Seed = seed
#pragma warning restore OPENAI001
                };

                _logger.LogDebug("Generating text with OpenAI model {Model}, seed {Seed} (attempt {Attempt}/{MaxRetries})", 
                    _settings.Model, seed, attempt, _settings.MaxRetries);
                
                var response = _chatClient.CompleteChat(messages, options);
                var result = response.Value.Content[0].Text;
                
                _logger.LogDebug("Successfully generated {Length} characters", result.Length);
                
                return result;
            }
            catch (ClientResultException ex) when (ex.Status == 401)
            {
                // Authentication error - don't retry
                _logger.LogError(ex, "Authentication failed. Check your API key.");
                throw new InvalidOperationException(
                    "Authentication failed. Your OpenAI API key appears to be invalid. Please check your API key.", ex);
            }
            catch (ClientResultException ex) when (ex.Status == 429)
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
            catch (ClientResultException ex) when (ex.Status >= 500)
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
