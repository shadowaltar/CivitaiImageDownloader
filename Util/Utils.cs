using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CivitaiImageDownloader.Util;

public static class Utils
{
    private static readonly string[] UnwantedInfoKeys = ["hash", "meta", "username", "baseModel", "modelVersionIds", "stats"];

    public static void StripUnwantedFields(JsonNode? item)
    {
        if (item is JsonObject obj)
        {
            foreach (var key in UnwantedInfoKeys)
            {
                obj.Remove(key);
            }
        }
    }

    internal static void CompressInfoFiles(string folder)
    {
        var jsonFiles = GetInfoFiles(folder);
        if (jsonFiles.Count == 0)
            return;

        var allItems = new JsonArray();
        var seenIds = new HashSet<int>();
        foreach (var file in jsonFiles)
        {
            try
            {
                var content = File.ReadAllText(file);
                var obj = JsonNode.Parse(content)?.AsObject();
                var items = obj?["items"]?.AsArray();
                if (items == null)
                    continue;

                foreach (var item in items)
                {
                    if (item == null)
                        continue;

                    var cloned = item.DeepClone();
                    StripUnwantedFields(cloned);

                    int.TryParse(cloned["id"]?.ToString(), out var id);
                    if (id > 0 && !seenIds.Add(id))
                        continue;

                    allItems.Add(cloned);
                }
            }
            catch { }
        }

        // delete all old json files
        jsonFiles = Directory.GetFiles(folder, "*.json").Where(f => !f.EndsWith(Downloader.SkipRecordFileName)).ToList();
        foreach (var file in jsonFiles)
        {
            try { File.Delete(file); }
            catch { }
        }

        if (allItems.Count == 0)
            return;

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var newFilePath = Path.Combine(folder, $"{timestamp}.json");
        var combinedJson = new JsonObject
        {
            ["items"] = allItems,
            ["metadata"] = new JsonObject { ["nextPage"] = null }
        };
        File.WriteAllText(newFilePath, combinedJson.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        ZipInfoFiles(folder);
    }

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
        return Directory.GetFiles(folder, "*.json").Where(f => !f.EndsWith(Downloader.SkipRecordFileName)).ToList();
    }

    internal static void ZipInfoFiles(string folder)
    {
        var jsonFiles = Directory.GetFiles(folder, "*.json").Where(f => !f.EndsWith(Downloader.SkipRecordFileName)).ToList();
        if (jsonFiles.Count == 0)
            return;

        var infoZipPath = Path.Combine(folder, "info.json.zip");
        if (File.Exists(infoZipPath))
        {
            File.Delete(infoZipPath);
        }
        using ZipArchive archive = ZipFile.Open(infoZipPath, ZipArchiveMode.Create);
        foreach (string file in jsonFiles)
        {
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
            File.Delete(file);
        }
    }
}
