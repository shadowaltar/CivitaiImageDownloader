using CivitaiImageDownloader.Models;
using CivitaiImageDownloader.Util;
using System.Diagnostics;

namespace CivitaiImageDownloader.Tabs;

public partial class HistoryTabControl : UserControl
{
    private readonly AppMediator _mediator;

    public HistoryTabControl(AppMediator mediator)
    {
        _mediator = mediator;
        InitializeComponent();

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
            var folder = FolderHelper.GetFolder(_mediator.TargetFolder, userName!);
            if (!string.IsNullOrEmpty(folder))
                Process.Start("explorer.exe", folder);
        };

        listBoxActionHistory.DoubleClick += (s, e) =>
        {
            if (listBoxActionHistory.SelectedItem is UsernameHistoryEntry entry)
            {
                _mediator.CopyUsernamesToDownload(entry.UsernamesConcatenated);
                _mediator.RequestSwitchToDownloadTab();
            }
        };

        btnCopyToDownloadTab.Click += (s, e) =>
        {
            if (listBoxActionHistory.SelectedItem is UsernameHistoryEntry entry)
                _mediator.CopyUsernamesToDownload(entry.UsernamesConcatenated);
        };

        btnCopyToVideoTab.Click += (s, e) =>
        {
            if (listBoxActionHistory.SelectedItem is UsernameHistoryEntry entry)
                _mediator.CopyUsernamesToVideo(entry.UsernamesConcatenated);
        };

        btnReloadExistingUserList.Click += (s, e) => _ = PopulateUserFolderStatus();

        _mediator.ActionHistoryChanged += LoadActionHistory;
    }

    public void LoadActionHistory()
    {
        if (!Directory.Exists(_mediator.TargetFolder))
            return;

        var entries = UsernameHistoryManager.LoadHistory(_mediator.TargetFolder);
        if (InvokeRequired)
            Invoke(() => UpdateListBox(entries));
        else
            UpdateListBox(entries);
    }

    private void UpdateListBox(List<UsernameHistoryEntry> entries)
    {
        listBoxActionHistory.Items.Clear();
        foreach (var entry in entries)
            listBoxActionHistory.Items.Add(entry);
    }

    public async Task PopulateUserFolderStatus()
    {
        btnReloadExistingUserList.Enabled = false;
        try
        {
            var targetFolder = _mediator.TargetFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!Directory.Exists(targetFolder))
                return;

            var results = await Task.Run(() =>
            {
                var list = new List<UserMeta>();
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

            void UpdateGrid(List<UserMeta> list)
            {
                dgvUserHistory.Rows.Clear();
                foreach (var r in list)
                    dgvUserHistory.Rows.Add(r.UserName, r.FileCount, r.FolderSize, r.ParentFolder);
            }
            if (InvokeRequired)
                Invoke(() => UpdateGrid(results));
            else
                UpdateGrid(results);
        }
        finally
        {
            if (InvokeRequired)
                Invoke(() => btnReloadExistingUserList.Enabled = true);
            else
                btnReloadExistingUserList.Enabled = true;
        }
    }

    private static UserMeta? BuildUserMeta(string folder, string parentFolder)
    {
        var allMediaFiles = Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
            .Where(f =>
            {
                var dirPath = Path.GetDirectoryName(f);
                if (dirPath != null && dirPath.Length > folder.Length)
                {
                    var relative = dirPath.Substring(folder.Length).TrimStart(Path.DirectorySeparatorChar);
                    if (relative.Split(Path.DirectorySeparatorChar).Any(seg => seg.StartsWith("!")))
                        return false;
                }
                var ext = Path.GetExtension(f).ToLower();
                if (ext == ".json" || ext == ".txt") return false;
                if (Path.GetFileName(f).EndsWith(".json.zip")) return false;
                return true;
            }).ToArray();

        if (allMediaFiles.Length == 0) return null;

        long totalSize = 0;
        foreach (var f in allMediaFiles)
        {
            try { totalSize += new FileInfo(f).Length; }
            catch { }
        }

        return new UserMeta(Path.GetFileName(folder), allMediaFiles.Length, totalSize / (1024.0 * 1024.0), parentFolder);
    }
}
