using System.Diagnostics;
using System.Runtime.InteropServices;
using CivitaiImageDownloader.Util;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using NReco.VideoConverter;
using NReco.VideoInfo;

namespace CivitaiImageDownloader.Tabs;

public partial class ViewerTabControl : UserControl
{
    private const int WmMouseWheel = 0x020A;
    private const int WhMouseLowLevel = 14;

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct MsllHookStruct
    {
        public Point pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(Point point);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    private IntPtr _mouseHook = IntPtr.Zero;
    private LowLevelMouseProc? _mouseProc;

    private readonly AppMediator _mediator;
    private Panel? _selectedViewerTile;
    private CancellationTokenSource? _loadCts;
    private const int TileWidth = 500;
    private const int TileHeight = 667; // keeps the original 3:4 tile proportion
    private float _zoomFactor = 1.0f;
    private DateTime _lastZoomTime = DateTime.MinValue;
    private int _pendingZoomDelta;
    private System.Windows.Forms.Timer? _zoomTimer;
    private LibVLC? _libVLC;
    private System.Windows.Forms.Timer? _videoStartTimer;
    private (Panel panel, string filePath, PictureBox pictureBox)? _pendingVideo;

    public ViewerTabControl(AppMediator mediator)
    {
        _mediator = mediator;
        Core.Initialize();
        _libVLC = new LibVLC();
        InitializeComponent();
        _mouseProc = MouseHookProc;
        _mouseHook = SetWindowsHookEx(WhMouseLowLevel, _mouseProc, GetModuleHandle(null), 0);
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

    public void SelectFirstUser()
    {
        foreach (TreeNode root in treeViewNavigator.Nodes)
        {
            if (root.Nodes.Count > 0)
            {
                treeViewNavigator.SelectedNode = root.Nodes[0];
                root.Nodes[0].EnsureVisible();
                return;
            }
        }
    }

    private void ApplyPendingZoom()
    {
        if (_pendingZoomDelta == 0) return;
        var steps = Math.Sign(_pendingZoomDelta);
        _lastZoomTime = DateTime.UtcNow;
        _pendingZoomDelta = 0;
        _zoomFactor = Math.Clamp(_zoomFactor + steps * 0.1f, 0.3f, 3.0f);
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

        // stop any video still playing from the previous folder before clearing the tiles
        StopAllVideos();

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
                    if (PathUtils.IsTempMediaFile(Path.GetFileName(f)))
                        return false;
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

            var tasks = files.Skip(i).Take(batchSize).Select(async file =>
            {
                bool isVideo = IsVideoFile(file);
                Image? thumb = isVideo
                    ? await Task.Run(() => LoadVideoThumbnail(file), token)
                    : await Task.Run(() => LoadThumbnailImage(file), token);
                return (file, isVideo, thumb);
            }).ToList();

            // add each tile as soon as its thumbnail is ready (a slow file can't hold up the rest)
            while (tasks.Count > 0)
            {
                var finished = await Task.WhenAny(tasks);
                tasks.Remove(finished);
                var (file, isVideo, thumb) = await finished;
                if (token.IsCancellationRequested) return;
                var tile = CreateThumbnailTile(file, isVideo, thumb);
                flowLayoutPanelViewer.Controls.Add(tile);
                index++;
                progressBarViewer.Value = index;
            }
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
        try { using var img = Image.FromFile(filePath); return img.GetThumbnailImage(TileWidth, TileHeight, null, IntPtr.Zero); }
        catch { return null; }
    }

    private void ApplyZoomToTiles()
    {
        flowLayoutPanelViewer.SuspendLayout();
        foreach (Panel tile in flowLayoutPanelViewer.Controls.OfType<Panel>())
        {
            var z = _zoomFactor;
            tile.Size = new Size((int)(TileWidth * z), (int)(TileHeight * z));
            foreach (Control c in tile.Controls)
            {
                if (c.Tag is string tag && tag == "vlc")
                    c.Bounds = tile.DisplayRectangle;
            }
        }
        flowLayoutPanelViewer.ResumeLayout();
    }

    private Panel CreateThumbnailTile(string filePath, bool isVideo, Image? thumbnail)
    {
        var z = _zoomFactor;
        var panel = new Panel { Size = new Size((int)(TileWidth * z), (int)(TileHeight * z)), Margin = new Padding(4), BackColor = Color.White, Tag = filePath, Padding = new Padding(3) };
        var pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black
        };

        if (thumbnail != null)
            pictureBox.Image = thumbnail;
        else
            pictureBox.BackColor = Color.Gray;

        panel.Controls.Add(pictureBox);

        EventHandler selectTile = (s, e) =>
        {
            if (_selectedViewerTile == panel) return;
            CancelPendingVideoStart();
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
                // keep showing the thumbnail until the VideoView actually covers the tile
                // (video start is deferred briefly so a double-click can open the OS viewer instead)
                ScheduleVideoStart(panel, filePath, pictureBox);
            }
        };
        panel.Click += selectTile;
        pictureBox.Click += selectTile;

        // Double-click opens the file in the OS default viewer. The deferred video start above keeps the
        // tile clickable for this even when the first click of the double-click selected the video.
        EventHandler openDefault = (s, e) =>
        {
            if (CancelPendingVideoStart())
                pictureBox.Visible = true; // playback was cancelled; keep showing the thumbnail
            try { Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); }
            catch { }
        };
        pictureBox.DoubleClick += openDefault;
        panel.DoubleClick += openDefault;

        return panel;
    }

    private void ScheduleVideoStart(Panel panel, string filePath, PictureBox pictureBox)
    {
        CancelPendingVideoStart();
        _pendingVideo = (panel, filePath, pictureBox);
        _videoStartTimer = new System.Windows.Forms.Timer { Interval = SystemInformation.DoubleClickTime + 50 };
        _videoStartTimer.Tick += (s, e) =>
        {
            _videoStartTimer?.Stop();
            _videoStartTimer?.Dispose();
            _videoStartTimer = null;
            if (_pendingVideo is { } p)
            {
                _pendingVideo = null;
                StartVideo(p.panel, p.filePath, p.pictureBox);
            }
        };
        _videoStartTimer.Start();
    }

    private bool CancelPendingVideoStart()
    {
        bool hadPending = _pendingVideo != null;
        if (_videoStartTimer != null)
        {
            _videoStartTimer.Stop();
            _videoStartTimer.Dispose();
            _videoStartTimer = null;
        }
        _pendingVideo = null;
        return hadPending;
    }

    private static Image? LoadVideoThumbnail(string filePath)
    {
        var tmpFile = Path.GetTempFileName() + ".jpg";
        try
        {
            var ffmpeg = new FFMpegConverter { ExecutionTimeout = TimeSpan.FromSeconds(20) };
            ffmpeg.GetVideoThumbnail(filePath, tmpFile, 0);
            if (File.Exists(tmpFile))
            {
                // load into memory and clone so the temp file can be deleted (Image.FromFile locks it)
                var bytes = File.ReadAllBytes(tmpFile);
                using var ms = new MemoryStream(bytes);
                using var img = Image.FromStream(ms);
                return new Bitmap(img);
            }
        }
        catch { }
        finally
        {
            try { if (File.Exists(tmpFile)) File.Delete(tmpFile); } catch { }
        }
        return null;
    }

    private async void StartVideo(Panel panel, string filePath, PictureBox pictureBox)
    {
        StopVideo(panel);
        try
        {
            // probe: skip if it's actually an image disguised as video
            var probe = await Task.Run(() =>
            {
                try
                {
                    var ffprobe = new FFProbe { ExecutionTimeout = TimeSpan.FromSeconds(20) };
                    var info = ffprobe.GetMediaInfo(filePath);
                    var stream = info.Streams.FirstOrDefault(s => s.CodecType?.ToLower() == "video");
                    if (stream == null) return false;
                    if (stream.CodecName?.ToLower() == "mjpeg") return false;
                    if (info.FormatName?.Contains("jpeg") == true) return false;
                    if (info.Duration.TotalSeconds < 0.1) return false;
                    return true;
                }
                catch { return true; } // assume playable on probe failure
            });

            if (!probe)
            {
                pictureBox.Visible = true;
                return;
            }

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

            // looping is handled by the ":input-repeat=65535" media option; an EndReached->Play
            // handler would race with Stop()/Dispose() and can deadlock.
            player.Play();
        }
        catch
        {
            try { Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); } catch { }
        }
    }

    /// <summary>Stops/disposes all inline videos (e.g. when leaving the tab or switching folders).</summary>
    public void StopAllVideos()
    {
        StopVideo(null);
        _selectedViewerTile = null;
    }

    private void StopVideo(Panel? exceptPanel)
    {
        foreach (Panel tile in flowLayoutPanelViewer.Controls.OfType<Panel>())
        {
            if (tile == exceptPanel) continue;
            foreach (Control c in tile.Controls.OfType<Control>().ToList())
            {
                if (c.Tag is string tag && tag == "vlc" && c is VideoView vv)
                {
                    // Detach first so this video can't be found/stopped again, then stop+dispose the
                    // player off the UI thread (libvlc Stop() can block/freeze the UI).
                    var player = vv.MediaPlayer;
                    vv.MediaPlayer = null;
                    try { tile.Controls.Remove(vv); } catch { }
                    try { vv.Dispose(); } catch { }
                    if (player != null)
                    {
                        Task.Run(() =>
                        {
                            try { player.Stop(); } catch { }
                            try { player.Dispose(); } catch { }
                        });
                    }
                }
            }
        }
    }

    // The VLC video surface is a native window that swallows the mouse wheel. This low-level mouse hook
    // (invoked on this UI thread) forwards the wheel to the tile pane so it scrolls/zooms over videos too.
    private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WmMouseWheel
            && flowLayoutPanelViewer.IsHandleCreated && flowLayoutPanelViewer.Visible
            && FindForm() is { } form && Form.ActiveForm == form)
        {
            var data = Marshal.PtrToStructure<MsllHookStruct>(lParam);
            var screenPoint = data.pt;
            if (flowLayoutPanelViewer.ClientRectangle.Contains(flowLayoutPanelViewer.PointToClient(screenPoint)))
            {
                var under = Control.FromHandle(WindowFromPoint(screenPoint));
                bool overVideo = under == null; // native (VLC) window -> not a managed control
                for (var c = under; c != null && !overVideo; c = c.Parent)
                {
                    if (c.Tag is string tag && tag == "vlc")
                        overVideo = true;
                    else if (c == flowLayoutPanelViewer)
                        break;
                }

                if (overVideo)
                {
                    int delta = unchecked((short)((data.mouseData >> 16) & 0xFFFF));
                    int lp = (screenPoint.Y << 16) | (screenPoint.X & 0xFFFF);
                    SendMessage(flowLayoutPanelViewer.Handle, WmMouseWheel, (IntPtr)(delta << 16), (IntPtr)lp);
                    return (IntPtr)1; // consumed here
                }
            }
        }
        return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Delete && _selectedViewerTile != null)
        {
            DeleteSelectedTile();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void DeleteSelectedTile()
    {
        if (_selectedViewerTile == null) return;
        var filePath = _selectedViewerTile.Tag as string;
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

        var targetFolder = _mediator.TargetFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var deletedDir = Path.Combine(targetFolder, "!deleted");
        Directory.CreateDirectory(deletedDir);

        var destPath = Path.Combine(deletedDir, Path.GetFileName(filePath));
        if (File.Exists(destPath))
        {
            var name = Path.GetFileNameWithoutExtension(filePath);
            var ext = Path.GetExtension(filePath);
            destPath = Path.Combine(deletedDir, $"{name}_{DateTime.Now:yyyyMMddHHmmss}{ext}");
        }

        try { File.Move(filePath, destPath); }
        catch { return; }

        // record to redoable-deleted.json
        var redoFile = Path.Combine(targetFolder, "redoable-deleted.json");
        var entries = new List<object>();
        if (File.Exists(redoFile))
        {
            try
            {
                var json = File.ReadAllText(redoFile);
                entries = System.Text.Json.JsonSerializer.Deserialize<List<object>>(json) ?? new();
            }
            catch { }
        }
        entries.Add(new { originalPath = filePath, movedTo = destPath, timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        File.WriteAllText(redoFile, System.Text.Json.JsonSerializer.Serialize(entries, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        // remove tile
        _selectedViewerTile.BackColor = Color.White;
        flowLayoutPanelViewer.Controls.Remove(_selectedViewerTile);
        _selectedViewerTile.Dispose();
        _selectedViewerTile = null;
    }
}
