using System.Text;
using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

/// <summary>
/// Reads the structure of a Photoshop file (PSD, and PSB "Large Document") without loading the pixel
/// data (these files are often hundreds of MB). GDI+ can't decode them; EXIF/XMP/IPTC contents come
/// from MetadataExtractor.
/// Layout (big-endian): 26-byte header ("8BPS", version 1=PSD/2=PSB, 6 reserved, channels, height,
/// width, depth, color mode), color mode data, image resources, layer and mask info (length is 8
/// bytes in a PSB), then the merged image data (starts with a 2-byte compression method).
/// </summary>
internal static class PsdInfoReader
{
    private static readonly Dictionary<int, string> ColorModes = new()
    {
        [0] = "Bitmap", [1] = "Grayscale", [2] = "Indexed color", [3] = "RGB", [4] = "CMYK",
        [7] = "Multichannel", [8] = "Duotone", [9] = "Lab",
    };

    private static readonly string[] Compressions = ["None", "RLE (PackBits)", "ZIP", "ZIP with prediction"];

    public static bool IsPsdFile(string filePath)
    {
        try
        {
            var b = new byte[4];
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return fs.Read(b, 0, 4) == 4 && Encoding.ASCII.GetString(b) == "8BPS";
        }
        catch { return false; }
    }

    public static void Read(string filePath, ImageInfoModel info)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var br = new BinaryReader(fs);

        ushort U16() { var b = br.ReadBytes(2); if (b.Length < 2) throw new EndOfStreamException(); return (ushort)((b[0] << 8) | b[1]); }
        uint U32() { var b = br.ReadBytes(4); if (b.Length < 4) throw new EndOfStreamException(); return ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3]; }
        ulong U64() => ((ulong)U32() << 32) | U32();

        if (fs.Length < 26 || Encoding.ASCII.GetString(br.ReadBytes(4)) != "8BPS")
            throw new InvalidDataException("Not a valid Photoshop file.");
        int version = U16();
        if (version is not (1 or 2)) throw new InvalidDataException($"Unknown Photoshop file version {version}.");
        bool isPsb = version == 2;
        fs.Seek(6, SeekOrigin.Current);
        int channels = U16();
        info.Height = (int)U32();
        info.Width = (int)U32();
        int depth = U16();
        int mode = U16();

        var modeName = ColorModes.GetValueOrDefault(mode, $"Unknown ({mode})");
        info.BitDepth = depth;
        info.PixelFormat = $"{modeName}, {depth} bits per channel";
        info.HorizontalDpi = info.VerticalDpi = 72; // replaced below if the file says otherwise

        var d = info.FormatDetails;
        d["Format"] = isPsb ? "Photoshop Large Document (PSB)" : "Photoshop Document (PSD)";
        d["Color Mode"] = modeName;
        d["Channels"] = channels.ToString();

        // Color mode data (palette / duotone curves): skip
        fs.Seek(U32(), SeekOrigin.Current);

        // Image resources
        long resLen = U32();
        long resEnd = fs.Position + resLen;
        string? iccName = null;
        bool hasIcc = false;
        while (fs.Position + 12 <= resEnd)
        {
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "8BIM") break;
            int id = U16();
            int nameLen = br.ReadByte();
            fs.Seek(((nameLen + 1 + 1) & ~1) - 1, SeekOrigin.Current); // Pascal string, padded to even
            long size = U32();
            long dataStart = fs.Position;
            long next = dataStart + ((size + 1) & ~1L);
            if (next > resEnd) break;

            if (id == 1005 && size >= 16) // ResolutionInfo
            {
                double h = U32() / 65536.0; int hUnit = U16(); fs.Seek(2, SeekOrigin.Current);
                double v = U32() / 65536.0; int vUnit = U16();
                info.HorizontalDpi = hUnit == 2 ? h * 2.54 : h;
                info.VerticalDpi = vUnit == 2 ? v * 2.54 : v;
            }
            else if (id == 1039 && size >= 132 && size < 64 * 1024 * 1024) // ICC profile
            {
                hasIcc = true;
                iccName = ReadIccDescription(br.ReadBytes((int)size));
            }
            fs.Seek(next, SeekOrigin.Begin);
        }
        fs.Seek(resEnd, SeekOrigin.Begin);
        if (hasIcc) d["ICC Profile"] = string.IsNullOrEmpty(iccName) ? "Embedded" : iccName;

        // Layer and mask info
        long lmLen = (long)(isPsb ? U64() : U32());
        long lmEnd = fs.Position + lmLen;
        if (lmLen > 0)
        {
            long layerInfoLen = (long)(isPsb ? U64() : U32());
            if (layerInfoLen >= 2)
            {
                short count = (short)U16();
                // A negative count means the first alpha channel holds the merged image's transparency
                d["Layers"] = Math.Abs((int)count).ToString();
                if (count < 0) d["Transparency"] = "Yes (in the merged image)";
            }
            else
            {
                d["Layers"] = "0 (flat image)";
            }
        }

        // Merged image data
        if (lmEnd + 2 <= fs.Length)
        {
            fs.Seek(lmEnd, SeekOrigin.Begin);
            int comp = U16();
            if (comp < Compressions.Length) d["Compression"] = Compressions[comp];
        }
    }

    // The profile description ("desc" tag): ICC v2 'desc' (ASCII) or v4 'mluc' (UTF-16BE)
    private static string? ReadIccDescription(byte[] icc)
    {
        try
        {
            static uint U32(byte[] b, int o) => ((uint)b[o] << 24) | ((uint)b[o + 1] << 16) | ((uint)b[o + 2] << 8) | b[o + 3];
            uint tags = U32(icc, 128);
            for (int i = 0; i < tags && i < 200; i++)
            {
                int e = 132 + i * 12;
                if (Encoding.ASCII.GetString(icc, e, 4) != "desc") continue;
                int off = (int)U32(icc, e + 4);
                var type = Encoding.ASCII.GetString(icc, off, 4);
                if (type == "desc")
                {
                    int len = (int)U32(icc, off + 8);
                    return Encoding.ASCII.GetString(icc, off + 12, Math.Max(0, len - 1)).Trim();
                }
                if (type == "mluc")
                {
                    int strLen = (int)U32(icc, off + 20), strOff = (int)U32(icc, off + 24);
                    return Encoding.BigEndianUnicode.GetString(icc, off + strOff, strLen).Trim();
                }
            }
        }
        catch { }
        return null;
    }
}
