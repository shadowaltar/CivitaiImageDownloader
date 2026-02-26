using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitaiImageDownloader.Models;
public enum VideoDownloadMode
{
    OriginalOnly = 0x01,
    Transcode = 0x10,
    Auto = 0x11,
}
