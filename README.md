# 📁 File Info Viewer

A .NET 10 WinForms application that collects as much metadata as possible about any file and presents it as a clean, self-contained HTML report opened in your default browser.

**[https://github.com/SweWolf/FileInfoViewer](https://github.com/SweWolf/FileInfoViewer)**

---

## Features

- **Rich HTML report** — single self-contained file, no internet connection required to view
- **Drag and drop** — drop any file onto the window to inspect it instantly
- **Command-line / Send To support** — pass a file path as an argument for headless operation
- **Wide file type support** — images, audio, video, executables, text files, and more (see below)
- **Clickable web links** — URLs inside any metadata value are rendered as clickable hyperlinks in the report, including inside syntax-highlighted JSON blocks
- **Copy buttons** — copy any metadata value to the clipboard with one click; optionally shown on hover only
- **Syntax-highlighted JSON** — structured metadata (e.g. embedded workflow data in AI-generated images) is pretty-printed with color coding
- **Configurable** — control timestamp format, timezone, copy buttons, content width, web links, and which sections to show
- **Dark mode** — Light, Dark, or follow the Windows setting (System)
- **Keyboard shortcuts** — F6 = file path box, Alt+B = Browse, Ctrl+E = View File Info
- **Update check** — the About box shows whether a newer version is available on GitHub

---

## Supported File Types

| Type | What is collected |
|---|---|
| **Images** (jpg, png, gif, bmp, tiff, webp, ico, heic, heif, avif) | Dimensions, DPI, pixel format, bit depth, full EXIF and XMP metadata, embedded textual/JSON data. WebP: format (lossy/lossless/extended), alpha, animation frames, loop count and duration |
| **Vector images** (svg) | Width, height, view box, SVG version, title, description, creating program (Illustrator, Inkscape), element count, embedded images, scripts |
| **Audio** (mp3, flac, ogg, m4a, aac, wav, wma, opus, ape, aiff) | ID3/Vorbis tags, lyrics, embedded cover art, duration, bitrate, sample rate, encoding, encoder software |
| **Video** (mp4, mkv, avi, mov, wmv, flv, webm, m4v, mpg, 3gp, ts, m2ts) | Duration, dimensions, frame rate, codec, audio streams, video streams, embedded MP4 tracks, full tag metadata |
| **PDF** (.pdf) | Page count, PDF version, title, author, subject, keywords, creator, producer, creation/modified dates, encryption |
| **Archives** (zip, rar, 7z, tar, tar.gz, tar.bz2, tar.xz) | Format, file and folder count, compressed and uncompressed size, encryption, ZIP comment |
| **SQLite databases** (.db, .sqlite, .sqlite3, .db3, .s3db) | SQLite version, page size and count, encoding, journal mode, tables, views and indexes, row count per table |
| **Torrents** (.torrent) | Name, trackers, file list, total size, piece size, info hash, magnet link, private flag, creator, creation date |
| **Executables / DLLs** (.exe, .dll) | File version info, .NET assembly info, target framework, architecture, referenced assemblies |
| **Text files** (.txt, .log, .cs, .json, .xml, etc.) | Line, word and character count, encoding detection, BOM presence |
| **All files** | Size, timestamps, MD5 + SHA-256 hashes, MIME type, file attributes, owner |

---

## Requirements

- Windows 10 (1607 or later) or Windows 11
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) — for the framework-dependent release only

---

## Installation

Go to the [Releases](https://github.com/SweWolf/FileInfoViewer/releases) page and download the version that suits you:

| Release | Description |
|---|---|
| **FrameworkDependent** (multifile) | Small download — requires .NET 10 Desktop Runtime to be installed |
| **Standalone** (single exe) | Larger download — no .NET installation required |

---

## Which download should I choose?

**Standalone (single .exe file)** — Recommended for most users. One file, no installation required. Just download `FileInfoViewer.exe`, place it anywhere, and run it. The download is larger (~130 MB) because it bundles the .NET runtime inside.

**Framework-dependent (multifile zip)** — Smaller download (~6 MB), but requires the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) to be installed on your PC. A good choice if you already have .NET 10 installed or want to keep the file size small.

## Where to put it

The application is fully portable — there is no installer and nothing is written to the Windows registry. You can place it anywhere you like, including a USB drive or a cloud folder (OneDrive, Dropbox, etc.) to use it across multiple computers.

Suggested locations:

- **Standalone**: place the single `FileInfoViewer.exe` directly in `C:\Program Files\SweWolfSoftware` or any folder of your choice.
- **Multifile**: extract the zip into a dedicated subfolder, e.g. `C:\Program Files\SweWolfSoftware\FileInfoViewer`, to keep all files together.

Once placed, use the built-in **Setup → Create Shortcut** option inside the app to add it to the Windows **Send To** menu for quick access from Explorer.

---

## Usage

**GUI mode** — open the application, type or browse to a file path, then click **View File Info**.

**Drag and drop** — drag any file directly onto the application window.

**Command line / Send To** — pass the file path as an argument:
```
FileInfoViewer.exe "C:\path\to\file.mp3"
```
This makes it ideal for use via the Windows **Send To** menu — use the built-in **Setup → Create Shortcut** menu option inside the app to set it up.

---

## Settings

Settings are stored at `%AppData%\SweWolfSoftware\FileInfoViewer\settings.json` and can be changed via the **⚙ Settings** button:

### File Date
| Setting | Description |
|---|---|
| Time zone | Show timestamps in Local time, UTC, or Both |
| Show seconds | Include seconds in all timestamps |

### Show
| Setting | Description |
|---|---|
| Copy button | Show a clipboard copy button next to metadata values (No / Yes / Yes on hover) |
| Owner | Include the file owner row in the report |
| File attributes | Include the file attributes section in the report |
| File hashes | Include MD5 and SHA-256 hashes in the report |
| Textual data | How to display structured/JSON metadata: None, Formatted, Raw data, or Both |

### Layout
| Setting | Description |
|---|---|
| Content width | Max width of the report content area: Narrow (800px), Normal (1100px), Wide (1400px), Very wide (1800px), Full width, or Custom (enter your own pixel or percentage value) |

### Web Links
| Setting | Description |
|---|---|
| Clickable | Render URLs found in metadata values as clickable hyperlinks (on by default) |

### Appearance
| Setting | Description |
|---|---|
| Style | Light, Dark, or System (follows the Windows setting) |

---

## Building from Source

```bash
git clone https://github.com/SweWolf/FileInfoViewer.git
cd FileInfoViewer
dotnet build
dotnet run
```

To publish:
```bash
# Framework-dependent (small, requires .NET 10)
dotnet publish -p:PublishProfile=FrameworkDependent

# Standalone (single exe, no .NET required)
dotnet publish -p:PublishProfile=Standalone
```

---

## Built With

- [.NET 10 / Windows Forms](https://dotnet.microsoft.com/)
- [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) 2.9.0
- [TagLibSharp](https://github.com/mono/taglib-sharp) 2.3.0
- [PdfPig](https://github.com/UglyToad/PdfPig) 1.7.0
- [SharpCompress](https://github.com/adamhathcock/sharpcompress) 0.50.4
- [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite) 10.0.12

---

*SweWolf Software*
