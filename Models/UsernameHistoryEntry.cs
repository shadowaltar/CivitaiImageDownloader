namespace CivitaiImageDownloader.Models;

public record UsernameHistoryEntry(string Action, string Timestamp, string UsernamesConcatenated)
{
    public override string ToString() => $"{Action} - {Timestamp} - {UsernamesConcatenated}";
}
