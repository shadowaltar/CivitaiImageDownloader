namespace CivitaiImageDownloader.Models;

[Flags]
public enum MediaType
{
    None = 0,
    Image = 0x1,
    Video = 0X10,
    ImageAndVideo = 0x11
}
