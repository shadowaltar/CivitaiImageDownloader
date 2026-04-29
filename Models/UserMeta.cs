using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitaiImageDownloader.Models;

internal record UserMeta(string UserName, int FileCount, double FolderSize, string ParentFolder)
{
}
