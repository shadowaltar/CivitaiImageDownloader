namespace CivitaiImageDownloader;

internal enum VideoCompressResult
{
    Good,
    Failed,
    SkippedFileSizeTooSmall,
    SkippedDimensionTooSmall,
    SkippedWrongFormat,
    SkippedBitrateLow,
}