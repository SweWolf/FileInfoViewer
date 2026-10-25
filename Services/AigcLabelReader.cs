using System.IO;
using System.Text;
using System.Text.Json;

namespace FileInfoViewer.Services;

/// <summary>
/// The "AIGC" label that Chinese AI services (Vidu, Kling, Doubao ...) write into generated media, as the Chinese labeling rules for
/// AI-generated content require. Fields: Label (1 = AI-generated, 2 = possibly, 3 = suspected), ContentProducer and ContentPropagator
/// (the service provider's code), ProduceID and PropagateID (the service's own id for the job, here often "model-version-resolution-number"),
/// ReservedCode1/2 (a hash or signature the service can check). Nothing here is verified.
/// </summary>
public sealed record AigcLabel(int? Label, IReadOnlyList<KeyValuePair<string, string>> Fields)
{
    public string LabelText => Label switch
    {
        1 => "AI-generated",
        2 => "Possibly AI-generated",
        3 => "Suspected AI-generated",
        _ => "AI label present",
    };
}

/// <summary>
/// Reads the "AIGC" item from the QuickTime-style metadata of an MP4/MOV (moov/udta/meta: a "keys" box with "mdta" names and an
/// "ilst" box whose items point at the keys by number). The value is a JSON text. Nothing is written. Null when there is none.
/// When copying: change the namespace.
/// History:
///   2026-10-25  First version (MP4/MOV only).
/// </summary>
public static class AigcLabelReader
{
    public static AigcLabel? Read(string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var moov = FindTopLevel(fs, "moov");
            if (moov == null) return null;

            var keys = new List<string>();
            var items = new Dictionary<int, string>();
            Walk(moov, 0, moov.Length, keys, items);

            for (int i = 0; i < keys.Count; i++)
            {
                if (!keys[i].Equals("AIGC", StringComparison.OrdinalIgnoreCase) || !items.TryGetValue(i + 1, out var json)) continue;
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
                int? label = null;
                var fields = new List<KeyValuePair<string, string>>();
                foreach (var p in doc.RootElement.EnumerateObject())
                {
                    var text = p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() ?? "" : p.Value.ToString();
                    if (p.Name == "Label" && int.TryParse(text, out var n)) label = n;
                    fields.Add(new(p.Name, text));
                }
                return new AigcLabel(label, fields);
            }
        }
        catch { }
        return null;
    }

    private static byte[]? FindTopLevel(FileStream fs, string type)
    {
        var hdr = new byte[16];
        long pos = 0;
        while (pos + 8 <= fs.Length)
        {
            fs.Position = pos;
            if (fs.Read(hdr, 0, 16) < 8) return null;
            long size = (uint)((hdr[0] << 24) | (hdr[1] << 16) | (hdr[2] << 8) | hdr[3]);
            int hdrLen = 8;
            if (size == 1) { size = 0; for (int i = 8; i < 16; i++) size = (size << 8) | hdr[i]; hdrLen = 16; }
            else if (size == 0) size = fs.Length - pos;
            if (size < hdrLen) return null;
            if (Encoding.ASCII.GetString(hdr, 4, 4) == type && size <= 64 * 1024 * 1024)
            {
                var body = new byte[size - hdrLen];
                fs.Position = pos + hdrLen;
                return fs.Read(body, 0, body.Length) == body.Length ? body : null;
            }
            pos += size;
        }
        return null;
    }

    private static void Walk(byte[] b, int start, int end, List<string> keys, Dictionary<int, string> items)
    {
        int pos = start;
        while (pos + 8 <= end)
        {
            int size = (b[pos] << 24) | (b[pos + 1] << 16) | (b[pos + 2] << 8) | b[pos + 3];
            var type = Encoding.Latin1.GetString(b, pos + 4, 4);
            if (size < 8 || pos + size > end) return;
            int body = pos + 8, bodyEnd = pos + size;
            switch (type)
            {
                case "udta":
                    Walk(b, body, bodyEnd, keys, items);
                    break;
                case "meta":
                    Walk(b, body + 4, bodyEnd, keys, items); // full box: skip version+flags
                    break;
                case "keys":
                    {
                        int p = body + 4;
                        int count = (b[p] << 24) | (b[p + 1] << 16) | (b[p + 2] << 8) | b[p + 3];
                        p += 4;
                        for (int i = 0; i < count && p + 8 <= bodyEnd; i++)
                        {
                            int ks = (b[p] << 24) | (b[p + 1] << 16) | (b[p + 2] << 8) | b[p + 3];
                            if (ks < 8 || p + ks > bodyEnd) break;
                            keys.Add(Encoding.UTF8.GetString(b, p + 8, ks - 8));
                            p += ks;
                        }
                        break;
                    }
                case "ilst":
                    {
                        int p = body;
                        while (p + 8 <= bodyEnd)
                        {
                            int isz = (b[p] << 24) | (b[p + 1] << 16) | (b[p + 2] << 8) | b[p + 3];
                            if (isz < 8 || p + isz > bodyEnd) break;
                            int index = (b[p + 4] << 24) | (b[p + 5] << 16) | (b[p + 6] << 8) | b[p + 7];
                            // inside: a "data" box = size, "data", type (4), locale (4), value
                            int d = p + 8;
                            if (d + 16 <= p + isz && Encoding.Latin1.GetString(b, d + 4, 4) == "data")
                                items[index] = Encoding.UTF8.GetString(b, d + 16, p + isz - (d + 16));
                            p += isz;
                        }
                        break;
                    }
            }
            pos += size;
        }
    }
}
