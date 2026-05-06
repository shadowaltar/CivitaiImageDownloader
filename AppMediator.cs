using CivitaiImageDownloader.Models;
using CivitaiImageDownloader.Util;

namespace CivitaiImageDownloader;

public class AppMediator
{
    public string TargetFolder { get; set; } = @"D:\AI\CivitAI\";

    public string DownloadUsernames { get; set; } = "";
    public string VideoUsernames { get; set; } = "";

    public List<DownloadResult> DownloadResults { get; } = [];

    public bool Stopping { get; set; }

    public string? CurrentUserFolder { get; set; }

    public event Action<string>? MessageLogged;
    public event Action? ActionHistoryChanged;
    public event Action<string>? UsernamesCopiedToDownload;
    public event Action<string>? UsernamesCopiedToVideo;
    public event Action? TabSwitchToDownloadRequested;
    public event Action? TabSwitchToVideoRequested;
    public event Action? TabSwitchToViewerRequested;

    public void LogMessage(string message)
    {
        MessageLogged?.Invoke(message);
    }

    public void RecordDownloadHistory(string usernames)
    {
        UsernameHistoryManager.RecordAction(TargetFolder, "Download", usernames);
        ActionHistoryChanged?.Invoke();
    }

    public void RecordVideoHistory(string usernames)
    {
        UsernameHistoryManager.RecordAction(TargetFolder, "VideoCompress", usernames);
        ActionHistoryChanged?.Invoke();
    }

    public void CopyUsernamesToDownload(string usernames)
    {
        UsernamesCopiedToDownload?.Invoke(usernames);
    }

    public void CopyUsernamesToVideo(string usernames)
    {
        UsernamesCopiedToVideo?.Invoke(usernames);
    }

    public void RequestSwitchToDownloadTab()
    {
        TabSwitchToDownloadRequested?.Invoke();
    }

    public void RequestSwitchToVideoTab()
    {
        TabSwitchToVideoRequested?.Invoke();
    }

    public void RequestSwitchToViewerTab()
    {
        TabSwitchToViewerRequested?.Invoke();
    }
}
