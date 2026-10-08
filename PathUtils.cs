namespace CivitaiImageDownloader;

internal static class PathUtils
{
    public static string GetAlternativeJpegPath(this string path)
    {
        var ext = Path.GetExtension(path);
        if (ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            return Path.ChangeExtension(path, ".jpg");
        if (ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
            return Path.ChangeExtension(path, ".jpeg");
        return path;
    }

    public static string GetPreferredJpegPath(this string path)
    {
        return Path.GetExtension(path).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            ? Path.ChangeExtension(path, ".jpg")
            : path;
    }

    /// <summary>True for in-progress compression/enhancement temp files, which should be ignored by viewers/lists.</summary>
    public static bool IsTempMediaFile(string fileName) =>
        fileName.StartsWith("compressing_", StringComparison.OrdinalIgnoreCase)
        || fileName.EndsWith(".enhanced.mp4", StringComparison.OrdinalIgnoreCase)
        || fileName.EndsWith(".tmp.mp4", StringComparison.OrdinalIgnoreCase);
}
