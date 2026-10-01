using System.Text.Json;

namespace CivitaiImageDownloader.Util;

/// <summary>
/// A per-user folder marker ("manual.json"). When it exists, download-related operations
/// (download, download index only, mark deleted, freeze) skip that user entirely.
/// The file is excluded from info-file handling so it is never zipped/parsed as index data.
/// </summary>
public static class ManualMarker
{
    public const string FileName = "manual.json";

    public static string GetMarkerPath(string userFolder) => Path.Combine(userFolder, FileName);

    public static bool Exists(string? userFolder)
        => !string.IsNullOrEmpty(userFolder) && File.Exists(GetMarkerPath(userFolder));

    public static void Mark(string userFolder)
    {
        Directory.CreateDirectory(userFolder);
        var payload = new { manual = true, markedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
        File.WriteAllText(GetMarkerPath(userFolder), JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static void Unmark(string userFolder)
    {
        try
        {
            var path = GetMarkerPath(userFolder);
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
        }
    }
}
