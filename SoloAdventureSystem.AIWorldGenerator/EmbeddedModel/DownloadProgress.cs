namespace SoloAdventureSystem.ContentGenerator.EmbeddedModel;

/// <summary>
/// Progress information for model download operations.
/// Provides both raw values and human-readable formatted properties.
/// </summary>
public class DownloadProgress
{
    public long DownloadedBytes { get; set; }
    public long TotalBytes { get; set; }
    public int PercentComplete { get; set; }
    public long SpeedBytesPerSecond { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    
    // Convenience properties for human-readable display
    
    /// <summary>
    /// Downloaded size in megabytes (MB).
    /// </summary>
    public double DownloadedMB => DownloadedBytes / 1024.0 / 1024.0;
    
    /// <summary>
    /// Total size in megabytes (MB).
    /// </summary>
    public double TotalMB => TotalBytes / 1024.0 / 1024.0;
    
    /// <summary>
    /// Download speed in megabytes per second (MB/s).
    /// </summary>
    public double SpeedMBPerSecond => SpeedBytesPerSecond / 1024.0 / 1024.0;
    
    /// <summary>
    /// Formatted estimated time remaining (mm:ss or hh:mm:ss).
    /// </summary>
    public string FormattedETA => EstimatedTimeRemaining.TotalHours >= 1 
        ? $"{EstimatedTimeRemaining:hh\\:mm\\:ss}" 
        : $"{EstimatedTimeRemaining:mm\\:ss}";
    
    /// <summary>
    /// Legacy method for backwards compatibility.
    /// </summary>
    public string GetSpeedFormatted()
    {
        return $"{SpeedMBPerSecond:F2} MB/s";
    }
    
    /// <summary>
    /// Legacy method for backwards compatibility.
    /// </summary>
    public string GetProgressFormatted()
    {
        return $"{DownloadedMB:F1} MB / {TotalMB:F1} MB ({PercentComplete}%)";
    }
    
    /// <summary>
    /// Human-readable progress summary (e.g., "45% - 2.5 MB/s - ETA: 02:30").
    /// </summary>
    public string ProgressSummary => 
        $"{PercentComplete}% - {SpeedMBPerSecond:F1} MB/s - ETA: {FormattedETA}";
    
    /// <summary>
    /// Detailed progress description.
    /// </summary>
    public string DetailedProgress => 
        $"{DownloadedMB:F1}/{TotalMB:F1} MB ({PercentComplete}%) at {SpeedMBPerSecond:F1} MB/s - {FormattedETA} remaining";
}
