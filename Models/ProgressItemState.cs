namespace CivitaiImageDownloader.Models;

/// <summary>
/// State of a single item shown in the progress grid.
/// </summary>
public enum ProgressItemState
{
    /// <summary>Not processed yet (dot).</summary>
    Pending,

    /// <summary>Currently being processed (spinning char).</summary>
    Processing,

    /// <summary>Processed successfully (tick).</summary>
    Done,

    /// <summary>Skipped / not applicable (x).</summary>
    Skipped,

    /// <summary>Already present / no re-download needed (hollow circle).</summary>
    Already,
}
