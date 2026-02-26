using System.Diagnostics;

using CivitaiImageDownloader.Models;
using CivitaiImageDownloader.Util;

namespace CivitaiImageDownloader;

public partial class Form1 : Form
{
    private const string DefaultTargetFolder = @"D:\AI\CivitAI\";
    private readonly List<DownloadResult> _downloadResults = [];

    private bool _stopping = false;

    public Form1()
    {
        InitializeComponent();

        txtTargetFolder.Text = DefaultTargetFolder;
        cbbVideoDownloadMode.Items.AddRange(Enum.GetValues<VideoDownloadMode>().OfType<Object>().ToArray());
        cbbVideoDownloadMode.SelectedItem = VideoDownloadMode.Auto;
    }

    private async void btnDownload_Click(object sender, EventArgs e)
    {
        _stopping = false;

        Invoke(listBoxMessages.Items.Clear);
        UpdateDownloadingCounter(-1);
        _downloadResults.Clear();
        var parameters = CreateDownloadParameters();
        if (parameters == null)
        {
            return;
        }

        Downloader? dl = null;
        foreach (var un in parameters.UserNames)
        {
            if (_stopping && dl != null)
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
            _downloadResults.Add(result);
            dl.RaiseMessage -= AddMessage;
            dl.UpdateDownloadingCounter -= UpdateDownloadingCounter;
            dl.Dispose();
        }

        // print summary
        AddMessage("====SUMMARY====");
        foreach (var r in Format(_downloadResults))
        {
            AddMessage(r);
        }
        AddMessage("====SUMMARY====");
    }

    private void btnDeleteInfoFiles_Click(object sender, EventArgs e)
    {
        var targetFolder = txtTargetFolder.Text;
        if (!Directory.Exists(targetFolder))
        {
            MessageBox.Show(this, "Invalid target folder.");
            return;
        }
        var un = txtUsername.Text;
        if (un == null) { return; }

        var uns = un.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var s in uns)
        {
            var folder = Path.Combine(targetFolder, s);
            var filePaths = Directory.GetFiles(folder, "*.txt");
            foreach (var f in filePaths)
            {
                File.Delete(f);
                AddMessage("Deleted file: " + f);
            }
        }
    }

    private void listBoxMessages_DoubleClick(object sender, EventArgs e)
    {
        if (listBoxMessages.SelectedIndex != -1)
        {
            string selectedItemText = listBoxMessages.SelectedItem?.ToString() ?? "";
            if (selectedItemText.Contains("https:"))
            {
                var i = selectedItemText.IndexOf("https");
                if (selectedItemText.Contains("Failed"))
                {
                    var txt = selectedItemText.Substring(i);
                    Clipboard.SetText(txt);
                }
                else
                {
                    var j = selectedItemText.IndexOf(" to ");
                    var txt = selectedItemText.Substring(i, j - i);
                    Clipboard.SetText(txt);
                }
            }
            else
            {
                Clipboard.SetText(selectedItemText);
            }
        }
    }

    private void btnCopyFailedUrls_Click(object sender, EventArgs e)
    {
        var failedUrls = _downloadResults.SelectMany(r => r.FailedUrls).ToList();
        if (failedUrls.Count == 0)
        {
            AddMessage("No failed URLs");
            return;
        }
        Clipboard.SetText(string.Join(Environment.NewLine, failedUrls));
        AddMessage($"Copied {failedUrls.Count} failed URLs to clipboard.");
    }

    private void btnOpenFirstUserFolder_Click(object sender, EventArgs e)
    {
        var targetFolder = txtTargetFolder.Text;
        if (!Directory.Exists(targetFolder))
        {
            MessageBox.Show(this, "Invalid target folder.");
            return;
        }

        var un = txtUsername.Text;
        if (un == null) { return; }

        var uns = un.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (uns.Length == 0)
        {
            MessageBox.Show(this, "Missing user name.");
            return;
        }

        var folder = FolderHelper.GetFolder(txtTargetFolder.Text, uns[0]);
        if (!string.IsNullOrEmpty(folder))
        {
            AddMessage("Use folder: " + folder);
        }
        else
        {
            AddMessage("Folder not found: " + folder);
            return;
        }

        Process.Start("explorer.exe", folder);
    }

    private async void btnMarkDeletedFilesNoRedownload_ClickAsync(object sender, EventArgs e)
    {
        Invoke(listBoxMessages.Items.Clear);
        _downloadResults.Clear();
        var parameters = CreateDownloadParameters();
        if (parameters == null)
        {
            return;
        }

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
        {
            AddMessage($"For user [{un}], marked {result.Count(r => !r.IsExists)}/{result.Count} files non-exist.");
        }
    }

    private DownloadParameters? CreateDownloadParameters()
    {

        var targetFolder = txtTargetFolder.Text;
        if (!Directory.Exists(targetFolder))
        {
            MessageBox.Show(this, "Invalid target folder.");
            return null;
        }

        var userNameConcat = txtUsername.Text.Trim();
        if (userNameConcat == null) { return null; }

        List<string> userNames = userNameConcat.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(u => u.Trim()).Where(u => !string.IsNullOrWhiteSpace(u)).ToList();
        List<string> nsfwLevels = [];
        if (chbNsfw.Checked)
        {
            nsfwLevels.Add("X");
        }
        if (chbMature.Checked)
        {
            nsfwLevels.Add("Mature");
        }
        if (chbNormal.Checked)
        {
            nsfwLevels.Add("Soft");
        }
        if (chbChildLevel.Checked)
        {
            nsfwLevels.Add("None");
        }
        if (nsfwLevels.Count == 0)
        {
            AddMessage("Must select at least one NSFW level.");
            return null;
        }

        var mediaType = MediaType.None;
        if (chbDownloadImage.Checked)
        {
            mediaType |= MediaType.Image;
        }
        if (chbDownloadVideo.Checked)
        {
            mediaType |= MediaType.Video;
        }
        if (mediaType == MediaType.None)
        {
            AddMessage("Must select at least one media type.");
            return null;
        }

        return new DownloadParameters(targetFolder, "", userNames, nsfwLevels, mediaType,
            chbAlwaysDownloadLatest.Checked, (VideoDownloadMode)cbbVideoDownloadMode.SelectedItem!);
    }

    private void AddMessage(string message)
    {
        Invoke(() =>
        {
            listBoxMessages.Items.Insert(0, message);
        });
    }

    private void UpdateDownloadingCounter(int downloadingCount)
    {
        Invoke(() =>
        {
            if (downloadingCount == -1)
            {
                lblDownloadingCounter.Text = "Not downloading anything.";
            }
            else
            {
                lblDownloadingCounter.Text = "Downloading: " + downloadingCount;
            }
        });
    }

    private static List<string> Format(List<DownloadResult> results)
    {
        int[] maxColLen = new int[5] { "UserName".Length, "Success".Length, "Target".Length, "Failed".Length, "Skipped".Length };
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
        {
            formatString += "{" + i + ",-" + maxColLen[i] + "}|";
        }

        // create formatting strings, add all sizes:
        var verticalLineLength = 2 + maxColLen.Sum() + maxColLen.Length - 1;
        List<string> lines = [];
        lines.Add(new('-', verticalLineLength));
        foreach (var r in results)
        {
            lines.Add(string.Format(formatString, r.UserName, r.ActualDownloadCount, r.DownloadTargetCount, r.FailedUrls.Count, r.SkippedCount));
            lines.Add(new('-', verticalLineLength));
        }
        lines.Add(string.Format(formatString, "UserName", "Success", "Target", "Failed", "Skipped"));
        lines.Add(new('-', verticalLineLength));
        return lines;
    }

    private static int CountDigits(int n)
    {
        return (int)Math.Floor(Math.Log10(n) + 1);
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        _stopping = true;
    }

    private async void btnCompressVideo_Click(object sender, EventArgs e)
    {
        _stopping = false;

        Invoke(listBoxMessages.Items.Clear);
        UpdateDownloadingCounter(-1);
        _downloadResults.Clear();
        var parameters = CreateDownloadParameters();
        if (parameters == null)
        {
            return;
        }

        VideoCompressor? vc = null;
        foreach (var un in parameters.UserNames)
        {
            if (_stopping && vc != null)
            {
                vc.ShouldStop = true;
                AddMessage("Video compression stopped.");
                break;
            }
            vc = new VideoCompressor(parameters.TargetFolder, un);
            vc.RaiseMessage += AddMessage;
            await vc.Run();
            vc.RaiseMessage -= AddMessage;
            vc.Dispose();
        }

        // print summary
        AddMessage("====SUMMARY====");
        foreach (var r in Format(_downloadResults))
        {
            AddMessage(r);
        }
        AddMessage("====SUMMARY====");
    }

    private void btnCopyAllSubdirNames_Click(object sender, EventArgs e)
    {
        var rootFolder = txtTargetFolder.Text;
        if (!Directory.Exists(rootFolder))
        {
            MessageBox.Show(this, "Invalid target folder.");
            return;
        }
        var allDirs = Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories);
        var dirNames = allDirs.Select(d => Path.GetFileName(d)).Where(d => !d.StartsWith('!')).Distinct().ToList();
        Clipboard.SetText(string.Join(",", dirNames));
    }
}
