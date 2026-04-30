using CivitaiImageDownloader.Models;
using CivitaiImageDownloader.Util;
using System.Diagnostics;
using Utils = CivitaiImageDownloader.Util.Utils;

namespace CivitaiImageDownloader;

public partial class MainForm : Form
{
    private const string DefaultTargetFolder = @"D:\AI\CivitAI\";
    private readonly List<DownloadResult> _downloadResults = [];
    private string currentUserFolder;

    private bool _stopping = false;
    private VideoCompressor? videoCompressor;
    private List<UserMeta> downloadedUserMeta = new();

    public MainForm()
    {
        InitializeComponent();

        txtTargetFolder.Text = DefaultTargetFolder;

        // Wire up History tab buttons
        btnCopyToDownloadTab.Click += btnCopyToDownloadTab_Click;
        btnCopyToVideoTab.Click += btnCopyToVideoTab_Click;

        dgvUserHistory.CellFormatting += (s, e) =>
        {
            if (e.ColumnIndex == FolderSize.Index && e.Value is double size)
            {
                e.Value = size.ToString("F2") + " MB";
                e.FormattingApplied = true;
            }
            else if (e.ColumnIndex == FileCount.Index && e.Value is int count)
            {
                e.Value = count.ToString("N0");
                e.FormattingApplied = true;
            }
        };

        dgvUserHistory.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex < 0) return;
            var userName = dgvUserHistory.Rows[e.RowIndex].Cells[UserName.Index].Value?.ToString();
            if (string.IsNullOrEmpty(userName)) return;
            var targetFolder = txtTargetFolder.Text;
            var folder = FolderHelper.GetFolder(targetFolder, userName!);
            if (!string.IsNullOrEmpty(folder))
                Process.Start("explorer.exe", folder);
        };

        listBoxActionHistory.DoubleClick += (s, e) =>
        {
            if (listBoxActionHistory.SelectedItem is CivitaiImageDownloader.Models.UsernameHistoryEntry entry)
            {
                txtUsernames.Text = entry.UsernamesConcatenated;
                mainTabControl.SelectedTab = DownloadPage;
            }
        };

        // Load history when switching to History tab
        mainTabControl.SelectedIndexChanged += (s, e) =>
        {
            if (mainTabControl.SelectedTab == tabPage1)
            {
                LoadActionHistory();
                _ = PopulateUserHistory();
            }
        };
    }

    private void LoadActionHistory()
    {
        var targetFolder = txtTargetFolder.Text;
        if (!Directory.Exists(targetFolder))
            return;

        var entries = CivitaiImageDownloader.Util.UsernameHistoryManager.LoadHistory(targetFolder);
        listBoxActionHistory.Items.Clear();
        foreach (var entry in entries)
        {
            listBoxActionHistory.Items.Add(entry);
        }
    }

    private async Task PopulateUserHistory()
    {
        var targetFolder = txtTargetFolder.Text.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!Directory.Exists(targetFolder))
            return;

        var results = await Task.Run(() =>
        {
            var list = new List<UserMeta>();

            // collect all meta-roots: the target folder and any !-prefixed folders found recursively
            var metaRoots = new List<string> { targetFolder };
            void CollectMetaRoots(string folder)
            {
                foreach (var dir in Directory.GetDirectories(folder))
                {
                    if (Path.GetFileName(dir).StartsWith("!"))
                    {
                        metaRoots.Add(dir);
                        CollectMetaRoots(dir);
                    }
                }
            }
            CollectMetaRoots(targetFolder);

            // for each meta-root, its immediate non-! subdirectories are user folders
            foreach (var root in metaRoots)
            {
                foreach (var dir in Directory.GetDirectories(root))
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName.StartsWith("!"))
                        continue;

                    var meta = BuildUserMeta(dir, Path.GetFileName(root));
                    if (meta != null)
                        list.Add(meta);
                }
            }

            list.Sort((a, b) => b.FolderSize.CompareTo(a.FolderSize));
            return list;
        });

        Invoke(() =>
        {
            dgvUserHistory.Rows.Clear();
            foreach (var r in results)
            {
                dgvUserHistory.Rows.Add(r.UserName, r.FileCount, r.FolderSize, r.ParentFolder);
            }
        });
    }

    private static UserMeta? BuildUserMeta(string folder, string parentFolder)
    {
        var allMediaFiles = Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
            .Where(f =>
            {
                // skip files inside !-prefixed directories at any depth
                var dirPath = Path.GetDirectoryName(f);
                if (dirPath != null && dirPath.Split(Path.DirectorySeparatorChar).Any(seg => seg.StartsWith("!")))
                    return false;

                var ext = Path.GetExtension(f).ToLower();
                if (ext == ".json" || ext == ".txt")
                    return false;
                if (Path.GetFileName(f).EndsWith(".json.zip"))
                    return false;
                return true;
            }).ToArray();

        if (allMediaFiles.Length == 0)
            return null;

        long totalSize = 0;
        foreach (var f in allMediaFiles)
        {
            try { totalSize += new FileInfo(f).Length; }
            catch { }
        }

        var folderSizeMB = totalSize / (1024.0 * 1024.0);
        return new UserMeta(Path.GetFileName(folder), allMediaFiles.Length, folderSizeMB, parentFolder);
    }

    private void RecordDownloadHistory()
    {
        var targetFolder = txtTargetFolder.Text;
        if (!Directory.Exists(targetFolder))
            return;
        var usernames = txtUsernames.Text.Trim();
        if (string.IsNullOrEmpty(usernames))
            return;
        CivitaiImageDownloader.Util.UsernameHistoryManager.RecordAction(targetFolder, "Download", usernames);
        LoadActionHistory();
    }

    private void RecordVideoHistory()
    {
        var targetFolder = txtTargetFolder.Text;
        if (!Directory.Exists(targetFolder))
            return;
        var usernames = txtVideoProcessingUsers.Text.Trim();
        if (string.IsNullOrEmpty(usernames))
            return;
        CivitaiImageDownloader.Util.UsernameHistoryManager.RecordAction(targetFolder, "VideoCompress", usernames);
        LoadActionHistory();
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

        RecordDownloadHistory();

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

        // history already recorded before download started
    }

    private void btnDeleteInfoFiles_Click(object sender, EventArgs e)
    {
        var targetFolder = txtTargetFolder.Text;
        if (!Directory.Exists(targetFolder))
        {
            MessageBox.Show(this, "Invalid target folder.");
            return;
        }

        var uns = txtUsernames.ParseUserNames();
        if (uns.Count == 0) { return; }
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

    private void listBoxVideoProcessingMessages_DoubleClick(object sender, EventArgs e)
    {
        if (videoCompressor == null || string.IsNullOrWhiteSpace(videoCompressor.UserName))
        {
            return;
        }
        currentUserFolder = FolderHelper.GetFolder(txtTargetFolder.Text, videoCompressor.UserName);
        if (listBoxVideoProcessingMessages.SelectedIndex != -1 && Directory.Exists(currentUserFolder))
        {
            string selectedItemText = listBoxVideoProcessingMessages.SelectedItem?.ToString() ?? "";
            if (selectedItemText.Contains("Compress video:"))
            {
                var parts = selectedItemText.Split("Compress video:");
                if (parts.Length >= 2)
                {
                    parts = parts[1].Split(" ... ");
                    if (parts.Length >= 2)
                    {
                        var candidate = parts[0].Trim();
                        var path = Path.Combine(currentUserFolder, candidate);
                        Process.Start(new ProcessStartInfo(path)
                        {
                            UseShellExecute = true
                        });
                        Clipboard.SetText(path);
                    }
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
        currentUserFolder = "";
        var targetFolder = txtTargetFolder.Text;
        if (!Directory.Exists(targetFolder))
        {
            MessageBox.Show(this, "Invalid target folder.");
            return;
        }

        var uns = txtUsernames.ParseUserNames();
        if (uns.Count == 0) { return; }
        var folder = FolderHelper.GetFolder(txtTargetFolder.Text, uns[0]);
        if (!string.IsNullOrEmpty(folder))
        {
            AddMessage("Use folder: " + folder);
            currentUserFolder = folder;
        }
        else
        {
            AddMessage("Folder not found: " + folder);
            return;
        }

        Process.Start("explorer.exe", folder);
    }

    private void btnOpenAllUserFolders_Click(object sender, EventArgs e)
    {
        currentUserFolder = "";
        var targetFolder = txtTargetFolder.Text;
        if (!Directory.Exists(targetFolder))
        {
            MessageBox.Show(this, "Invalid target folder.");
            return;
        }

        var uns = txtUsernames.ParseUserNames();
        if (uns.Count == 0) { return; }
        foreach (var userName in uns)
        {
            if (userName.Length == 0)
            {
                continue;
            }
            var folder = FolderHelper.GetFolder(txtTargetFolder.Text, userName);
            if (!string.IsNullOrEmpty(folder))
            {
                AddMessage("Use folder: " + folder);
                currentUserFolder = folder;
            }
            else
            {
                AddMessage("Folder not found: " + folder);
                return;
            }

            Process.Start("explorer.exe", folder);
        }
    }

    private async void btnMarkDeletedFilesNoRedownload_Click(object sender, EventArgs e)
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
        List<string> userNames = txtUsernames.ParseUserNames();
        if (userNames.Count == 0) { return null; }
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

        var p = new DownloadParameters(targetFolder, "", userNames,
            nsfwLevels, mediaType, chbAlwaysDownloadLatest.Checked);
        p.DownloadedUserMeta = downloadedUserMeta;
        return p;
    }

    private void AddMessage(string message)
    {
        Invoke(() =>
        {
            listBoxMessages.Items.Insert(0, message);
        });
    }

    private void AppendMessage(string message)
    {
        Invoke(() =>
        {
            try
            {
                var item = (string)listBoxMessages.Items[0];
                listBoxMessages.Items[0] = item + message;
            }
            catch (Exception e)
            {
                // do nothing
            }
        });
    }

    private void AddVideoProcessingMessage(string message)
    {
        Invoke(() =>
        {
            listBoxVideoProcessingMessages.Items.Insert(0, message);
        });
    }

    private void AppendVideoProcessingMessage(string message)
    {
        Invoke(() =>
        {
            try
            {
                var item = (string)listBoxVideoProcessingMessages.Items[0];
                listBoxVideoProcessingMessages.Items[0] = item + message;
            }
            catch (Exception e)
            {
                // do nothing
            }
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

    private void btnCopyFromDownloadTab_Click(object sender, EventArgs e)
    {
        txtVideoProcessingUsers.Text = txtUsernames.Text;
    }

    private void btnCopyToDownloadTab_Click(object? sender, EventArgs e)
    {
        if (listBoxActionHistory.SelectedItem is CivitaiImageDownloader.Models.UsernameHistoryEntry entry)
        {
            txtUsernames.Text = entry.UsernamesConcatenated;
        }
    }

    private void btnCopyToVideoTab_Click(object? sender, EventArgs e)
    {
        if (listBoxActionHistory.SelectedItem is CivitaiImageDownloader.Models.UsernameHistoryEntry entry)
        {
            txtVideoProcessingUsers.Text = entry.UsernamesConcatenated;
        }
    }

    private async void btnCompressVideo_Click(object sender, EventArgs e)
    {
        Invoke(listBoxVideoProcessingMessages.Items.Clear);

        videoCompressor = null;

        List<string> names = txtVideoProcessingUsers.ParseUserNames();
        if (names.Count == 0) { return; }
        if (txtVideoProcessingUsers.Text.Length == 0 && names.Count == 0 && !Path.Exists(txtTargetFolder.Text))
        {
            var result = MessageBox.Show("Do you want to compress everything in " + txtTargetFolder.Text + "?", "Warning", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                names = [txtTargetFolder.Text];
            }
        }
        foreach (var name in names)
        {
            var mode = VideoProcessInputMode.UserName;
            if (Path.Exists(name))
            {
                // it is a file path
                mode = VideoProcessInputMode.FilePath;
            }
            if (_stopping && videoCompressor != null)
            {
                videoCompressor.ShouldStop = true;
                AddMessage("Video compression stopped.");
                break;
            }
            videoCompressor = new VideoCompressor(txtTargetFolder.Text, name, mode);
            videoCompressor.RaiseAddMessage += AddVideoProcessingMessage;
            videoCompressor.RaiseAppendMessage += AppendVideoProcessingMessage;
            await videoCompressor.Run();
            videoCompressor.RaiseAppendMessage -= AddVideoProcessingMessage;
            videoCompressor.RaiseAppendMessage -= AppendVideoProcessingMessage;
            videoCompressor.Dispose();
        }
        AddVideoProcessingMessage("ALL DONE!");

        RecordVideoHistory();
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
    {
        // Check if the data being dragged is a file (FileDrop)
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Show the "Copy" cursor
            e.Effect = DragDropEffects.Copy;
        }
        else
        {
            // Show the "No Entry" cursor
            e.Effect = DragDropEffects.None;
        }
    }

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        // 3. Extract the data (an array of file paths)
        string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
        List<string> userNames = [];
        List<string> fileNames = [];
        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                // use this path's last piece as user name
                userNames.Add(Path.GetFileNameWithoutExtension(path));
            }
            else if (File.Exists(path))
            {
                fileNames.Add(path);
            }
        }
        if (mainTabControl.SelectedTab == tabPage2)
        {
            if (userNames.Count > 0)
                txtVideoProcessingUsers.Text = string.Join(",", userNames);
            else if (fileNames.Count > 0)
                txtVideoProcessingUsers.Text = string.Join(",", fileNames);
        }
        else
        {
            txtUsernames.Text = string.Join(",", userNames);
        }
    }

    private void btnMoveUsersToRating_Click(object sender, EventArgs e)
    {
        currentUserFolder = "";
        bool r3 = chb3Star.Checked;
        bool r4 = chb4Star.Checked;
        bool r45 = chb4p5Star.Checked;
        bool r5 = chb5Star.Checked;
        bool r6 = chb6Star.Checked;
        var bools = new List<bool> { r3, r4, r45, r5, r6 };
        if (bools.Count(b => b) > 1)
        {
            MessageBox.Show("Can't move the user to this rating folder: multiple ratings are selected.");
            return;
        }
        var subFolder = getSubFolderByRating(r3, r4, r45, r5, r6);
        if (subFolder == null)
        {
            AddMessage("Rating folder does not exist.");
            return;
        }
        var userNames = txtUsernames.ParseUserNames();
        if (userNames.Count == 0) { return; }
        var newBase = Path.Combine(txtTargetFolder.Text, subFolder);
        if (!Path.Exists(newBase))
        {
            Directory.CreateDirectory(newBase);
            AddMessage($"Created dir: {newBase}");
        }
        foreach (var userName in userNames)
        {
            var source = FolderHelper.GetFolder(txtTargetFolder.Text, userName);
            var destination = Path.Combine(newBase, userName);
            if (source.Equals(destination))
            {
                currentUserFolder = source;
                AddMessage($"No action for {userName}: Already in {destination}");
            }
            else if (string.IsNullOrEmpty(source))
            {
                AddMessage($"No action for {userName}: source folder {source} does not exist.");
            }
            else
            {
                if (!Path.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                    AddMessage($"Created dir: {destination}");
                }
                AddMessage($"Moved {Utils.MergeDirectories(source, destination)} files from {source} to {destination}");
            }
        }
    }

    private static string getSubFolderByRating(bool r3, bool r4, bool r45, bool r5, bool r6)
    {
        if (r3)
        {
            return "!3";
        }
        if (r4)
        {
            return "!4";
        }
        if (r45)
        {
            return "!4.5";
        }
        if (r5)
        {
            return "!5";
        }
        if (r6)
        {
            return "!6";
        }
        return null;
    }

    private void btnCompressInfo_Click(object sender, EventArgs e)
    {
        var userNames = UserNameHelper.ParseUserNames(txtUsernames);
        if (userNames.Count == 0)
            return;

        var targetFolder = txtTargetFolder.Text;
        foreach (var user in userNames)
        {
            var folder = FolderHelper.GetFolder(targetFolder, user);
            if (string.IsNullOrEmpty(folder))
            {
                AddMessage($"Skipping {user}: folder not found");
                continue;
            }
            Utils.CompressInfoFiles(folder);
            AddMessage($"Compressed info files for {user}");
        }
        AddMessage("Compress Info Files complete.");
    }

    private void btnReloadExistingUserList_Click(object sender, EventArgs e)
    {

    }
}
