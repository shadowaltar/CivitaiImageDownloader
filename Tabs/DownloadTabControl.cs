using CivitaiImageDownloader.Models;
using CivitaiImageDownloader.Util;
using System.Diagnostics;
using Utils = CivitaiImageDownloader.Util.Utils;

namespace CivitaiImageDownloader.Tabs;

public partial class DownloadTabControl : UserControl
{
    private readonly AppMediator _mediator;
    private List<UserMeta> _downloadedUserMeta = new();

    public DownloadTabControl(AppMediator mediator)
    {
        _mediator = mediator;
        InitializeComponent();

        txtUsernames.TextChanged += (s, e) => _mediator.DownloadUsernames = txtUsernames.Text;
        _mediator.UsernamesCopiedToDownload += usernames => txtUsernames.Text = usernames;
        _mediator.MessageLogged += AddMessage;
        btnShowFirstUserInViewer.Click += (s, e) => _mediator.RequestSwitchToViewerTab();
        btnClearUsernames.Click += (s, e) => txtUsernames.Clear();

        var ratingCheckBoxes = new[] { chb3Star, chb4Star, chb4p5Star, chb5Star, chb6Star };
        foreach (var cb in ratingCheckBoxes)
            cb.CheckedChanged += (s, e) =>
            {
                if (!((CheckBox)s!).Checked) return;
                foreach (var other in ratingCheckBoxes)
                    if (other != s)
                        other.Checked = false;
            };
    }

    private async void btnDownload_Click(object sender, EventArgs e)
    {
        _mediator.Stopping = false;
        _mediator.DownloadResults.Clear();

        Invoke(listBoxMessages.Items.Clear);
        UpdateDownloadingCounter(-1);

        var parameters = CreateDownloadParameters();
        if (parameters == null) return;

        _mediator.RecordDownloadHistory(txtUsernames.Text.Trim());

        Downloader? dl = null;
        foreach (var un in parameters.UserNames)
        {
            if (_mediator.Stopping && dl != null)
            {
                dl.ShouldStop = true;
                AddMessage("Download stopped.");
                break;
            }
            var p = parameters with { UserName = un };
            dl = new Downloader(p);
            dl.RaiseMessage += AddMessage;
            dl.UpdateDownloadingCounter += UpdateDownloadingCounter;
            var result = await dl.Run();
            _mediator.DownloadResults.Add(result);
            dl.RaiseMessage -= AddMessage;
            dl.UpdateDownloadingCounter -= UpdateDownloadingCounter;
            dl.Dispose();
        }

        AddMessage("====SUMMARY====");
        foreach (var r in Format(_mediator.DownloadResults))
            AddMessage(r);
        AddMessage("====SUMMARY====");
    }

    private void btnStop_Click(object sender, EventArgs e) => _mediator.Stopping = true;

    private void btnDeleteInfoFiles_Click(object sender, EventArgs e)
    {
        if (!Directory.Exists(_mediator.TargetFolder))
        {
            MessageBox.Show(this, "Invalid target folder.");
            return;
        }
        var uns = txtUsernames.ParseUserNames();
        if (uns.Count == 0) return;
        foreach (var s in uns)
        {
            var folder = Path.Combine(_mediator.TargetFolder, s);
            foreach (var f in Directory.GetFiles(folder, "*.txt"))
            {
                File.Delete(f);
                AddMessage("Deleted file: " + f);
            }
        }
    }

    private void btnOpenFirstUserFolder_Click(object sender, EventArgs e)
    {
        _mediator.CurrentUserFolder = "";
        if (!Directory.Exists(_mediator.TargetFolder)) { MessageBox.Show(this, "Invalid target folder."); return; }
        var uns = txtUsernames.ParseUserNames();
        if (uns.Count == 0) return;
        var folder = FolderHelper.GetFolder(_mediator.TargetFolder, uns[0]);
        if (!string.IsNullOrEmpty(folder)) { AddMessage("Use folder: " + folder); _mediator.CurrentUserFolder = folder; }
        else { AddMessage("Folder not found: " + folder); return; }
        Process.Start("explorer.exe", folder);
    }

    private void btnOpenAllUserFolders_Click(object sender, EventArgs e)
    {
        _mediator.CurrentUserFolder = "";
        if (!Directory.Exists(_mediator.TargetFolder)) { MessageBox.Show(this, "Invalid target folder."); return; }
        var uns = txtUsernames.ParseUserNames();
        if (uns.Count == 0) return;
        foreach (var userName in uns)
        {
            if (userName.Length == 0) continue;
            var folder = FolderHelper.GetFolder(_mediator.TargetFolder, userName);
            if (!string.IsNullOrEmpty(folder)) { AddMessage("Use folder: " + folder); _mediator.CurrentUserFolder = folder; }
            else { AddMessage("Folder not found: " + folder); return; }
            Process.Start("explorer.exe", folder);
        }
    }

    private async void btnMarkDeletedFilesNoRedownload_Click(object sender, EventArgs e)
    {
        Invoke(listBoxMessages.Items.Clear);
        _mediator.DownloadResults.Clear();
        var parameters = CreateDownloadParameters();
        if (parameters == null) return;

        var results = new Dictionary<string, List<ExistenceResult>>();
        foreach (var un in parameters.UserNames)
        {
            var p = parameters with { UserName = un };
            using var dl = new Downloader(p);
            dl.RaiseMessage += AddMessage;
            var result = await dl.MarkNonExistFiles();
            results[un] = result;
            dl.RaiseMessage -= AddMessage;
        }
        foreach (var (un, result) in results)
            AddMessage($"For user [{un}], marked {result.Count(r => !r.IsExists && r.WasDownloaded)}/{result.Count} files non-exist.");
    }

    private void btnCopyFailedUrls_Click(object sender, EventArgs e)
    {
        var failedUrls = _mediator.DownloadResults.SelectMany(r => r.FailedUrls).ToList();
        if (failedUrls.Count == 0) { AddMessage("No failed URLs"); return; }
        Clipboard.SetText(string.Join(Environment.NewLine, failedUrls));
        AddMessage($"Copied {failedUrls.Count} failed URLs to clipboard.");
    }

    private void btnCopyAllSubdirNames_Click(object sender, EventArgs e)
    {
        if (!Directory.Exists(_mediator.TargetFolder)) { MessageBox.Show(this, "Invalid target folder."); return; }
        var allDirs = Directory.GetDirectories(_mediator.TargetFolder, "*", SearchOption.AllDirectories);
        var dirNames = allDirs.Select(d => Path.GetFileName(d)).Where(d => !d.StartsWith('!')).Distinct().ToList();
        Clipboard.SetText(string.Join(",", dirNames));
    }

    private void btnMoveUsersToRating_Click(object sender, EventArgs e)
    {
        _mediator.CurrentUserFolder = "";
        bool r3 = chb3Star.Checked, r4 = chb4Star.Checked, r45 = chb4p5Star.Checked, r5 = chb5Star.Checked, r6 = chb6Star.Checked;
        var bools = new List<bool> { r3, r4, r45, r5, r6 };
        if (bools.Count(b => b) > 1) { MessageBox.Show("Can't move the user to this rating folder: multiple ratings are selected."); return; }
        var subFolder = GetSubFolderByRating(r3, r4, r45, r5, r6);
        if (subFolder == null) { AddMessage("Rating folder does not exist."); return; }
        var userNames = txtUsernames.ParseUserNames();
        if (userNames.Count == 0) return;
        var newBase = Path.Combine(_mediator.TargetFolder, subFolder);
        if (!Path.Exists(newBase)) { Directory.CreateDirectory(newBase); AddMessage($"Created dir: {newBase}"); }
        foreach (var userName in userNames)
        {
            var source = FolderHelper.GetFolder(_mediator.TargetFolder, userName);
            var destination = Path.Combine(newBase, userName);
            if (source.Equals(destination))
            {
                _mediator.CurrentUserFolder = source;
                AddMessage($"No action for {userName}: Already in {destination}");
            }
            else if (string.IsNullOrEmpty(source))
                AddMessage($"No action for {userName}: source folder {source} does not exist.");
            else
            {
                if (!Path.Exists(destination)) { Directory.CreateDirectory(destination); AddMessage($"Created dir: {destination}"); }
                AddMessage($"Moved {Utils.MergeDirectories(source, destination)} files from {source} to {destination}");
            }
        }
    }

    private void btnCompressInfo_Click(object sender, EventArgs e)
    {
        var userNames = UserNameHelper.ParseUserNames(txtUsernames);
        if (userNames.Count == 0) return;
        foreach (var user in userNames)
        {
            var folder = FolderHelper.GetFolder(_mediator.TargetFolder, user);
            if (string.IsNullOrEmpty(folder)) { AddMessage($"Skipping {user}: folder not found"); continue; }
            Utils.CompressInfoFiles(folder);
            AddMessage($"Compressed info files for {user}");
        }
        AddMessage("Compress Info Files complete.");
    }

    private static string? GetSubFolderByRating(bool r3, bool r4, bool r45, bool r5, bool r6)
    {
        if (r3) return "!3"; if (r4) return "!4"; if (r45) return "!4.5"; if (r5) return "!5"; if (r6) return "!6";
        return null;
    }

    private DownloadParameters? CreateDownloadParameters()
    {
        if (!Directory.Exists(_mediator.TargetFolder)) { MessageBox.Show(this, "Invalid target folder."); return null; }
        List<string> userNames = txtUsernames.ParseUserNames();
        if (userNames.Count == 0) return null;

        List<string> nsfwLevels = new();
        if (chbNsfw.Checked) nsfwLevels.Add("X");
        if (chbMature.Checked) nsfwLevels.Add("Mature");
        if (chbNormal.Checked) nsfwLevels.Add("Soft");
        if (chbChildLevel.Checked) nsfwLevels.Add("None");
        if (nsfwLevels.Count == 0) { AddMessage("Must select at least one NSFW level."); return null; }

        var mediaType = Models.MediaType.None;
        if (chbDownloadImage.Checked) mediaType |= Models.MediaType.Image;
        if (chbDownloadVideo.Checked) mediaType |= Models.MediaType.Video;
        if (mediaType == Models.MediaType.None) { AddMessage("Must select at least one media type."); return null; }

        var p = new DownloadParameters(_mediator.TargetFolder, "", userNames, nsfwLevels, mediaType, chbAlwaysDownloadLatest.Checked, GetLimit());
        p.DownloadedUserMeta = _downloadedUserMeta;
        return p;
    }

    private int GetLimit()
    {
        if (int.TryParse(txtLimit.Text.Trim(), out var limit) && limit > 0)
            return limit;
        AddMessage($"Invalid limit value \"{txtLimit.Text}\", using default 500.");
        txtLimit.Text = "500";
        return 500;
    }

    private void AddMessage(string message)
    {
        try
        {
            Invoke(() =>
            {
                listBoxMessages.BeginUpdate();
                listBoxMessages.Items.Add(message);
                listBoxMessages.TopIndex = listBoxMessages.Items.Count - 1;
                listBoxMessages.EndUpdate();
            });
        }
        catch
        {
        }
    }

    private void AppendMessage(string message)
    {
        Invoke(() =>
        {
            try
            {
                listBoxMessages.BeginUpdate();
                var lastIdx = listBoxMessages.Items.Count - 1;
                if (lastIdx >= 0)
                    listBoxMessages.Items[lastIdx] = listBoxMessages.Items[lastIdx] + message;
            }
            catch { }
            finally { listBoxMessages.EndUpdate(); }
        });
    }

    private void UpdateDownloadingCounter(int downloadingCount)
    {
        Invoke(() => lblDownloadingCounter.Text = downloadingCount == -1 ? "Not downloading anything." : "Downloading: " + downloadingCount);
    }

    private static List<string> Format(List<DownloadResult> results)
    {
        int[] maxColLen = ["UserName".Length, "Success".Length, "Target".Length, "Failed".Length, "Skipped".Length];
        foreach (var item in results)
        {
            maxColLen[0] = Math.Max(item.UserName.Length, maxColLen[0]);
            maxColLen[1] = Math.Max(CountDigits(item.ActualDownloadCount), maxColLen[1]);
            maxColLen[2] = Math.Max(CountDigits(item.DownloadTargetCount), maxColLen[2]);
            maxColLen[3] = Math.Max(CountDigits(item.FailedUrls.Count), maxColLen[3]);
            maxColLen[4] = Math.Max(CountDigits(item.SkippedCount), maxColLen[4]);
        }
        string formatString = "|";
        for (int i = 0; i < maxColLen.Length; i++)
            formatString += "{" + i + ",-" + maxColLen[i] + "}|";

        var verticalLineLength = 2 + maxColLen.Sum() + maxColLen.Length - 1;
        List<string> lines = new();
        lines.Add(new string('-', verticalLineLength));
        lines.Add(string.Format(formatString, "UserName", "Success", "Target", "Failed", "Skipped"));
        lines.Add(new string('-', verticalLineLength));
        foreach (var r in results) { lines.Add(string.Format(formatString, r.UserName, r.ActualDownloadCount, r.DownloadTargetCount, r.FailedUrls.Count, r.SkippedCount)); lines.Add(new string('-', verticalLineLength)); }
        return lines;
    }

    private static int CountDigits(int n) => (int)Math.Floor(Math.Log10(n) + 1);

    private void listBoxMessages_DoubleClick(object? sender, EventArgs e)
    {
        if (listBoxMessages.SelectedIndex != -1)
        {
            string txt = listBoxMessages.SelectedItem?.ToString() ?? "";
            if (txt.Contains("https:"))
            {
                var i = txt.IndexOf("https");
                Clipboard.SetText(txt.Contains("Failed") ? txt[i..] : txt[i..txt.IndexOf(" to ")]);
            }
            else Clipboard.SetText(txt);
        }
    }
}
