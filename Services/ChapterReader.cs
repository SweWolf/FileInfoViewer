using System.IO;
using System.Text;

namespace FileInfoViewer.Services;

/// <summary>One chapter of a video: where it starts and its title (empty when the file has none).</summary>
public sealed record ChapterInfo(TimeSpan Start, string Title);

/// <summary>
/// Reads the chapter list of a video without FFmpeg/ffprobe, by parsing the container itself:
///   Matroska/WebM: the Chapters element (first edition; nested chapters count too), start times in nanoseconds.
///   MP4/M4V/MOV:   the Nero "chpl" box in moov/udta, or else the QuickTime chapter track (a text track that another track
///                  points at with tref/chap; one text sample per chapter).
/// Nothing is written. An empty list means the file has no chapters (or a kind this reader doesn't know).
/// History:
///   2026-10-25  First version (MKV/WebM and MP4/MOV).
/// </summary>
public static class ChapterReader
{
    public static List<ChapterInfo> Read(string path, bool matroska, bool mp4)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            if (matroska) return ReadMatroska(fs);
            if (mp4) return ReadMp4(fs);
        }
        catch { }
        return [];
    }

    // ---- Matroska / WebM ----

    private const uint IdEbml = 0x1A45DFA3, IdSegment = 0x18538067, IdChapters = 0x1043A770, IdEdition = 0x45B9,
                       IdAtom = 0xB6, IdTimeStart = 0x91, IdDisplay = 0x80, IdChapString = 0x85;
    private const long Unknown = -1;

    private static List<ChapterInfo> ReadMatroska(FileStream fs)
    {
        var list = new List<ChapterInfo>();
        fs.Position = 0;
        if (ReadId(fs) != IdEbml) return list;
        var hs = ReadSize(fs);
        if (hs < 0) return list;
        fs.Seek(hs, SeekOrigin.Current);
        if (ReadId(fs) != IdSegment) return list;
        ReadSize(fs); // may be unknown: the segment simply runs to the end of the file

        // Walk the segment's children, skipping the big clusters by seeking (nothing is read)
        while (fs.Position < fs.Length - 2)
        {
            uint id = ReadId(fs);
            long size = ReadSize(fs);
            long data = fs.Position;
            if (size < 0) break;
            if (id == IdChapters)
            {
                var body = new byte[size];
                if (fs.Read(body, 0, body.Length) != body.Length) break;
                ParseChapters(body, 0, body.Length, list, firstEditionOnly: true);
                break;
            }
            fs.Seek(data + size, SeekOrigin.Begin);
        }
        return list;
    }

    private static void ParseChapters(byte[] b, int start, int end, List<ChapterInfo> list, bool firstEditionOnly)
    {
        int pos = start;
        while (pos < end)
        {
            if (!TryElement(b, ref pos, end, out uint id, out int bodyStart, out int bodyEnd)) return;
            if (id == IdEdition)
            {
                ParseChapters(b, bodyStart, bodyEnd, list, false);
                if (firstEditionOnly) return;
            }
            else if (id == IdAtom)
            {
                long ns = 0;
                string title = "";
                int p = bodyStart;
                while (p < bodyEnd && TryElement(b, ref p, bodyEnd, out uint cid, out int cs, out int ce))
                {
                    if (cid == IdTimeStart) ns = (long)ReadUInt(b, cs, ce);
                    else if (cid == IdDisplay && title == "")
                    {
                        int q = cs;
                        while (q < ce && TryElement(b, ref q, ce, out uint did, out int ds, out int de))
                            if (did == IdChapString) { title = Encoding.UTF8.GetString(b, ds, de - ds); break; }
                    }
                    p = ce;
                }
                list.Add(new ChapterInfo(TimeSpan.FromTicks(ns / 100), title));
                ParseChapters(b, bodyStart, bodyEnd, list, false); // chapters nested inside this one
            }
            pos = bodyEnd;
        }
    }

    /// <summary>Reads an element header at pos; pos is left at the start of the body. False when the data runs out.</summary>
    private static bool TryElement(byte[] b, ref int pos, int end, out uint id, out int bodyStart, out int bodyEnd)
    {
        id = 0; bodyStart = bodyEnd = pos;
        if (pos >= end) return false;
        int idLen = LeadingLength(b[pos]);
        if (idLen == 0 || pos + idLen >= end) return false;
        for (int i = 0; i < idLen; i++) id = (id << 8) | b[pos + i];
        int p = pos + idLen;
        int sizeLen = LeadingLength(b[p]);
        if (sizeLen == 0 || p + sizeLen > end) return false;
        long size = b[p] & (0xFF >> sizeLen);
        for (int i = 1; i < sizeLen; i++) size = (size << 8) | b[p + i];
        p += sizeLen;
        if (size < 0 || p + size > end) return false;
        bodyStart = p;
        bodyEnd = (int)(p + size);
        pos = bodyStart;
        return true;
    }

    private static int LeadingLength(byte first)
    {
        for (int i = 0; i < 8; i++) if ((first & (0x80 >> i)) != 0) return i + 1;
        return 0;
    }

    private static ulong ReadUInt(byte[] b, int start, int end)
    {
        ulong v = 0;
        for (int i = start; i < end && i < start + 8; i++) v = (v << 8) | b[i];
        return v;
    }

    private static uint ReadId(FileStream fs)
    {
        int first = fs.ReadByte();
        int len = first < 0 ? 0 : LeadingLength((byte)first);
        if (len == 0 || len > 4) return 0;
        uint id = (uint)first;
        for (int i = 1; i < len; i++) id = (id << 8) | (uint)fs.ReadByte();
        return id;
    }

    /// <summary>Element size from the stream; -1 for "unknown".</summary>
    private static long ReadSize(FileStream fs)
    {
        int first = fs.ReadByte();
        int len = first < 0 ? 0 : LeadingLength((byte)first);
        if (len == 0) return Unknown;
        long v = first & (0xFF >> len);
        bool allOnes = v == (0xFF >> len);
        for (int i = 1; i < len; i++)
        {
            int c = fs.ReadByte();
            if (c != 0xFF) allOnes = false;
            v = (v << 8) | (uint)c;
        }
        return allOnes ? Unknown : v;
    }

    // ---- MP4 / MOV ----

    private sealed class Trak
    {
        public uint Id;
        public uint Timescale;
        public string Handler = "";
        public List<uint> ChapterRefs = [];
        public List<(uint Count, uint Delta)> Stts = [];
        public List<uint> SampleSizes = [];
        public List<(uint FirstChunk, uint PerChunk)> Stsc = [];
        public List<long> ChunkOffsets = [];
    }

    private static List<ChapterInfo> ReadMp4(FileStream fs)
    {
        // Find moov at the top level (it may be at the end of the file)
        long pos = 0;
        var hdr = new byte[16];
        byte[]? moov = null;
        while (pos + 8 <= fs.Length)
        {
            fs.Position = pos;
            if (fs.Read(hdr, 0, 16) < 8) break;
            long size = (uint)((hdr[0] << 24) | (hdr[1] << 16) | (hdr[2] << 8) | hdr[3]);
            int hdrLen = 8;
            if (size == 1) { size = 0; for (int i = 8; i < 16; i++) size = (size << 8) | hdr[i]; hdrLen = 16; }
            else if (size == 0) size = fs.Length - pos;
            if (size < hdrLen) break;
            if (hdr[4] == 'm' && hdr[5] == 'o' && hdr[6] == 'o' && hdr[7] == 'v' && size <= 256L * 1024 * 1024)
            {
                moov = new byte[size - hdrLen];
                fs.Position = pos + hdrLen;
                if (fs.Read(moov, 0, moov.Length) != moov.Length) return [];
                break;
            }
            pos += size;
        }
        if (moov == null) return [];

        var traks = new List<Trak>();
        List<ChapterInfo>? nero = null;
        foreach (var (type, s, e) in Boxes(moov, 0, moov.Length))
        {
            if (type == "udta" && nero == null)
                foreach (var (t2, s2, e2) in Boxes(moov, s, e))
                    if (t2 == "chpl") nero = ParseChpl(moov, s2, e2);
            if (type == "trak") traks.Add(ParseTrak(moov, s, e));
        }
        if (nero is { Count: > 0 }) return nero;

        // QuickTime chapter track: some track lists it in tref/chap
        var chapterTrackId = traks.SelectMany(t => t.ChapterRefs).FirstOrDefault();
        var chapTrak = chapterTrackId == 0 ? null : traks.FirstOrDefault(t => t.Id == chapterTrackId);
        return chapTrak == null ? [] : ReadChapterTrack(fs, chapTrak);
    }

    private static IEnumerable<(string Type, int Start, int End)> Boxes(byte[] b, int start, int end)
    {
        int pos = start;
        while (pos + 8 <= end)
        {
            long size = (uint)((b[pos] << 24) | (b[pos + 1] << 16) | (b[pos + 2] << 8) | b[pos + 3]);
            var type = Encoding.Latin1.GetString(b, pos + 4, 4);
            int hdrLen = 8;
            if (size == 1 && pos + 16 <= end)
            {
                size = 0;
                for (int i = 8; i < 16; i++) size = (size << 8) | b[pos + i];
                hdrLen = 16;
            }
            else if (size == 0) size = end - pos;
            if (size < hdrLen || pos + size > end) yield break;
            yield return (type, pos + hdrLen, (int)(pos + size));
            pos += (int)size;
        }
    }

    private static uint U32(byte[] b, int p) => (uint)((b[p] << 24) | (b[p + 1] << 16) | (b[p + 2] << 8) | b[p + 3]);

    private static long U64(byte[] b, int p) => ((long)U32(b, p) << 32) | U32(b, p + 4);

    /// <summary>Nero chapters: version, flags (3), [4 reserved if version 1], count, then start time (100 ns units, 8 bytes), title length (1), title.</summary>
    private static List<ChapterInfo> ParseChpl(byte[] b, int s, int e)
    {
        var list = new List<ChapterInfo>();
        int p = s + 4;
        if (b[s] == 1) p += 4;
        if (p >= e) return list;
        int count = b[p++];
        for (int i = 0; i < count && p + 9 <= e; i++)
        {
            long start = U64(b, p);
            int len = b[p + 8];
            p += 9;
            if (p + len > e) break;
            list.Add(new ChapterInfo(TimeSpan.FromTicks(start), Encoding.UTF8.GetString(b, p, len)));
            p += len;
        }
        return list;
    }

    private static Trak ParseTrak(byte[] b, int s, int e)
    {
        var t = new Trak();
        foreach (var (type, bs, be) in Boxes(b, s, e))
        {
            if (type == "tkhd") t.Id = U32(b, bs + (b[bs] == 1 ? 20 : 12));
            else if (type == "tref")
            {
                foreach (var (rt, rs, re) in Boxes(b, bs, be))
                    if (rt == "chap") for (int p = rs; p + 4 <= re; p += 4) t.ChapterRefs.Add(U32(b, p));
            }
            else if (type == "mdia") ParseMdia(b, bs, be, t);
        }
        return t;
    }

    private static void ParseMdia(byte[] b, int s, int e, Trak t)
    {
        foreach (var (type, bs, be) in Boxes(b, s, e))
        {
            if (type == "mdhd") t.Timescale = U32(b, bs + (b[bs] == 1 ? 20 : 12));
            else if (type == "hdlr") t.Handler = Encoding.Latin1.GetString(b, bs + 8, 4);
            else if (type == "minf")
                foreach (var (t2, s2, e2) in Boxes(b, bs, be))
                    if (t2 == "stbl") ParseStbl(b, s2, e2, t);
        }
    }

    private static void ParseStbl(byte[] b, int s, int e, Trak t)
    {
        foreach (var (type, bs, be) in Boxes(b, s, e))
        {
            uint n = be - bs >= 8 ? U32(b, bs + 4) : 0;
            switch (type)
            {
                case "stts":
                    for (int i = 0; i < n && bs + 8 + i * 8 + 8 <= be; i++)
                        t.Stts.Add((U32(b, bs + 8 + i * 8), U32(b, bs + 12 + i * 8)));
                    break;
                case "stsz":
                    {
                        uint fixedSize = U32(b, bs + 4);
                        uint count = U32(b, bs + 8);
                        for (int i = 0; i < count; i++)
                        {
                            if (fixedSize != 0) t.SampleSizes.Add(fixedSize);
                            else if (bs + 12 + i * 4 + 4 <= be) t.SampleSizes.Add(U32(b, bs + 12 + i * 4));
                        }
                        break;
                    }
                case "stsc":
                    for (int i = 0; i < n && bs + 8 + i * 12 + 12 <= be; i++)
                        t.Stsc.Add((U32(b, bs + 8 + i * 12), U32(b, bs + 12 + i * 12)));
                    break;
                case "stco":
                    for (int i = 0; i < n && bs + 8 + i * 4 + 4 <= be; i++) t.ChunkOffsets.Add(U32(b, bs + 8 + i * 4));
                    break;
                case "co64":
                    for (int i = 0; i < n && bs + 8 + i * 8 + 8 <= be; i++) t.ChunkOffsets.Add(U64(b, bs + 8 + i * 8));
                    break;
            }
        }
    }

    /// <summary>Each sample of the chapter track is one chapter: a 2-byte length and the title (UTF-8, or UTF-16 with a BOM).</summary>
    private static List<ChapterInfo> ReadChapterTrack(FileStream fs, Trak t)
    {
        var list = new List<ChapterInfo>();
        if (t.SampleSizes.Count == 0 || t.ChunkOffsets.Count == 0 || t.Stsc.Count == 0 || t.Timescale == 0) return list;

        // Sample start times from the time-to-sample table
        var times = new List<long>();
        long time = 0;
        foreach (var (count, delta) in t.Stts)
            for (uint i = 0; i < count && times.Count < t.SampleSizes.Count; i++) { times.Add(time); time += delta; }

        // Sample file offsets from the chunk table
        int sample = 0;
        for (int chunk = 0; chunk < t.ChunkOffsets.Count && sample < t.SampleSizes.Count; chunk++)
        {
            uint perChunk = t.Stsc.LastOrDefault(x => x.FirstChunk <= chunk + 1).PerChunk;
            long offset = t.ChunkOffsets[chunk];
            for (uint i = 0; i < perChunk && sample < t.SampleSizes.Count; i++, sample++)
            {
                uint size = t.SampleSizes[sample];
                string title = "";
                if (size >= 2 && size < 4096)
                {
                    var buf = new byte[size];
                    fs.Position = offset;
                    if (fs.Read(buf, 0, buf.Length) == buf.Length)
                    {
                        int len = Math.Min((buf[0] << 8) | buf[1], buf.Length - 2);
                        title = len >= 2 && buf[2] == 0xFE && buf[3] == 0xFF
                            ? Encoding.BigEndianUnicode.GetString(buf, 4, len - 2)
                            : Encoding.UTF8.GetString(buf, 2, len);
                    }
                }
                long start = sample < times.Count ? times[sample] : 0;
                list.Add(new ChapterInfo(TimeSpan.FromSeconds((double)start / t.Timescale), title));
                offset += size;
            }
        }
        return list;
    }
}
