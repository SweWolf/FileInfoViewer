using System.Buffers.Binary;
using System.Text;
using FileInfoViewer.Models;

namespace FileInfoViewer.Services;

// Reads the RIFF chunks of a WebP file (GDI+ can't decode WebP). Shows the same fields as
// ExifTool's RIFF group; EXIF/XMP/ICC contents come from MetadataExtractor.
internal static class WebpInfoReader
{
    private static readonly string[] Vp8Versions =
    [
        "0 (bicubic reconstruction, normal loop)",
        "1 (bilinear reconstruction, simple loop)",
        "2 (bilinear reconstruction, no loop)",
        "3 (no reconstruction, no loop)",
    ];

    public static void Read(string filePath, ImageInfoModel imageInfo)
    {
        var raw = File.ReadAllBytes(filePath);
        if (raw.Length < 12 || Encoding.ASCII.GetString(raw, 0, 4) != "RIFF" || Encoding.ASCII.GetString(raw, 8, 4) != "WEBP")
            throw new InvalidDataException("Not a valid WebP file.");

        var details = imageInfo.FormatDetails;
        bool seenImage = false;
        int frameCount = 0;
        long totalDurationMs = 0;
        int pos = 12;

        while (pos + 8 <= raw.Length)
        {
            var fourCc = Encoding.ASCII.GetString(raw, pos, 4);
            var size   = BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(pos + 4));
            int start  = pos + 8;
            if (size > raw.Length - start) break; // truncated file
            var data = raw.AsSpan(start, (int)size);

            switch (fourCc)
            {
                case "VP8X" when data.Length >= 10:
                    details["Format"] = "Extended (VP8X)";
                    details["WebP Flags"] = FormatFlags(data[0]);
                    imageInfo.Width  = ReadUInt24(data[4..]) + 1;
                    imageInfo.Height = ReadUInt24(data[7..]) + 1;
                    break;

                case "VP8 " when !seenImage && data.Length >= 10:
                    seenImage = true;
                    details.TryAdd("Format", "Lossy (VP8)");
                    details["VP8 Version"] = Vp8Versions[(data[0] >> 1) & 3];
                    int w = BinaryPrimitives.ReadUInt16LittleEndian(data[6..]);
                    int h = BinaryPrimitives.ReadUInt16LittleEndian(data[8..]);
                    if (imageInfo.Width == 0)
                    {
                        imageInfo.Width  = w & 0x3FFF;
                        imageInfo.Height = h & 0x3FFF;
                    }
                    details["Horizontal Scale"] = (w >> 14).ToString();
                    details["Vertical Scale"]   = (h >> 14).ToString();
                    break;

                case "VP8L" when !seenImage && data.Length >= 5 && data[0] == 0x2F:
                    seenImage = true;
                    details.TryAdd("Format", "Lossless (VP8L)");
                    var bits = BinaryPrimitives.ReadUInt32LittleEndian(data[1..]);
                    if (imageInfo.Width == 0)
                    {
                        imageInfo.Width  = (int)(bits & 0x3FFF) + 1;
                        imageInfo.Height = (int)((bits >> 14) & 0x3FFF) + 1;
                    }
                    details["Alpha Is Used"] = ((bits >> 28) & 1) == 1 ? "Yes" : "No";
                    break;

                case "ALPH" when data.Length >= 1:
                    details["Alpha Preprocessing"] = ((data[0] >> 4) & 3) == 1 ? "Level Reduction" : "None";
                    details["Alpha Filtering"] = ((data[0] >> 2) & 3) switch
                    {
                        1 => "Horizontal", 2 => "Vertical", 3 => "Gradient", _ => "None",
                    };
                    details["Alpha Compression"] = (data[0] & 3) == 1 ? "Lossless" : "None";
                    break;

                case "ANIM" when data.Length >= 6:
                    // Stored as blue, green, red, alpha
                    details["Background Color"] = $"R {data[2]}, G {data[1]}, B {data[0]}, A {data[3]}";
                    var loops = BinaryPrimitives.ReadUInt16LittleEndian(data[4..]);
                    details["Animation Loop Count"] = loops == 0 ? "Infinite" : loops.ToString();
                    break;

                case "ANMF" when data.Length >= 16:
                    frameCount++;
                    totalDurationMs += ReadUInt24(data[12..]);
                    break;
            }

            pos = start + (int)size + (int)(size & 1); // chunks are padded to an even size
        }

        if (frameCount > 0)
        {
            details["Frame Count"] = frameCount.ToString();
            details["Duration"] = $"{totalDurationMs / 1000.0:F2} s";
        }
    }

    private static int ReadUInt24(ReadOnlySpan<byte> b) => b[0] | (b[1] << 8) | (b[2] << 16);

    private static string FormatFlags(byte flags)
    {
        var names = new List<string>();
        if ((flags & 0x02) != 0) names.Add("Animation");
        if ((flags & 0x04) != 0) names.Add("XMP");
        if ((flags & 0x08) != 0) names.Add("EXIF");
        if ((flags & 0x10) != 0) names.Add("Alpha");
        if ((flags & 0x20) != 0) names.Add("ICC Profile");
        return names.Count > 0 ? string.Join(", ", names) : "None";
    }
}
