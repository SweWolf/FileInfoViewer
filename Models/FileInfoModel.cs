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

    // Video file info
    public VideoInfoModel? VideoInfo { get; set; }

    // Archive file info
    public ArchiveInfoModel? ArchiveInfo { get; set; }

    // Torrent file info
    public TorrentInfoModel? TorrentInfo { get; set; }

    // PDF file info
    public PdfInfoModel? PdfInfo { get; set; }

    // SQLite database info
    public SqliteInfoModel? SqliteInfo { get; set; }

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
    public string Comments { get; set; } = "";
    public string LegalTrademarks { get; set; } = "";
    public string PrivateBuild { get; set; } = "";
    public string SpecialBuild { get; set; } = "";
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
    public Dictionary<string, string> PngTextChunks { get; set; } = [];
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

public class VideoInfoModel
{
    // Description (Windows Explorer "Description" group)
    public string Title    { get; set; } = "";
    public string Subject  { get; set; } = "";  // "Subtitle" in Explorer
    public string Comment  { get; set; } = "";
    public string Tags     { get; set; } = "";
    public string Rating   { get; set; } = "";

    // Extra tags (from TagLib# / MP4 atoms)
    public string Creator   { get; set; } = "";
    public string Year      { get; set; } = "";
    public string Genre     { get; set; } = "";
    public string Copyright { get; set; } = "";

    // Technical (Windows Explorer "Video" group)
    public string Duration     { get; set; } = "";
    public int    Width        { get; set; }
    public int    Height       { get; set; }
    public string FrameRate    { get; set; } = "";
    public string DataRate     { get; set; } = "";  // video stream only
    public string TotalBitrate { get; set; } = "";

    // Technical (Windows Explorer "Audio" group)
    public string AudioBitrate    { get; set; } = "";
    public string AudioSampleRate { get; set; } = "";
    public string AudioChannels   { get; set; } = "";

    // Codec string from TagLib# properties
    public string VideoCodec { get; set; } = "";

    // Lyrics (from ID3v2 USLT or similar)
    public string Lyrics { get; set; } = "";

    // Cover art
    public bool HasCoverArt { get; set; }
    public string? CoverArtBase64 { get; set; }
    public string CoverArtMimeType { get; set; } = "";
    public string CoverArtPictureType { get; set; } = "";
    public string CoverArtDescription { get; set; } = "";

    // Embedded stream tracks (from Matroska/EBML parser)
    public List<MediaTrackInfo> Tracks { get; set; } = [];

    // All raw metadata tags
    public Dictionary<string, string> AllTags { get; set; } = [];
}

public class MediaTrackInfo
{
    public int    Number      { get; set; }
    public string Type        { get; set; } = "";   // "Video", "Audio", "Subtitle"
    public string Name        { get; set; } = "";
    public string Language    { get; set; } = "";
    public string CodecId     { get; set; } = "";
    public string CodecName   { get; set; } = "";
    public bool   IsDefault   { get; set; } = true;
    public bool   IsForced    { get; set; }
    // Audio
    public double SampleRate  { get; set; }
    public int    Channels    { get; set; }
    public int    BitDepth    { get; set; }
    // Video
    public int    TrackWidth  { get; set; }
    public int    TrackHeight { get; set; }
}

public class SqliteInfoModel
{
    public string SqliteVersion { get; set; } = "";
    public long PageSizeBytes { get; set; }
    public long PageCount { get; set; }
    public long FreePageCount { get; set; }
    public string TextEncoding { get; set; } = "";
    public string JournalMode { get; set; } = "";
    public int UserVersion { get; set; }
    public int ApplicationId { get; set; }
    public int TableCount { get; set; }
    public int ViewCount { get; set; }
    public int IndexCount { get; set; }
    public List<SqliteTableInfo> Tables { get; set; } = [];
}

public class SqliteTableInfo
{
    public string Name { get; set; } = "";
    public long RowCount { get; set; }
    public bool RowCountFailed { get; set; }
}

public class PdfInfoModel
{
    public int PageCount { get; set; }
    public string PdfVersion { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Keywords { get; set; } = "";
    public string Creator { get; set; } = "";
    public string Producer { get; set; } = "";
    public DateTime? CreationDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsEncrypted { get; set; }
}

public class TorrentInfoModel
{
    public bool IsMagnetLink { get; set; }
    public string Name { get; set; } = "";
    public string Comment { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public DateTime? CreationDate { get; set; }
    public string InfoHash { get; set; } = "";
    public string MagnetLink { get; set; } = "";
    public long TotalSizeBytes { get; set; }
    public int FileCount { get; set; }
    public long PieceSizeBytes { get; set; }
    public bool IsPrivate { get; set; }
    public string Source { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string PublisherUrl { get; set; } = "";
    public string PrimaryTracker { get; set; } = "";
    public List<string> Trackers { get; set; } = [];
    public List<TorrentFileEntry> Files { get; set; } = [];
}

public class TorrentFileEntry
{
    public string Path { get; set; } = "";
    public long SizeBytes { get; set; }
}

public class ArchiveInfoModel
{
    public string Format { get; set; } = "";
    public int FileCount { get; set; }
    public int FolderCount { get; set; }
    public long TotalUncompressedBytes { get; set; }
    public long TotalCompressedBytes { get; set; }
    public bool IsEncrypted { get; set; }
    public string Comment { get; set; } = "";
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
