using System.Text.Json.Nodes;

namespace CivitaiImageDownloader;
public record InfoParseResult(List<MediaMeta> Metas, int SkippedCount, JsonObject? Node, string? CurrentInfoUrl, string? NextInfoUrl);
