using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FileInfoViewer.Services;

/// <summary>
/// The "Audio Source": the web address of the page a music or video file was made on or taken from. Self-contained (needs only
/// TagLibSharp), a copy of SwewolfFileExplorer's Helpers/AudioSourceReader.cs (only the namespace differs): change both together.
///
/// Where it comes from, first match wins:
///  1. The ID3v2 frame WOAS ("Official audio source webpage") of the file (an MP3 from Suno has it: https://suno.com/song/&lt;id&gt;).
///  2. A comment that says the file was made with Suno and holds `id=&lt;guid&gt;` (a WAV, M4A or FLAC from Suno:
///     "made with suno; created=2026-11-04T12:46:17Z; id=c0ab21d6-...") gives https://suno.com/song/&lt;id&gt;.
///  3. An MP4 / M4V / MOV / M4A from Suno has no comment, but carries C2PA provenance data in the first bytes of the file: the
///     block `com.suno.provenance` holds `contentId` followed by the song id. It gives https://suno.com/song/&lt;id&gt; too.
/// Only http / https addresses are returned. Nothing is fetched; nothing is written.
///
/// History:
///  2026-11-04  First version (SwewolfFileExplorer): WOAS frame, Suno comment.
///  2026-11-04  M4A: the ©cmt atom is read piece by piece (Tag.Comment returned only the first piece, "made with suno").
///  2026-11-04  MP4 family: the Suno song id is read from the C2PA provenance block (`com.suno.provenance` / `contentId`); video
///              extensions (.mp4 .m4v .mov) added.
/// </summary>
public static class AudioSourceReader
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".m4a", ".m4b", ".flac", ".wma", ".ogg", ".oga", ".opus", ".aac", ".wav", ".aif", ".aiff", ".wv", ".mka", ".ape", ".mpc",
        ".mp4", ".m4v", ".mov"
    };

    /// <summary>The files that can hold C2PA data in a box near their start.</summary>
    private static readonly HashSet<string> Mp4Family = new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".m4v", ".mov", ".m4a", ".m4b" };

    private const string GuidPattern = @"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}";

    private static readonly Regex SunoId = new(@"\bid=(" + GuidPattern + @")\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex ContentId = new(@"contentId[^0-9a-f]{0,6}(" + GuidPattern + ")", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>How much of the start of an MP4 is searched for the provenance block (it sits within the first few hundred bytes).</summary>
    private const int ProvenanceBytes = 256 * 1024;

    /// <summary>The audio source address of a music or video file on the disk, or null. Not on the UI thread (it opens the file).</summary>
    public static string? Read(string path)
    {
        string extension = Path.GetExtension(path);
        if (!Extensions.Contains(extension)) return null;

        string? found = null;
        try { found = FromTags(path); }
        catch { /* an unreadable tag simply has none; the provenance block may still be there */ }
        if (found != null) return found;

        return Mp4Family.Contains(extension) ? FromProvenance(path) : null;
    }

    private static string? FromTags(string path)
    {
        using var file = TagLib.File.Create(path, TagLib.ReadStyle.None);

        // 1. The ID3v2 "audio source" frame.
        if (file.GetTag(TagLib.TagTypes.Id3v2, false) is TagLib.Id3v2.Tag id3)
        {
            foreach (var frame in id3.GetFrames<TagLib.Id3v2.UrlLinkFrame>("WOAS"))
                foreach (var text in frame.Text)
                    if (IsWebAddress(text)) return text.Trim();
        }

        // 2. A Suno comment with the song id.
        var fromComment = FromComment(file.Tag?.Comment);
        if (fromComment != null) return fromComment;

        // An M4A keeps the comment in several pieces ("made with suno", "created=...", "id=...") and Tag.Comment gives only the
        // first one, so the pieces of the ©cmt atom are joined.
        if (file.GetTag(TagLib.TagTypes.Apple, false) is TagLib.Mpeg4.AppleTag apple)
        {
            var pieces = apple.GetText(new TagLib.ByteVector(new byte[] { 0xA9, (byte)'c', (byte)'m', (byte)'t' }));
            return FromComment(string.Join("; ", pieces));
        }
        return null;
    }

    /// <summary>"made with suno; ...; id=&lt;guid&gt;" gives the song's page; any other comment gives null.</summary>
    public static string? FromComment(string? comment)
    {
        if (string.IsNullOrEmpty(comment) || comment.IndexOf("suno", StringComparison.OrdinalIgnoreCase) < 0) return null;
        var m = SunoId.Match(comment);
        return m.Success ? SongAddress(m.Groups[1].Value) : null;
    }

    /// <summary>3. The C2PA block of a Suno MP4: `com.suno.provenance` ... `contentId` + the song id, in the first bytes of the file.</summary>
    private static string? FromProvenance(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var buffer = new byte[(int)Math.Min(stream.Length, ProvenanceBytes)];
            int read = stream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream: false);
            return FromProvenanceText(Encoding.Latin1.GetString(buffer, 0, read));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>The text (bytes read as Latin-1) of the start of an MP4: the song id named by `contentId` after `com.suno.provenance`.</summary>
    public static string? FromProvenanceText(string text)
    {
        int at = text.IndexOf("com.suno.provenance", StringComparison.Ordinal);
        if (at < 0) return null;
        int end = Math.Min(text.Length, at + 600);   // the contentId follows within a few dozen bytes
        var m = ContentId.Match(text, at, end - at);
        return m.Success ? SongAddress(m.Groups[1].Value) : null;
    }

    private static string SongAddress(string id) => "https://suno.com/song/" + id.ToLowerInvariant();

    private static bool IsWebAddress(string? text) =>
        !string.IsNullOrWhiteSpace(text)
        && Uri.TryCreate(text.Trim(), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
