using System.Text.Json;
using System.Text.Json.Nodes;

using CivitaiImageDownloader.Models;
using CivitaiImageDownloader.Util;

using Common;

using NReco.VideoInfo;

namespace CivitaiImageDownloader;

public class Downloader : IDisposable
{
    private string SkipRecordFileName = "media-to-ignore.json";

    private readonly string _rootFolder;
    private readonly string _userName;
    private readonly List<string> _nsfwLevels;
    private readonly MediaType _mediaType;
    private HttpClient httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private FFProbe _ffProbe = new FFProbe();

    public event Action<string>? RaiseMessage;
    public event Action<int>? UpdateDownloadingCounter;

    private readonly List<string> _failedUrls = [];
    private int _skippedCount = 0;
    private int _downloadingCount = 0;

    public bool ShouldStop { get; set; }

    public bool ParallelMode { get; set; } = true;

    public Downloader(string rootFolder, string userName, List<string> nsfwLevels, MediaType mediaType)
    {
        _rootFolder = rootFolder;
        _userName = userName;
        _nsfwLevels = nsfwLevels;
        _mediaType = mediaType;
    }

    public async Task<DownloadResult> Run()
    {
        var allMetas = new List<MediaMeta>();
        var folder = Path.Combine(_rootFolder, _userName);
        Directory.CreateDirectory(folder);
        RenameTxtToJson(folder);
        await GetAllMetaInfos(allMetas, folder);
        var result = await DownloadMedia(allMetas, folder);

        httpClient.Dispose();
        return result;
    }

    private void RenameTxtToJson(string folder)
    {
        var files = Directory.GetFiles(folder, "*.txt");
        foreach (var file in files)
        {
            var fi = new FileInfo(file);
            fi.MoveTo(fi.FullName.Replace(".txt", ".json"));
        }
    }

    public async Task<List<ExistenceResult>> MarkNonExistFiles()
    {
        var allMetas = new List<MediaMeta>();
        var folder = Path.Combine(_rootFolder, _userName);
        Directory.CreateDirectory(folder);
        var existingInfoFiles = Directory.GetFiles(folder, "*.json");
        await GetLocalMetaInfo(allMetas, existingInfoFiles);

        List<ExistenceResult> results = [];

        LoopHelper.Loop(ParallelMode, allMetas, MarkMeta);

        var resultString = JsonSerializer.Serialize(results.Where(r => !r.IsExists).ToArray(), new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(folder, SkipRecordFileName), resultString);
        return results;

        bool MarkMeta(MediaMeta meta)
        {
            if (ShouldStop)
                return false;
            var url = meta.Url;
            var fileName = url.Split('/').Last();
            var path = meta.GetExpectedFilePath(folder);
            var existence = new ExistenceResult(url, meta.ExpectedFileName, path, File.Exists(path));
            lock (results)
            {
                results.Add(existence);
            }
            return true;
        }
    }

    private async Task GetAllMetaInfos(List<MediaMeta> allMetas, string folder)
    {
        // do not seek from website if info files exists
        var existingInfoFiles = Directory.GetFiles(folder, "*.json")
            .Where(f => !f.Contains(SkipRecordFileName)).ToArray();
        if (existingInfoFiles.Length != 0)
        {
            await GetLocalMetaInfo(allMetas, existingInfoFiles);
            return;
        }

        // get info from website
        var count = 0;
        var initInfoUrl = $"https://civitai.com/api/v1/images?username={_userName}&period=AllTime&sort=Newest&nsfw={_nsfwLevels.FirstOrDefault()}";
        string infoUrl = initInfoUrl;

        // never in parallel-mode
        while (!ShouldStop)
        {
            try
            {
                var infoContent = await GetInfoContentAsync(httpClient, infoUrl);
                if (string.IsNullOrWhiteSpace(infoContent))
                {
                    RaiseMessage?.Invoke($"Failed to get contents: {infoUrl}.");
                    break;
                }
                InfoParseResult? infoParseResult = await ParseAsync(infoContent);
                if (infoParseResult == null)
                    break;

                var (mediaMetas, skippedCount, jObj, thisInfoUrl, nextInfoUrl) = infoParseResult;
                if (jObj != null)
                {
                    jObj?.Add(new KeyValuePair<string, JsonNode?>("url", infoUrl));
                    File.WriteAllText(Path.Combine(folder, $"{count:D4}.txt"), jObj?.ToJsonString());
                }
                count++;

                RaiseMessage?.Invoke($"Parsed #{count} info url, filtered in {mediaMetas.Count} media urls.");
                allMetas.AddRange(mediaMetas);
                _skippedCount += skippedCount;

                if (nextInfoUrl == null)
                    break;

                infoUrl = nextInfoUrl!;
            }
            catch (Exception e)
            {
                RaiseMessage?.Invoke($"Error: {e}.");
                count++;
            }
        }
    }

    private async Task GetLocalMetaInfo(List<MediaMeta> allMetas, string[] existingInfoFiles)
    {
        // never in parallel-mode as it is fast
        foreach (var file in existingInfoFiles)
        {
            if (ShouldStop)
                return;

            var infoContent = File.ReadAllText(file);
            InfoParseResult? infoParseResult = await ParseAsync(infoContent);
            if (infoParseResult == null)
                continue;

            var (mediaMetas, skippedCount, jObj, thisInfoUrl, nextInfoUrl) = infoParseResult;
            _skippedCount += skippedCount;
            RaiseMessage?.Invoke($"Parsed local: {file}, got {mediaMetas.Count} image urls, next? {nextInfoUrl != null}");
            allMetas.AddRange(mediaMetas);
        }
    }

    private async Task<DownloadResult> DownloadMedia(List<MediaMeta> allMetas, string folder)
    {
        var fileNamesExist = GetFileNamesAlreadyExist(folder);
        var fileNamesToSkip = GetFileNamesToSkip(folder);
        var toDownloadMetas = allMetas.Where(meta => !fileNamesToSkip.Contains(meta.ExpectedFileName)).ToList();
        if (toDownloadMetas.Count != allMetas.Count)
        {
            RaiseMessage?.Invoke($"[{fileNamesToSkip.Count}] was marked as IGNORE; actual downloading count: [{toDownloadMetas.Count}]");
            allMetas = toDownloadMetas;
        }
        toDownloadMetas = allMetas.Where(meta => !fileNamesExist.Any(f => f.Contains(meta.ExpectedFileName))).ToList();
        if (toDownloadMetas.Count != allMetas.Count)
        {
            RaiseMessage?.Invoke($"[{fileNamesExist.Count}] was already downloaded; actual downloading count: [{toDownloadMetas.Count}]");
            allMetas = toDownloadMetas;
        }
        var actualDownloadedCount = 0;
        var totalCount = allMetas.Count;

        await LoopHelper.LoopAsync(ParallelMode, _cts, totalCount, async i =>
        {
            var r = await Download(allMetas, folder, i, totalCount);
            var count = Interlocked.Decrement(ref _downloadingCount);
            UpdateDownloadingCounter?.Invoke(count);
            return r;
        });


        RaiseMessage?.Invoke($"TargetUser [{_userName}]: downloaded [{actualDownloadedCount}/{totalCount}]; failure: [{_failedUrls.Count}].");

        return new DownloadResult(_userName, _skippedCount, totalCount, actualDownloadedCount, _failedUrls);

        async Task<bool> Download(List<MediaMeta> allMetas, string folder, int i, int totalCount)
        {
            if (ShouldStop)
                return false;

            var meta = allMetas[i];
            var url = meta.Url;
            var retriedCount = 0;
            var shallRetry = false;

            UpdateDownloadingCounter?.Invoke(Interlocked.Increment(ref _downloadingCount));
            do
            {
                if (ShouldStop)
                    return false;

                retriedCount++;
                var fileName = url.Split('/').Last();
                var path = meta.GetExpectedFilePath(folder);
                try
                {
                    if (File.Exists(path))
                    {
                        meta.IsExists = true;
                        return false;
                    }
                    if (meta.IsImage && !_mediaType.HasFlag(MediaType.Image))
                    {
                        Interlocked.Increment(ref _skippedCount);
                        return false;
                    }
                    if (meta.IsVideo && !_mediaType.HasFlag(MediaType.Video))
                    {
                        Interlocked.Increment(ref _skippedCount);
                        return false;
                    }

                    Thread.Sleep(100);

                    await SaveToFile(url, path, i);
                    shallRetry = false;

                    // detect webp/yuv format
                    MediaInfo videoInfo = _ffProbe.GetMediaInfo(path);
                    MediaInfo.StreamInfo? stream = videoInfo.Streams.FirstOrDefault(stream => stream.CodecType.ToLower() == "video");
                    if (stream?.CodecName == "webp" && meta.WebpUrl != "")
                    {
                        // redownload with transcode and non-original params
                        File.Delete(path);
                        await SaveToFile(meta.WebpUrl, path, i);
                        RaiseMessage?.Invoke($"[{i}/{totalCount}] IsTranscoded from WEBP: {url} to {path}");
                    }
                }
                catch (Exception e1)
                {
                    _failedUrls.Add(url);
                    RaiseMessage?.Invoke($"[{e1.GetType().Name}] Failed to download media: {url}");
                    shallRetry = true;
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
            while (retriedCount < 10 && shallRetry);
            return true;
        }

        async Task SaveToFile(string url, string path, int i)
        {
            var startTime = DateTime.Now;
            HttpResponseMessage response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync();
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            fs.Write(bytes, 0, bytes.Length);

            long length = new FileInfo(path).Length;
            var endTime = DateTime.Now;
            var elapsed = (endTime - startTime).TotalSeconds;
            if (length == 0)
            {
                RaiseMessage?.Invoke($"[{elapsed:F4}] [{i}/{totalCount}] [{length}b] [FAILED] Download: {url} to {path}");
            }
            else
            {
                RaiseMessage?.Invoke($"[{elapsed:F4}] [{i}/{totalCount}] [{length}b] Downloaded: {url} to {path}");
                Interlocked.Increment(ref actualDownloadedCount);
            }
        }
    }

    private List<string> GetFileNamesToSkip(string folder)
    {
        List<string> filesToSkip = [];
        var path = Path.Combine(folder, SkipRecordFileName);
        if (File.Exists(path))
        {
            var content = File.ReadAllText(path);

            var nodes = JsonNode.Parse(content)?.AsArray();
            foreach (var node in nodes)
            {
                filesToSkip.Add(node.GetStr("FileName"));
            }
        }
        return filesToSkip;
    }

    private List<string> GetFileNamesAlreadyExist(string folder)
    {
        return Directory.GetFiles(folder).Where(s => !s.EndsWith(".txt") && !s.EndsWith(".json"))
            .ToList();
    }

    private async Task<string> GetInfoContentAsync(HttpClient httpClient, string infoUrl)
    {
        var tryCount = 0;
        var maxCount = 10;
        while (tryCount < maxCount)
        {
            if (ShouldStop)
                return "";

            try
            {
                var response = await httpClient.GetAsync(infoUrl);
                if (!response.IsSuccessStatusCode)
                    return "";
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception e)
            {
                tryCount++;
                RaiseMessage?.Invoke($"Retrying {tryCount}/{maxCount} for url: " + infoUrl);
            }
        }
        return "";
    }

    private async Task<InfoParseResult?> ParseAsync(string content)
    {
        JsonObject? node = null;
        string? thisInfoUrl = null;
        string? nextInfoUrl = null;
        var mediaMetas = new List<MediaMeta>();
        var skippedCount = 0;

        try
        {
            node = JsonNode.Parse(content)?.AsObject();
            JsonArray? array = node?["items"]?.AsArray();
            if (array == null)
                return null;
            thisInfoUrl = node.GetStr("url");

            await Task.Run(() =>
            {
                LoopHelper.Loop(ParallelMode, array, ParseMeta);
            });


            var nextItemNode = node?["metadata"]?.AsObject();
            nextInfoUrl = nextItemNode?["nextPage"]?.ToString()?.Replace("\u0026", "&");
        }
        catch (Exception e)
        {
            RaiseMessage?.Invoke($"ERR! {e}");
            nextInfoUrl = null;
        }

        return new InfoParseResult(mediaMetas, skippedCount, node, thisInfoUrl, nextInfoUrl);

        bool ParseMeta(JsonNode? item)
        {
            if (ShouldStop)
                return false;

            if (item == null)
                return true;

            var url = item.GetStr("url");
            if (url == null)
                return true;

            var mediaMeta = new MediaMeta();

            var nsfwLevel = item.GetStr("nsfwLevel");
            if (!_nsfwLevels.Contains(nsfwLevel))
            {
                Interlocked.Increment(ref skippedCount);
                return true;
            }

            mediaMeta.Url = url;
            mediaMeta.WebpUrl = url.EndsWith(".mp4") ? url.Replace("original=true", "transcode=false,original=false,optimized=true") : "";
            mediaMeta.NsfwLevel = nsfwLevel;
            mediaMeta.Type = item.GetStr("type");
            mediaMeta.Id = item.GetInt("id");
            mediaMeta.PostId = item.GetInt("postId");
            lock (mediaMetas)
                mediaMetas.Add(mediaMeta);

            return true;
        }
    }

    public void Dispose()
    {
        httpClient?.Dispose();
    }
}
