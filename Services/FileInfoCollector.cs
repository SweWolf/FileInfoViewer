using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using FileInfoViewer.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace FileInfoViewer.Services;

public static class FileInfoCollector
{
    private static readonly HashSet<string> ImageExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".ico", ".heic", ".heif"];

    private static readonly HashSet<string> TextExtensions =
        [".txt", ".log", ".csv", ".xml", ".json", ".md", ".htm", ".html", ".css", ".js",
         ".ts", ".cs", ".vb", ".py", ".rb", ".sh", ".bat", ".cmd", ".ps1", ".yaml", ".yml",
         ".ini", ".cfg", ".conf", ".toml", ".sql", ".ts", ".jsx", ".tsx", ".rs", ".go",
         ".java", ".cpp", ".c", ".h", ".hpp", ".php", ".swift", ".kt", ".dart", ".r"];

    private static readonly HashSet<string> AssemblyExtensions = [".exe", ".dll"];

    private static readonly HashSet<string> AudioExtensions =
        [".mp3", ".flac", ".ogg", ".m4a", ".aac", ".wav", ".wma", ".opus", ".ape", ".aiff", ".aif"];

    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg", "image/jpeg" }, { ".jpeg", "image/jpeg" }, { ".png", "image/png" },
        { ".gif", "image/gif" }, { ".bmp", "image/bmp" }, { ".tiff", "image/tiff" },
        { ".tif", "image/tiff" }, { ".webp", "image/webp" }, { ".ico", "image/x-icon" },
        { ".heic", "image/heic" }, { ".heif", "image/heif" },
        { ".pdf", "application/pdf" }, { ".zip", "application/zip" },
        { ".7z", "application/x-7z-compressed" }, { ".rar", "application/vnd.rar" },
        { ".tar", "application/x-tar" }, { ".gz", "application/gzip" },
        { ".exe", "application/vnd.microsoft.portable-executable" },
        { ".dll", "application/vnd.microsoft.portable-executable" },
        { ".msi", "application/x-msdownload" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".doc", "application/msword" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".xls", "application/vnd.ms-excel" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".txt", "text/plain" }, { ".csv", "text/csv" }, { ".html", "text/html" },
        { ".htm", "text/html" }, { ".xml", "text/xml" }, { ".json", "application/json" },
        { ".css", "text/css" }, { ".js", "text/javascript" }, { ".md", "text/markdown" },
        { ".mp3", "audio/mpeg" }, { ".mp4", "video/mp4" }, { ".avi", "video/x-msvideo" },
        { ".mkv", "video/x-matroska" }, { ".mov", "video/quicktime" },
        { ".wav", "audio/wav" }, { ".flac", "audio/flac" }, { ".ogg", "audio/ogg" },
        { ".svg", "image/svg+xml" }, { ".ttf", "font/ttf" }, { ".otf", "font/otf" },
        { ".woff", "font/woff" }, { ".woff2", "font/woff2" },
    };

    public static FileInfoModel Collect(string filePath)
    {
        var model = new FileInfoModel();

        var fi = new FileInfo(filePath);
        if (!fi.Exists)
            throw new FileNotFoundException("File not found.", filePath);

        // Basic info
        model.FileName = fi.Name;
        model.FullPath = fi.FullName;
        model.Extension = fi.Extension.ToLowerInvariant();
        model.DirectoryPath = fi.DirectoryName ?? "";
        model.SizeBytes = fi.Length;
        model.SizeFormatted = FormatSize(fi.Length);
        model.FileAttributes = FormatAttributes(fi.Attributes);
        model.MimeType = MimeTypes.TryGetValue(model.Extension, out var mime) ? mime : "application/octet-stream";

        // Timestamps
        model.CreatedUtc = fi.CreationTimeUtc;
        model.ModifiedUtc = fi.LastWriteTimeUtc;
        model.AccessedUtc = fi.LastAccessTimeUtc;

        // Owner
        try
        {
            var acl = fi.GetAccessControl();
            var owner = acl.GetOwner(typeof(NTAccount));
            model.Owner = owner?.ToString() ?? "Unknown";
        }
        catch (Exception ex)
        {
            model.Owner = "Unknown";
            model.Warnings.Add($"Could not read file owner: {ex.Message}");
        }

        // Hashes (skip for large files > 500 MB to avoid long waits)
        if (fi.Length <= 500 * 1024 * 1024)
        {
            try
            {
                using var stream = fi.OpenRead();
                model.Md5 = Convert.ToHexString(MD5.HashData(stream)).ToLowerInvariant();
                stream.Position = 0;
                model.Sha256 = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
            }
            catch (Exception ex)
            {
                model.Warnings.Add($"Could not compute hashes: {ex.Message}");
            }
        }
        else
        {
            model.Warnings.Add("File is larger than 500 MB — hashes skipped.");
        }

        // Version info
        if (AssemblyExtensions.Contains(model.Extension))
        {
            try
            {
                var fvi = FileVersionInfo.GetVersionInfo(filePath);
                model.VersionInfo = new VersionInfoModel
                {
                    ProductName = fvi.ProductName ?? "",
                    FileVersion = fvi.FileVersion ?? "",
                    ProductVersion = fvi.ProductVersion ?? "",
                    CompanyName = fvi.CompanyName ?? "",
                    FileDescription = fvi.FileDescription ?? "",
                    Copyright = fvi.LegalCopyright ?? "",
                    OriginalFilename = fvi.OriginalFilename ?? "",
                    InternalName = fvi.InternalName ?? "",
                    IsDebug = fvi.IsDebug,
                    IsPatched = fvi.IsPatched,
                    IsPreRelease = fvi.IsPreRelease,
                    Language = fvi.Language ?? "",
                };
            }
            catch (Exception ex)
            {
                model.Warnings.Add($"Could not read version info: {ex.Message}");
            }

            // .NET assembly info
            try
            {
                CollectAssemblyInfo(filePath, model);
            }
            catch (Exception ex)
            {
                model.Warnings.Add($"Could not read assembly info: {ex.Message}");
            }
        }

        // Image info
        if (ImageExtensions.Contains(model.Extension))
        {
            try
            {
                CollectImageInfo(filePath, model);
            }
            catch (Exception ex)
            {
                model.Warnings.Add($"Could not read image info: {ex.Message}");
            }
        }

        // Text file info
        if (TextExtensions.Contains(model.Extension))
        {
            try
            {
                CollectTextInfo(filePath, model);
            }
            catch (Exception ex)
            {
                model.Warnings.Add($"Could not read text info: {ex.Message}");
            }
        }

        // Audio file info
        if (AudioExtensions.Contains(model.Extension))
        {
            try
            {
                CollectAudioInfo(filePath, model);
            }
            catch (Exception ex)
            {
                model.Warnings.Add($"Could not read audio info: {ex.Message}");
            }
        }

        return model;
    }

    private static void CollectImageInfo(string filePath, FileInfoModel model)
    {
        using var img = System.Drawing.Image.FromFile(filePath);
        var pixelFormat = img.PixelFormat;
        var bitDepth = System.Drawing.Image.GetPixelFormatSize(pixelFormat);

        var imageInfo = new ImageInfoModel
        {
            Width = img.Width,
            Height = img.Height,
            HorizontalDpi = img.HorizontalResolution,
            VerticalDpi = img.VerticalResolution,
            PixelFormat = pixelFormat.ToString(),
            BitDepth = bitDepth,
        };

        // EXIF via MetadataExtractor
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);
            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                {
                    var key = $"{directory.Name} / {tag.Name}";
                    var value = tag.Description ?? "";
                    if (!string.IsNullOrWhiteSpace(value))
                        imageInfo.ExifTags[key] = value;
                }
            }
        }
        catch
        {
            // EXIF read failure is non-critical
        }

        model.ImageInfo = imageInfo;
    }

    private static void CollectAudioInfo(string filePath, FileInfoModel model)
    {
        var audio = new AudioInfoModel();

        // TagLib# — reliable reading of standard tags, lyrics, and technical properties
        using var tagFile = TagLib.File.Create(filePath);
        var tag = tagFile.Tag;


        audio.Title       = tag.Title ?? "";
        audio.Artist      = tag.Performers?.Length > 0 ? string.Join(", ", tag.Performers) : "";
        audio.AlbumArtist = tag.AlbumArtists?.Length > 0 ? string.Join(", ", tag.AlbumArtists) : "";
        audio.Album       = tag.Album ?? "";
        audio.Year        = tag.Year > 0 ? tag.Year.ToString() : "";
        audio.TrackNumber = tag.Track > 0 ? (tag.TrackCount > 0 ? $"{tag.Track}/{tag.TrackCount}" : tag.Track.ToString()) : "";
        audio.Genre       = tag.Genres?.Length > 0 ? string.Join(", ", tag.Genres) : "";
        audio.Composer    = tag.Composers?.Length > 0 ? string.Join(", ", tag.Composers) : "";
        audio.Comment     = tag.Comment ?? "";
        audio.Lyrics      = tag.Lyrics ?? "";
        audio.HasCoverArt = tag.Pictures?.Length > 0;

        // Extract cover art — prefer FrontCover type, fall back to first picture
        if (audio.HasCoverArt)
        {
            var pictures = tag.Pictures ?? [];
            var picture = pictures.FirstOrDefault(p => p.Type == TagLib.PictureType.FrontCover)
                       ?? pictures[0];

            audio.CoverArtMimeType    = picture.MimeType ?? "image/jpeg";
            audio.CoverArtPictureType = FormatPictureType(picture.Type);
            audio.CoverArtDescription = picture.Description ?? "";
            audio.CoverArtBase64      = Convert.ToBase64String(picture.Data.Data);
        }

        // Explicitly read ID3v2 frames that require direct access.
        var id3v2 = tagFile.GetTag(TagLib.TagTypes.Id3v2) as TagLib.Id3v2.Tag;
        if (id3v2 != null)
        {
            // USLT — Unsynchronised Lyrics (tag.Lyrics can miss these when ID3v1 is also present)
            if (string.IsNullOrEmpty(audio.Lyrics))
            {
                foreach (var frame in id3v2.GetFrames<TagLib.Id3v2.UnsynchronisedLyricsFrame>())
                {
                    if (!string.IsNullOrWhiteSpace(frame.Text))
                    {
                        audio.Lyrics = frame.Text;
                        break;
                    }
                }
            }

            // Custom TXXX frames (e.g. "LYRICS", "UNSYNCED LYRICS", "UNSYNCEDLYRICS")
            if (string.IsNullOrEmpty(audio.Lyrics))
            {
                foreach (var frame in id3v2.GetFrames<TagLib.Id3v2.UserTextInformationFrame>())
                {
                    if (frame.Description.Contains("lyric", StringComparison.OrdinalIgnoreCase)
                        && frame.Text?.Length > 0)
                    {
                        audio.Lyrics = string.Join("\n", frame.Text);
                        break;
                    }
                }
            }

            // WOAS — Official audio source webpage (WWWAUDIOSOURCE)
            var woasFrame = id3v2.GetFrames<TagLib.Id3v2.UrlLinkFrame>()
                .FirstOrDefault(f => f.FrameId.ToString() == "WOAS");
            if (woasFrame != null)
                audio.AudioSourceUrl = woasFrame.Text?.FirstOrDefault() ?? "";
        }

        var props = tagFile.Properties;
        if (props != null)
        {
            audio.Duration   = props.Duration.TotalSeconds > 0
                ? $"{(int)props.Duration.TotalMinutes}:{props.Duration.Seconds:D2} ({props.Duration.TotalSeconds:F1}s)"
                : "";
            audio.BitRate    = props.AudioBitrate > 0 ? $"{props.AudioBitrate} kbps" : "";
            audio.SampleRate = props.AudioSampleRate > 0 ? $"{props.AudioSampleRate} Hz" : "";
            audio.Channels   = props.AudioChannels > 0 ? props.AudioChannels.ToString() : "";
            audio.BitDepth   = props.BitsPerSample > 0 ? $"{props.BitsPerSample} bit" : "";
        }

        // MetadataExtractor — supplementary raw tags (EXIF-style extended metadata)
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);
            foreach (var directory in directories)
            {
                foreach (var rawTag in directory.Tags)
                {
                    var value = rawTag.Description ?? "";
                    if (string.IsNullOrWhiteSpace(value)) continue;

                    var tagName = rawTag.Name.Trim();
                    var fullKey = $"{directory.Name.Trim()} / {tagName}";

                    // Skip binary picture data
                    if (tagName.Contains("picture", StringComparison.OrdinalIgnoreCase) ||
                        tagName.Contains("cover", StringComparison.OrdinalIgnoreCase) ||
                        tagName.Contains("artwork", StringComparison.OrdinalIgnoreCase))
                    {
                        audio.AllTags[fullKey] = "(binary data)";
                        continue;
                    }

                    // Skip lyrics — already captured via TagLib#
                    if (tagName.Contains("lyric", StringComparison.OrdinalIgnoreCase))
                        continue;

                    audio.AllTags[fullKey] = value;
                }
            }
        }
        catch
        {
            // MetadataExtractor supplementary pass is non-critical
        }

        model.AudioInfo = audio;
    }

    private static void CollectTextInfo(string filePath, FileInfoModel model)
    {
        var encoding = DetectEncoding(filePath, out bool hasBom);
        long lines = 0, words = 0, chars = 0;

        using var reader = new StreamReader(filePath, encoding, detectEncodingFromByteOrderMarks: true);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            lines++;
            chars += line.Length + Environment.NewLine.Length;
            words += CountWords(line);
        }

        model.TextInfo = new TextInfoModel
        {
            LineCount = lines,
            WordCount = words,
            CharCount = chars,
            DetectedEncoding = encoding.EncodingName,
            HasBom = hasBom,
        };
    }

    private static void CollectAssemblyInfo(string filePath, FileInfoModel model)
    {
        using var stream = File.OpenRead(filePath);
        using var peReader = new PEReader(stream);

        if (!peReader.HasMetadata)
        {
            model.Warnings.Add("File is not a managed .NET assembly.");
            return;
        }

        var metadataReader = peReader.GetMetadataReader();
        var assemblyDef = metadataReader.GetAssemblyDefinition();
        var version = assemblyDef.Version;
        var assemblyVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

        var headers = peReader.PEHeaders;
        var architecture = headers.CoffHeader.Machine switch
        {
            Machine.I386 => "x86",
            Machine.Amd64 => "x64",
            Machine.Arm => "ARM",
            Machine.Arm64 => "ARM64",
            _ => headers.CoffHeader.Machine.ToString(),
        };

        string targetFramework = "";
        string runtimeVersion = "";

        foreach (var attrHandle in assemblyDef.GetCustomAttributes())
        {
            try
            {
                var attr = metadataReader.GetCustomAttribute(attrHandle);
                var ctorHandle = attr.Constructor;
                if (ctorHandle.Kind == HandleKind.MemberReference)
                {
                    var memberRef = metadataReader.GetMemberReference((MemberReferenceHandle)ctorHandle);
                    if (memberRef.Parent.Kind == HandleKind.TypeReference)
                    {
                        var typeRef = metadataReader.GetTypeReference((TypeReferenceHandle)memberRef.Parent);
                        var typeName = metadataReader.GetString(typeRef.Name);
                        if (typeName == "TargetFrameworkAttribute")
                        {
                            var value = attr.DecodeValue(new SimpleAttributeTypeProvider());
                            if (value.FixedArguments.Length > 0)
                                targetFramework = value.FixedArguments[0].Value?.ToString() ?? "";
                        }
                    }
                }
            }
            catch { }
        }

        // Referenced assemblies (up to 20)
        var refs = new List<string>();
        foreach (var refHandle in metadataReader.AssemblyReferences)
        {
            var asmRef = metadataReader.GetAssemblyReference(refHandle);
            refs.Add(metadataReader.GetString(asmRef.Name));
            if (refs.Count >= 20) break;
        }

        model.AssemblyInfo = new AssemblyInfoModel
        {
            AssemblyVersion = assemblyVersion,
            TargetFramework = targetFramework,
            RuntimeVersion = runtimeVersion,
            IsManaged = true,
            Architecture = architecture,
            ReferencedAssemblies = refs,
        };
    }

    private static Encoding DetectEncoding(string filePath, out bool hasBom)
    {
        hasBom = false;
        byte[] bom = new byte[4];
        using var fs = File.OpenRead(filePath);
        int read = fs.Read(bom, 0, 4);

        if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        { hasBom = true; return Encoding.UTF8; }
        if (read >= 4 && bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xFE && bom[3] == 0xFF)
        { hasBom = true; return Encoding.UTF32; }
        if (read >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
        { hasBom = true; return Encoding.Unicode; }
        if (read >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
        { hasBom = true; return Encoding.BigEndianUnicode; }

        return Encoding.UTF8; // default assumption
    }

    private static long CountWords(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return 0;
        long count = 0;
        bool inWord = false;
        foreach (char c in line)
        {
            if (char.IsWhiteSpace(c)) inWord = false;
            else if (!inWord) { inWord = true; count++; }
        }
        return count;
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }

    private static string FormatPictureType(TagLib.PictureType type)
    {
        // Insert a space before each capital letter after the first, e.g. "FrontCover" → "Front Cover"
        var name = type.ToString();
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
                sb.Append(' ');
            sb.Append(name[i]);
        }
        return sb.ToString();
    }

    private static string FormatAttributes(FileAttributes attrs)
    {
        var parts = new List<string>();
        if (attrs.HasFlag(FileAttributes.ReadOnly)) parts.Add("ReadOnly");
        if (attrs.HasFlag(FileAttributes.Hidden)) parts.Add("Hidden");
        if (attrs.HasFlag(FileAttributes.System)) parts.Add("System");
        if (attrs.HasFlag(FileAttributes.Archive)) parts.Add("Archive");
        if (attrs.HasFlag(FileAttributes.Compressed)) parts.Add("Compressed");
        if (attrs.HasFlag(FileAttributes.Encrypted)) parts.Add("Encrypted");
        if (attrs.HasFlag(FileAttributes.Temporary)) parts.Add("Temporary");
        if (attrs.HasFlag(FileAttributes.Offline)) parts.Add("Offline");
        if (attrs.HasFlag(FileAttributes.NotContentIndexed)) parts.Add("NotContentIndexed");
        if (attrs.HasFlag(FileAttributes.ReparsePoint)) parts.Add("ReparsePoint");
        if (attrs.HasFlag(FileAttributes.SparseFile)) parts.Add("SparseFile");
        return parts.Count > 0 ? string.Join(", ", parts) : "Normal";
    }
}

// Minimal type provider for reading TargetFrameworkAttribute via System.Reflection.Metadata
internal sealed class SimpleAttributeTypeProvider
    : ICustomAttributeTypeProvider<object>
{
    public object GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode;
    public object GetSystemType() => typeof(Type);
    public object GetSZArrayType(object elementType) => Array.CreateInstance(typeof(object), 0).GetType();
    public object GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => typeof(object);
    public object GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => typeof(object);
    public object GetTypeFromSerializedName(string name) => typeof(object);
    public PrimitiveTypeCode GetUnderlyingEnumType(object type) => PrimitiveTypeCode.Int32;
    public bool IsSystemType(object type) => type is Type t && t == typeof(Type);
}
