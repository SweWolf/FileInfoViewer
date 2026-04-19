# 📁 File Info Viewer

A .NET 9 WinForms application that collects as much metadata as possible about any file and presents it as a clean, self-contained HTML report opened in your default browser.

**[https://github.com/SweWolf/FileInfoViewer](https://github.com/SweWolf/FileInfoViewer)**

---

## Features

- **Rich HTML report** — single self-contained file, no internet connection required to view
- **Drag and drop** — drop any file onto the window to inspect it instantly
- **Command-line / Send To support** — pass a file path as an argument for headless operation
- **Wide file type support** — images, audio, executables, text files, and more (see below)
- **Configurable** — control timestamp format, timezone display, copy buttons, and which sections to show

---

## Supported File Types

| Type | What is collected |
|---|---|
| **Images** (jpg, png, gif, bmp, tiff, webp, ico, heic) | Dimensions, DPI, pixel format, bit depth, full EXIF metadata |
| **Audio** (mp3, flac, ogg, m4a, aac, wav, wma, opus, ape, aiff) | ID3/Vorbis tags, lyrics, embedded cover art, duration, bitrate, sample rate |
| **Executables / DLLs** (.exe, .dll) | File version info, .NET assembly info, target framework, architecture, referenced assemblies |
| **Text files** (.txt, .log, .cs, .json, .xml, etc.) | Line, word and character count, encoding detection, BOM presence |
| **All files** | Size, timestamps, MD5 + SHA-256 hashes, MIME type, file attributes, owner |

---

## Requirements

- Windows 10 (1607 or later) or Windows 11
- [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) — for the framework-dependent release only

---

## Installation

Go to the [Releases](https://github.com/SweWolf/FileInfoViewer/releases) page and download the version that suits you:

| Release | Description |
|---|---|
| **FrameworkDependent** (multifile) | Small download — requires .NET 9 Desktop Runtime to be installed |
| **Standalone** (single exe) | Larger download — no .NET installation required |

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

| Setting | Description |
|---|---|
| Time zone | Show timestamps in Local time, UTC, or Both |
| Show seconds | Include seconds in all timestamps |
| Copy button | Show a clipboard copy button next to selected fields (No / Yes / Yes on hover) |
| Show owner | Include the file owner row in the report |
| Show file attributes | Include the file attributes section in the report |
| Show file hashes | Include MD5 and SHA-256 hashes in the report |

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
# Framework-dependent (small, requires .NET 9)
dotnet publish -p:PublishProfile=FrameworkDependent

# Standalone (single exe, no .NET required)
dotnet publish -p:PublishProfile=Standalone
```

---

## Built With

- [.NET 9 / Windows Forms](https://dotnet.microsoft.com/)
- [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) 2.9.0
- [TagLibSharp](https://github.com/mono/taglib-sharp) 2.3.0
- [System.Reflection.Metadata](https://www.nuget.org/packages/System.Reflection.Metadata) 10.0.5

---

*SweWolf Software*
