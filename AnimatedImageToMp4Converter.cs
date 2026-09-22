using System.Diagnostics;
using NetVips;
using NReco.VideoConverter;
using VipsImage = NetVips.Image;

namespace CivitaiImageDownloader;

/// <summary>
/// Converts animated WebP and animated GIF files to MP4. Static images are skipped.
/// Frames are decoded with libvips and piped as raw video into ffmpeg; each source frame
/// is held for as many output frames as needed to preserve the original timing at the target fps.
/// </summary>
public class AnimatedImageToMp4Converter
{
    private static readonly string[] Extensions = [".webp", ".gif"];

    private readonly string _folder;
    private readonly int _targetFps;

    public AnimatedImageToMp4Converter(string folder, int targetFps)
    {
        _folder = folder;
        _targetFps = Math.Clamp(targetFps, 1, 120);
    }

    public Action<string>? RaiseMessage { get; set; }

    public async Task Run()
    {
        var files = Directory.EnumerateFiles(_folder, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true
        })
            .Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => f)
            .ToArray();

        RaiseMessage?.Invoke($"Found {files.Length} webp/gif file(s) under {_folder} (including subfolders).");
        int converted = 0, failed = 0, skipped = 0;
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var prefix = $"[{i + 1}/{files.Length}] ";
            if (!IsAnimated(file))
            {
                skipped++;
                RaiseMessage?.Invoke(prefix + $"Skipped static image: {Path.GetFileName(file)}");
                continue;
            }

            var outPath = Path.ChangeExtension(file, ".mp4");
            if (File.Exists(outPath))
            {
                skipped++;
                RaiseMessage?.Invoke(prefix + $"Skipped, mp4 already exists: {Path.GetFileName(outPath)}");
                continue;
            }

            try
            {
                RaiseMessage?.Invoke(prefix + $"Converting {Path.GetFileName(file)} @ {_targetFps}fps ...");
                await Task.Run(() => ConvertFile(file, outPath));

                bool originalDeleted = true;
                try { File.Delete(file); }
                catch (Exception de)
                {
                    originalDeleted = false;
                    RaiseMessage?.Invoke(prefix + $"Warning: converted but failed to delete original {Path.GetFileName(file)}: {de.Message}");
                }
                converted++;
                RaiseMessage?.Invoke(prefix + (originalDeleted
                    ? $"Done: {Path.GetFileName(outPath)} (original deleted)"
                    : $"Done: {Path.GetFileName(outPath)} (original kept)"));
            }
            catch (Exception e)
            {
                failed++;
                RaiseMessage?.Invoke(prefix + $"Failed {Path.GetFileName(file)}: {e.Message}");
                try { if (File.Exists(outPath)) File.Delete(outPath); } catch { }
            }
        }
        RaiseMessage?.Invoke($"Webp/Gif to MP4 finished. Converted/Failed/Skipped: {converted}/{failed}/{skipped}");
    }

    // Animated WebP/GIF files expose an "n-pages" metadata value > 1; static images have none.
    // The image is loaded from memory so no file handle is kept open (the original must be deletable).
    private static bool IsAnimated(string path)
    {
        try
        {
            using var img = VipsImage.NewFromBuffer(File.ReadAllBytes(path), access: Enums.Access.Sequential);
            try { return Convert.ToInt32(img.Get("n-pages")) > 1; }
            catch { return false; }
        }
        catch
        {
            return false;
        }
    }

    private static string GetFFmpegPath()
    {
        var converter = new FFMpegConverter();
        var dir = string.IsNullOrWhiteSpace(converter.FFMpegToolPath) ? AppContext.BaseDirectory : converter.FFMpegToolPath;
        var exe = string.IsNullOrWhiteSpace(converter.FFMpegExeName) ? "ffmpeg.exe" : converter.FFMpegExeName;
        var full = Path.Combine(dir, exe);
        return File.Exists(full) ? full : exe;
    }

    private void ConvertFile(string inputPath, string outPath)
    {
        using var img = VipsImage.NewFromBuffer(File.ReadAllBytes(inputPath), access: Enums.Access.Sequential, kwargs: new VOption { { "n", -1 } });

        int pages = 1;
        try { pages = Convert.ToInt32(img.Get("n-pages")); } catch { }
        int w = img.Width;
        int h = img.PageHeight > 0 ? img.PageHeight : img.Height / Math.Max(pages, 1);
        int bands = img.Bands;
        int fps = _targetFps;
        string pixFmt = bands == 4 ? "rgba" : "rgb";

        int[] delays;
        try { delays = (int[])img.Get("delay"); } catch { delays = []; }
        if (delays.Length != pages)
            delays = Enumerable.Repeat(100, pages).ToArray();

        var tmpPath = outPath + ".tmp.mp4";
        try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }

        var psi = new ProcessStartInfo(GetFFmpegPath())
        {
            RedirectStandardInput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = $"-hide_banner -y -f rawvideo -pixel_format {pixFmt} -video_size {w}x{h} -framerate {fps} " +
                        $"-i - -an -c:v libx264 -preset veryfast -crf 20 -pix_fmt yuv420p -movflags +faststart \"{tmpPath}\""
        };

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start ffmpeg.");
        var errTask = proc.StandardError.ReadToEndAsync();
        try
        {
            using (var stdin = proc.StandardInput.BaseStream)
            {
                for (int i = 0; i < pages; i++)
                {
                    using var page = img.Crop(0, i * h, w, h);
                    var raw = page.WriteToMemory();
                    int hold = Math.Max(1, (int)Math.Round(delays[i] * fps / 1000.0));
                    for (int k = 0; k < hold; k++)
                        stdin.Write(raw, 0, raw.Length);
                }
            }
            proc.WaitForExit();
        }
        catch
        {
            try { if (!proc.HasExited) proc.Kill(true); } catch { }
            throw;
        }

        var err = errTask.Result;
        if (proc.ExitCode != 0)
        {
            try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }
            throw new Exception("ffmpeg exited with code " + proc.ExitCode + ": " + err);
        }

        File.Move(tmpPath, outPath, overwrite: true);
    }
}
