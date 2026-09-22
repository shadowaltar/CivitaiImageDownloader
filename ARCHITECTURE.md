# Architecture & Algorithms

This document describes the algorithms, file formats and retention rules used by **CivitaiImageDownloader**.
It is intentionally *not* `README.md`, so GitHub does not render it on the repository landing page.

---

## 0. Overview

- **Runtime**: .NET 8 WinForms (`net8.0-windows`), single main window `MainForm` ("CivitAI Worker") with four tabs:
  - **Download** – fetch CivitAI index metadata and media, mark deleted files, move users between ratings, info-file maintenance.
  - **Video** – compress videos, enhance frame rate, convert animated WebP/GIF to MP4.
  - **History** – per-user folder statistics and action history.
  - **Viewer** – thumbnail grid with inline video playback and file deletion.
- **Shared state**: `AppMediator` holds the target output folder, current usernames text, `Stopping` flag, results and cross-tab events (copy usernames, tab switching).
- **Layout inside a user folder**: media files live next to the metadata JSON (optionally zipped). Media file names are `{PostId}_{Id}_{OriginalFileName}`.

---

## 1. CivitAI download algorithm

### 1.1 Inputs (`DownloadParameters`)
`TargetFolder`, `UserName`, `UserNames`, `NsfwLevels`, `MediaType`, `SkipLatestIndexFetch`, `Limit` (default 500), and the info-only flag `DownloadInfoOnly`.

UI mapping for NSFW levels: `NSFW → "X"`, `Mature → "Mature"`, `Normal → "Soft"`, `Child → "None"`. `MediaType` is a flag enum (`Image`/`Video`).

### 1.2 Folder resolution
`FolderHelper.GetFolder(root, userName)` recursively searches for a directory whose **name equals the username** and returns the first match. This lets user folders live under rating sub-folders (see §3). If found, it is used; otherwise root is created as `<root>\<userName>`.

### 1.3 Index assembly (`GetAllMetaInfos`)
1. **Local first** – read every existing info file in the folder (`Utils.GetInfoFiles`, see §2); if none are loose it first extracts `info.json.zip`, then parses each `items` array into `MediaMeta`.
2. **Remote** (unless `SkipLatestIndexFetch`): page through
   `https://civitai.com/api/v1/images?username={user}&period=AllTime&sort=Newest&nsfw={firstNsfw}`.
   Pagination follows `metadata.nextPage` until it is null.
   Requests attach a bearer token (hard-coded in `Downloader.GetInfoContentAsync`; change it there if needed).
3. **De-duplication key** = `{id}|{url}|{postId}` — the `url`/`postId` are included because item `id` alone is not guaranteed unique.
4. **Stop condition**: on a page where all items are already known, fetching stops ("hit known items").
5. **Persistence**: only *new* items are written to a fresh `{yyyyMMdd-HHmmss}.json` (see §2). Unwanted fields (`hash`, `meta`, `username`, `baseModel`, `modelVersionIds`, `stats`) are stripped before saving.

### 1.4 Media selection (`DownloadMedia`)
1. `fileNamesExist` = non-info files already on disk (top level).
2. **Limit**: if `existingCount >= Limit` → stop immediately.
3. **Ignore list**: metas whose `ExpectedFileName` appears in `media-to-ignore.json` are removed.
4. **Already-present**: metas whose `ExpectedFileName` is contained in an existing file path are removed.
5. Remaining metas are sorted **newest first** (`OrderByDescending(m => m.Id)`) so a limit keeps the latest media.
6. The loop runs sequentially (`ParallelMode = false`); each download sleeps 150 ms to be gentle on the API.

### 1.5 Per-file download (`Download` → `SaveToFile`)
- Target path = `folder + {PostId}_{Id}_{OriginalFileName}` with `.jpeg` normalized to `.jpg` (`GetPreferredJpegPath`).
- If a file already exists (either `.jpg`/`.jpeg` variant) it is skipped (`meta.IsExists = true`).
- Media-type filters skip images/videos that are not selected.
- Retry: one extra attempt on failure; failed URLs are collected.
- Zero-byte downloads are treated as failures.
- **Limit enforcement mid-run**: once `existing + downloaded >= Limit` the run sets `ShouldStop`.

### 1.6 Image post-processing (after a successful download)
- `TryConvertFromWebpOrYuv`: probe the saved file; if it is actually **WebP** (`codec == webp`) and a `WebpUrl` exists, the original is deleted and re-downloaded from the `transcode=false,original=false,optimized=true` URL (which returns a browser-friendly format).
- `CompressJpeg`: if the file is an image and not converted, it is re-encoded in place with **NetVips** at JPEG quality **80**.
- `TryDetectFakeMp4`: if a `.mp4` is really an **MJPEG/JPEG** stream, it is deleted and re-downloaded from the optimized URL.

### 1.7 Downloaded record
`UpdateDownloadedRecord` merges the existing `downloaded-record.json` with the current non-info file paths and rewrites the file. It is called before and after a download run, so it accumulates history even after files are deleted.

### 1.8 Mark deleted files ("no re-download") — `MarkNonExistFiles`
1. Build `allMetas` from **local info files only** (no network).
2. Refresh and load `downloaded-record.json`.
3. For every meta compute:
   - `isExists` = the expected file (or its `.jpg`/`.jpeg` alternative) exists on disk.
   - `wasDownloaded` = a downloaded-record path contains the expected file name (or its alternative).
4. Write **only** `!IsExists && WasDownloaded` entries to `media-to-ignore.json`. Future runs skip these names, so intentionally deleted files are not downloaded again.
   > Note: record files (`media-to-ignore.json`, `downloaded-record.json`) are explicitly excluded from info-file handling so this history is never zipped away or mistaken for index data.

### 1.9 Info-only mode
If `DownloadInfoOnly` is set, `Run()` fetches/saves the index and returns before `DownloadMedia`, so only metadata is downloaded.

---

## 2. Metadata JSON files

| File | Scope | Schema / contents | Lifetime |
|------|-------|-------------------|----------|
| `{yyyyMMdd-HHmmss}.json` | per user folder | `{ "items": [ …stripped media objects… ], "metadata": { "nextPage": null } }` | temporary; merged + zipped by `CompressInfoFiles` |
| `info.json.zip` | per user folder | ZIP archive of the timestamped index files (flat entries, no folders; record files never included) | durable index archive |
| `media-to-ignore.json` | per user folder | array of `ExistenceResult` = `{ Url, FileName, FilePath, IsExists, WasDownloaded }` (only deleted-but-downloaded entries) | rewritten by "Mark Deleted Files" |
| `downloaded-record.json` | per user folder | array of absolute file paths ever downloaded | appended/merged on each run |
| `username-history.json` | **parent output folder** | array of `{ Action, Timestamp, UsernamesConcatenated }` (newest first, de-duplicated against the previous entry) | durable history |
| `redoable-deleted.json` | **parent output folder** | array of `{ originalPath, movedTo, timestamp }` for viewer deletions | durable undo log |

### 2.1 Info-file handling rules (`Util/Utils.cs`)
- `IsInfoFile(file)` = not `media-to-ignore.json` and not `downloaded-record.json`.
- `GetInfoFiles(folder)`: returns the loose `*.json` info files, **excluding** the two record files.
- `ZipInfoFiles(folder)`: if any loose info `*.json` exist, it deletes the existing `info.json.zip`, creates a new archive, adds each loose file under its **flat** name (`Path.GetFileName(file)`), then deletes each loose file. If there are no loose info files it does nothing and leaves the existing archive in place. Record files are never included.
- `CompressInfoFiles(folder)`: parses every info file, strips unwanted keys, de-duplicates by item `id`, writes one combined `{timestamp}.json` with `metadata.nextPage = null`, deletes the old info files, and re-zips.
- Legacy `*.txt` index files are renamed to `*.json` on entry (`RenameTxtToJson`).

### 2.2 Zip lifecycle (when zipping happens)
Index JSON is kept **loose only while it is being read or written**; otherwise it lives inside `info.json.zip`. The cycle is *extract on demand → read/modify → re-zip*:

1. **Extract on demand** — `GetInfoFiles` returns the loose files; if the folder has no loose info JSON it first extracts `info.json.zip`. Extraction skips entry names that are record files and **overwrites** existing info entries (`ExtractToFile(..., overwrite: true)`), so re-running is safe.
2. **Re-zip after the operation** — the same entry points always call `ZipInfoFiles` again:
   - `Downloader.Run()`: `GetAllMetaInfos` (reads local info, which may extract) → `Utils.ZipInfoFiles(folder)` → `DownloadMedia`.
   - `Downloader.MarkNonExistFiles()`: `Utils.GetInfoFiles` → `GetLocalMetaInfo` → `Utils.ZipInfoFiles`.
   - `Utils.CompressInfoFiles(...)` (the **Compress Info Files** button): consolidate → write combined JSON → `ZipInfoFiles`.
3. **Net effect**: after any run the user folder is left with `info.json.zip` and no loose index JSON, so the next reader transparently extracts again. Because record files (`media-to-ignore.json`, `downloaded-record.json`) are excluded at every step, they survive re-zipping and are never archived. A typical archive is therefore a single (or few) `{yyyyMMdd-HHmmss}.json` entry with `items` + `metadata.nextPage = null`.

---

## 3. File & folder retention

### 3.1 Layout
```
<TargetFolder>/
  username-history.json
  redoable-deleted.json
  <userName>/                       (or under a rating folder, see below)
      <media files>
      20250101-120000.json  → info.json.zip
      media-to-ignore.json
      downloaded-record.json
  !deleted/                         moved (not destroyed) viewer deletions
  !3/  !4/  !4.5/  !5/  !6/         rating folders (Move users to rating)
      <userName>/ ...
```

- Any directory whose name **starts with `!`** is treated as a *metadata root*: the History and Viewer tabs scan the parent folder **and** recursively into these `!` folders to discover user folders. A user folder is a directory that does **not** start with `!`.
- `!deleted` holds files removed from the Viewer, so deletion is recoverable via `redoable-deleted.json`.

### 3.2 Deletion / replacement semantics
- **Viewer `Delete`**: moves the file into `!deleted` (renaming with a timestamp on collision) and appends an entry to `redoable-deleted.json`. Nothing is destroyed.
- **Enhance Frame Rate / Compress Video / Webp-Gif→MP4**: on success the original is **replaced** (temporary `.enhanced.mp4` / `compressing_*` / `*.tmp.mp4`, then the source is deleted). Older metadata is unchanged; the file name (base name) is preserved.
- **Delete Info Files** button: removes `*.txt` files in the selected users' folders.
- **History / statistics counting** excludes `!`-subfolders and non-media extensions (`.json`, `.txt`, `.json.zip`).

---

## 4. Image processing

### 4.1 JPEG variants
`PathUtils` provides `GetPreferredJpegPath` (`.jpeg → .jpg`, used when saving) and `GetAlternativeJpegPath` (`.jpg ↔ .jpeg`, used when checking existence/records).

### 4.2 Static vs animated detection
- **WebP**: read the RIFF header. Animated WebP has a `VP8X` chunk whose flags byte (offset 20) has the ANIMATION bit `0x02` set; static files start with `VP8 ` (lossy) or `VP8L` (lossless). Animated files additionally carry `ANIM` + `ANMF` chunks.
- **GIF / general**: load the first frame with libvips and read the `n-pages` metadata; `> 1` means animated. Static images have no `n-pages` at all. This single check also covers WebP, so the converter uses `n-pages` as the unified test.

### 4.3 Compression heuristics (JPEG)
After download, images are re-encoded with **NetVips** (`Jpegsave`, quality 80) to shrink oversized originals.

---

## 5. Video processing

### 5.1 Webp/Gif → MP4 (`AnimatedImageToMp4Converter`)
1. Recursively enumerate `*.webp` and `*.gif` (`IgnoreInaccessible = true`).
2. Skip static images (`n-pages <= 1`); skip if the target `.mp4` already exists.
3. Decode **from memory** (avoids locking the source file) with libvips, `n = -1` to load all frames.
4. Frame geometry: `width`, `pageHeight`; band count selects `rgba` (4) or `rgb` (3) raw pixel format.
5. **Timing preservation**: each source frame is written `max(1, round(delay_ms × targetFps / 1000))` times, so the animation keeps its original speed while the container runs at the tab's **Target FPS**.
6. Frames are piped as `-f rawvideo` into **ffmpeg**:
   `-c:v libx264 -preset veryfast -crf 20 -pix_fmt yuv420p -movflags +faststart`.
7. On success the source file is **deleted** (the `.mp4` keeps the original base name).

Measured benefit: animated GIFs converted to MP4 came out at **~6% of the original size** (~16× smaller); animated WebP was similar.

### 5.2 Compress Video (`VideoCompressor`)
- Applies only to `.mp4/.webm/.mov/.avi`; `.mp4` only in username mode.
- **Skips**: files `< 3 MB`, or dimensions equal to 640, or (after scaling) target `<= 640`.
- Downscale factor **0.75**, dimensions forced even, with a **640 px floor** (aspect ratio preserved).
- Encode: `libx264`, `-preset fast`, `-crf 23`, output frame rate **30** (`-r 30`). If `EnableMotionInterpolation` were enabled it would instead use `minterpolate` (off by default).
- The compressed result replaces the original (`compressing_<name>` → original name).

### 5.3 Enhance Frame Rate (`VideoTabControl`)
- Reads **Min FPS** and **Target FPS** (Target must be > Min).
- **Effective frame rate** = `uniqueFrames / duration`. Unique frames are counted with an ffmpeg `mpdecimate` pass. This catches videos whose container fps is high but whose real motion is low because frames are duplicated (e.g. GIF-derived MP4s, 30 fps container / ~10 fps unique).
- A file is enhanced when `effectiveFps < Min FPS`.
- Enhancement filter chain: `mpdecimate,minterpolate=fps={target}:mi_mode=mci:mc_mode=aobmc:me_mode=bidir:vsbmc=1` — drop duplicates, then motion-interpolate up to the target. Runs in parallel across files; output replaces the original.

### 5.4 Fake-video detection
Both the downloader and the Viewer probe a video stream; if the codec is `mjpeg` or the format name contains `jpeg`, the file is treated as an image (downloader re-downloads the optimized URL; the Viewer shows the thumbnail instead of playing).

### 5.5 Viewer playback
- Thumbnails: images via `Image.FromFile(...).GetThumbnailImage`, videos via `FFMpegConverter.GetVideoThumbnail` (first frame).
- Clicking a video tile starts **LibVLC** inline playback (hardware decoding, looped); double-click opens the file in the OS default player.
- `Ctrl` + mouse wheel zooms the tile grid (debounced, 0.3×–3×).

---

## 6. Concurrency & control flow

- `AppMediator.Stopping` is the cooperative cancel flag, shared by Download and Video tabs.
- `LoopHelper.Loop` / `LoopAsync` provide sequential (default) or parallel iteration and honour the stop flag by throwing `OperationCanceledException` (parallel) or breaking (sequential).
- All UI updates from worker threads go through `Control.Invoke`; the Video/Download tabs' message list boxes auto-tail by setting `TopIndex` to the last item.

---

## 7. Key constants

| Constant | Value |
|----------|-------|
| Download limit | 500 (UI-configurable) |
| Download inter-request delay | 150 ms |
| HTTP timeout | 15 s |
| Info fetch max retries | 100 |
| JPEG re-encode quality | 80 |
| MP4 (WebP/GIF) encode | libx264 veryfast / crf 20 / yuv420p / faststart |
| Video compress encode | libx264 fast / crf 23 / min 640 px / scale 0.75 |
| Video compress skip size | 3 MB |
| Default Min / Target FPS | 24 / 30 |
| Viewer media extensions | `.jpg .jpeg .png .gif .bmp .webp .mp4 .webm .mov .avi` |
| Rating folders | `!3 !4 !4.5 !5 !6` |
