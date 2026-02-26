namespace CivitaiImageDownloader.Util;
public class FolderHelper
{
    public static string GetFolder(string rootFolder, string uuserName)
    {
        var folder = Path.Combine(rootFolder, uuserName);
        var allFolders = Directory.GetDirectories(rootFolder, uuserName, SearchOption.AllDirectories);
        if (allFolders.Length > 0)
        {
            folder = allFolders[0];
            return folder;
        }
        else
        {
            return "";
        }
    }
}
