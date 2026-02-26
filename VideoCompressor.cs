using CivitaiImageDownloader.Util;

using NReco.VideoConverter;
using NReco.VideoInfo;

namespace CivitaiImageDownloader;
public class VideoCompressor : IDisposable
{
    private string _rootFolder;
    private string _userName;
    private readonly FFMpegConverter _ffMpeg = new FFMpegConverter();
    private readonly FFProbe _ffProbe = new FFProbe();

    public int CompressionSizeBytesThreshold { get; set; } = 4 * 1024 * 1024;
    public int CompressionFrameSizeDimensionThreshold { get; set; } = 1000;
    public double CompressionFrameSizeRatio { get; set; } = .75;

    public VideoCompressor(string targetFolder, string un)
    {
        _rootFolder = targetFolder;
        _userName = un;
        _ffMpeg = new FFMpegConverter();
    }

    public bool ShouldStop { get; internal set; }

    public Action<string> RaiseMessage { get; internal set; }

    public void Dispose()
    {

    }

    public async Task Run()
    {
        var folder = FolderHelper.GetFolder(_rootFolder, _userName);
        if (!string.IsNullOrEmpty(folder))
        {
            RaiseMessage?.Invoke("Use folder: " + folder);
        }
        else
        {
            RaiseMessage?.Invoke("Folder not found: " + folder);
            return;
        }
        foreach (var path in Directory.GetFiles(folder, "*.mp4", SearchOption.AllDirectories))
        {
            try
            {
                if (ShouldStop)
                {
                    RaiseMessage?.Invoke("Video compression stopped.");
                    break;
                }
                var ext = Path.GetExtension(path).ToLower();
                if (ext != ".mp4" && ext != ".webm" && ext != ".mov" && ext != ".avi")
                {
                    continue;
                }

                var fi = new FileInfo(path);
                if (fi.Length < CompressionSizeBytesThreshold)
                {
                    // skip small files
                    continue;
                }
                var compressedFile = Path.Combine(folder, "compressed_" + fi.Name);
                RaiseMessage?.Invoke($"Compressing video: {fi.Name} ...");

                int videoWidth = 0;
                int videoHeight = 0;
                int newVideoWidth = 0;
                int newVideoHeight = 0;
                bool shallResize = false;
                await Task.Run(() =>
                {
                    MediaInfo videoInfo = _ffProbe.GetMediaInfo(path);
                    if (videoInfo.Streams != null && videoInfo.Streams.Length > 0)
                    {
                        var videoStream = videoInfo.Streams[0]; // Assuming the first stream is the video stream

                        videoWidth = videoStream.Width;
                        videoHeight = videoStream.Height;
                    }
                    shallResize = videoHeight > CompressionFrameSizeDimensionThreshold || videoWidth > CompressionFrameSizeDimensionThreshold;
                    var settings = new ConvertSettings()
                    {
                        //VideoFrameRate = 30,
                        VideoCodec = "libx264",
                        CustomOutputArgs = "-preset fast -crf 22"
                    };
                    if (shallResize)
                    {
                        newVideoWidth = (int)(videoWidth * CompressionFrameSizeRatio);
                        newVideoHeight = (int)(videoHeight * CompressionFrameSizeRatio);
                        settings.VideoFrameSize = newVideoWidth + "x" + newVideoHeight;
                    }
                    _ffMpeg.ConvertMedia(path, null, compressedFile, null, settings);
                });
                var resultFile = new FileInfo(compressedFile);
                if (!shallResize)
                    RaiseMessage?.Invoke($"Compressed {fi.Name}: {fi.Length / 1024}KB -> {resultFile.Length / 1024}KB");
                else
                    RaiseMessage?.Invoke($"Compressed {fi.Name}: {fi.Length / 1024}KB -> {resultFile.Length / 1024}KB; {videoWidth}x{videoHeight} -> {newVideoWidth}x{newVideoHeight}");

                File.Delete(path);
                File.Move(compressedFile, compressedFile.Replace("compressed_", ""));
            }
            catch (Exception e)
            {
                RaiseMessage?.Invoke($"Error: {e}.");
            }
        }
    }
}
