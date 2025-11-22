using SoloAdventureSystem.ContentGenerator.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SoloAdventureSystem.ContentGenerator.Adapters;

/// <summary>
/// Adapter for Groq API - FREE and FAST inference!
/// Sign up at https://console.groq.com for free API key.
/// </summary>
public class GroqAdapter : ILocalSLMAdapter
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly ILogger<GroqAdapter> _logger;

    public GroqAdapter(IOptions<AISettings> settings, ILogger<GroqAdapter> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrEmpty(_settings.Token))
        {
            throw new InvalidOperationException(
                "Groq API key is required. Get a FREE key at https://console.groq.com");
        }

        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.Token}");
        
        _logger.LogInformation(
            "Initialized Groq adapter with model {Model} - FREE TIER!", 
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
                var request = new GroqChatRequest
                {
                    Model = _settings.Model,
                    Messages = new[]
                    {
                        new GroqMessage { Role = "system", Content = systemPrompt },
                        new GroqMessage { Role = "user", Content = userPrompt }
                    },
                    Temperature = (float)_settings.Temperature,
                    MaxTokens = 200,
                    Seed = seed
                };

                _logger.LogDebug("Generating text with Groq model {Model}, seed {Seed} (attempt {Attempt}/{MaxRetries})", 
                    _settings.Model, seed, attempt, _settings.MaxRetries);
                
                var response = _httpClient.PostAsJsonAsync("chat/completions", request).Result;
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Authentication failed. Check your Groq API key.");
                    throw new InvalidOperationException(
                        "Authentication failed. Your Groq API key appears to be invalid. Get a FREE key at https://console.groq.com");
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    lastException = new InvalidOperationException("Rate limited");
                    _logger.LogWarning("Rate limited (attempt {Attempt}/{MaxRetries}). Waiting before retry...", 
                        attempt, _settings.MaxRetries);
                    
                    if (attempt < _settings.MaxRetries)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                        continue;
                    }
                }
                
                if ((int)response.StatusCode >= 500)
                {
                    lastException = new InvalidOperationException($"Server error: {response.StatusCode}");
                    _logger.LogWarning("Server error {StatusCode} (attempt {Attempt}/{MaxRetries})", 
                        response.StatusCode, attempt, _settings.MaxRetries);
                    
                    if (attempt < _settings.MaxRetries)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(attempt));
                        continue;
                    }
                }
                
                response.EnsureSuccessStatusCode();
                
                var result = response.Content.ReadFromJsonAsync<GroqChatResponse>().Result;
                var text = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "";
                
                _logger.LogDebug("Successfully generated {Length} characters", text.Length);
                
                return text;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Failed to generate text: {Message}", ex.Message);
                throw new InvalidOperationException(
                    $"AI generation failed: {ex.Message}. Check your API key and network connection.", ex);
            }
        }
        
        throw new InvalidOperationException(
            $"Failed to generate text after {_settings.MaxRetries} attempts. Last error: {lastException?.Message}", 
            lastException);
    }

    // DTOs for Groq API
    private class GroqChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "";
        
        [JsonPropertyName("messages")]
        public GroqMessage[] Messages { get; set; } = Array.Empty<GroqMessage>();
        
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }
        
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
        
        [JsonPropertyName("seed")]
        public int Seed { get; set; }
    }

    private class GroqMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "";
        
        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }

    private class GroqChatResponse
    {
        [JsonPropertyName("choices")]
        public GroqChoice[]? Choices { get; set; }
    }

    private class GroqChoice
    {
        [JsonPropertyName("message")]
        public GroqMessage? Message { get; set; }
    }
}
