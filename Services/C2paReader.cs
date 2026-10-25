using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace FileInfoViewer.Services;

/// <summary>What the Content Credentials (C2PA) in a file say. Nothing here is verified: it is only what the file claims.</summary>
public sealed class C2paInfo
{
    public int ManifestCount { get; set; }
    /// <summary>True when an action says the media was made (or partly made) by an AI model.</summary>
    public bool IsAiGenerated { get; set; }
    public List<KeyValuePair<string, string>> Summary { get; } = [];
    public List<string> Actions { get; } = [];
    public List<string> OtherAssertions { get; } = [];
}

/// <summary>
/// Reads Content Credentials (C2PA, the "this is AI-generated / edited with ..." signature that Google Gemini, OpenAI, Adobe, Leica
/// and others embed). The data is a JUMBF box tree: in a JPEG it is split over APP11 segments, in a PNG it is the "caBX" chunk.
/// Inside, each manifest has assertions (CBOR), a claim (CBOR) and a COSE signature (CBOR) that carries the signer's certificate
/// chain and often a trusted timestamp. A small CBOR decoder is enough to show the interesting parts.
/// The signature is NOT checked (that needs a full C2PA library): the card says so. Nothing is written. Null when there is none.
/// History:
///   2026-10-25  First version (JPEG and PNG).
///   2026-10-25  MP4/MOV: the C2PA "uuid" box.
///   2026-10-25  CBOR with indefinite lengths (Microsoft); action times and agent versions; watermark names.
/// </summary>
public static class C2paReader
{
    public static C2paInfo? Read(string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var jumbf = ReadJumbf(fs);
            if (jumbf == null) return null;
            var roots = ParseBoxes(jumbf, 0, jumbf.Length);
            var store = roots.FirstOrDefault(n => n.Type == "jumb" && n.Label == "c2pa");
            if (store == null) return null;
            return Describe(store);
        }
        catch { return null; }
    }

    // ---- Finding the JUMBF bytes in the container ----

    private static byte[]? ReadJumbf(FileStream fs)
    {
        var head = new byte[8];
        if (fs.Read(head, 0, 8) < 8) return null;
        if (head[0] == 0xFF && head[1] == 0xD8) return ReadJpeg(fs);
        if (head[4] == 'f' && head[5] == 't' && head[6] == 'y' && head[7] == 'p') return ReadMp4(fs);
        if (head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47) return ReadPng(fs);
        return null;
    }

    private static byte[]? ReadJpeg(FileStream fs)
    {
        fs.Position = 2;
        var parts = new List<(int En, uint Seq, byte[] Data)>();
        var hdr = new byte[4];
        while (fs.Read(hdr, 0, 4) == 4 && hdr[0] == 0xFF)
        {
            int marker = hdr[1];
            if (marker == 0xDA || marker == 0xD9) break; // image data starts: no more metadata
            int len = (hdr[2] << 8) | hdr[3];
            if (len < 2) break;
            if (marker == 0xEB && len >= 2 + 8)
            {
                var seg = new byte[len - 2];
                if (fs.Read(seg, 0, seg.Length) < seg.Length) break;
                // "JP", Box Instance number (2), Packet sequence number (4), then the box (or, in a later packet, its header again)
                if (seg[0] == 'J' && seg[1] == 'P')
                {
                    int en = (seg[2] << 8) | seg[3];
                    uint seq = (uint)((seg[4] << 24) | (seg[5] << 16) | (seg[6] << 8) | seg[7]);
                    parts.Add((en, seq, seg[8..]));
                }
            }
            else fs.Seek(len - 2, SeekOrigin.Current);
        }
        if (parts.Count == 0) return null;

        // Use the first box instance that has a "jumb" box; later packets repeat the 8-byte box header, which is skipped
        var first = parts.OrderBy(p => p.Seq).First();
        using var ms = new MemoryStream();
        bool firstPacket = true;
        foreach (var p in parts.Where(p => p.En == first.En).OrderBy(p => p.Seq))
        {
            if (firstPacket) ms.Write(p.Data); else if (p.Data.Length > 8) ms.Write(p.Data, 8, p.Data.Length - 8);
            firstPacket = false;
        }
        return ms.ToArray();
    }

    private static readonly byte[] C2paUuid = [0xD8, 0xFE, 0xC3, 0xD6, 0x1B, 0x0E, 0x48, 0x3C, 0x92, 0x97, 0x58, 0x28, 0x87, 0x7E, 0xC4, 0x81];

    /// <summary>MP4/MOV: a top-level "uuid" box with the C2PA id: version+flags (4), the purpose "manifest" and a zero byte, an 8-byte offset, then the JUMBF.</summary>
    private static byte[]? ReadMp4(FileStream fs)
    {
        fs.Position = 0;
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
            if (hdr[4] == 'u' && hdr[5] == 'u' && hdr[6] == 'i' && hdr[7] == 'd' && size <= 64 * 1024 * 1024 && size > hdrLen + 16 + 4 + 9 + 8)
            {
                var body = new byte[size - hdrLen];
                fs.Position = pos + hdrLen;
                if (fs.Read(body, 0, body.Length) < body.Length) return null;
                if (body.AsSpan(0, 16).SequenceEqual(C2paUuid))
                {
                    int p = 20; // after uuid and version/flags
                    int zero = Array.IndexOf(body, (byte)0, p);
                    if (zero > 0 && Encoding.ASCII.GetString(body, p, zero - p) == "manifest")
                        return body[(zero + 1 + 8)..];
                }
            }
            pos += size;
        }
        return null;
    }

    private static byte[]? ReadPng(FileStream fs)
    {
        fs.Position = 8;
        var hdr = new byte[8];
        while (fs.Read(hdr, 0, 8) == 8)
        {
            uint len = (uint)((hdr[0] << 24) | (hdr[1] << 16) | (hdr[2] << 8) | hdr[3]);
            var type = Encoding.ASCII.GetString(hdr, 4, 4);
            if (type == "IDAT" || type == "IEND") { if (type == "IEND") break; }
            if (type == "caBX")
            {
                var data = new byte[len];
                return fs.Read(data, 0, data.Length) == data.Length ? data : null;
            }
            if (fs.Position + len + 4 > fs.Length) break;
            fs.Seek(len + 4, SeekOrigin.Current); // data + CRC
        }
        return null;
    }

    // ---- JUMBF boxes ----

    private sealed class Node
    {
        public string Type = "";
        public string? Label;
        public byte[] Data = [];
        public List<Node> Children = [];
    }

    private static List<Node> ParseBoxes(byte[] buf, int start, int end)
    {
        var list = new List<Node>();
        int pos = start;
        while (pos + 8 <= end)
        {
            long size = (uint)((buf[pos] << 24) | (buf[pos + 1] << 16) | (buf[pos + 2] << 8) | buf[pos + 3]);
            var type = Encoding.Latin1.GetString(buf, pos + 4, 4);
            int hdrLen = 8;
            if (size == 1)
            {
                if (pos + 16 > end) break;
                size = 0;
                for (int i = 0; i < 8; i++) size = (size << 8) | buf[pos + 8 + i];
                hdrLen = 16;
            }
            else if (size == 0) size = end - pos;
            if (size < hdrLen || pos + size > end) break;

            int bodyStart = pos + hdrLen, bodyEnd = (int)(pos + size);
            var node = new Node { Type = type };
            if (type == "jumb")
            {
                foreach (var child in ParseBoxes(buf, bodyStart, bodyEnd))
                {
                    if (child.Type == "jumd" && node.Label == null)
                        node.Label = DescriptionLabel(child.Data);
                    else
                        node.Children.Add(child);
                }
            }
            else node.Data = buf[bodyStart..bodyEnd];
            list.Add(node);
            pos = bodyEnd;
        }
        return list;
    }

    /// <summary>jumd: 16-byte content type, 1 byte toggles (bit 1 = a label follows), then the label as a zero-terminated string.</summary>
    private static string? DescriptionLabel(byte[] d)
    {
        if (d.Length < 18 || (d[16] & 0x02) == 0) return "";
        int end = Array.IndexOf(d, (byte)0, 17);
        if (end < 0) end = d.Length;
        return Encoding.UTF8.GetString(d, 17, end - 17);
    }

    // ---- Describing the manifests ----

    private static C2paInfo Describe(Node store)
    {
        var manifests = store.Children.Where(n => n.Type == "jumb").ToList();
        var info = new C2paInfo { ManifestCount = manifests.Count };
        if (manifests.Count == 0) return info;

        var timestamps = new List<DateTime>();
        var seenActions = new HashSet<string>();
        var seenOther = new HashSet<string>();
        string? generator = null, title = null, format = null;
        X509Certificate2? signer = null;

        foreach (var manifest in manifests) // oldest first; the last one is the active manifest
        {
            foreach (var part in manifest.Children.Where(n => n.Type == "jumb"))
            {
                var label = part.Label ?? "";
                var cbor = FirstCbor(part);
                if (label == "c2pa.assertions")
                {
                    foreach (var assertion in part.Children.Where(n => n.Type == "jumb"))
                    {
                        var name = Regex.Replace(assertion.Label ?? "", "__\\d+$", "");
                        var value = FirstCbor(assertion);
                        if (name.StartsWith("c2pa.actions"))
                        {
                            foreach (var action in Items(Get(value, "actions")))
                                AddAction(info, action, seenActions);
                        }
                        else if (seenOther.Add(name))
                        {
                            var alg = name == "c2pa.soft-binding" ? Text(Get(value, "alg")) : null;
                            info.OtherAssertions.Add(alg != null ? $"Invisible watermark ({alg})" : AssertionName(name));
                        }
                    }
                }
                else if (label.StartsWith("c2pa.claim") && cbor != null)
                {
                    generator = ClaimGenerator(cbor) ?? generator;
                    title = Text(Get(cbor, "dc:title")) ?? title;
                    format = Text(Get(cbor, "dc:format")) ?? format;
                }
                else if (label == "c2pa.signature" && cbor != null)
                {
                    ReadSignature(cbor, ref signer, timestamps);
                }
            }
        }

        if (generator != null) info.Summary.Add(new("Generator", generator));
        if (title != null) info.Summary.Add(new("Title", title));
        if (format != null) info.Summary.Add(new("Format", format));
        if (signer != null)
        {
            var cn = signer.GetNameInfo(X509NameType.SimpleName, false);
            var org = Regex.Match(signer.Subject, @"(?:^|,\s*)O=(?:""([^""]+)""|([^,]+))");
            var orgName = org.Success ? (org.Groups[1].Success ? org.Groups[1].Value : org.Groups[2].Value) : null;
            info.Summary.Add(new("Signed By", orgName ?? cn ?? signer.Subject));
            var issuer = signer.GetNameInfo(X509NameType.SimpleName, true);
            if (!string.IsNullOrEmpty(issuer)) info.Summary.Add(new("Certificate Issuer", issuer));
            info.Summary.Add(new("Certificate Valid", $"{signer.NotBefore:yyyy-MM-dd} to {signer.NotAfter:yyyy-MM-dd}"));
        }
        if (timestamps.Count > 0)
        {
            info.Summary.Add(new(timestamps.Count > 1 ? "First Signed (UTC)" : "Signed (UTC)", timestamps[0].ToString("yyyy-MM-dd HH:mm:ss")));
            if (timestamps.Count > 1 && timestamps[^1] != timestamps[0])
                info.Summary.Add(new("Last Signed (UTC)", timestamps[^1].ToString("yyyy-MM-dd HH:mm:ss")));
        }
        info.Summary.Add(new("Manifests", manifests.Count.ToString()));
        if (manifests[^1].Label is { Length: > 0 } id) info.Summary.Add(new("Active Manifest", id));
        return info;
    }

    private static object? FirstCbor(Node n) =>
        n.Children.Where(c => c.Type == "cbor").Select(c => Cbor.Decode(c.Data)).FirstOrDefault();

    private static void AddAction(C2paInfo info, object? action, HashSet<string> seen)
    {
        var name = Text(Get(action, "action"));
        if (name == null) return;
        var sourceType = Text(Get(action, "digitalSourceType"));
        var desc = Text(Get(action, "description"));
        var agent = Get(action, "softwareAgent");
        var agentText = agent is string s ? s : Text(Get(agent, "name"));
        if (agent is not string && agentText != null && Text(Get(agent, "version")) is { } agentVer) agentText += " " + agentVer;
        var whenText = Text(Get(action, "when")) is { } w && DateTimeOffset.TryParse(w, out var when)
            ? when.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss") + " UTC" : null;

        if (sourceType != null)
        {
            var shortType = sourceType[(sourceType.LastIndexOf('/') + 1)..];
            if (shortType.Contains("AlgorithmicMedia", StringComparison.OrdinalIgnoreCase)) info.IsAiGenerated = true;
        }

        var sb = new StringBuilder(FriendlyAction(name));
        if (desc != null) sb.Append(": ").Append(desc.TrimEnd('.'));
        var extras = new List<string>();
        if (sourceType != null) extras.Add(FriendlySourceType(sourceType));
        if (agentText != null) extras.Add("by " + agentText);
        if (whenText != null) extras.Add(whenText);
        if (extras.Count > 0) sb.Append(" (").Append(string.Join(", ", extras)).Append(')');
        var text = sb.ToString();
        if (seen.Add(text)) info.Actions.Add(text);
    }

    private static string FriendlyAction(string a) => a switch
    {
        "c2pa.created" => "Created",
        "c2pa.watermarked" => "Watermarked",
        "c2pa.edited" => "Edited",
        "c2pa.opened" => "Opened",
        "c2pa.placed" => "Placed",
        "c2pa.converted" => "Converted",
        "c2pa.cropped" => "Cropped",
        "c2pa.resized" => "Resized",
        "c2pa.filtered" => "Filtered",
        "c2pa.color_adjustments" => "Color adjusted",
        "c2pa.published" => "Published",
        "c2pa.transcoded" => "Transcoded",
        "c2pa.drawing" => "Drawing",
        _ => a,
    };

    internal static string FriendlySourceType(string uri)
    {
        var t = uri[(uri.LastIndexOf('/') + 1)..];
        return t switch
        {
            "trainedAlgorithmicMedia" => "AI-generated",
            "compositeWithTrainedAlgorithmicMedia" => "composite with AI-generated media",
            "algorithmicMedia" => "algorithmically generated",
            "digitalCapture" => "digital capture",
            "digitalCreation" => "digital creation",
            "composite" => "composite",
            _ => t,
        };
    }

    private static string AssertionName(string n) => n switch
    {
        "c2pa.hash.data" => "Hash of the image data",
        "c2pa.hash.bmff" or "c2pa.hash.bmff.v2" or "c2pa.hash.bmff.v3" => "Hash of the video data",
        "c2pa.ingredient" or "c2pa.ingredient.v2" or "c2pa.ingredient.v3" => "Ingredient (source file)",
        "c2pa.thumbnail.claim.jpeg" or "c2pa.thumbnail.ingredient" => "Thumbnail",
        "c2pa.soft-binding" => "Invisible watermark",
        "c2pa.training-mining" => "Data mining / AI training permissions",
        "stds.exif" => "EXIF data",
        "stds.schema-org.CreativeWork" => "Creative work (author info)",
        _ => n,
    };

    private static string? ClaimGenerator(object claim)
    {
        var infoNode = Get(claim, "claim_generator_info");
        var first = infoNode is List<object?> l ? l.FirstOrDefault() : infoNode;
        var name = Text(Get(first, "name"));
        if (name != null)
        {
            var ver = Text(Get(first, "version"));
            return ver != null ? $"{name} {ver}" : name;
        }
        return Text(Get(claim, "claim_generator"));
    }

    private static void ReadSignature(object cose, ref X509Certificate2? signer, List<DateTime> timestamps)
    {
        // COSE_Sign1: [protected (bstr holding a CBOR map), unprotected (map), payload, signature]
        if (cose is not List<object?> arr || arr.Count < 2) return;
        var unprotected = arr[1];
        var protectedMap = arr[0] is byte[] pb && pb.Length > 0 ? Cbor.Decode(pb) : null;

        if (signer == null)
        {
            var chain = Get(unprotected, "33") ?? Get(protectedMap, "33");
            var first = chain is List<object?> cl ? cl.FirstOrDefault() : chain;
            if (first is byte[] der)
            {
                try { signer = X509CertificateLoader.LoadCertificate(der); } catch { }
            }
        }

        // The trusted timestamp is a CMS token; its genTime is the only 15-character GeneralizedTime (tag 0x18) before the certificates
        var tst = Get(Get(unprotected, "sigTst2") ?? Get(unprotected, "sigTst"), "tstTokens");
        var token = Get(Items(tst).FirstOrDefault(), "val") as byte[];
        if (token != null)
        {
            for (int i = 0; i + 17 <= token.Length; i++)
            {
                if (token[i] == 0x18 && token[i + 1] == 0x0F &&
                    DateTime.TryParseExact(Encoding.ASCII.GetString(token, i + 2, 15), "yyyyMMddHHmmss'Z'",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out var t))
                {
                    timestamps.Add(t);
                    break;
                }
            }
        }
    }

    // ---- CBOR helpers ----

    private static object? Get(object? map, string key) =>
        map is List<KeyValuePair<object, object?>> m ? m.FirstOrDefault(kv => kv.Key.ToString() == key).Value : null;

    private static IEnumerable<object?> Items(object? o) => o is List<object?> l ? l : [];

    private static string? Text(object? o) => o is string s && s.Length > 0 ? s : null;

    /// <summary>A small CBOR (RFC 8949) decoder: maps are lists of pairs, arrays are lists, tags are skipped.</summary>
    private static class Cbor
    {
        public static object? Decode(byte[] data)
        {
            int pos = 0;
            try { return Read(data, ref pos, 0); } catch { return null; }
        }

        private static object? Read(byte[] d, ref int pos, int depth)
        {
            if (depth > 32) throw new InvalidDataException();
            int ib = d[pos++];
            int major = ib >> 5, info = ib & 0x1F;

            if (major == 7)
            {
                switch (info)
                {
                    case 20: return false;
                    case 21: return true;
                    case 22: case 23: return null;
                    case 25: pos += 2; return null; // half float: not needed
                    case 26: pos += 4; return null;
                    case 27: pos += 8; return null;
                    default: return null;
                }
            }

            if (info == 31) return ReadIndefinite(d, ref pos, major, depth);

            long arg = info < 24 ? info : info switch
            {
                24 => d[pos++],
                25 => (d[pos++] << 8) | d[pos++],
                26 => ((long)d[pos++] << 24) | ((long)d[pos++] << 16) | ((long)d[pos++] << 8) | d[pos++],
                27 => ReadU64(d, ref pos),
                _ => throw new InvalidDataException("indefinite length not supported"),
            };

            switch (major)
            {
                case 0: return arg;
                case 1: return -1 - arg;
                case 2: { var b = d[pos..(pos + (int)arg)]; pos += (int)arg; return b; }
                case 3: { var s = Encoding.UTF8.GetString(d, pos, (int)arg); pos += (int)arg; return s; }
                case 4:
                    {
                        var list = new List<object?>();
                        for (long i = 0; i < arg; i++) list.Add(Read(d, ref pos, depth + 1));
                        return list;
                    }
                case 5:
                    {
                        var map = new List<KeyValuePair<object, object?>>();
                        for (long i = 0; i < arg; i++)
                        {
                            var k = Read(d, ref pos, depth + 1) ?? "";
                            map.Add(new(k, Read(d, ref pos, depth + 1)));
                        }
                        return map;
                    }
                default: return Read(d, ref pos, depth + 1); // tag: use the value inside
            }
        }

        /// <summary>Indefinite length (0x1F): items follow until a 0xFF "break" byte.</summary>
        private static object? ReadIndefinite(byte[] d, ref int pos, int major, int depth)
        {
            switch (major)
            {
                case 2:
                    {
                        var bytes = new List<byte>();
                        while (d[pos] != 0xFF) if (Read(d, ref pos, depth + 1) is byte[] chunk) bytes.AddRange(chunk);
                        pos++;
                        return bytes.ToArray();
                    }
                case 3:
                    {
                        var sb = new StringBuilder();
                        while (d[pos] != 0xFF) if (Read(d, ref pos, depth + 1) is string chunk) sb.Append(chunk);
                        pos++;
                        return sb.ToString();
                    }
                case 4:
                    {
                        var list = new List<object?>();
                        while (d[pos] != 0xFF) list.Add(Read(d, ref pos, depth + 1));
                        pos++;
                        return list;
                    }
                case 5:
                    {
                        var map = new List<KeyValuePair<object, object?>>();
                        while (d[pos] != 0xFF)
                        {
                            var k = Read(d, ref pos, depth + 1) ?? "";
                            map.Add(new(k, Read(d, ref pos, depth + 1)));
                        }
                        pos++;
                        return map;
                    }
                default: throw new InvalidDataException("indefinite length not allowed here");
            }
        }

        private static long ReadU64(byte[] d, ref int pos)
        {
            long v = 0;
            for (int i = 0; i < 8; i++) v = (v << 8) | d[pos++];
            return v;
        }
    }
}
