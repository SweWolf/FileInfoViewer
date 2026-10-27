# File Info Viewer Help

File Info Viewer collects as much information as it can about any file and shows it as a report
in your web browser. You can look at pictures, sound, video, documents, programs and more, and
find out when and where a file was made, what program made it, whether an AI made it, and where
it was downloaded from.

## Contents

- [Getting Started](#getting-started)
- [The Main Window](#the-main-window)
- [The Report](#the-report)
  - [Basic Information and Timestamps](#basic-information-and-timestamps)
  - [Warning](#warning)
  - [Mark of the Web](#mark-of-the-web)
  - [File Hashes](#file-hashes)
  - [Images](#images)
  - [Is This AI-Generated?](#is-this-ai-generated)
  - [Audio](#audio)
  - [Video](#video)
  - [Other File Types](#other-file-types)
  - [Copy Buttons and Web Links](#copy-buttons-and-web-links)
- [Settings](#settings)
- [Create Shortcut](#create-shortcut)
- [Prompt and Seed](#prompt-and-seed)
- [Command Line](#command-line)
- [Keyboard Shortcuts](#keyboard-shortcuts)
- [Troubleshooting](#troubleshooting)

## Getting Started

### What You Need

- Windows 10 (version 1607 or later) or Windows 11.
- The [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), but
  only for the small download. The standalone download (a single EXE) needs nothing else.

Nothing is installed and nothing is written to the Windows registry. You can keep the program
anywhere, for example on a USB drive.

### Your First Report

1. Drag a file from File Explorer onto the window, **or** click the **...** button next to
   **File Path** and choose a file.
2. The report opens in your default web browser.

You can also type or paste a path into the **File Path** box and click **View File Info**
(or press Enter).

## The Main Window

- **File Path** is the file to look at. Click **...** to browse for it, or drag a file onto the
  window. Dropping or browsing opens the report at once. You can also type or paste a path. A path
  copied with **Copy as path** in File Explorer can be pasted as it is: the quotes around it are
  removed. If you copy a file in File Explorer and press **Ctrl+V** in the window, that file is
  used, just as if you had dropped it.
- **View File Info** opens the report for the path in the box.

The menus:

- **Tools > Create Shortcut...** puts a shortcut to the program on the desktop, in the Start
  menu or in the Send To menu. See [Create Shortcut](#create-shortcut).
- **Tools > Settings** opens the [settings](#settings).
- **Help > File Info Viewer Help** opens this help (F1).
- **Help > About** shows the version, the web address of the program and whether a newer version
  is available on GitHub. This needs an internet connection; without one, nothing is shown.
- **New Version Available** appears in green at the right end of the menu bar when a newer
  version has been found on GitHub. Click it to open the download page. It is only checked when
  **Check for Updates on Startup** is on in the settings, and needs an internet connection.

A message under the box tells you if something went wrong, for example if the file does not
exist.

## The Report

The report is one web page with several cards. It is saved as a temporary file and opened in
your default browser. It needs no internet connection to be read, and you can save it from the
browser if you want to keep it. A card appears only when the file has something to show for it.

### Basic Information and Timestamps

File name, extension, folder, size (in readable units and in bytes), MIME type and, if you choose
to show it, the owner. The timestamps are the dates the file was created, modified and last
accessed. You choose in the [settings](#settings) whether they are shown in your local time, in
UTC or both, and whether seconds are included.

### Warning

If the content of a file does not match its extension (for example a file named `holiday.jpg`
that is really a PNG picture, or an MP4 video saved as `.mkv`), a **Warning** card says so near
the top and again under **Warnings** at the bottom. This is useful for files that were renamed,
and it is a classic sign of a file that is not what it claims to be. You can turn it off in the
settings.

### Mark of the Web

When a browser downloads a file, Windows can attach a small hidden note to it: the **Mark of the
Web**. The **Mark of the Web** card shows what it says:

- **Zone**: where the file came from, for example 3 - Internet.
- **Downloaded From**: the web address of the file, if the program that saved it wrote one.
- **Referrer Page**: the web page that linked to the download.

Some download links are temporary and signed. They have a long part after a `?` with an expiry
time, a signature and sometimes your own IP address. Such a link would not work later, so the
card shows only the address before the `?`, with a note, and does not make it clickable. Point at
the address to see the whole link, or use the copy button to copy it.

The note is part of the file on an NTFS drive. It is lost when the file is copied to a drive that
is not NTFS, when a cloud service removes it, or when you click **Unblock** in the file's
properties. Many files simply do not have one. You can turn the card off in the settings.

### File Hashes

The **File Hashes** card shows the MD5 and SHA-256 checksums of the file. Use them to check that a
download is complete and not changed, or to compare two files.

Calculating hashes of a very large file takes time. A small window shows the progress and has a
**Cancel** button. If you cancel, the report is made without hashes and a warning says so. In the
settings you can set a size limit: files above it are not hashed at all.

### Images

For pictures the report shows the size in pixels, the resolution (DPI), the color format, the bit
depth and the number of megapixels. Under **EXIF / Metadata** you find everything the picture
stores about itself: the camera and lens, exposure, date, GPS position, the program that edited it
and so on. Very technical tags that few people need (Lightroom settings, edit history, color
profile details, the small embedded preview) are folded away under **Show N more technical
tags**. Click that line to see them.

Special cases:

- **WebP**: the format (lossy, lossless, extended), transparency, animation frames and duration.
- **SVG**: size, view box, title, the program that made it, number of elements, embedded images and
  scripts.
- **Photoshop (PSD, PSB)**: size, color mode, bit depth, layers, compression and color profile.
- **Camera RAW** (NEF, CR2, ARW, DNG and others): the size and all camera and maker-note tags.
- **PNG** pictures can hold text, for example the prompt and the whole workflow of a ComfyUI
  image. Such text is shown nicely formatted, and you can choose in the settings how much you
  want to see.

### Is This AI-Generated?

Some files say so themselves. File Info Viewer shows three kinds of such information, each in its
own card. The information comes from the file itself and is only shown, not checked.

- **Content Credentials (C2PA)** is a digital signature that tools such as Google Gemini,
  Microsoft Designer and OpenAI add to pictures and videos. The card shows:
  - an **AI-generated** badge, when the file says it was made with AI;
  - the **Generator** and who signed the file (**Signed By**), with the certificate and its dates;
  - when it was signed;
  - the **Actions** that were recorded, such as Created, Edited, Resized and Watermarked, often
    with the time and the program that did them;
  - an invisible watermark, if there is one (for example Google's SynthID or Microsoft's
    invisible watermark).

  A file that was edited several times can have several signed steps (**Manifests**). It works for
  JPEG, PNG, MP4 and MOV files.
- **AI Origin** appears for pictures that only have the plain "Digital Source Type" tag in their
  metadata, with no signature.
- **AI Origin (AIGC Label)** appears for MP4 and MOV videos from Chinese AI services (for example
  Vidu). It shows the label, the service's code and the ID of the job.

**Please remember:** a missing label proves nothing. Many programs and web sites remove this
information when a picture is saved again, shared or taken as a screenshot. A label that is there
can also be added or removed by anyone. The report says "Not checked" on these cards, because the
signatures are not verified.

### Audio

Tags (title, artist, album, year, genre), lyrics, cover art, duration, bit rate, sample rate,
channels, the format and the program that encoded the file. All tags are listed under **All
Metadata Tags**.

### Video

The **Video Information** card has the title and other description tags, then:

- **Video**: media created, length, frame size, rotation, frame rate, data rate, total bit rate,
  codec and container.
- **Has chapters**: Yes with the number of chapters, or No. Click **Show N chapters** to see the
  start time and title of each chapter. This works for MKV, WebM, MP4 and MOV files.
- **Audio**: data rate, sample rate and channels.
- **Embedded Tracks**: the video, audio and subtitle tracks inside the file, with language and
  codec. This works for MKV, WebM, MP4 and MOV files.
- **All Metadata Tags** and any cover art.

### Other File Types

- **Programs and libraries** (EXE, DLL): version information, and for .NET programs the target
  framework, the architecture and the assemblies they use.
- **PDF**: pages, version, title, author, creator, producer, dates and encryption.
- **Archives** (ZIP, RAR, 7z, TAR and more): number of files and folders, sizes, encryption and the
  ZIP comment.
- **SQLite databases**: version, page size, tables, views, indexes and the number of rows.
- **Torrents**: name, trackers, files, size, info hash and magnet link.
- **SID music** (Commodore 64): title, author, number of songs, chip model and addresses.
- **Text files**: lines, words, characters, the detected encoding and whether there is a byte order
  mark (BOM).

### Copy Buttons and Web Links

Next to many values there can be a small clipboard button that copies the value. Web addresses in
the report can be clickable links. Both are set in the [settings](#settings).

## Settings

Choose **Tools > Settings**. Changes are saved at once in
`%AppData%\SweWolfSoftware\FileInfoViewer\settings.json`.

### Appearance

- **Style**: **Light**, **Dark** or **System** (follows the Windows setting).

### File Date

- **Time zone**: **Local**, **UTC** or **Both**.
- **Show Seconds**: include seconds in all times.

### Show

- **Copy Button**: **No**, **Yes**, or **Yes on hover over**.
- **Owner**: show the owner of the file.
- **File Attributes**: show attributes such as Read-only and Hidden.
- **Mark of the Web**: show where a downloaded file came from.
- **File Hashes**: show the MD5 and SHA-256 hashes. **Skip Above** sets the size in MB above which
  a file is not hashed (500 by default; 0 means hash every file).
- **Textual Data**: how to show text and JSON found in metadata: **None**, **Formatted**, **Raw
  data** or **Both Formatted and Raw Data**.

### Web Links

- **Clickable**: show web addresses in the report as links.

### Layout

- **Content Width**: how wide the report is: **Narrow (800px)**, **Normal (1100px)**, **Wide
  (1400px)**, **Very wide (1800px)**, **Full width** or **Custom**. With **Custom**, enter a
  number and choose **Pixels** or **Percent**.

### Warnings

- **Wrong File Extension**: warn when the content of a file does not match its extension.

### Updates

- **Check for Updates on Startup**: look for a newer version on GitHub when the program starts.
  When there is one, **New Version Available** appears in the menu bar. It is on by default.

## Create Shortcut

Choose **Tools > Create Shortcut...** to make shortcuts to the program. Tick where you want them:

- **Desktop**
- **Start Menu (Programs)**
- **Send To menu**: the program appears when you right-click a file in File Explorer and choose
  **Send to**. The report for that file opens at once.
- **Send To menu: Prompt and Seed (Template 1)**: see [Prompt and Seed](#prompt-and-seed).

Under **For:**, choose **Current user only** or **All users**. Shortcuts for all users may need
administrator rights. Send To shortcuts are always for the current user only. A shortcut points to
the EXE that is running now, so create it again if you move the program.

## Prompt and Seed

Pictures and videos made with ComfyUI (PNG) or Wan2GP (MP4) store the **Prompt**, the **Negative
Prompt** and the **Seed** that were used. This template opens a small window with just those values
instead of the whole report, each with a button that copies it. A value the file does not have
shows "(not found)".

To use it, tick **Send To menu: Prompt and Seed (Template 1)** in **Create Shortcut**. Then
right-click a picture or video in File Explorer and choose **Send to**, then **File Info Viewer -
Prompt and Seed**. You can also start it from the command line, see below.

## Command Line

Give a file as the first argument to open its report without showing the main window:

```
FileInfoViewer.exe "C:\Pictures\photo.jpg"
```

To show the Prompt and Seed window instead of the report:

```
FileInfoViewer.exe -T1 "C:\Pictures\image.png"
```

## Keyboard Shortcuts

| Key | What it does |
|---|---|
| F1 | Open this help |
| F6 | Go to the **File Path** box and select its text |
| Ctrl+V | Use a file copied in File Explorer (text is pasted as usual) |
| Alt+B | Click **...** (browse for a file) |
| Ctrl+E | Click **View File Info** |
| Enter (in the **File Path** box) | View the file info |
| Esc | Close the program |

## Troubleshooting

**The report shows very little for my file.** Not every file type holds much information. Files with
no metadata at all, such as many pictures from AI services, only have the basic information, the
timestamps and the hashes.

**There is no "AI-generated" label on a picture I know was made by AI.** The service may not add
one, or it was removed when the file was saved again, shared or screenshotted. See [Is This
AI-Generated?](#is-this-ai-generated).

**There is no Mark of the Web card.** The file has no such note. Files you created yourself, or
copied from a drive that is not NTFS, do not have one, and it can be removed by cloud services or
with **Unblock** in the file's properties.

**The hashes are missing.** The file is larger than the **Skip Above** size in the settings, you
canceled the calculation, or **File Hashes** is turned off.

**The report does not open.** It is opened in your default web browser. Check that Windows has one
set.

**Chapters or tracks are not shown for a video.** They are read for MKV, WebM, MP4 and MOV files.
Other video formats show the basic information only.

**No "New Version Available" or no information about new versions in About.** Both need an internet connection and
access to GitHub. Without them, nothing is shown. The check at startup can be turned off in the settings.

You can find the program, the source code and the latest version at
[github.com/SweWolf/FileInfoViewer](https://github.com/SweWolf/FileInfoViewer).
