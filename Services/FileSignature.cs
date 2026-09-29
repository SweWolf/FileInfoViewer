namespace FileInfoViewer.Services;

/// <summary>
/// Identifies common file formats from their first bytes ("magic numbers"), so that files with a
/// wrong extension (e.g. an MP4 renamed to .mkv) can be read correctly and reported.
/// </summary>
internal static class FileSignature
{
    internal sealed record Format(string DisplayName, string[] Extensions);

    // Extensions[0] is the one suggested when renaming. Formats without a Detect() rule (MP3) are
    // listed only so that e.g. an MP4 named .mp3 is recognised as having a wrong extension.
    internal static readonly Format Matroska = new("Matroska (MKV/WebM)", [".mkv", ".webm", ".mka", ".mks", ".mk3d"]);
    internal static readonly Format Mp4      = new("MP4/QuickTime",       [".mp4", ".m4v", ".m4a", ".m4b", ".mov", ".3gp", ".3g2", ".f4v"]);
    internal static readonly Format Heif     = new("HEIF/AVIF",           [".heic", ".heif", ".avif"]);
    internal static readonly Format Avi      = new("AVI",                 [".avi"]);
    internal static readonly Format Wav      = new("WAV",                 [".wav"]);
    internal static readonly Format WebP     = new("WebP",                [".webp"]);
    internal static readonly Format Asf      = new("Windows Media (ASF)", [".wmv", ".wma", ".asf"]);
    internal static readonly Format Flv      = new("Flash Video (FLV)",   [".flv"]);
    internal static readonly Format Flac     = new("FLAC",                [".flac"]);
    internal static readonly Format Ogg      = new("Ogg",                 [".ogg", ".oga", ".ogv", ".opus"]);
    internal static readonly Format Mp3      = new("MP3",                 [".mp3"]);
    internal static readonly Format Jpeg     = new("JPEG",                [".jpg", ".jpeg", ".jpe", ".jfif"]);
    internal static readonly Format Png      = new("PNG",                 [".png"]);
    internal static readonly Format Gif      = new("GIF",                 [".gif"]);
    internal static readonly Format Bmp      = new("BMP",                 [".bmp"]);
    internal static readonly Format Tiff     = new("TIFF",                [".tif", ".tiff"]);
    internal static readonly Format Pdf      = new("PDF",                 [".pdf"]);
    internal static readonly Format Zip      = new("ZIP",                 [".zip", ".docx", ".xlsx", ".pptx", ".odt", ".ods", ".odp",
                                                                           ".epub", ".jar", ".apk", ".nupkg", ".vsix", ".xpi", ".cbz"]);
    internal static readonly Format Rar      = new("RAR",                 [".rar", ".cbr"]);
    internal static readonly Format SevenZip = new("7-Zip",               [".7z"]);
    internal static readonly Format Gzip     = new("gzip",                [".gz", ".tgz"]);
    internal static readonly Format Pe       = new("Windows EXE/DLL",     [".exe", ".dll"]);
    internal static readonly Format Sqlite   = new("SQLite",              [".sqlite", ".sqlite3", ".db3", ".s3db"]);

    private static readonly Format[] All =
        [Matroska, Mp4, Heif, Avi, Wav, WebP, Asf, Flv, Flac, Ogg, Mp3, Jpeg, Png, Gif, Bmp, Tiff,
         Pdf, Zip, Rar, SevenZip, Gzip, Pe, Sqlite];

    // HEIF/AVIF use the same "ftyp" box as MP4; the major brand tells them apart
    private static readonly HashSet<string> HeifBrands =
        ["heic", "heix", "hevc", "hevx", "heim", "heis", "hevm", "hevs", "mif1", "msf1", "avif", "avis"];

    /// <summary>The format found in the file's first bytes, or null if it isn't recognised.</summary>
    public static Format? Detect(string filePath)
    {
        try
        {
            var b = new byte[16];
            int n;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                n = fs.Read(b, 0, b.Length);
            if (n < 12) return null;

            string Ascii(int start, int len) => System.Text.Encoding.ASCII.GetString(b, start, len);
            bool Starts(params byte[] sig) => b.AsSpan(0, sig.Length).SequenceEqual(sig);

            if (Starts(0x1A, 0x45, 0xDF, 0xA3)) return Matroska;
            var box = Ascii(4, 4);
            if (box == "ftyp") return HeifBrands.Contains(Ascii(8, 4)) ? Heif : Mp4;
            if (box is "moov" or "mdat" or "free" or "wide" or "skip") return Mp4;
            if (Ascii(0, 4) == "RIFF")
                return Ascii(8, 4) switch { "AVI " => Avi, "WAVE" => Wav, "WEBP" => WebP, _ => null };
            if (Starts(0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11)) return Asf;
            if (Ascii(0, 3) == "FLV") return Flv;
            if (Ascii(0, 4) == "fLaC") return Flac;
            if (Ascii(0, 4) == "OggS") return Ogg;
            if (Starts(0xFF, 0xD8, 0xFF)) return Jpeg;
            if (Starts(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A)) return Png;
            if (Ascii(0, 6) is "GIF87a" or "GIF89a") return Gif;
            if (Ascii(0, 2) == "BM" && b[6] == 0 && b[7] == 0 && b[8] == 0 && b[9] == 0) return Bmp;
            if (Starts(0x49, 0x49, 0x2A, 0x00) || Starts(0x4D, 0x4D, 0x00, 0x2A)) return Tiff;
            if (Ascii(0, 5) == "%PDF-") return Pdf;
            if (Starts(0x50, 0x4B, 0x03, 0x04) || Starts(0x50, 0x4B, 0x05, 0x06)) return Zip;
            if (Ascii(0, 7) == "Rar!\u001A\u0007\0" || Ascii(0, 7) == "Rar!\u001A\u0007\u0001") return Rar;
            if (Starts(0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C)) return SevenZip;
            if (Starts(0x1F, 0x8B)) return Gzip;
            if (Ascii(0, 2) == "MZ") return Pe;
            if (Ascii(0, 16) == "SQLite format 3\0") return Sqlite;
        }
        catch { }
        return null;
    }

    /// <summary>
    /// The format the extension normally belongs to, or null for extensions not listed here
    /// (those never give a wrong-extension warning).
    /// </summary>
    public static Format? FromExtension(string extension)
        => All.FirstOrDefault(f => f.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// A warning text if the content is a known format that doesn't match a known extension,
    /// e.g. an MP4 file named .mkv; otherwise null.
    /// </summary>
    public static string? WrongExtensionWarning(Format? content, string extension)
    {
        var expected = FromExtension(extension);
        if (content == null || expected == null || content == expected) return null;
        return $"The file extension is {extension.ToLowerInvariant()}, but the file is actually in {content.DisplayName} format. " +
               $"Some programs may not open it correctly; consider renaming it to {content.Extensions[0]}.";
    }
}
