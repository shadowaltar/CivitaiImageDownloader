namespace CivitaiImageDownloader;
public record MediaMeta
{
    private string url = "";

    public string Url
    {
        get => url;
        set
        {
            url = value;
            OriginalFileName = Url.Split('/').Last();
        }
    }

    public string NsfwLevel { get; set; } = "None";

    public int Id { get; set; }

    public int PostId { get; set; }

    public string Type { get; set; } = "image";

    public bool IsVideo => Type == "video";

    public bool IsImage => Type == "image";

    public bool IsExists { get; set; } = false;

    public string? OriginalFileName { get; set; }

    public string ExpectedFileName => $"{PostId}_{Id}_{OriginalFileName}";

    public string GetExpectedFilePath(string folder)
    {
        return Path.Combine(folder, ExpectedFileName);
    }
}
