using CivitaiImageDownloader.Models;

namespace CivitaiImageDownloader;
public record DownloadParameters(string TargetFolder,
                                 string UserName,
                                 List<string> UserNames,
                                 List<string> NsfwLevels,
                                 MediaType MediaType,
                                 bool IsAlwaysLatestMetaInfo)
{
}
