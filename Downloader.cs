using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using CivitaiImageDownloader.Models;

using Common;

namespace CivitaiImageDownloader;

public class Downloader : IDisposable
{
    private readonly string _rootFolder;
    private readonly string _userName;
    private readonly List<string> _nsfwLevels;
    private readonly MediaType _mediaType;
    private HttpClient httpClient;

    public event Action<string>? RaiseMessage;

    private readonly List<string> _failedUrls = [];
    private int _skippedCount = 0;

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
        await GetAllMetaInfos(allMetas, folder);
        var result = await DownloadMedia(allMetas, folder);

        httpClient.Dispose();
        return result;
    }

    public async Task<List<ExistenceResult>> MarkNonExistFiles()
    {
        var allMetas = new List<MediaMeta>();
        var folder = Path.Combine(_rootFolder, _userName);
        Directory.CreateDirectory(folder);
        var existingInfoFiles = Directory.GetFiles(folder, "*.txt");
        await GetLocalMetaInfo(allMetas, existingInfoFiles);

        List<ExistenceResult> results = [];
        Parallel.ForEach(allMetas, meta =>
        {
            var url = meta.Url;
            var fileName = url.Split('/').Last();
            var path = meta.GetExpectedFilePath(folder);
            var existence = new ExistenceResult(url, meta.ExpectedFileName, path, File.Exists(path));
            lock (results)
            {
                results.Add(existence);
            }
        });

        var resultString = JsonSerializer.Serialize(results.Where(r => !r.IsExists).ToArray(), new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(folder, "files-not-exist.json"), resultString);
        return results;
    }

    private async Task GetAllMetaInfos(List<MediaMeta> allMetas, string folder)
    {
        // do not seek from website if info files exists
        var existingInfoFiles = Directory.GetFiles(folder, "*.txt");
        if (existingInfoFiles.Length != 0)
        {
            await GetLocalMetaInfo(allMetas, existingInfoFiles);
            return;
        }

        // get info from website
        var count = 0;
        httpClient ??= new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        var initInfoUrl = $"https://civitai.com/api/v1/images?username={_userName}&period=AllTime&sort=Newest&nsfw={_nsfwLevels.FirstOrDefault()}";
        string infoUrl = initInfoUrl;

        while (true)
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
                RaiseMessage?.Invoke($"Parsed: {infoUrl}, got {mediaMetas.Count} image urls, next? {nextInfoUrl != null}");
                allMetas.AddRange(mediaMetas);
                count++;
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
        foreach (var file in existingInfoFiles)
        {
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
        var totalCount = allMetas.Count;
        var actualDownloadedCount = 0;

        httpClient ??= new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
        await Parallel.ForAsync(0, totalCount, async (i, s) =>
        {
            var meta = allMetas[i];
            var url = meta.Url;
            var retriedCount = 0;
            var shallRetry = false;
            do
            {
                retriedCount++;
                try
                {
                    var fileName = url.Split('/').Last();
                    var path = meta.GetExpectedFilePath(folder);
                    if (File.Exists(path))
                    {
                        meta.IsExists = true;
                        return;
                    }
                    if (meta.IsImage && !_mediaType.HasFlag(MediaType.Image))
                    {
                        Interlocked.Increment(ref _skippedCount);
                        return;
                    }
                    if (meta.IsVideo && !_mediaType.HasFlag(MediaType.Video))
                    {
                        Interlocked.Increment(ref _skippedCount);
                        return;
                    }

                    Thread.Sleep(100);

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
                    shallRetry = false;
                }
                catch (Exception e1)
                {
                    _failedUrls.Add(url);
                    RaiseMessage?.Invoke($"[{e1.GetType().Name}] Failed to download media: {url}");
                    shallRetry = true;
                }
            }
            while (retriedCount < 10 && shallRetry);
        });
        RaiseMessage?.Invoke($"TargetUser [{_userName}]: downloaded [{actualDownloadedCount}/{totalCount}]; failure: [{_failedUrls.Count}].");

        return new DownloadResult(_userName, _skippedCount, totalCount, actualDownloadedCount, _failedUrls);
    }

    private async Task<string> GetInfoContentAsync(HttpClient httpClient, string infoUrl)
    {
        var tryCount = 0;
        var maxCount = 10;
        while (tryCount < maxCount)
        {
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
            var array = node?["items"]?.AsArray();
            if (array == null)
                return null;
            thisInfoUrl = node.GetStr("url");

            await Task.Run(() =>
            {
                Parallel.ForEach(array, item =>
                {
                    if (item == null)
                        return;

                    var url = item.GetStr("url");
                    if (url == null)
                        return;

                    var mediaMeta = new MediaMeta();

                    var nsfwLevel = item.GetStr("nsfwLevel");
                    if (!_nsfwLevels.Contains(nsfwLevel))
                    {
                        Interlocked.Increment(ref skippedCount);
                        return;
                    }

                    mediaMeta.Url = url;
                    mediaMeta.NsfwLevel = nsfwLevel;
                    mediaMeta.Type = item.GetStr("type");
                    mediaMeta.Id = item.GetInt("id");
                    mediaMeta.PostId = item.GetInt("postId");
                    lock (mediaMetas)
                        mediaMetas.Add(mediaMeta);
                });
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
    }

    public void Dispose()
    {
        httpClient?.Dispose();
    }
}
