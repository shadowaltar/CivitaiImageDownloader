namespace CivitaiImageDownloader.Util;

public static class UserNameHelper
{
    public static List<string> ParseUserNames(this TextBox textBox)
    {
        var userNameConcat = textBox.Text;
        if (userNameConcat == null) { return []; }

        List<string> userNames = userNameConcat.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(u => u.Trim()).Where(u => !string.IsNullOrWhiteSpace(u)).ToList();

        if (userNames.Count == 0)
        {
            MessageBox.Show("Missing user name.");
            return [];
        }
        return userNames;
    }
}
