namespace FileInfoViewer.Models;

public class FileInfoModel
{
    // Basic file system info
    public string FileName { get; set; } = "";
    public string FullPath { get; set; } = "";
    public string Extension { get; set; } = "";
    public string DirectoryPath { get; set; } = "";
    public long SizeBytes { get; set; }
    public string SizeFormatted { get; set; } = "";
    public string FileAttributes { get; set; } = "";
    public string Owner { get; set; } = "";
    public string MimeType { get; set; } = "";

    // Timestamps
    public DateTime CreatedUtc { get; set; }
    public DateTime ModifiedUtc { get; set; }
    public DateTime AccessedUtc { get; set; }

    // Hashes
    public string Md5 { get; set; } = "";
    public string Sha256 { get; set; } = "";

    // Version info (exe/dll)
    public VersionInfoModel? VersionInfo { get; set; }

    // Image info
    public ImageInfoModel? ImageInfo { get; set; }

    // Text file info
    public TextInfoModel? TextInfo { get; set; }

    // .NET assembly info
    public AssemblyInfoModel? AssemblyInfo { get; set; }

    // Audio file info
    public AudioInfoModel? AudioInfo { get; set; }

    // Errors/warnings encountered during collection
    public List<string> Warnings { get; set; } = [];
}

public class VersionInfoModel
{
    public string ProductName { get; set; } = "";
    public string FileVersion { get; set; } = "";
    public string ProductVersion { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string FileDescription { get; set; } = "";
    public string Copyright { get; set; } = "";
    public string OriginalFilename { get; set; } = "";
    public string InternalName { get; set; } = "";
    public bool IsDebug { get; set; }
    public bool IsPatched { get; set; }
    public bool IsPreRelease { get; set; }
    public string Language { get; set; } = "";
}

public class ImageInfoModel
{
    public int Width { get; set; }
    public int Height { get; set; }
    public double HorizontalDpi { get; set; }
    public double VerticalDpi { get; set; }
    public string PixelFormat { get; set; } = "";
    public int BitDepth { get; set; }
    public Dictionary<string, string> ExifTags { get; set; } = [];
}

public class TextInfoModel
{
    public long LineCount { get; set; }
    public long WordCount { get; set; }
    public long CharCount { get; set; }
    public string DetectedEncoding { get; set; } = "";
    public bool HasBom { get; set; }
}

public class AudioInfoModel
{
    // Common tags
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string AlbumArtist { get; set; } = "";
    public string Album { get; set; } = "";
    public string Year { get; set; } = "";
    public string TrackNumber { get; set; } = "";
    public string Genre { get; set; } = "";
    public string Composer { get; set; } = "";
    public string Comment { get; set; } = "";

    // Technical
    public string Duration { get; set; } = "";
    public string BitRate { get; set; } = "";
    public string SampleRate { get; set; } = "";
    public string Channels { get; set; } = "";
    public string BitDepth { get; set; } = "";

    // Special
    public bool HasCoverArt { get; set; }
    public string? CoverArtBase64 { get; set; }
    public string CoverArtMimeType { get; set; } = "";
    public string CoverArtPictureType { get; set; } = "";
    public string CoverArtDescription { get; set; } = "";
    public string Lyrics { get; set; } = "";
    public string AudioSourceUrl { get; set; } = "";

    // All raw metadata tags (for anything not mapped above)
    public Dictionary<string, string> AllTags { get; set; } = [];
}

public class AssemblyInfoModel
{
    public string TargetFramework { get; set; } = "";
    public string AssemblyVersion { get; set; } = "";
    public string RuntimeVersion { get; set; } = "";
    public bool IsManaged { get; set; }
    public string Architecture { get; set; } = "";
    public List<string> ReferencedAssemblies { get; set; } = [];
}
