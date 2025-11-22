namespace SoloAdventureSystem.ContentGenerator.EmbeddedModel;

/// <summary>
/// Progress information for model download operations.
/// </summary>
public class DownloadProgress
{
    public long DownloadedBytes { get; set; }
    public long TotalBytes { get; set; }
    public int PercentComplete { get; set; }
    public long SpeedBytesPerSecond { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    
    public string GetSpeedFormatted()
    {
        var speedMB = SpeedBytesPerSecond / 1024.0 / 1024.0;
        return $"{speedMB:F2} MB/s";
    }
    
    public string GetProgressFormatted()
    {
        var downloadedMB = DownloadedBytes / 1024.0 / 1024.0;
        var totalMB = TotalBytes / 1024.0 / 1024.0;
        return $"{downloadedMB:F1} MB / {totalMB:F1} MB ({PercentComplete}%)";
    }
}
