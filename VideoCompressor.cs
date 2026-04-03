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

    public int CompressionSizeBytesThreshold { get; set; } = 3 * 1024 * 1024; // 3MB
    public int CompressionFrameSizeDimensionThreshold { get; set; } = 1000;
    public double CompressionFrameSizeRatio { get; set; } = .75;

    public VideoCompressor(string targetFolder, string name, VideoProcessInputMode mode)
    {
        _rootFolder = targetFolder;
        this.name = name;
        this.mode = mode;
    }

    public bool ShouldStop { get; internal set; }

    public Action<string> RaiseAddMessage { get; internal set; }

    public Action<string> RaiseAppendMessage { get; internal set; }

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
            for (int i = 0; i < totalCount; i++)
            {
                string? path = files[i];
                string logPrefix = $"[{i}/{totalCount}] ";
                var result = await Compress(folder, ffmpeg, logPrefix, path);
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
            var ffmpeg = new FFMpegConverter();
            var result = await Compress(folder, ffmpeg, "", name);
            RaiseAddMessage?.Invoke($"Finished, result: {(result == VideoCompressResult.Good ? "Good" : "Failed")}");
        }
    }

    private async Task<VideoCompressResult> Compress(string folder, FFMpegConverter ffmpeg, string logPrefix, string path)
    {
        string? compressedFile = null;
        try
        {
            var ext = Path.GetExtension(path).ToLower();
            if (ext != ".mp4" && ext != ".webm" && ext != ".mov" && ext != ".avi")
            {
                return VideoCompressResult.SkippedWrongFormat;
            }

            var fi = new FileInfo(path);
            if (fi.Length < CompressionSizeBytesThreshold)
            {
                // skip small files
                return VideoCompressResult.SkippedFileSizeTooSmall;
            }
            compressedFile = Path.Combine(folder, "compressing_" + fi.Name);
            RaiseAddMessage?.Invoke($"{logPrefix}Compress video: {fi.Name} ...");

            await Task.Run(() =>
            {
                (var result, Rect old, Rect @new) = Convert(ffmpeg, path, compressedFile);
                var resultFile = new FileInfo(compressedFile);
                if (resultFile.Exists)
                {
                    var oldMb = fi.Length / 1024.0 / 1024.0;
                    var newMb = resultFile.Length / 1024.0 / 1024.0;
                    RaiseAppendMessage?.Invoke($" Result: {oldMb:.00}MB -> {newMb:.00}MB; {old.Width}x{old.Height} -> {@new.Width}x{@new.Height}");

                    File.Delete(path);
                    File.Move(compressedFile, compressedFile.Replace("compressing_", ""));
                }
                else
                    RaiseAppendMessage?.Invoke($" Skipped: {result}");
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

        return VideoCompressResult.Good;
    }

    private (VideoCompressResult result, Rect oldDimension, Rect newDimension) Convert(FFMpegConverter ffmpeg, string path, string? compressedFile)
    {
        bool isGood = false;
        int videoWidth = 0;
        int videoHeight = 0;
        float frameRate = 0;
        int newVideoWidth = 0;
        int newVideoHeight = 0;
        const float newFrameRate = 30;
        const float qualityRate = 23;
        MediaInfo videoInfo = _ffProbe.GetMediaInfo(path);
        //bool isYuv420 = false;
        if (videoInfo.Streams != null && videoInfo.Streams.Length > 0)
        {
            var videoStream = videoInfo.Streams[0]; // Assuming the first stream is the video stream
            videoWidth = videoStream.Width;
            videoHeight = videoStream.Height;
            frameRate = videoStream.FrameRate;
            //if (videoStream.PixelFormat == "yuv420p") // raw data
            //{
            //    isYuv420 = true;
            //}
        }
        else
        {
            return default;
        }
        // must enforce H.264 dimensions to even numbers
        newVideoWidth = toEven(videoWidth * CompressionFrameSizeRatio);
        newVideoHeight = toEven(videoHeight * CompressionFrameSizeRatio);

        var oldDimension = new Rect(videoWidth, videoHeight);
        if (new FileInfo(path).Length / 1024.0 / 1024.0 < 1)
        {
            return (VideoCompressResult.SkippedFileSizeTooSmall, oldDimension, oldDimension);
        }
        if (videoWidth == 500 || videoHeight == 500)
        {
            return (VideoCompressResult.SkippedDimensionTooSmall, oldDimension, oldDimension);
        }
        if (newVideoWidth <= 500 || newVideoHeight <= 500)
        {
            var ratio = videoWidth / (double)videoHeight;
            if (newVideoWidth <= 500)
            {
                newVideoWidth = 500;
                newVideoHeight = toEven(newVideoWidth / ratio);
            }
            else
            {
                newVideoHeight = 500;
                newVideoWidth = toEven(newVideoHeight * ratio);
            }
        }
        var settings = new ConvertSettings();
        //if (isYuv420)
        //{
        //    settings.CustomInputArgs = "-f h264";
        //    settings.CustomOutputArgs = $"-c:v libx264 -preset fast -crf {qualityRate} -s {newVideoWidth}x{newVideoHeight} -r 30 -pix_fmt yuv420p";

        //    //settings.CustomInputArgs = $"-f rawvideo -pixel_format yuv420p -video_size {videoWidth}x{videoHeight} -framerate {frameRate}";
        //    //settings.CustomOutputArgs = $"-c:v libx264 -preset fast -crf {qualityRate} -s {newVideoWidth}x{newVideoHeight} -r {newFrameRate} -pix_fmt yuv420p";
        //}
        //else

        settings.VideoCodec = "libx264";
        settings.CustomOutputArgs = $"-preset fast -crf {qualityRate} -r {newFrameRate}";
        settings.VideoFrameSize = $"{newVideoWidth}x{newVideoHeight}";

        ffmpeg.ConvertMedia(path, null, compressedFile, null, settings);
        isGood = true;
        return (VideoCompressResult.Good, oldDimension, new Rect(newVideoWidth, newVideoHeight));
    }

    private static int toEven(double input)
    {
        return (int)(input / 2) * 2;
    }
}
