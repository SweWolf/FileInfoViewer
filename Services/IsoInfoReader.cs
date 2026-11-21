using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

/// <summary>
/// Reads the structure of an ISO 9660 disc image (volume descriptors and the directory tree) and, when the disc is a
/// DVD-Video, the IFO files inside it: titles, durations, chapters, video format, audio tracks and subtitles.
/// Only the sectors that are needed are read, so a 7 GB image opens in a moment.
/// </summary>
internal static class IsoInfoReader
{
    private const int MaxEntries = 500_000;
    private const int MaxDepth   = 40;
    private const int MaxIfoSize = 8 * 1024 * 1024;

    private sealed record Entry(string Path, long Lba, long Size, bool IsDir);

    public static bool IsIsoFile(string filePath)
    {
        try
        {
            using var fs = OpenRead(filePath);
            var b = ReadAt(fs, 16L * 2048, 8);
            return b is { Length: 8 } && b[0] == 1 && Encoding.ASCII.GetString(b, 1, 5) == "CD001";
        }
        catch { return false; }
    }

    public static IsoInfoModel Read(string filePath)
    {
        using var fs = OpenRead(filePath);
        var iso = new IsoInfoModel();

        // ── Volume descriptors (start at sector 16) ──────────────────────────────
        byte[]? pvd = null;
        for (int s = 16; s < 16 + 40; s++)
        {
            var b = ReadAt(fs, (long)s * 2048, 2048);
            if (b == null || b.Length < 8) break;
            var id = Encoding.ASCII.GetString(b, 1, 5);
            if (id == "CD001")
            {
                if (b[0] == 1 && pvd == null) pvd = b;
                else if (b[0] == 0 && Encoding.ASCII.GetString(b, 7, 23) == "EL TORITO SPECIFICATION")
                    iso.IsBootable = true;
            }
            else if (id is "NSR02" or "NSR03") iso.HasUdf = true;
            else if (id is "BEA01" or "TEA01") { if (id == "TEA01") break; }
            else break;
        }

        if (pvd == null)
        {
            iso.Kind = iso.HasUdf ? "UDF disc image (no ISO 9660 part)" : "Unknown";
            return iso;
        }

        int blockSize = BitConverter.ToUInt16(pvd, 128);
        if (blockSize < 512) blockSize = 2048;

        iso.SystemId    = Text(pvd, 8, 32);
        iso.VolumeLabel = Text(pvd, 40, 32);
        iso.VolumeSet   = Text(pvd, 190, 128);
        iso.Publisher   = Text(pvd, 318, 128);
        iso.Preparer    = Text(pvd, 446, 128);
        iso.Application = Text(pvd, 574, 128);
        iso.Created     = ParseDate(pvd, 813);
        iso.Modified    = ParseDate(pvd, 830);
        iso.ImageBytes  = (long)BitConverter.ToUInt32(pvd, 80) * blockSize;

        // ── Directory tree ───────────────────────────────────────────────────────
        var entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        var rootLba = BitConverter.ToUInt32(pvd, 156 + 2);
        var rootLen = BitConverter.ToUInt32(pvd, 156 + 10);
        var seen    = new HashSet<long> { rootLba };
        var stack   = new Stack<(string Path, long Lba, long Len, int Depth)>();
        stack.Push(("", rootLba, rootLen, 0));

        while (stack.Count > 0 && entries.Count < MaxEntries)
        {
            var (dirPath, lba, len, depth) = stack.Pop();
            if (len <= 0 || len > 64 * 1024 * 1024) continue;
            var d = ReadAt(fs, lba * blockSize, (int)len);
            if (d == null) continue;

            int o = 0;
            while (o < d.Length)
            {
                int recLen = d[o];
                if (recLen == 0) { o = (o / blockSize + 1) * blockSize; continue; }   // padding up to the next sector
                if (o + recLen > d.Length || recLen < 34) break;

                int  nameLen = d[o + 32];
                var  flags   = d[o + 25];
                long extent  = BitConverter.ToUInt32(d, o + 2);
                long size    = BitConverter.ToUInt32(d, o + 10);
                bool isDir   = (flags & 2) != 0;
                bool dotEntry = nameLen == 1 && d[o + 33] <= 1;     // "." and ".."

                if (!dotEntry && o + 33 + nameLen <= d.Length)
                {
                    var name = Encoding.Latin1.GetString(d, o + 33, nameLen);
                    var semi = name.IndexOf(';');
                    if (semi >= 0) name = name[..semi];
                    if (name.EndsWith('.')) name = name[..^1];
                    var path = dirPath + "/" + name;

                    if (isDir)
                    {
                        iso.FolderCount++;
                        entries[path] = new Entry(path, extent, size, true);
                        if (depth < MaxDepth && seen.Add(extent))
                            stack.Push((path, extent, size, depth + 1));
                    }
                    else
                    {
                        iso.FileCount++;
                        iso.TotalFileBytes += size;
                        // A file split into several extents has one record per extent: keep the first, add the rest.
                        if (entries.TryGetValue(path, out var prev))
                            entries[path] = prev with { Size = prev.Size + size };
                        else
                            entries[path] = new Entry(path, extent, size, false);
                    }
                }
                o += recLen;
            }
        }

        // Names in the root folder, for a quick idea of what the disc holds
        foreach (var e in entries.Values.Where(e => e.Path.Count(c => c == '/') == 1).OrderByDescending(e => e.IsDir).ThenBy(e => e.Path))
            iso.RootItems.Add(e.Path[1..] + (e.IsDir ? "/" : ""));

        // ── What kind of disc is it? ─────────────────────────────────────────────
        bool Has(string p) => entries.ContainsKey(p);

        if (Has("/VIDEO_TS/VIDEO_TS.IFO"))
        {
            iso.Kind = "DVD-Video";
            try { iso.Dvd = ReadDvd(fs, entries, blockSize); }
            catch { /* damaged IFO files: the general information is still shown */ }
        }
        else if (Has("/BDMV/INDEX.BDMV") || Has("/BDMV/INDEX.BDM"))
        {
            iso.Kind = "Blu-ray Disc";
            ReadBluray(fs, entries, blockSize, iso);
        }
        else if (Has("/SOURCES/INSTALL.WIM") || Has("/SOURCES/INSTALL.ESD") || Has("/SOURCES/INSTALL.SWM"))
            iso.Kind = "Windows installation disc";
        else if (Has("/MPEGAV") || Has("/VCD"))
            iso.Kind = "Video CD";
        else if (iso.IsBootable)
            iso.Kind = "Bootable disc";
        else
            iso.Kind = "Data disc";

        return iso;
    }

    // ═══ DVD-Video ═══════════════════════════════════════════════════════════════

    private static DvdInfoModel? ReadDvd(FileStream fs, Dictionary<string, Entry> files, int blockSize)
    {
        var vmg = ReadFile(fs, files, "/VIDEO_TS/VIDEO_TS.IFO", blockSize) ?? ReadFile(fs, files, "/VIDEO_TS/VIDEO_TS.BUP", blockSize);
        if (vmg == null || vmg.Length < 0x100 || Encoding.ASCII.GetString(vmg, 0, 12) != "DVDVIDEO-VMG") return null;

        var dvd = new DvdInfoModel();

        // Region mask: a set bit means "not playable in that region"
        // (VMG category, byte 0x23)
        // (regions 7 and 8 are reserved; region-free discs often have those two bits set)
        int mask = vmg[0x23] & 0x3F;
        dvd.Regions = mask == 0 ? "Region-free"
            : mask == 0x3F ? ""
            : "Region " + string.Join(", ", Enumerable.Range(1, 6).Where(r => (mask & (1 << (r - 1))) == 0));

        // Title table (TT_SRPT)
        long ttSector = U32(vmg, 0xC4);
        int  ttOff    = (int)(ttSector * 2048);
        if (ttSector == 0 || ttOff + 8 > vmg.Length) return dvd;
        int titleCount = U16(vmg, ttOff);

        var vtsCache = new Dictionary<int, byte[]?>();
        DvdVtsInfo? mainVts = null;
        TimeSpan best = TimeSpan.Zero;

        for (int i = 0; i < titleCount && i < 99; i++)
        {
            int e = ttOff + 8 + i * 12;
            if (e + 12 > vmg.Length) break;
            int angles = vmg[e + 1];
            int vtsn   = vmg[e + 6];
            int vtsTtn = vmg[e + 7];

            if (!vtsCache.TryGetValue(vtsn, out var vts))
            {
                var name = $"/VIDEO_TS/VTS_{vtsn:00}_0.";
                vts = ReadFile(fs, files, name + "IFO", blockSize) ?? ReadFile(fs, files, name + "BUP", blockSize);
                vtsCache[vtsn] = vts;
            }
            if (vts == null || vts.Length < 0x300 || Encoding.ASCII.GetString(vts, 0, 12) != "DVDVIDEO-VTS") continue;

            var (dur, chapters) = TitleLength(vts, vtsTtn);
            // Menu clips and hidden helper titles can list dozens of "chapters" that are shorter than a second:
            // that is authoring tricks, not chapters, so don't report them.
            if (chapters > 0 && dur.TotalSeconds / chapters < 1) chapters = 0;
            var t = new DvdTitleInfo { Number = i + 1, TitleSet = vtsn, Angles = angles, Duration = dur, Chapters = chapters };
            dvd.Titles.Add(t);

            if (dur > best)
            {
                best = dur;
                dvd.MainTitle = t;
                mainVts = ReadStreams(vts);
            }
        }

        if (mainVts != null)
        {
            dvd.Video     = mainVts.Video;
            dvd.Audio     = mainVts.Audio;
            dvd.Subtitles = mainVts.Subtitles;
        }
        return dvd;
    }

    /// <summary>Playing time and chapter count of one title inside a title set (VTS IFO).</summary>
    private static (TimeSpan Duration, int Chapters) TitleLength(byte[] vts, int vtsTtn)
    {
        var duration = TimeSpan.Zero;
        int chapters = 0;

        // Chapters: VTS_PTT_SRPT — one list of (program chain, program) pairs per title
        long pttSector = U32(vts, 0xC8);
        int  pttOff    = (int)(pttSector * 2048);
        if (pttSector != 0 && pttOff + 8 <= vts.Length)
        {
            int  n    = U16(vts, pttOff);
            long end  = pttOff + U32(vts, pttOff + 4) + 1;
            if (vtsTtn >= 1 && vtsTtn <= n && pttOff + 8 + n * 4 <= vts.Length)
            {
                long from = pttOff + U32(vts, pttOff + 8 + (vtsTtn - 1) * 4);
                long to   = vtsTtn < n ? pttOff + U32(vts, pttOff + 8 + vtsTtn * 4) : end;
                if (to > from) chapters = (int)((to - from) / 4);
            }
        }

        // Duration: playback time of the title's entry program chain (VTS_PGCIT)
        long pgcSector = U32(vts, 0xCC);
        int  pgcit     = (int)(pgcSector * 2048);
        if (pgcSector != 0 && pgcit + 8 <= vts.Length)
        {
            int n = U16(vts, pgcit);
            for (int i = 0; i < n; i++)
            {
                int srp = pgcit + 8 + i * 8;
                if (srp + 8 > vts.Length) break;
                bool entry = (vts[srp] & 0x80) != 0;
                if (!entry || (vts[srp] & 0x7F) != vtsTtn) continue;

                int pgc = pgcit + (int)U32(vts, srp + 4);
                if (pgc + 8 > vts.Length) break;
                duration = PlaybackTime(vts, pgc + 4);
                break;
            }
        }
        return (duration, chapters);
    }

    /// <summary>BCD hours, minutes, seconds and frames (the top two bits of the frame byte give 25 or 30 fps).</summary>
    private static TimeSpan PlaybackTime(byte[] b, int o)
    {
        static int Bcd(int v) => (v >> 4) * 10 + (v & 15);
        int h = Bcd(b[o]), m = Bcd(b[o + 1]), s = Bcd(b[o + 2]);
        int fps    = (b[o + 3] >> 6) == 3 ? 30 : (b[o + 3] >> 6) == 1 ? 25 : 0;
        int frames = ((b[o + 3] >> 4) & 3) * 10 + (b[o + 3] & 15);
        var t = new TimeSpan(0, h, m, s);
        if (fps > 0) t += TimeSpan.FromSeconds((double)frames / fps);
        return t;
    }

    private sealed class DvdVtsInfo
    {
        public string Video = "";
        public List<string> Audio = [];
        public List<string> Subtitles = [];
    }

    private static DvdVtsInfo ReadStreams(byte[] vts)
    {
        var r = new DvdVtsInfo();

        // Video attributes (2 bytes at 0x200)
        int v      = U16(vts, 0x200);
        int coding = (v >> 14) & 3;
        int std    = (v >> 12) & 3;     // 0 = NTSC, 1 = PAL
        int aspect = (v >> 10) & 3;
        int res    = (v >> 3) & 7;
        bool pal   = std == 1;
        int width  = res switch { 0 => 720, 1 => 704, _ => 352 };
        int height = res == 3 ? (pal ? 288 : 240) : (pal ? 576 : 480);
        var parts = new List<string>
        {
            coding == 0 ? "MPEG-1" : "MPEG-2",
            $"{width}×{height}",
            aspect == 3 ? "16:9" : "4:3",
            pal ? "PAL 25 fps" : "NTSC 29.97 fps",
        };
        r.Video = string.Join(", ", parts);

        // Audio attributes: count at 0x202, 8 bytes each from 0x204
        int na = Math.Min(U16(vts, 0x202), 8);
        for (int i = 0; i < na; i++)
        {
            int o = 0x204 + i * 8;
            int b0 = vts[o], b1 = vts[o + 1];
            string codec = (b0 >> 5) switch
            {
                0 => "Dolby Digital", 2 => "MPEG-1 Audio", 3 => "MPEG-2 Audio", 4 => "LPCM", 6 => "DTS", _ => "Unknown",
            };
            int ch = (b1 & 7) + 1;
            string layout = ch switch { 1 => "mono", 2 => "stereo", 6 => "5.1", 7 => "6.1", 8 => "7.1", _ => $"{ch} channels" };
            var s = new List<string> { Language(vts, o + 2, (b0 >> 2) & 3), codec, layout };
            if ((b0 >> 5) == 4)
                s.Add((b1 >> 6) switch { 1 => "20-bit", 2 => "24-bit", _ => "16-bit" });
            if (((b1 >> 4) & 3) == 1) s.Add("96 kHz");
            switch (vts[o + 5])
            {
                case 2: s.Add("audio description"); break;
                case 3: case 4: s.Add("commentary"); break;
            }
            r.Audio.Add(string.Join(", ", s));
        }

        // Subtitle attributes: count at 0x254, 6 bytes each from 0x256
        int ns = Math.Min(U16(vts, 0x254), 32);
        for (int i = 0; i < ns; i++)
        {
            int o = 0x256 + i * 6;
            if (o + 6 > vts.Length) break;
            var s = Language(vts, o + 2, vts[o] & 3);
            switch (vts[o + 5])
            {
                case 5: case 6: case 7: s += " (closed captions)"; break;
                case 9: s += " (forced)"; break;
                case 13: case 14: case 15: s += " (commentary)"; break;
            }
            r.Subtitles.Add(s);
        }
        return r;
    }

    private static string Language(byte[] b, int o, int langType)
    {
        if (langType != 1 || b[o] < 'a' || b[o] > 'z' || b[o + 1] < 'a' || b[o + 1] > 'z') return "Unspecified language";
        var code = new string([(char)b[o], (char)b[o + 1]]);
        try
        {
            var name = CultureInfo.GetCultureInfo(code).EnglishName;
            return name.Length > 0 && !name.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase) ? name : code.ToUpperInvariant();
        }
        catch { return code.ToUpperInvariant(); }
    }

    // ═══ Blu-ray ═════════════════════════════════════════════════════════════════

    private static void ReadBluray(FileStream fs, Dictionary<string, Entry> files, int blockSize, IsoInfoModel iso)
    {
        var xml = ReadFile(fs, files, "/BDMV/META/DL/BDMT_ENG.XML", blockSize)
               ?? files.Keys.Where(k => k.StartsWith("/BDMV/META/DL/BDMT_", StringComparison.OrdinalIgnoreCase)).Select(k => ReadFile(fs, files, k, blockSize)).FirstOrDefault(x => x != null);
        if (xml != null)
        {
            var m = Regex.Match(Encoding.UTF8.GetString(xml), @"<di:name>\s*([^<]+?)\s*</di:name>");
            if (m.Success) iso.DiscTitle = System.Net.WebUtility.HtmlDecode(m.Groups[1].Value);
        }

        var streams = files.Values.Where(e => !e.IsDir && e.Path.StartsWith("/BDMV/STREAM/", StringComparison.OrdinalIgnoreCase)
                                              && e.Path.EndsWith(".M2TS", StringComparison.OrdinalIgnoreCase)).ToList();
        iso.VideoStreamFiles = streams.Count;
        iso.VideoStreamBytes = streams.Sum(e => e.Size);
    }

    // ═══ Helpers ═════════════════════════════════════════════════════════════════

    private static FileStream OpenRead(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1, FileOptions.RandomAccess);

    private static byte[]? ReadAt(FileStream fs, long offset, int count)
    {
        if (offset < 0 || offset >= fs.Length) return null;
        count = (int)Math.Min(count, fs.Length - offset);
        var buf = new byte[count];
        fs.Seek(offset, SeekOrigin.Begin);
        int got = 0;
        while (got < count)
        {
            int n = fs.Read(buf, got, count - got);
            if (n <= 0) break;
            got += n;
        }
        return got == count ? buf : buf[..got];
    }

    private static byte[]? ReadFile(FileStream fs, Dictionary<string, Entry> files, string path, int blockSize)
    {
        if (!files.TryGetValue(path, out var e) || e.IsDir || e.Size <= 0) return null;
        return ReadAt(fs, e.Lba * blockSize, (int)Math.Min(e.Size, MaxIfoSize));
    }

    private static string Text(byte[] b, int offset, int length) =>
        Encoding.Latin1.GetString(b, offset, length).Trim(' ', '\0');

    private static int U16(byte[] b, int o) => o + 2 <= b.Length ? (b[o] << 8) | b[o + 1] : 0;

    private static long U32(byte[] b, int o) =>
        o + 4 <= b.Length ? ((long)b[o] << 24) | ((long)b[o + 1] << 16) | ((long)b[o + 2] << 8) | b[o + 3] : 0;

    /// <summary>ISO 9660 date "YYYYMMDDHHMMSScc" plus a time-zone byte (15-minute steps); all zeros means "not set".</summary>
    private static DateTime? ParseDate(byte[] b, int o)
    {
        var s = Encoding.ASCII.GetString(b, o, 16);
        if (s.All(c => c == '0' || c == '\0' || c == ' ')) return null;
        if (!DateTime.TryParseExact(s[..14], "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return null;
        int tz = (sbyte)b[o + 16];
        return DateTime.SpecifyKind(dt.AddMinutes(-tz * 15), DateTimeKind.Utc);
    }
}
