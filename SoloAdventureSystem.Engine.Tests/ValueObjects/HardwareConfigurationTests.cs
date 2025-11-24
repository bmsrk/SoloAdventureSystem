using System;
using Xunit;
using SoloAdventureSystem.ContentGenerator.Configuration;

namespace SoloAdventureSystem.Engine.Tests;

/// <summary>
/// Tests for HardwareConfiguration value object
/// </summary>
public class HardwareConfigurationTests
{
    [Fact]
    public void WellKnownConfigurations_HaveCorrectProperties()
    {
        // Assert - CPU
        Assert.False(HardwareConfiguration.Cpu.UseGPU);
        Assert.Equal(4, HardwareConfiguration.Cpu.MaxThreads);

        // Assert - GPU
        Assert.True(HardwareConfiguration.Gpu.UseGPU);
        Assert.Equal(4, HardwareConfiguration.Gpu.MaxThreads);

        // Assert - High Performance CPU
        Assert.False(HardwareConfiguration.HighPerformanceCpu.UseGPU);
        Assert.Equal(8, HardwareConfiguration.HighPerformanceCpu.MaxThreads);
    }

    [Fact]
    public void Create_WithValidParameters_ReturnsConfiguration()
    {
        // Act
        var config = HardwareConfiguration.Create(true, 16);

        // Assert
        Assert.True(config.UseGPU);
        Assert.Equal(16, config.MaxThreads);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Create_WithInvalidMaxThreads_ThrowsArgumentException(int maxThreads)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => HardwareConfiguration.Create(false, maxThreads));
    }

    [Fact]
    public void Auto_ReturnsValidConfiguration()
    {
        // Act
        var config = HardwareConfiguration.Auto();

        // Assert
        Assert.NotNull(config);
        Assert.True(config.MaxThreads > 0);
        Assert.True(config.MaxThreads <= Environment.ProcessorCount);
    }

    [Fact]
    public void Auto_UsesHalfOfProcessorCount()
    {
        // Act
        var config = HardwareConfiguration.Auto();
        var expectedThreads = Math.Max(1, Environment.ProcessorCount / 2);

        // Assert
        Assert.Equal(expectedThreads, config.MaxThreads);
    }

    [Fact]
    public void Auto_DefaultsToNonGPU()
    {
        // Act
        var config = HardwareConfiguration.Auto();

        // Assert - Currently defaults to CPU
        Assert.False(config.UseGPU);
    }

    [Fact]
    public void ValueObjectEquality_SameConfiguration_AreEqual()
    {
        // Arrange
        var config1 = HardwareConfiguration.Cpu;
        var config2 = HardwareConfiguration.Cpu;

        // Assert
        Assert.Equal(config1, config2);
    }

    [Fact]
    public void ValueObjectEquality_DifferentConfigurations_AreNotEqual()
    {
        // Arrange
        var config1 = HardwareConfiguration.Cpu;
        var config2 = HardwareConfiguration.Gpu;

        // Assert
        Assert.NotEqual(config1, config2);
    }

    [Fact]
    public void Create_MultipleCallsWithSameParameters_AreEqual()
    {
        // Act
        var config1 = HardwareConfiguration.Create(true, 8);
        var config2 = HardwareConfiguration.Create(true, 8);

        // Assert
        Assert.Equal(config1, config2);
    }
}
