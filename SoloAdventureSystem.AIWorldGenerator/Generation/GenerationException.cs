using System;

namespace SoloAdventureSystem.ContentGenerator.Generation;

/// <summary>
/// Custom exception for generation failures
/// </summary>
public class GenerationException : Exception
{
    public string OperationName { get; }
    public int ConsecutiveFailures { get; }

    public GenerationException(string message) : base(message)
    {
        OperationName = "Unknown";
    }

    public GenerationException(string message, Exception innerException) 
        : base(message, innerException)
    {
        OperationName = "Unknown";
    }

    public GenerationException(string operationName, string message, int consecutiveFailures = 0) 
        : base(message)
    {
        OperationName = operationName;
        ConsecutiveFailures = consecutiveFailures;
    }

    public GenerationException(string operationName, string message, Exception innerException, int consecutiveFailures = 0) 
        : base(message, innerException)
    {
        OperationName = operationName;
        ConsecutiveFailures = consecutiveFailures;
    }
}
