using System.Diagnostics;
using CivitaiImageDownloader.Util;
using NReco.VideoConverter;

namespace CivitaiImageDownloader.Tabs;

public partial class ViewerTabControl : UserControl
{
    private readonly AppMediator _mediator;
    private Panel? _selectedViewerTile;
    private CancellationTokenSource? _loadCts;

    public ViewerTabControl(AppMediator mediator)
    {
        _mediator = mediator;
        InitializeComponent();
        listBoxViewerUsers.SelectedIndexChanged += listBoxViewerUsers_SelectedIndexChanged;
    }

    public async Task PopulateViewerUserList()
    {
        var targetFolder = _mediator.TargetFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!Directory.Exists(targetFolder))
            return;

        var users = await Task.Run(() =>
        {
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

            var userList = new List<string>();
            foreach (var root in metaRoots)
            {
                foreach (var dir in Directory.GetDirectories(root))
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName.StartsWith("!"))
                        continue;
                    userList.Add(dirName);
                }
            }
            userList.Sort();
            return userList;
        });

        if (InvokeRequired)
            Invoke(() =>
            {
                listBoxViewerUsers.Items.Clear();
                foreach (var u in users)
                    listBoxViewerUsers.Items.Add(u);
            });
        else
        {
            listBoxViewerUsers.Items.Clear();
            foreach (var u in users)
                listBoxViewerUsers.Items.Add(u);
        }
    }

    private async void listBoxViewerUsers_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (listBoxViewerUsers.SelectedItem is not string userName)
            return;

        // cancel any previous load
        _loadCts?.Cancel();
        var cts = new CancellationTokenSource();
        _loadCts = cts;
        var token = cts.Token;

        var targetFolder = _mediator.TargetFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var folder = FolderHelper.GetFolder(targetFolder, userName);
        if (string.IsNullOrEmpty(folder))
            return;

        flowLayoutPanelViewer.Controls.Clear();
        flowLayoutPanelViewer.AutoScrollPosition = Point.Empty;

        var loadingLabel = new Label
        {
            Text = "Loading...",
            AutoSize = true,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 10F)
        };
        flowLayoutPanelViewer.Controls.Add(loadingLabel);
        progressBarViewer.Visible = true;

        var files = await Task.Run(() =>
        {
            return Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
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
                    return ext switch
                    {
                        ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".mp4" or ".webm" or ".mov" or ".avi" => true,
                        _ => false
                    };
                })
                .ToList();
        });

        flowLayoutPanelViewer.Controls.Clear();
        if (token.IsCancellationRequested) return;
        if (files.Count == 0)
        {
            flowLayoutPanelViewer.Controls.Add(new Label { Text = "No media files found.", AutoSize = true, ForeColor = Color.Gray });
            progressBarViewer.Visible = false;
            return;
        }

        progressBarViewer.Maximum = files.Count;
        progressBarViewer.Value = 0;

        int index = 0;
        foreach (var file in files)
        {
            if (token.IsCancellationRequested) return;

            Image? thumb = null;
            bool isVideo = IsVideoFile(file);
            if (isVideo)
                thumb = await Task.Run(() => LoadVideoThumbnail(file), token);
            else
                thumb = await Task.Run(() => LoadThumbnailImage(file), token);

            var tile = CreateThumbnailTile(file, isVideo, thumb);
            flowLayoutPanelViewer.Controls.Add(tile);
            index++;
            progressBarViewer.Value = index;
            if (index % 5 == 0)
                await Task.Delay(1);
        }
        flowLayoutPanelViewer.AutoScrollPosition = Point.Empty;
        progressBarViewer.Visible = false;
    }

    private static bool IsVideoFile(string filePath) => Path.GetExtension(filePath).ToLower() switch
    {
        ".mp4" or ".webm" or ".mov" or ".avi" => true,
        _ => false
    };

    private static Image? LoadThumbnailImage(string filePath)
    {
        try { using var img = Image.FromFile(filePath); return img.GetThumbnailImage(132, 176, null, IntPtr.Zero); }
        catch { return null; }
    }

    private Panel CreateThumbnailTile(string filePath, bool isVideo, Image? thumbnail)
    {
        // tile: 180x240, image: 132x176 (w:h = 3:4), 3px border via padding
        var panel = new Panel { Size = new Size(180, 240), Margin = new Padding(4), BackColor = Color.White, Tag = filePath, Padding = new Padding(3) };
        var pictureBox = new PictureBox { Size = new Size(132, 176), Location = new Point(21, 2), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };

        if (thumbnail != null)
            pictureBox.Image = thumbnail;
        else
            pictureBox.BackColor = Color.Gray;

        var label = new Label { Text = Path.GetFileName(filePath), Size = new Size(174, 53), Location = new Point(0, 181), TextAlign = ContentAlignment.TopCenter, AutoEllipsis = true, Font = new Font("Segoe UI", 8F) };

        panel.Controls.Add(pictureBox);
        panel.Controls.Add(label);

        EventHandler selectTile = (s, e) =>
        {
            if (_selectedViewerTile != null && _selectedViewerTile != panel)
                _selectedViewerTile.BackColor = Color.White;
            _selectedViewerTile = panel;
            panel.BackColor = Color.Orange;
        };
        panel.Click += selectTile;
        pictureBox.Click += selectTile;
        label.Click += selectTile;

        pictureBox.DoubleClick += (s, e) =>
        {
            try { Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); }
            catch { }
        };
        panel.DoubleClick += (s, e) =>
        {
            try { Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); }
            catch { }
        };

        return panel;
    }

    private static Image? LoadVideoThumbnail(string filePath)
    {
        try
        {
            var tmpFile = Path.GetTempFileName() + ".jpg";
            var ffmpeg = new FFMpegConverter();
            ffmpeg.GetVideoThumbnail(filePath, tmpFile, 0);
            if (File.Exists(tmpFile))
            {
                var img = Image.FromFile(tmpFile);
                try { File.Delete(tmpFile); } catch { }
                return img;
            }
        }
        catch { }
        return null;
    }
}
