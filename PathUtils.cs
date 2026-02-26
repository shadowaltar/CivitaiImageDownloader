namespace CivitaiImageDownloader;

internal static class PathUtils
{
    public static string GetAlternativeJpegPath(this string path)
    {
        return path.EndsWith(".jpeg") ? path.Replace(".jpeg", ".jpg") :
            path.EndsWith(".jpg") ? path.Replace(".jpg", ".jpeg") : path;
    }

    public static string GetPreferredJpegPath(this string path)
    {
        return path.EndsWith(".jpeg") ? path.Replace(".jpeg", ".jpg") : path;
    }
}
