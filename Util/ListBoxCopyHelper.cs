namespace CivitaiImageDownloader.Util;

/// <summary>
/// Enables multi-row selection and Ctrl+C copying for a message list box.
/// </summary>
internal static class ListBoxCopyHelper
{
    public static void EnableCopy(ListBox listBox)
    {
        listBox.SelectionMode = SelectionMode.MultiExtended;
        listBox.KeyDown += (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelection(listBox);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        };
    }

    public static void CopySelection(ListBox listBox)
    {
        if (listBox.SelectedIndices.Count == 0)
            return;

        var text = string.Join(Environment.NewLine,
            listBox.SelectedIndices.Cast<int>()
                .OrderBy(i => i)
                .Select(i => listBox.Items[i]?.ToString() ?? ""));

        if (!string.IsNullOrEmpty(text))
            Clipboard.SetText(text);
    }
}
