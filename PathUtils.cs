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
}
