using System.IO.Compression;

namespace CivitaiImageDownloader.Util;

public static class Utils
{
    public static int MergeDirectories(string source, string dest)
    {
        int count = 0;
        // Move all files
        foreach (var file in Directory.GetFiles(source))
        {
            string destFile = Path.Combine(dest, Path.GetFileName(file));

            // .NET Core 3.0+ supports overwrite: true
            File.Move(file, destFile, overwrite: true);
            count++;
        }

        // Move all subdirectories (recursively)
        foreach (var dir in Directory.GetDirectories(source))
        {
            string destDir = Path.Combine(dest, Path.GetFileName(dir));
            MergeDirectories(dir, destDir);
            count++;
        }

        // Finally, delete the empty source folder
        Directory.Delete(source, true);

        return count;
    }

    internal static List<string> GetInfoFiles(string folder)
    {
        var jsonFiles = Directory.GetFiles(folder, "*.json").Where(f => !f.EndsWith(Downloader.SkipRecordFileName)).ToList();
        if (jsonFiles.Count == 0)
        {
            var infoZipPath = Path.Combine(folder, "info.json.zip");
            if (File.Exists(infoZipPath))
            {
                ZipFile.ExtractToDirectory(infoZipPath, folder);
            }
        }
        return Directory.GetFiles(folder, "*.json").Where(f => f != Downloader.SkipRecordFileName).ToList();
    }

    internal static void ZipInfoFiles(string folder)
    {
        var jsonFiles = Directory.GetFiles(folder, "*.json").Where(f => f != Downloader.SkipRecordFileName).ToList();
        var infoZipPath = Path.Combine(folder, "info.json.zip");
        if (Path.Exists(infoZipPath))
        {
            foreach (string file in jsonFiles)
            {
                File.Delete(file);
            }
            return;
        }
        using ZipArchive archive = ZipFile.Open(infoZipPath, ZipArchiveMode.Create);
        foreach (string file in jsonFiles)
        {
            // Path.GetFileName(file) ensures the file is stored by its name only, 
            // not its full original directory structure.
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
            File.Delete(file);
        }
    }
}
