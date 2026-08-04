using CivitaiImageDownloader.Models;
using CivitaiImageDownloader.Util;
using NReco.VideoConverter;
using NReco.VideoInfo;
using System.Diagnostics;

namespace CivitaiImageDownloader.Tabs;

public partial class VideoTabControl : UserControl
{
    private readonly AppMediator _mediator;
    private VideoCompressor? _videoCompressor;

    public VideoTabControl(AppMediator mediator)
    {
        _mediator = mediator;
        InitializeComponent();
        _mediator.UsernamesCopiedToVideo += usernames =>
        {
            txtVideoProcessingUsers.Text = usernames;
            _mediator.VideoUsernames = usernames;
        };
        txtVideoProcessingUsers.TextChanged += (s, e) => _mediator.VideoUsernames = txtVideoProcessingUsers.Text;
        _mediator.Stopping = false;
    }

    private void btnCopyFromDownloadTab_Click(object sender, EventArgs e)
    {
        txtVideoProcessingUsers.Text = _mediator.DownloadUsernames;
    }

    private async void btnCompressVideo_Click(object sender, EventArgs e)
    {
        Invoke(listBoxVideoProcessingMessages.Items.Clear);

        _videoCompressor = null;

        List<string> names = txtVideoProcessingUsers.ParseUserNames();
        if (names.Count == 0) { return; }
        if (txtVideoProcessingUsers.Text.Length == 0 && names.Count == 0 && !Path.Exists(_mediator.TargetFolder))
        {
            var result = MessageBox.Show("Do you want to compress everything in " + _mediator.TargetFolder + "?", "Warning", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                names = [_mediator.TargetFolder];
            }
        }
        foreach (var name in names)
        {
            var mode = VideoProcessInputMode.UserName;
            if (Path.Exists(name))
            {
                mode = VideoProcessInputMode.FilePath;
            }
            if (_mediator.Stopping && _videoCompressor != null)
            {
                _videoCompressor.ShouldStop = true;
                _mediator.LogMessage("Video compression stopped.");
                break;
            }
            _videoCompressor = new VideoCompressor(_mediator.TargetFolder, name, mode);
            _videoCompressor.RaiseAddMessage += AddVideoProcessingMessage;
            _videoCompressor.RaiseAppendMessage += AppendVideoProcessingMessage;
            await _videoCompressor.Run();
            _videoCompressor.RaiseAddMessage -= AddVideoProcessingMessage;
            _videoCompressor.RaiseAppendMessage -= AppendVideoProcessingMessage;
            _videoCompressor.Dispose();
        }
        AddVideoProcessingMessage("ALL DONE!");

        _mediator.RecordVideoHistory(txtVideoProcessingUsers.Text.Trim());
    }

    private void listBoxVideoProcessingMessages_DoubleClick(object sender, EventArgs e)
    {
        if (_videoCompressor == null || string.IsNullOrWhiteSpace(_videoCompressor.UserName))
            return;
        _mediator.CurrentUserFolder = FolderHelper.GetFolder(_mediator.TargetFolder, _videoCompressor.UserName);
        if (listBoxVideoProcessingMessages.SelectedIndex != -1 && Directory.Exists(_mediator.CurrentUserFolder))
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
                        var path = Path.Combine(_mediator.CurrentUserFolder, candidate);
                        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
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

    private void AddVideoProcessingMessage(string message)
    {
        Invoke(() =>
        {
            listBoxVideoProcessingMessages.BeginUpdate();
            listBoxVideoProcessingMessages.Items.Add(message);
            listBoxVideoProcessingMessages.TopIndex = listBoxVideoProcessingMessages.Items.Count - 1;
            listBoxVideoProcessingMessages.EndUpdate();
        });
    }

    private void AppendVideoProcessingMessage(string message)
    {
        Invoke(() =>
        {
            try
            {
                listBoxVideoProcessingMessages.BeginUpdate();
                var lastIdx = listBoxVideoProcessingMessages.Items.Count - 1;
                if (lastIdx >= 0)
                    listBoxVideoProcessingMessages.Items[lastIdx] = listBoxVideoProcessingMessages.Items[lastIdx] + message;
            }
            catch { }
            finally { listBoxVideoProcessingMessages.EndUpdate(); }
        });
    }

    private async void btnEnhanceFrameRate_Click(object sender, EventArgs e)
    {
        List<string> names = txtVideoProcessingUsers.ParseUserNames();
        if (names.Count == 0) return;

        btnEnhanceFrameRate.Enabled = false;
        try
        {
            await Task.Run(() =>
            {
                foreach (var name in names)
                {
                    var folder = FolderHelper.GetFolder(_mediator.TargetFolder, name);
                    if (string.IsNullOrEmpty(folder))
                    {
                        Invoke(() => AddVideoProcessingMessage($"Skipping {name}: folder not found"));
                        continue;
                    }

                    var videoFiles = Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
                        .Where(f => Path.GetExtension(f).ToLower() is ".mp4" or ".webm" or ".mov" or ".avi")
                        .ToArray();

                    Invoke(() => AddVideoProcessingMessage($"Found {videoFiles.Length} videos for {name}"));

                    // collect files needing enhancement
                    var toEnhance = new List<(string file, string fileName, double fps)>();
                    foreach (var file in videoFiles)
                    {
                        try
                        {
                            var probe = new FFProbe();
                            var info = probe.GetMediaInfo(file);
                            var stream = info.Streams.FirstOrDefault(s => s.CodecType?.ToLower() == "video");
                            if (stream == null) continue;
                            if (stream.FrameRate < 24)
                                toEnhance.Add((file, Path.GetFileName(file), stream.FrameRate));
                        }
                        catch { }
                    }

                    Invoke(() => AddVideoProcessingMessage($"  {toEnhance.Count} need enhancement"));

                    var done = 0;
                    var total = toEnhance.Count;
                    Parallel.ForEach(toEnhance,
                        new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                        item =>
                        {
                            var n = Interlocked.Increment(ref done);
                            var tmpFile = item.file + ".enhanced.mp4";
                            try
                            {
                                Invoke(() => AddVideoProcessingMessage($"  [{n}/{total}] Enhancing {item.fileName}: {item.fps:F1}fps → 30fps..."));
                                var ffmpeg = new FFMpegConverter();
                                ffmpeg.ConvertMedia(item.file, null, tmpFile, null,
                                    new ConvertSettings
                                    {
                                        VideoCodec = "libx264",
                                        CustomOutputArgs = $"-preset fast -crf 23 -vf minterpolate=fps=30:mi_mode=mci:mc_mode=aobmc:me_mode=bidir:vsbmc=1"
                                    });

                                File.Delete(item.file);
                                File.Move(tmpFile, item.file);
                                Invoke(() => AddVideoProcessingMessage($"  [{n}/{total}] Done {item.fileName}"));
                            }
                            catch (Exception ex)
                            {
                                try { File.Delete(tmpFile); } catch { }
                                Invoke(() => AddVideoProcessingMessage($"  [{n}/{total}] Failed {item.fileName}: {ex.Message}"));
                            }
                        });
                }
            });
        }
        finally
        {
            Invoke(() =>
            {
                AddVideoProcessingMessage("Enhance Frame Rate complete.");
                btnEnhanceFrameRate.Enabled = true;
            });
        }
    }
}
