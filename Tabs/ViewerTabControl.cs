using System.Diagnostics;
using CivitaiImageDownloader.Util;
using NReco.VideoConverter;

namespace CivitaiImageDownloader.Tabs;

public partial class ViewerTabControl : UserControl
{
    private readonly AppMediator _mediator;
    private Panel? _selectedViewerTile;
    private CancellationTokenSource? _loadCts;
    private float _zoomFactor = 1.0f;

    public ViewerTabControl(AppMediator mediator)
    {
        _mediator = mediator;
        InitializeComponent();
        listBoxViewerUsers.SelectedIndexChanged += listBoxViewerUsers_SelectedIndexChanged;
        listBoxViewerUsers.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                var index = listBoxViewerUsers.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                    listBoxViewerUsers.SelectedIndex = index;
            }
        };

        var ctxMenu = new ContextMenuStrip();
        var itemDownload = new ToolStripMenuItem("Show in Download Tab");
        itemDownload.Click += (s, e) =>
        {
            if (listBoxViewerUsers.SelectedItem is string user)
                _mediator.CopyUsernamesToDownload(user);
        };
        var itemVideo = new ToolStripMenuItem("Show in Video Tab");
        itemVideo.Click += (s, e) =>
        {
            if (listBoxViewerUsers.SelectedItem is string user)
            {
                _mediator.CopyUsernamesToVideo(user);
                _mediator.RequestSwitchToVideoTab();
            }
        };
        ctxMenu.Items.Add(itemDownload);
        ctxMenu.Items.Add(itemVideo);
        listBoxViewerUsers.ContextMenuStrip = ctxMenu;

        flowLayoutPanelViewer.MouseWheel += (s, e) =>
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                _zoomFactor = Math.Clamp(_zoomFactor + (e.Delta > 0 ? 0.1f : -0.1f), 0.3f, 3.0f);
                ApplyZoomToTiles();
            }
        };
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
        const int batchSize = 8;
        for (int i = 0; i < files.Count; i += batchSize)
        {
            if (token.IsCancellationRequested) return;

            var batch = files.Skip(i).Take(batchSize).Select(async file =>
            {
                bool isVideo = IsVideoFile(file);
                Image? thumb = isVideo
                    ? await Task.Run(() => LoadVideoThumbnail(file), token)
                    : await Task.Run(() => LoadThumbnailImage(file), token);
                return (file, isVideo, thumb);
            }).ToArray();

            var results = await Task.WhenAll(batch);

            foreach (var (file, isVideo, thumb) in results)
            {
                if (token.IsCancellationRequested) return;
                var tile = CreateThumbnailTile(file, isVideo, thumb);
                flowLayoutPanelViewer.Controls.Add(tile);
                index++;
                progressBarViewer.Value = index;
            }

            if (i + batchSize < files.Count)
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

    private void ApplyZoomToTiles()
    {
        flowLayoutPanelViewer.SuspendLayout();
        foreach (Panel tile in flowLayoutPanelViewer.Controls.OfType<Panel>())
        {
            var z = _zoomFactor;
            tile.Size = new Size((int)(180 * z), (int)(240 * z));
            if (tile.Controls.Count >= 2)
            {
                var pb = tile.Controls[0];
                pb.Size = new Size((int)(132 * z), (int)(176 * z));
                pb.Location = new Point((int)(21 * z), (int)(2 * z));
                var lbl = tile.Controls[1];
                lbl.Size = new Size((int)(174 * z), (int)(53 * z));
                lbl.Location = new Point(0, (int)(181 * z));
                lbl.Font = new Font("Segoe UI", Math.Max(6, 8 * z));
            }
        }
        flowLayoutPanelViewer.ResumeLayout();
    }

    private Panel CreateThumbnailTile(string filePath, bool isVideo, Image? thumbnail)
    {
        var z = _zoomFactor;
        // tile: 180x240 * z, image: 132x176 * z (w:h = 3:4), 3px border via padding
        var panel = new Panel { Size = new Size((int)(180 * z), (int)(240 * z)), Margin = new Padding(4), BackColor = Color.White, Tag = filePath, Padding = new Padding(3) };
        var pictureBox = new PictureBox { Size = new Size((int)(132 * z), (int)(176 * z)), Location = new Point((int)(21 * z), (int)(2 * z)), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };

        if (thumbnail != null)
            pictureBox.Image = thumbnail;
        else
            pictureBox.BackColor = Color.Gray;

        var label = new Label { Text = Path.GetFileName(filePath), Size = new Size((int)(174 * z), (int)(53 * z)), Location = new Point(0, (int)(181 * z)), TextAlign = ContentAlignment.TopCenter, AutoEllipsis = true, Font = new Font("Segoe UI", Math.Max(6, 8 * z)) };

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
