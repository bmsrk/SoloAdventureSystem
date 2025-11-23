using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SoloAdventureSystem.ContentGenerator.EmbeddedModel;

/// <summary>
/// Optimizes prompts for small embedded models.
/// Reduces token count while preserving quality.
/// </summary>
public class PromptOptimizer
{
    private readonly ILogger<PromptOptimizer>? _logger;
    
    public PromptOptimizer(ILogger<PromptOptimizer>? logger = null)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Optimizes a system prompt for smaller models.
    /// Modern small models (Phi-3-mini: 4K context, TinyLlama: 2K context) can handle reasonable prompt sizes.
    /// We preserve examples as they're critical for output quality.
    /// </summary>
    public string OptimizeSystemPrompt(string systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(systemPrompt))
            return string.Empty;
        
        // Condense multiple spaces and newlines, but preserve structure
        var result = Regex.Replace(systemPrompt, @"[ \t]+", " "); // Condense spaces/tabs
        result = Regex.Replace(result, @"\n\s*\n", "\n"); // Remove blank lines
        result = result.Trim();
        
        // Phi-3-mini has 4K context, TinyLlama has 2K
        // Increased from 1500 to 3000 to preserve examples and quality
        // Examples are CRITICAL for small models to understand desired output format
        const int MaxPromptLength = 3000;
        
        if (result.Length > MaxPromptLength)
        {
            _logger?.LogWarning("?? System prompt is {Length} chars, truncating to {Max} chars", 
                result.Length, MaxPromptLength);
            
            // Try to cut at sentence boundary for better coherence
            var cutPoint = result.LastIndexOf('.', MaxPromptLength);
            if (cutPoint > MaxPromptLength / 2)
            {
                result = result.Substring(0, cutPoint + 1);
            }
            else
            {
                result = result.Substring(0, MaxPromptLength);
                // Add ellipsis only if we actually truncated mid-sentence
                if (!result.EndsWith('.'))
                    result += "...";
            }
            
            _logger?.LogWarning("?? Truncated system prompt to {Length} chars", result.Length);
        }
        
        return result;
    }
    
    /// <summary>
    /// Creates a chat-formatted prompt for instruction-tuned models.
    /// </summary>
    public string FormatChatPrompt(string systemPrompt, string userPrompt)
    {
        // Phi-3 uses a specific chat template
        return $"<|system|>{systemPrompt}<|end|><|user|>{userPrompt}<|end|><|assistant|>";
    }
    
    /// <summary>
    /// Estimates token count (rough approximation).
    /// </summary>
    public int EstimateTokenCount(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        
        // Rough estimate: ~4 characters per token on average
        return text.Length / 4;
    }
}
