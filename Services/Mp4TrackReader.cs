using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

/// <summary>Reads embedded track metadata from MP4/MOV/M4V files via lightweight ISO BMFF box parsing.</summary>
internal static class Mp4TrackReader
{
    public static List<MediaTrackInfo> ReadTracks(string filePath)
    {
        var tracks = new List<MediaTrackInfo>();
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs);

            // Scan top-level boxes using full file length — moov may be at the end for
            // non-streaming-optimized files; seeking over mdat is instant, no data is read.
            while (fs.Position < fs.Length - 8)
            {
                long prev = fs.Position;
                (string? type, long _, long dataEnd) = ReadBox(br, fs.Length);
                if (type == null) { if (fs.Position == prev) break; continue; }
                if (type == "moov") { ParseMoov(br, dataEnd, tracks); break; }
                fs.Seek(dataEnd, SeekOrigin.Begin);
            }
        }
        catch { }
        return tracks;
    }

    private static void ParseMoov(BinaryReader br, long end, List<MediaTrackInfo> tracks)
    {
        var fs = br.BaseStream;
        while (fs.Position < end - 8)
        {
            long prev = fs.Position;
            (string? type, long _, long dataEnd) = ReadBox(br, end);
            if (type == null) { if (fs.Position == prev) break; continue; }
            if (type == "trak")
            {
                var t = ParseTrak(br, dataEnd);
                if (t != null) tracks.Add(t);
            }
            fs.Seek(dataEnd, SeekOrigin.Begin);
        }
    }

    private static MediaTrackInfo? ParseTrak(BinaryReader br, long end)
    {
        var t  = new MediaTrackInfo();
        var fs = br.BaseStream;
        while (fs.Position < end - 8)
        {
            long prev = fs.Position;
            (string? type, long _, long dataEnd) = ReadBox(br, end);
            if (type == null) { if (fs.Position == prev) break; continue; }
            if      (type == "tkhd") { try { ParseTkhd(br, t); } catch { } }
            else if (type == "mdia") ParseMdia(br, dataEnd, t);
            fs.Seek(dataEnd, SeekOrigin.Begin);
        }
        return t.Type is "" or "Timecode" or "Metadata" or "Hint" ? null : t;
    }

    private static void ParseTkhd(BinaryReader br, MediaTrackInfo t)
    {
        byte version = ReadByteSafe(br);
        var  flags   = br.ReadBytes(3);
        t.IsDefault  = flags.Length >= 3 && (flags[2] & 0x01) != 0;
        br.BaseStream.Seek(version == 1 ? 16 : 8, SeekOrigin.Current); // skip creation + modification time
        t.Number = (int)ReadU32(br);
    }

    private static void ParseMdia(BinaryReader br, long end, MediaTrackInfo t)
    {
        var fs = br.BaseStream;
        while (fs.Position < end - 8)
        {
            long prev = fs.Position;
            (string? type, long _, long dataEnd) = ReadBox(br, end);
            if (type == null) { if (fs.Position == prev) break; continue; }
            if      (type == "hdlr") { try { ParseHdlr(br, t); } catch { } }
            else if (type == "mdhd") { try { ParseMdhd(br, t); } catch { } }
            else if (type == "minf") ParseMinf(br, dataEnd, t);
            fs.Seek(dataEnd, SeekOrigin.Begin);
        }
    }

    private static void ParseHdlr(BinaryReader br, MediaTrackInfo t)
    {
        br.ReadBytes(8); // version+flags + pre-defined
        var handlerBytes = br.ReadBytes(4);
        var handler = handlerBytes.Length == 4
            ? new string(handlerBytes.Select(b => (char)b).ToArray()).Trim()
            : "";
        t.Type = handler switch
        {
            "vide"                     => "Video",
            "soun"                     => "Audio",
            "text" or "sbtl" or "subt" => "Subtitle",
            "tmcd"                     => "Timecode",
            "meta"                     => "Metadata",
            "hint"                     => "Hint",
            var x                      => x
        };
    }

    private static void ParseMdhd(BinaryReader br, MediaTrackInfo t)
    {
        byte version = ReadByteSafe(br);
        br.ReadBytes(3);
        if (version == 1) { br.ReadBytes(8); br.ReadBytes(8); br.ReadBytes(4); br.ReadBytes(8); }
        else              { br.ReadBytes(4); br.ReadBytes(4); br.ReadBytes(4); br.ReadBytes(4); }
        // 2-byte packed ISO 639-2/T language: three 5-bit values, each + 0x60
        var langBytes = br.ReadBytes(2);
        if (langBytes.Length < 2) return;
        byte hi = langBytes[0], lo = langBytes[1];
        char l1 = (char)(((hi & 0x7C) >> 2) + 0x60);
        char l2 = (char)(((hi & 0x03) << 3 | (lo >> 5)) + 0x60);
        char l3 = (char)((lo & 0x1F) + 0x60);
        var lang = new string([l1, l2, l3]);
        if (lang.All(c => c is >= 'a' and <= 'z') && lang != "und")
            t.Language = lang;
    }

    private static void ParseMinf(BinaryReader br, long end, MediaTrackInfo t)
    {
        var fs = br.BaseStream;
        while (fs.Position < end - 8)
        {
            long prev = fs.Position;
            (string? type, long _, long dataEnd) = ReadBox(br, end);
            if (type == null) { if (fs.Position == prev) break; continue; }
            if (type == "stbl") { ParseStbl(br, dataEnd, t); break; }
            fs.Seek(dataEnd, SeekOrigin.Begin);
        }
    }

    private static void ParseStbl(BinaryReader br, long end, MediaTrackInfo t)
    {
        var fs = br.BaseStream;
        while (fs.Position < end - 8)
        {
            long prev = fs.Position;
            (string? type, long _, long dataEnd) = ReadBox(br, end);
            if (type == null) { if (fs.Position == prev) break; continue; }
            if (type == "stsd") { try { ParseStsd(br, t); } catch { } break; }
            fs.Seek(dataEnd, SeekOrigin.Begin);
        }
    }

    private static void ParseStsd(BinaryReader br, MediaTrackInfo t)
    {
        br.ReadBytes(4); // version + flags
        if (ReadU32(br) > 0)
        {
            ReadU32(br);  // entry size (discarded)
            var codecBytes = br.ReadBytes(4);
            if (codecBytes.Length == 4)
            {
                t.CodecId   = new string(codecBytes.Select(b => (char)b).ToArray()).Trim();
                t.CodecName = CodecDisplay(t.CodecId);
            }
        }
    }

    // Box reader — returns (type, dataStart, dataEnd); type is null on error/overflow
    private static (string?, long, long) ReadBox(BinaryReader br, long parentEnd)
    {
        var fs = br.BaseStream;
        if (fs.Position + 8 > parentEnd) return (null, 0, 0);
        long boxStart  = fs.Position;
        uint rawSize   = ReadU32(br);
        var  typeBytes = br.ReadBytes(4);
        if (typeBytes.Length < 4) return (null, 0, 0);
        string type    = new string(typeBytes.Select(b => (char)b).ToArray());
        long dataStart = fs.Position;
        long dataEnd;
        if (rawSize == 1)
        {
            if (fs.Position + 8 > parentEnd) return (null, 0, 0);
            long size64 = (long)ReadU64(br);
            dataStart = fs.Position;
            dataEnd   = boxStart + size64;
        }
        else if (rawSize == 0) dataEnd = parentEnd;
        else                   dataEnd = boxStart + (long)rawSize;
        if (dataEnd > parentEnd || dataEnd < dataStart) return (null, 0, 0);
        return (type, dataStart, dataEnd);
    }

    private static byte ReadByteSafe(BinaryReader br)
    {
        var b = br.ReadBytes(1);
        return b.Length > 0 ? b[0] : (byte)0;
    }

    private static uint ReadU32(BinaryReader br)
    {
        var b = br.ReadBytes(4);
        if (b.Length < 4) return 0;
        return (uint)((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
    }

    private static ulong ReadU64(BinaryReader br)
    {
        var b = br.ReadBytes(8);
        if (b.Length < 8) return 0;
        ulong v = 0;
        foreach (var x in b) v = (v << 8) | x;
        return v;
    }

    internal static string CodecDisplay(string id) => id.Trim().ToLowerInvariant() switch
    {
        "avc1" or "avc2" or "avc3" or "avc4" or "h264" or "x264" => "H.264",
        "hvc1" or "hev1" or "dvh1" or "dvhe"                      => "H.265 (HEVC)",
        "av01"                                                     => "AV1",
        "vp08"                                                     => "VP8",
        "vp09"                                                     => "VP9",
        "mp4v" or "xvid" or "divx"                                 => "MPEG-4",
        "mp4a"                                                     => "AAC",
        "ac-3"                                                     => "AC-3 (Dolby Digital)",
        "ec-3"                                                     => "E-AC-3 (Dolby Digital Plus)",
        "dtsc" or "dtse" or "dtsh" or "dtsl"                       => "DTS",
        "alac"                                                     => "ALAC",
        "opus"                                                     => "Opus",
        "tx3g" or ".tx3" or "text"                                 => "Timed Text",
        "wvtt"                                                     => "WebVTT",
        "c608" or "c708"                                           => "Closed Captions",
        "stpp"                                                     => "TTML",
        var x                                                      => x
    };
}
