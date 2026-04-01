namespace CivitaiImageDownloader.Models;
public record DownloadResult(string UserName,
    int SkippedCount,
    int DownloadTargetCount,
    int ActualDownloadCount,
    List<string> FailedUrls);
