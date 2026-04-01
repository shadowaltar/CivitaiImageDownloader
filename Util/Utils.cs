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
}
