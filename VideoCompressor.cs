using CivitaiImageDownloader.Models;
using CivitaiImageDownloader.Util;

using NReco.VideoConverter;
using NReco.VideoInfo;

namespace CivitaiImageDownloader;

public class VideoCompressor : IDisposable
{
    private string _rootFolder;
    private string name;
    private readonly VideoProcessInputMode mode;
    private readonly FFProbe _ffProbe = new FFProbe();

    public string UserName => mode == VideoProcessInputMode.UserName ? name : "";

    // Skip tiny/very short files outright.
    public long CompressionMinBytes { get; set; } = 300 * 1024; // 300 KB
    public double CompressionMinDurationSeconds { get; set; } = 1.0;

    // Downscale factor; the shorter output side is never allowed below CompressionMinDimension.
    public double CompressionFrameSizeRatio { get; set; } = .75;
    public int CompressionMinDimension { get; set; } = 640;

    // Bits-per-pixel-per-frame the target CRF ~23 H.264 encode is expected to need,
    // and the margin above it before re-encoding is worthwhile.
    public double CompressionBppTarget { get; set; } = 0.08;
    public double CompressionBppMargin { get; set; } = 1.25;

    public VideoCompressor(string targetFolder, string name, VideoProcessInputMode mode)
    {
        _rootFolder = targetFolder;
        this.name = name;
        this.mode = mode;
    }

    public bool ShouldStop { get; internal set; }

    public Action<string> RaiseAddMessage { get; internal set; }

    public Action<string> RaiseAppendMessage { get; internal set; }

    /// <summary>Raised once per run with the number of discovered video files.</summary>
    public event Action<int>? ProgressStarted;

    /// <summary>Raised when an item's progress state changes (item index, state).</summary>
    public event Action<int, ProgressItemState>? ProgressChanged;

    public void Dispose()
    {

    }

    public async Task Run()
    {
        if (mode == VideoProcessInputMode.UserName)
        {
            var folder = FolderHelper.GetFolder(_rootFolder, name);
            if (!string.IsNullOrEmpty(folder))
            {
                RaiseAddMessage?.Invoke("Use folder: " + folder);
            }
            else
            {
                RaiseAddMessage?.Invoke("Folder not found: " + folder);
                return;
            }

            int goodCount = 0;
            int failedCount = 0;

            var ffmpeg = new FFMpegConverter();
            var files = Directory.GetFiles(folder, "*.mp4", SearchOption.AllDirectories);
            var totalCount = files.Length;
            ProgressStarted?.Invoke(totalCount);
            for (int i = 0; i < totalCount; i++)
            {
                string? path = files[i];
                string logPrefix = $"[{i}/{totalCount}] ";
                ProgressChanged?.Invoke(i, ProgressItemState.Processing);
                var result = await Compress(folder, ffmpeg, logPrefix, path);
                ProgressChanged?.Invoke(i, result == VideoCompressResult.Good ? ProgressItemState.Done : ProgressItemState.Skipped);
                if (result == VideoCompressResult.Failed)
                {
                    failedCount++;
                }
                else
                {
                    goodCount++;
                }
            }
            RaiseAddMessage?.Invoke($"Finished. Good/Error/Total: {goodCount}/{failedCount}/{totalCount}");
        }
        else
        {
            var ffmpeg = new FFMpegConverter();
            if (Directory.Exists(name))
            {
                var files = Directory.GetFiles(name, "*", SearchOption.AllDirectories)
                    .Where(f => Path.GetExtension(f).ToLower() is ".mp4" or ".webm" or ".mov" or ".avi")
                    .ToArray();
                var totalCount = files.Length;
                int goodCount = 0;
                int failedCount = 0;
                ProgressStarted?.Invoke(totalCount);
                for (int i = 0; i < totalCount; i++)
                {
                    string logPrefix = $"[{i}/{totalCount}] ";
                    ProgressChanged?.Invoke(i, ProgressItemState.Processing);
                    var result = await Compress(name, ffmpeg, logPrefix, files[i]);
                    ProgressChanged?.Invoke(i, result == VideoCompressResult.Good ? ProgressItemState.Done : ProgressItemState.Skipped);
                    if (result == VideoCompressResult.Failed)
                    {
                        failedCount++;
                    }
                    else
                    {
                        goodCount++;
                    }
                }
                RaiseAddMessage?.Invoke($"Finished. Good/Error/Total: {goodCount}/{failedCount}/{totalCount}");
            }
            else
            {
                var folder = Path.GetDirectoryName(name) ?? "";
                var result = await Compress(folder, ffmpeg, "", name);
                RaiseAddMessage?.Invoke($"Finished, result: {(result == VideoCompressResult.Good ? "Good" : "Failed")}");
            }
        }
    }

    private async Task<VideoCompressResult> Compress(string folder, FFMpegConverter ffmpeg, string logPrefix, string path)
    {
        string? compressedFile = null;
        var convertResult = VideoCompressResult.Good;
        try
        {
            var ext = Path.GetExtension(path).ToLower();
            if (ext != ".mp4" && ext != ".webm" && ext != ".mov" && ext != ".avi")
            {
                return VideoCompressResult.SkippedWrongFormat;
            }

            var fi = new FileInfo(path);
            if (fi.Length < CompressionMinBytes)
            {
                // skip small files
                return VideoCompressResult.SkippedFileSizeTooSmall;
            }
            compressedFile = Path.Combine(Path.GetDirectoryName(path) ?? folder, "compressing_" + fi.Name);
            RaiseAddMessage?.Invoke($"{logPrefix}Compress video: {fi.Name} ...");

            await Task.Run(() =>
            {
                (convertResult, Rect old, Rect @new) = Convert(ffmpeg, path, compressedFile);
                var resultFile = new FileInfo(compressedFile);
                if (resultFile.Exists)
                {
                    var oldMb = fi.Length / 1024.0 / 1024.0;
                    var newMb = resultFile.Length / 1024.0 / 1024.0;
                    RaiseAppendMessage?.Invoke($" Result: {oldMb:.00}MB -> {newMb:.00}MB; {old.Width}x{old.Height} -> {@new.Width}x{@new.Height}");

                    File.Delete(path);
                    File.Move(compressedFile, path);
                }
                else
                    RaiseAppendMessage?.Invoke($" Skipped: {convertResult}");
            });
        }
        catch (FFMpegException ex)
        {
            // This ErrorCode tells you exactly why FFmpeg died (e.g., 1 for general error)
            RaiseAddMessage?.Invoke($"{logPrefix}FFMpeg Error Code: {ex.ErrorCode}; Msg: {ex.Message}");
            if (compressedFile != null)
                File.Delete(compressedFile);
            return VideoCompressResult.Failed;
        }
        catch (Exception e)
        {
            RaiseAddMessage?.Invoke($"Error: {e}.");
            if (compressedFile != null)
                File.Delete(compressedFile);
            return VideoCompressResult.Failed;
        }

        return convertResult;
    }

    private (VideoCompressResult result, Rect oldDimension, Rect newDimension) Convert(FFMpegConverter ffmpeg, string path, string? compressedFile)
    {
        const float newFrameRate = 30;
        const float qualityRate = 23;
        MediaInfo videoInfo = _ffProbe.GetMediaInfo(path);

        // Pick the actual video stream (Streams[0] may be audio/other and report -1x-1).
        var videoStream = videoInfo.Streams?.FirstOrDefault(s => s.CodecType?.ToLower() == "video");
        if (videoStream == null || videoStream.Width <= 0 || videoStream.Height <= 0)
        {
            return (VideoCompressResult.SkippedWrongFormat, new Rect(0, 0), new Rect(0, 0));
        }
        int videoWidth = videoStream.Width;
        int videoHeight = videoStream.Height;
        float frameRate = videoStream.FrameRate;
        var oldDimension = new Rect(videoWidth, videoHeight);

        double durationSec = videoInfo.Duration.TotalSeconds;
        if (durationSec < CompressionMinDurationSeconds)
        {
            return (VideoCompressResult.SkippedBitrateLow, oldDimension, oldDimension);
        }

        // Dimension lower bound: don't compress videos whose shorter side is already below the minimum.
        if (Math.Min(videoWidth, videoHeight) < CompressionMinDimension)
        {
            return (VideoCompressResult.SkippedDimensionTooSmall, oldDimension, oldDimension);
        }

        // Output geometry: scale down by the ratio (H.264 needs even dimensions), but never let the
        // shorter side fall below CompressionMinDimension. Because the source is already >= the
        // minimum here, flooring it never upscales.
        double ratio = videoWidth / (double)videoHeight;
        int newVideoWidth = toEven(videoWidth * CompressionFrameSizeRatio);
        int newVideoHeight = toEven(videoHeight * CompressionFrameSizeRatio);
        if (videoWidth >= videoHeight)
        {
            if (newVideoHeight < CompressionMinDimension)
            {
                newVideoHeight = CompressionMinDimension;
                newVideoWidth = toEven(newVideoHeight * ratio);
            }
        }
        else
        {
            if (newVideoWidth < CompressionMinDimension)
            {
                newVideoWidth = CompressionMinDimension;
                newVideoHeight = toEven(newVideoWidth / ratio);
            }
        }

        // Decide whether re-encoding is worthwhile: compare the current bitrate against the
        // bitrate the target encode is expected to need (bits-per-pixel-per-frame heuristic).
        int outFps = (int)Math.Round(frameRate > 0 ? Math.Min(frameRate, newFrameRate) : newFrameRate);
        if (outFps <= 0) outFps = (int)newFrameRate;
        double currentBps = new FileInfo(path).Length * 8.0 / durationSec;
        double targetBps = CompressionBppTarget * newVideoWidth * newVideoHeight * outFps;
        if (currentBps <= targetBps * CompressionBppMargin)
        {
            return (VideoCompressResult.SkippedBitrateLow, oldDimension, new Rect(newVideoWidth, newVideoHeight));
        }

        var settings = new ConvertSettings
        {
            VideoCodec = "libx264",
            CustomOutputArgs = $"-preset fast -crf {qualityRate} -r {newFrameRate}",
            VideoFrameSize = $"{newVideoWidth}x{newVideoHeight}"
        };

        ffmpeg.ConvertMedia(path, null, compressedFile, null, settings);
        return (VideoCompressResult.Good, oldDimension, new Rect(newVideoWidth, newVideoHeight));
    }

    private static int toEven(double input)
    {
        return (int)(input / 2) * 2;
    }
}
