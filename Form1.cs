using System.Diagnostics;
using System.Net;

using CivitaiImageDownloader.Models;

namespace CivitaiImageDownloader;

public partial class Form1 : Form
{
    private const string DefaultTargetFolder = @"D:\AI\CivitAI\";
    private readonly List<DownloadResult> _downloadResults = [];

    public Form1()
    {
        InitializeComponent();

        txtTargetFolder.Text = DefaultTargetFolder;
    }

    private async void btnDownload_Click(object sender, EventArgs e)
    {
        Invoke(listBoxMessages.Items.Clear);
        _downloadResults.Clear();
        bool isGood = CreateDownloadParameters(out var targetFolder, out var userNames, out var nsfwLevels, out var mediaType);
        if (!isGood)
        {
            return;
        }

        foreach (var un in userNames)
        {
            using var dl = new Downloader(targetFolder, un.Trim(), nsfwLevels, mediaType);
            dl.RaiseMessage += AddMessage;
            var result = await dl.Run();
            _downloadResults.Add(result);
            dl.RaiseMessage -= AddMessage;
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
            Clipboard.SetText(selectedItemText);
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

        Process.Start("explorer.exe", Path.Combine(targetFolder, un));
    }

    private async void btnMarkDeletedFilesNoRedownload_ClickAsync(object sender, EventArgs e)
    {
        Invoke(listBoxMessages.Items.Clear);
        _downloadResults.Clear();
        bool isGood = CreateDownloadParameters(out var targetFolder, out var userNames, out var nsfwLevels, out var mediaType);
        if (!isGood)
        {
            return;
        }

        var results = new Dictionary<string, List<ExistenceResult>>();
        foreach (var un in userNames)
        {
            using var dl = new Downloader(targetFolder, un.Trim(), nsfwLevels, mediaType);
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

    private bool CreateDownloadParameters(out string targetFolder, out string[] userNames, out List<string> nsfwLevels, out MediaType mediaType)
    {
        targetFolder = "";
        userNames = [];
        nsfwLevels = [];
        mediaType = MediaType.None;

        targetFolder = txtTargetFolder.Text;
        if (!Directory.Exists(targetFolder))
        {
            MessageBox.Show(this, "Invalid target folder.");
            return false;
        }

        var un = txtUsername.Text;
        if (un == null) { return false; }

        userNames = un.Split(',', StringSplitOptions.RemoveEmptyEntries);
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
            return false;
        }

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
            return false;
        }

        return true;
    }

    private void AddMessage(string message)
    {
        Invoke(() =>
        {
            listBoxMessages.Items.Insert(0, message);
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
        List<string> lines = [];
        lines.Add(new('-', 2 + maxColLen.Sum()));
        foreach (var r in results)
        {
            lines.Add(string.Format(formatString, r.UserName, r.ActualDownloadCount, r.DownloadTargetCount, r.FailedUrls.Count, r.SkippedCount));
        }
        lines.Add(string.Format(formatString, "UserName", "Success", "Target", "Failed", "Skipped"));
        lines.Add(new('-', 2 + maxColLen.Sum()));
        return lines;
    }

    private static int CountDigits(int n)
    {
        return (int)Math.Floor(Math.Log10(n) + 1);
    }
}
