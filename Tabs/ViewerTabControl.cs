using System.Diagnostics;
using CivitaiImageDownloader.Util;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using NReco.VideoConverter;

namespace CivitaiImageDownloader.Tabs;

public partial class ViewerTabControl : UserControl
{
    private readonly AppMediator _mediator;
    private Panel? _selectedViewerTile;
    private CancellationTokenSource? _loadCts;
    private float _zoomFactor = 1.0f;
    private DateTime _lastZoomTime = DateTime.MinValue;
    private int _pendingZoomDelta;
    private System.Windows.Forms.Timer? _zoomTimer;
    private LibVLC? _libVLC;

    public ViewerTabControl(AppMediator mediator)
    {
        _mediator = mediator;
        Core.Initialize();
        _libVLC = new LibVLC();
        InitializeComponent();
        treeViewNavigator.AfterSelect += treeViewNavigator_AfterSelect;
        treeViewNavigator.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                var node = treeViewNavigator.GetNodeAt(e.Location);
                if (node != null)
                    treeViewNavigator.SelectedNode = node;
            }
        };

        var ctxMenu = new ContextMenuStrip();
        var itemDownload = new ToolStripMenuItem("Show in Download Tab");
        itemDownload.Click += (s, e) =>
        {
            var user = treeViewNavigator.SelectedNode?.Text;
            if (!string.IsNullOrEmpty(user) && treeViewNavigator.SelectedNode?.Parent != null)
                _mediator.CopyUsernamesToDownload(user);
        };
        var itemVideo = new ToolStripMenuItem("Show in Video Tab");
        itemVideo.Click += (s, e) =>
        {
            var user = treeViewNavigator.SelectedNode?.Text;
            if (!string.IsNullOrEmpty(user) && treeViewNavigator.SelectedNode?.Parent != null)
            {
                _mediator.CopyUsernamesToVideo(user);
                _mediator.RequestSwitchToVideoTab();
            }
        };
        ctxMenu.Items.Add(itemDownload);
        ctxMenu.Items.Add(itemVideo);
        treeViewNavigator.ContextMenuStrip = ctxMenu;

        flowLayoutPanelViewer.MouseWheel += (s, e) =>
        {
            if (!ModifierKeys.HasFlag(Keys.Control)) return;
            _pendingZoomDelta += e.Delta > 0 ? 1 : -1;
            var now = DateTime.UtcNow;
            if ((now - _lastZoomTime).TotalMilliseconds >= 250)
            {
                ApplyPendingZoom();
            }
            else
            {
                _zoomTimer?.Stop();
                _zoomTimer = new System.Windows.Forms.Timer { Interval = 250 };
                _zoomTimer.Tick += (_, _) => { _zoomTimer.Stop(); ApplyPendingZoom(); };
                _zoomTimer.Start();
            }
        };
    }

    private void ApplyPendingZoom()
    {
        if (_pendingZoomDelta == 0) return;
        var steps = Math.Sign(_pendingZoomDelta);
        _lastZoomTime = DateTime.UtcNow;
        _pendingZoomDelta = 0;
        _zoomFactor = Math.Clamp(_zoomFactor + steps * 3.0f, 0.3f, 3.0f);
        ApplyZoomToTiles();
    }

    public async Task PopulateViewerUserList()
    {
        var targetFolder = _mediator.TargetFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!Directory.Exists(targetFolder))
            return;

        var rootNodes = await Task.Run(() =>
        {
            var metaRoots = new List<(string path, string name)> { (targetFolder, Path.GetFileName(targetFolder)) };
            void CollectMetaRoots(string folder)
            {
                foreach (var dir in Directory.GetDirectories(folder))
                {
                    if (Path.GetFileName(dir).StartsWith("!"))
                    {
                        metaRoots.Add((dir, Path.GetFileName(dir)));
                        CollectMetaRoots(dir);
                    }
                }
            }
            CollectMetaRoots(targetFolder);

            var result = new List<(string metaName, string[] users)>();
            foreach (var (root, metaName) in metaRoots)
            {
                var users = new List<string>();
                foreach (var dir in Directory.GetDirectories(root))
                {
                    var dirName = Path.GetFileName(dir);
                    if (dirName.StartsWith("!"))
                        continue;
                    users.Add(dirName);
                }
                users.Sort();
                if (users.Count > 0)
                    result.Add((metaName, users.ToArray()));
            }
            return result;
        });

        void UpdateTree()
        {
            treeViewNavigator.BeginUpdate();
            treeViewNavigator.Nodes.Clear();
            foreach (var (metaName, users) in rootNodes)
            {
                var parentNode = new TreeNode(metaName);
                foreach (var user in users)
                    parentNode.Nodes.Add(new TreeNode(user));
                treeViewNavigator.Nodes.Add(parentNode);
            }
            treeViewNavigator.EndUpdate();
        }

        if (InvokeRequired)
            Invoke(UpdateTree);
        else
            UpdateTree();
    }

    private async void treeViewNavigator_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        var node = e.Node;
        // only handle leaf nodes (username folders)
        if (node.Parent == null)
            return;

        var userName = node.Text;

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
            foreach (Control c in tile.Controls)
            {
                if (c is Label lbl)
                    lbl.Height = (int)(53 * z);
                else if (c.Tag is string tag && tag == "vlc")
                    c.Bounds = tile.DisplayRectangle with { Height = tile.DisplayRectangle.Height - (int)(53 * z) };
            }
        }
        flowLayoutPanelViewer.ResumeLayout();
    }

    private Panel CreateThumbnailTile(string filePath, bool isVideo, Image? thumbnail)
    {
        var z = _zoomFactor;
        var panel = new Panel { Size = new Size((int)(180 * z), (int)(240 * z)), Margin = new Padding(4), BackColor = Color.White, Tag = filePath, Padding = new Padding(3) };
        var pictureBox = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };

        if (thumbnail != null)
            pictureBox.Image = thumbnail;
        else
            pictureBox.BackColor = Color.Gray;

        var label = new Label { Text = Path.GetFileName(filePath), Dock = DockStyle.Bottom, Height = (int)(53 * z), AutoSize = false, TextAlign = ContentAlignment.TopCenter, AutoEllipsis = true, Font = new Font("Segoe UI", 8F) };

        panel.Controls.Add(pictureBox);
        panel.Controls.Add(label);

        EventHandler selectTile = (s, e) =>
        {
            if (_selectedViewerTile == panel) return;
            // deselect previous
            if (_selectedViewerTile != null)
            {
                _selectedViewerTile.BackColor = Color.White;
                StopVideo(_selectedViewerTile);
                var prevPb = _selectedViewerTile.Controls.OfType<PictureBox>().FirstOrDefault();
                if (prevPb != null) prevPb.Visible = true;
            }
            _selectedViewerTile = panel;
            panel.BackColor = Color.Orange;
            if (isVideo)
            {
                pictureBox.Visible = false;
                StartVideo(panel, filePath, pictureBox);
            }
        };
        panel.Click += selectTile;
        pictureBox.Click += selectTile;
        label.Click += selectTile;

        pictureBox.DoubleClick += (s, e) =>
        {
            try { Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); }
            catch { }
        };
        label.DoubleClick += (s, e) =>
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

    private async void StartVideo(Panel panel, string filePath, PictureBox pictureBox)
    {
        StopVideo(panel);
        try
        {
            var pb = pictureBox; // closure-captured PictureBox
            var videoView = new VideoView
            {
                Bounds = pb.Bounds,
                Anchor = AnchorStyles.None,
                Tag = "vlc"
            };
            panel.Controls.Add(videoView);
            videoView.BringToFront();

            var media = new Media(_libVLC, new Uri(filePath), ":input-repeat=65535");
            var player = new MediaPlayer(media);
            player.EnableHardwareDecoding = true;
            videoView.MediaPlayer = player;
            videoView.DoubleClick += (s, e) =>
            {
                try { Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); }
                catch { }
            };

            await Task.Run(() =>
            {
                media.Parse(MediaParseOptions.ParseNetwork);
            });

            player.EndReached += (s, e) => Task.Run(() => player.Play());
            player.Play();
        }
        catch
        {
            try { Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); } catch { }
        }
    }

    private void StopVideo(Panel exceptPanel)
    {
        foreach (Panel tile in flowLayoutPanelViewer.Controls.OfType<Panel>())
        {
            if (tile == exceptPanel) continue;
            foreach (Control c in tile.Controls.OfType<Control>())
            {
                if (c.Tag is string tag && tag == "vlc" && c is VideoView vv)
                {
                    vv.MediaPlayer?.Stop();
                    vv.MediaPlayer?.Dispose();
                    try { tile.Controls.Remove(vv); vv.Dispose(); } catch { }
                }
            }
        }
    }
}
