using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivitaiImageDownloader.Models;
public record ExistenceResult(string Url, string FileName, string FilePath, bool IsExists);
