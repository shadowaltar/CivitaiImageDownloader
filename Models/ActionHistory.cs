namespace CivitaiImageDownloader.Models;

/// <summary>
/// Represents a single logged action (download or video compression) saved to username-history.json.
/// </summary>
public record ActionHistory(string Action, string Timestamp, string UserNames)
{
    /// <summary>
    /// Format for display in the listbox: "Action - Timestamp - Usernames"
    /// </summary>
    public string DisplayText => $"{Action} - {Timestamp} - {UserNames}";
}
