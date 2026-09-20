using System.Security.Cryptography;
using System.Text;
using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

internal static class TorrentInfoReader
{
    public static TorrentInfoModel Read(string filePath)
    {
        var model = new TorrentInfoModel();
        var raw = File.ReadAllBytes(filePath);

        // Some tools save a magnet link with a .torrent extension
        if (raw.Length >= 7 && Encoding.ASCII.GetString(raw, 0, 7) == "magnet:")
        {
            model.IsMagnetLink = true;
            model.MagnetLink   = Encoding.UTF8.GetString(raw).Trim();
            return model;
        }

        using var ms = new MemoryStream(raw);
        if (ParseValue(ms) is not BDict root) return model;

        model.Name         = Str(root, "name") ?? "";
        model.Comment      = Str(root, "comment") ?? "";
        model.CreatedBy    = Str(root, "created by") ?? "";
        model.Publisher    = Str(root, "publisher") ?? "";
        model.PublisherUrl = Str(root, "publisher-url") ?? "";

        if (root.TryGetValue("creation date", out var cdObj) && cdObj is long cdTs)
            model.CreationDate = DateTimeOffset.FromUnixTimeSeconds(cdTs).UtcDateTime;

        // Trackers
        var ann = Str(root, "announce");
        if (!string.IsNullOrEmpty(ann)) model.Trackers.Add(ann);
        if (root.TryGetValue("announce-list", out var alObj) && alObj is List<object?> annList)
        {
            foreach (var tier in annList)
                if (tier is List<object?> urls)
                    foreach (var u in urls)
                    {
                        var url = Str(u);
                        if (!string.IsNullOrEmpty(url) && !model.Trackers.Contains(url))
                            model.Trackers.Add(url);
                    }
        }
        model.PrimaryTracker = model.Trackers.FirstOrDefault() ?? "";

        // Info dictionary
        if (root.TryGetValue("info", out var infoObj) && infoObj is BDict info)
        {
            if (string.IsNullOrEmpty(model.Name))
                model.Name = Str(info, "name") ?? Str(info, "name.utf-8") ?? "";

            if (info.TryGetValue("piece length", out var plObj) && plObj is long pl)
                model.PieceSizeBytes = pl;

            model.IsPrivate = info.TryGetValue("private", out var pvObj) && pvObj is long pv && pv == 1;
            model.Source    = Str(info, "source") ?? "";

            if (info.TryGetValue("length", out var lenObj) && lenObj is long singleLen)
            {
                // Single-file torrent
                model.FileCount      = 1;
                model.TotalSizeBytes = singleLen;
                model.Files.Add(new TorrentFileEntry { Path = model.Name, SizeBytes = singleLen });
            }
            else if (info.TryGetValue("files", out var filesObj) && filesObj is List<object?> fileList)
            {
                // Multi-file torrent
                foreach (var f in fileList)
                {
                    if (f is not BDict fd) continue;
                    long size = fd.TryGetValue("length", out var fl) && fl is long l ? l : 0;
                    var pathParts = fd.TryGetValue("path.utf-8", out var pU) && pU is List<object?> partsU ? partsU
                                  : fd.TryGetValue("path",       out var pN) && pN is List<object?> partsN ? partsN
                                  : null;
                    var path = pathParts != null
                        ? string.Join("/", pathParts.Select(p => Str(p) ?? "").Where(s => s.Length > 0))
                        : "";
                    model.Files.Add(new TorrentFileEntry { Path = path, SizeBytes = size });
                    model.TotalSizeBytes += size;
                }
                model.FileCount = model.Files.Count;
            }
        }

        // Info hash — SHA-1 of the raw bencoded "info" value
        var infoBytes = ExtractInfoBytes(raw);
        if (infoBytes != null)
        {
            model.InfoHash   = Convert.ToHexString(SHA1.HashData(infoBytes)).ToLowerInvariant();
            var dn           = string.IsNullOrEmpty(model.Name) ? "" : $"&dn={Uri.EscapeDataString(model.Name)}";
            model.MagnetLink = $"magnet:?xt=urn:btih:{model.InfoHash}{dn}";
        }

        return model;
    }

    // Returns the raw bencoded bytes of the top-level "info" value for SHA-1 hashing
    private static byte[]? ExtractInfoBytes(byte[] raw)
    {
        using var s = new MemoryStream(raw);
        if (s.ReadByte() != 'd') return null;
        while (s.Position < s.Length && Peek(s) != 'e')
        {
            var kb = ReadStrBytes(s);
            if (kb == null) break;
            if (Encoding.UTF8.GetString(kb) == "info")
            {
                long start = s.Position;
                SkipValue(s);
                return raw[(int)start..(int)s.Position];
            }
            SkipValue(s);
        }
        return null;
    }

    private static object? ParseValue(Stream s) => Peek(s) switch
    {
        'i'          => ParseInt(s),
        'l'          => ParseList(s),
        'd'          => ParseDict(s),
        >= '0' and <= '9' => ReadStrBytes(s),
        _            => null,
    };

    private static long? ParseInt(Stream s)
    {
        s.ReadByte();
        var sb = new StringBuilder(16);
        int b;
        while ((b = s.ReadByte()) != 'e' && b != -1) sb.Append((char)b);
        return long.TryParse(sb.ToString(), out long n) ? n : null;
    }

    private static List<object?> ParseList(Stream s)
    {
        s.ReadByte();
        var list = new List<object?>();
        int p;
        while ((p = Peek(s)) != 'e' && p != -1) list.Add(ParseValue(s));
        s.ReadByte();
        return list;
    }

    private static BDict ParseDict(Stream s)
    {
        s.ReadByte();
        var dict = new BDict();
        int p;
        while ((p = Peek(s)) != 'e' && p != -1)
        {
            var kb = ReadStrBytes(s);
            if (kb == null) break;
            var key = Encoding.UTF8.GetString(kb);
            if (key == "pieces") { SkipValue(s); continue; } // raw SHA-1 chunk hashes — not useful to display
            dict[key] = ParseValue(s);
        }
        s.ReadByte();
        return dict;
    }

    private static byte[]? ReadStrBytes(Stream s)
    {
        var lenSb = new StringBuilder(10);
        int b;
        while ((b = s.ReadByte()) != ':' && b != -1) lenSb.Append((char)b);
        if (!int.TryParse(lenSb.ToString(), out int len) || len < 0) return null;
        if (s.Position + len > s.Length) return null;
        var data = new byte[len];
        s.ReadExactly(data);
        return data;
    }

    private static void SkipValue(Stream s)
    {
        int b = Peek(s);
        if (b == 'i')
        {
            s.ReadByte();
            while (s.ReadByte() is not ('e' or -1)) { }
        }
        else if (b == 'l')
        {
            s.ReadByte();
            int p;
            while ((p = Peek(s)) != 'e' && p != -1) SkipValue(s);
            s.ReadByte();
        }
        else if (b == 'd')
        {
            s.ReadByte();
            int p;
            while ((p = Peek(s)) != 'e' && p != -1) { SkipValue(s); SkipValue(s); }
            s.ReadByte();
        }
        else if (b >= '0' && b <= '9')
        {
            var lenSb = new StringBuilder(10);
            int c;
            while ((c = s.ReadByte()) != ':' && c != -1) lenSb.Append((char)c);
            if (int.TryParse(lenSb.ToString(), out int len) && len > 0)
                s.Seek(len, SeekOrigin.Current);
        }
    }

    private static int Peek(Stream s)
    {
        if (s.Position >= s.Length) return -1;
        int b = s.ReadByte();
        s.Position--;
        return b;
    }

    private static string? Str(BDict dict, string key) =>
        dict.TryGetValue(key, out var v) ? Str(v) : null;

    private static string? Str(object? v) =>
        v is byte[] b ? Encoding.UTF8.GetString(b) : null;
}

internal sealed class BDict : Dictionary<string, object?> { }
