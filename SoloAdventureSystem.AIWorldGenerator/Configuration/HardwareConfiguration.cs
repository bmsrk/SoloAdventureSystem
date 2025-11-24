using System;

namespace SoloAdventureSystem.ContentGenerator.Configuration;

/// <summary>
/// Value object representing hardware configuration with validation.
/// </summary>
public sealed record HardwareConfiguration
{
    public bool UseGPU { get; }
    public int MaxThreads { get; }

    private HardwareConfiguration(bool useGPU, int maxThreads)
    {
        if (maxThreads <= 0)
            throw new ArgumentException("Max threads must be positive", nameof(maxThreads));

        UseGPU = useGPU;
        MaxThreads = maxThreads;
    }

    // Well-known configurations
    public static HardwareConfiguration Cpu { get; } = new(false, 4);
    public static HardwareConfiguration Gpu { get; } = new(true, 4);
    public static HardwareConfiguration HighPerformanceCpu { get; } = new(false, 8);

    /// <summary>
    /// Create custom hardware configuration
    /// </summary>
    public static HardwareConfiguration Create(bool useGPU, int maxThreads)
    {
        return new HardwareConfiguration(useGPU, maxThreads);
    }

    /// <summary>
    /// Auto-detect optimal configuration
    /// </summary>
    public static HardwareConfiguration Auto()
    {
        var processorCount = Environment.ProcessorCount;
        var maxThreads = Math.Max(1, processorCount / 2);
        
        // GPU detection would go here - for now default to CPU
        return new HardwareConfiguration(false, maxThreads);
    }
}
