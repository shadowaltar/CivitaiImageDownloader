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
        ListBoxCopyHelper.EnableCopy(listBoxVideoProcessingMessages);
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

    private void btnSelectFolder_Click(object sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            txtVideoProcessingUsers.Text = dlg.SelectedPath;
        }
    }

    private async void btnWebpToMp4_Click(object sender, EventArgs e)
    {
        Invoke(() =>
        {
            listBoxVideoProcessingMessages.Items.Clear();
            progressBox.Clear();
            _lastMessageWasSkip = false;
            _skippedSoFar = 0;
        });

        // A selected folder is used verbatim (so paths containing commas work);
        // otherwise fall back to parsing usernames.
        var input = txtVideoProcessingUsers.Text.Trim();
        List<string> names;
        if (Directory.Exists(input))
        {
            names = [input];
        }
        else
        {
            names = txtVideoProcessingUsers.ParseUserNames();
            if (names.Count == 0) return;
        }

        if (!int.TryParse(txtTargetFps.Text.Trim(), out var targetFps) || targetFps <= 0)
        {
            AddVideoProcessingMessage($"Invalid Target FPS \"{txtTargetFps.Text}\", using default 30.");
            targetFps = 30;
        }

        btnWebpToMp4.Enabled = false;
        try
        {
            foreach (var name in names)
            {
                var folder = Directory.Exists(name)
                    ? name
                    : FolderHelper.GetFolder(_mediator.TargetFolder, name);
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                {
                    AddVideoProcessingMessage($"Skipping {name}: folder not found");
                    continue;
                }

                AddVideoProcessingMessage($"Scanning {folder} (including subfolders) ...");
                var converter = new AnimatedImageToMp4Converter(folder, targetFps)
                {
                    RaiseMessage = AddVideoProcessingMessage
                };
                converter.ProgressStarted += total => Invoke(() => BeginProgress($"Progress: Webp/Gif \u2192 MP4 \u2013 {Path.GetFileName(folder)} \u2013 {total} item(s)", total));
                converter.ProgressChanged += (idx, state) => progressBox.SetState(idx, state);
                await converter.Run();
                AddVideoProcessingMessage($"{Path.GetFileName(folder)} convert is done");
            }
        }
        finally
        {
            AddVideoProcessingMessage("All done");
            btnWebpToMp4.Enabled = true;
        }
    }

    private async void btnCompressVideo_Click(object sender, EventArgs e)
    {
        Invoke(() =>
        {
            listBoxVideoProcessingMessages.Items.Clear();
            progressBox.Clear();
        });

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
        long totalBefore = 0, totalAfter = 0;
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
            _videoCompressor.ProgressStarted += total => Invoke(() => BeginProgress($"Progress: Compress \u2013 {name} \u2013 {total} item(s)", total));
            _videoCompressor.ProgressChanged += (idx, state) => progressBox.SetState(idx, state);
            await _videoCompressor.Run();
            totalBefore += _videoCompressor.TotalBytesBefore;
            totalAfter += _videoCompressor.TotalBytesAfter;
            _videoCompressor.RaiseAddMessage -= AddVideoProcessingMessage;
            _videoCompressor.RaiseAppendMessage -= AppendVideoProcessingMessage;
            _videoCompressor.Dispose();
            AddVideoProcessingMessage($"{name} compress is done");
        }

        const double mb = 1024.0 * 1024.0;
        var saved = totalBefore - totalAfter;
        var pct = totalBefore > 0 ? 100.0 * saved / totalBefore : 0;
        AddVideoProcessingMessage($"Compression done. Before: {totalBefore / mb:F2} MB, After: {totalAfter / mb:F2} MB, Saved: {saved / mb:F2} MB ({pct:F1}%)");
        AddVideoProcessingMessage("All done");

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

    private bool _lastMessageWasSkip;
    private int _skippedSoFar;

    private void BeginProgress(string header, int count)
    {
        progressBox.BeginRun(header, count);
        AutoSizeProgressPanel();
    }

    private void AutoSizeProgressPanel()
    {
        try
        {
            int usable = Math.Max(0, messageSplitContainer.Height - messageSplitContainer.SplitterWidth);
            int maxTop = usable / 2;                        // progress never exceeds half the shared height
            int distance = Math.Min(progressBox.PreferredHeight, maxTop);
            messageSplitContainer.SplitterDistance = Math.Max(0, Math.Min(distance, usable));
        }
        catch
        {
        }
    }

    private void AddVideoProcessingMessage(string message)
    {
        Invoke(() =>
        {
            var isSkip = message.StartsWith(AnimatedImageToMp4Converter.SkipMessageMarker, StringComparison.Ordinal);
            if (isSkip)
            {
                _skippedSoFar++;
                message = $"Skipped {_skippedSoFar} file(s): " + message[AnimatedImageToMp4Converter.SkipMessageMarker.Length..];
            }

            listBoxVideoProcessingMessages.BeginUpdate();
            if (isSkip && _lastMessageWasSkip && listBoxVideoProcessingMessages.Items.Count > 0)
            {
                // consecutive skips coalesce into a single updating line
                listBoxVideoProcessingMessages.Items[listBoxVideoProcessingMessages.Items.Count - 1] = message;
            }
            else
            {
                listBoxVideoProcessingMessages.Items.Add(message);
            }
            _lastMessageWasSkip = isSkip;
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
                _lastMessageWasSkip = false;
            }
            catch { }
            finally { listBoxVideoProcessingMessages.EndUpdate(); }
        });
    }

    private async void btnEnhanceFrameRate_Click(object sender, EventArgs e)
    {
        List<string> names = txtVideoProcessingUsers.ParseUserNames();
        if (names.Count == 0) return;

        if (!double.TryParse(txtMinFps.Text.Trim(), out var minFps) || minFps <= 0)
        {
            AddVideoProcessingMessage($"Invalid Min FPS \"{txtMinFps.Text}\", using default 24.");
            minFps = 24;
        }
        if (!double.TryParse(txtTargetFps.Text.Trim(), out var targetFps) || targetFps <= 0)
        {
            AddVideoProcessingMessage($"Invalid Target FPS \"{txtTargetFps.Text}\", using default 30.");
            targetFps = 30;
        }
        if (targetFps <= minFps)
        {
            AddVideoProcessingMessage("Target FPS must be greater than Min FPS.");
            return;
        }

        Invoke(() => progressBox.Clear());
        btnEnhanceFrameRate.Enabled = false;
        try
        {
            await Task.Run(() =>
            {
                foreach (var name in names)
                {
                    var folder = Directory.Exists(name)
                        ? name
                        : FolderHelper.GetFolder(_mediator.TargetFolder, name);
                    if (string.IsNullOrEmpty(folder))
                    {
                        Invoke(() => AddVideoProcessingMessage($"Skipping {name}: folder not found"));
                        continue;
                    }

                    var videoFiles = Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
                        .Where(f => Path.GetExtension(f).ToLower() is ".mp4" or ".webm" or ".mov" or ".avi")
                        .ToArray();

                    Invoke(() => AddVideoProcessingMessage($"Found {videoFiles.Length} videos for {name}"));

                    var fileIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int k = 0; k < videoFiles.Length; k++)
                        fileIndex[videoFiles[k]] = k;
                    Invoke(() => BeginProgress($"Progress: Enhance \u2013 {name} \u2013 {videoFiles.Length} item(s)", videoFiles.Length));

                    // collect files needing enhancement (scan in parallel and report progress so the UI stays live)
                    var toEnhance = new System.Collections.Concurrent.ConcurrentBag<(string file, string fileName, double fps)>();
                    int scannedCount = 0;
                    Invoke(() => AddVideoProcessingMessage($"Scanning {videoFiles.Length} videos for {name}..."));
                    Parallel.ForEach(videoFiles,
                        new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Math.Min(Environment.ProcessorCount, 4)) },
                        file =>
                        {
                            try
                            {
                                var probe = new FFProbe();
                                var info = probe.GetMediaInfo(file);
                                var stream = info.Streams.FirstOrDefault(s => s.CodecType?.ToLower() == "video");
                                if (stream != null)
                                {
                                    // Use the effective (unique) frame rate so videos that only look high-fps
                                    // because of duplicated frames are still detected as needing enhancement.
                                    var effectiveFps = GetEffectiveFrameRate(file, info.Duration.TotalSeconds, stream.FrameRate);
                                    if (effectiveFps < minFps)
                                        toEnhance.Add((file, Path.GetFileName(file), effectiveFps));
                                }
                            }
                            catch { }

                            var k = Interlocked.Increment(ref scannedCount);
                            Invoke(() => AddVideoProcessingMessage($"  scanning {k}/{videoFiles.Length}: {Path.GetFileName(file)}"));
                        });

                    Invoke(() => AddVideoProcessingMessage($"  {toEnhance.Count} need enhancement (effective fps < {minFps})"));

                    var enhanceSet = toEnhance.Select(t => t.file).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    for (int k = 0; k < videoFiles.Length; k++)
                        if (!enhanceSet.Contains(videoFiles[k]))
                            progressBox.SetState(k, ProgressItemState.Skipped);

                    var done = 0;
                    var total = toEnhance.Count;
                    Parallel.ForEach(toEnhance,
                        new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                        item =>
                        {
                            var n = Interlocked.Increment(ref done);
                            var tmpFile = item.file + ".enhanced.mp4";
                            if (fileIndex.TryGetValue(item.file, out var idx))
                                progressBox.SetState(idx, ProgressItemState.Processing);
                            try
                            {
                                var sizeBefore = new FileInfo(item.file).Length;
                                Invoke(() => AddVideoProcessingMessage($"  [{n}/{total}] Enhancing {item.fileName}: {item.fps:F1}fps → {targetFps}fps..."));
                                var ffmpeg = new FFMpegConverter();
                                ffmpeg.ConvertMedia(item.file, null, tmpFile, null,
                                    new ConvertSettings
                                    {
                                        VideoCodec = "libx264",
                                        CustomOutputArgs = $"-preset fast -crf 23 -vf mpdecimate,minterpolate=fps={targetFps}:mi_mode=mci:mc_mode=aobmc:me_mode=bidir:vsbmc=1"
                                    });

                                File.Delete(item.file);
                                File.Move(tmpFile, item.file);
                                var sizeAfter = new FileInfo(item.file).Length;
                                if (fileIndex.TryGetValue(item.file, out var doneIdx))
                                    progressBox.SetState(doneIdx, ProgressItemState.Done);
                                Invoke(() => AddVideoProcessingMessage(
                                    $"  [{n}/{total}] Done {item.fileName}: {sizeBefore / 1024.0 / 1024.0:F2} MB -> {sizeAfter / 1024.0 / 1024.0:F2} MB"));
                            }
                            catch (Exception ex)
                            {
                                try { File.Delete(tmpFile); } catch { }
                                if (fileIndex.TryGetValue(item.file, out var failIdx))
                                    progressBox.SetState(failIdx, ProgressItemState.Skipped);
                                Invoke(() => AddVideoProcessingMessage($"  [{n}/{total}] Failed {item.fileName}: {ex.Message}"));
                            }
                        });
                    Invoke(() => AddVideoProcessingMessage($"{name} enhance is done"));
                }
            });
        }
        finally
        {
            Invoke(() =>
            {
                AddVideoProcessingMessage("All done");
                btnEnhanceFrameRate.Enabled = true;
            });
        }
    }

    // Unique frames per second = number of frames left after dropping duplicates / duration.
    private static double GetEffectiveFrameRate(string file, double durationSeconds, double fallbackFps)
    {
        if (durationSeconds <= 0)
            return fallbackFps;
        var uniqueFrames = CountUniqueFrames(file);
        if (uniqueFrames <= 0)
            return fallbackFps;
        return uniqueFrames / durationSeconds;
    }

    private static int CountUniqueFrames(string file)
    {
        var psi = new ProcessStartInfo(GetFFmpegPath())
        {
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = $"-hide_banner -i \"{file}\" -vf mpdecimate -an -f null -"
        };
        using var proc = Process.Start(psi);
        if (proc == null)
            return -1;
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        var matches = System.Text.RegularExpressions.Regex.Matches(stderr, @"frame=\s*(\d+)");
        if (matches.Count == 0)
            return -1;
        return int.TryParse(matches[^1].Groups[1].Value, out var frames) ? frames : -1;
    }

    private static string GetFFmpegPath()
    {
        var converter = new FFMpegConverter();
        var dir = string.IsNullOrWhiteSpace(converter.FFMpegToolPath) ? AppContext.BaseDirectory : converter.FFMpegToolPath;
        var exe = string.IsNullOrWhiteSpace(converter.FFMpegExeName) ? "ffmpeg.exe" : converter.FFMpegExeName;
        var full = Path.Combine(dir, exe);
        return File.Exists(full) ? full : exe;
    }
}
