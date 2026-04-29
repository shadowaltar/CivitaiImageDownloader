using System.Text.Json;
using CivitaiImageDownloader.Models;

namespace CivitaiImageDownloader.Util;

public static class UsernameHistoryManager
{
    private const string HistoryFileName = "username-history.json";

    public static string GetHistoryFilePath(string parentOutputFolder)
    {
        return Path.Combine(parentOutputFolder, HistoryFileName);
    }

    public static List<UsernameHistoryEntry> LoadHistory(string parentOutputFolder)
    {
        var path = GetHistoryFilePath(parentOutputFolder);
        if (!File.Exists(path))
            return [];

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<UsernameHistoryEntry>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static void SaveHistory(string parentOutputFolder, List<UsernameHistoryEntry> entries)
    {
        var path = GetHistoryFilePath(parentOutputFolder);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public static void RecordAction(string parentOutputFolder, string action, string usernamesConcatenated)
    {
        var entries = LoadHistory(parentOutputFolder);
        var entry = new UsernameHistoryEntry(
            action,
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            usernamesConcatenated
        );
        entries.Insert(0, entry); // newest at top
        SaveHistory(parentOutputFolder, entries);
    }
}
