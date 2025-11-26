using System;
using System.Collections;
using Microsoft.Extensions.Logging;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Policy interface for resilient generation operations
/// </summary>
public interface IGenerationPolicy
{
    T Execute<T>(Func<T> operation, string operationName) where T : class;
}

/// <summary>
/// Resilience policy for generation operations with retry logic and failure tracking.
/// Centralizes error handling that was previously duplicated across all generation methods.
/// </summary>
public class ResilienceGenerationPolicy : IGenerationPolicy
{
    private readonly ILogger<ResilienceGenerationPolicy>? _logger;
    private int _consecutiveFailures = 0;
    private const int MAX_FAILURES = 3;

    public ResilienceGenerationPolicy(ILogger<ResilienceGenerationPolicy>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Execute a generation operation with resilience and failure tracking
    /// </summary>
    public T Execute<T>(Func<T> operation, string operationName) where T : class
    {
        try
        {
            _logger?.LogDebug("Executing generation operation: {OperationName}", operationName);

            var result = operation();

            if (IsEmptyResult(result))
            {
                _consecutiveFailures++;
                _logger?.LogWarning(
                    "Empty result for {Operation} ({Count}/{Max})",
                    operationName,
                    _consecutiveFailures,
                    MAX_FAILURES);

                if (_consecutiveFailures >= MAX_FAILURES)
                {
                    var message = $"Model is consistently producing empty outputs for {operationName}";
                    _logger?.LogError(message);
                    throw new GenerationException(
                        operationName,
                        message,
                        _consecutiveFailures);
                }
            }
            else
            {
                if (_consecutiveFailures > 0)
                {
                    _logger?.LogInformation(
                        "Recovery successful for {Operation} after {Count} failures",
                        operationName,
                        _consecutiveFailures);
                }
                _consecutiveFailures = 0;
            }

            _logger?.LogDebug("Generation operation {OperationName} completed successfully", operationName);
            return result;
        }
        catch (Exception ex) when (ex is not GenerationException)
        {
            _logger?.LogError(ex, "Generation operation {Operation} failed", operationName);
            throw new GenerationException(
                operationName,
                $"AI generation failed for {operationName}. Error: {ex.Message}",
                ex,
                _consecutiveFailures);
        }
    }

    /// <summary>
    /// Reset failure counter (e.g., when switching to a new world generation)
    /// </summary>
    public void Reset()
    {
        _consecutiveFailures = 0;
        _logger?.LogDebug("Generation policy reset");
    }

    private bool IsEmptyResult<T>(T result) where T : class
    {
        if (result == null) return true;
        if (result is string str) return string.IsNullOrWhiteSpace(str);

        // Handle non-generic collections (covers arrays, lists etc.) using ICollection
        if (result is ICollection col) return col.Count == 0;

        return false;
    }
}
