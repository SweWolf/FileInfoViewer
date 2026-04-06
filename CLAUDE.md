# FileInfoViewer

## Project Overview
A .NET 9 WinForms application that accepts a file path (as a command-line argument or via the GUI), collects as much metadata as possible about the file, generates a self-contained HTML report, and opens it in the default browser.

**Location:** `C:\Develop\FileInfoViewer\` — not in Git.

## Build & Run

```bash
dotnet build
dotnet run                          # Opens the WinForms GUI
dotnet run -- "C:\path\to\file.mp3" # Processes file directly, opens HTML in browser, no window
```

## Project Structure

```
FileInfoViewer.csproj       (.NET 9, WinForms, net9.0-windows)
Program.cs                  Entry point — args[0] triggers headless mode, no args opens GUI
MainForm.cs / .Designer.cs  Main window: file path input, Browse, View File Info, Settings buttons
SettingsForm.cs / .Designer.cs  Settings dialog (opened from MainForm)
Models/
  AppSettings.cs            User preferences (ShowSeconds, TimeZoneDisplay)
  FileInfoModel.cs          All data models: FileInfoModel, AudioInfoModel, ImageInfoModel, etc.
Services/
  FileInfoCollector.cs      Collects all metadata for a given file path
  HtmlReportGenerator.cs    Builds the self-contained HTML report from a FileInfoModel
  BrowserLauncher.cs        Opens a file path in the default browser via Process.Start
  SettingsService.cs        Loads/saves AppSettings as JSON
```

## NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `MetadataExtractor` | 2.9.0 | EXIF/metadata from images; supplementary raw tags for audio |
| `TagLibSharp` | 2.3.0 | Primary audio tag reader (ID3v2/v1, FLAC, OGG, M4A, etc.) — lyrics, cover art, WOAS |
| `System.Reflection.Metadata` | 10.0.5 | Reads .NET PE assembly metadata without loading the assembly |

## Settings File

Stored at `%AppData%\FileInfoViewer\settings.json`:
```json
{
  "ShowSeconds": false,
  "TimeZoneDisplay": "Local"
}
```
- `ShowSeconds`: controls whether `:ss` is shown in all timestamps in the HTML report
- `TimeZoneDisplay`: `"Local"`, `"UTC"`, or `"Both"` — controls which timestamp rows are shown

## File Type Support

| Type | What is collected |
|---|---|
| **Images** (jpg, png, gif, bmp, tiff, webp, ico, heic) | Dimensions, DPI, pixel format, bit depth, full EXIF via MetadataExtractor |
| **Audio** (mp3, flac, ogg, m4a, aac, wav, wma, opus, ape, aiff) | ID3/Vorbis tags, lyrics (USLT + TXXX fallback), cover art (embedded as base64), WWWAUDIOSOURCE link, duration/bitrate/sample rate via TagLibSharp |
| **Executables/DLLs** (.exe, .dll) | FileVersionInfo (product, company, copyright, etc.) + .NET assembly info (target framework, architecture, referenced assemblies) via PEReader |
| **Text files** (.txt, .log, .cs, .json, .xml, etc.) | Line/word/char count, encoding detection, BOM presence |
| **All files** | Size, timestamps (local/UTC), MD5 + SHA-256, MIME type, file attributes, owner |

## Key Design Notes

- **HTML report** is a single self-contained file written to `%TEMP%\FileInfoViewer_<random>.html`. All images (cover art) are embedded as base64 data URLs — no external dependencies.
- **`TagLib.File`** conflicts with `System.IO.File` — do not add `using TagLib;`. Always use the fully qualified `TagLib.File.Create(path)`.
- **WinForms Designer compatibility**: `MainForm.Designer.cs` must use classic C# syntax (no collection expressions `[a, b]`, no lambdas in `InitializeComponent`). Use named event handler methods and `new Control[] { }`.
- **Hashes are skipped for files > 500 MB** to avoid blocking the UI.
- **Audio cover art**: prefers `PictureType.FrontCover`, falls back to first picture.
- **Lyrics**: TagLib# `tag.Lyrics` → ID3v2 `USLT` frames → TXXX frames with "lyric" in description (in that priority order).
