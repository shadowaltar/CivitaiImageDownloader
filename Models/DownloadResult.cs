using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitaiImageDownloader.Models;
public record DownloadResult(string UserName,
    int SkippedCount,
    int DownloadTargetCount,
    int ActualDownloadCount,
    List<string> FailedUrls);
