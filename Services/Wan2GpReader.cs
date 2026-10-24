using System.Text;
using System.Text.Json;

namespace FileInfoViewer.Services;

/// <summary>
/// Template 1 for MP4/MOV files made by Wan2GP (WanGP): the generation settings are stored as JSON in the
/// iTunes-style "comment" atom (moov/udta/meta/ilst/©cmt). The JSON has the keys prompt, negative_prompt
/// (only for models that have one, MiniMax H3 doesn't) and seed.
/// </summary>
internal static class Wan2GpReader
{
    private const long MaxMoovSize = 64 * 1024 * 1024;

    public static TemplateResult Read(string filePath)
    {
        string? json;
        try { json = ReadCommentAtom(filePath); }
        catch (Exception ex) { return TemplateResult.Fail($"The file could not be read: {ex.Message}"); }

        if (string.IsNullOrWhiteSpace(json))
            return TemplateResult.Fail("No Wan2GP generation data was found in this file.");

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return TemplateResult.Fail("No Wan2GP generation data was found in this file.");

            return new TemplateResult(
            [
                new("Prompt", GetValue(root, "prompt"), MultiLine: true),
                new("Negative Prompt", GetValue(root, "negative_prompt"), MultiLine: true),
                new("Seed", GetValue(root, "seed"), MultiLine: false),
            ]);
        }
        catch (JsonException)
        {
            // The comment is some other tool's text, not Wan2GP's JSON
            return TemplateResult.Fail("No Wan2GP generation data was found in this file.");
        }
    }

    private static string? GetValue(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value)) return null;
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null,
        };
    }

    /// <summary>Returns the text of the ©cmt atom, or null when the file has none.</summary>
    private static string? ReadCommentAtom(string filePath)
    {
        using var fs = File.OpenRead(filePath);
        var header = new byte[16];

        // Top-level boxes: skip everything (the huge mdat in particular) except moov
        while (fs.Position + 8 <= fs.Length)
        {
            var start = fs.Position;
            fs.ReadExactly(header, 0, 8);
            long size = ReadUInt32(header, 0);
            var type = Encoding.Latin1.GetString(header, 4, 4);
            var headerSize = 8;
            if (size == 1)
            {
                fs.ReadExactly(header, 8, 8);
                size = (long)(((ulong)ReadUInt32(header, 8) << 32) | ReadUInt32(header, 12));
                headerSize = 16;
            }
            else if (size == 0)
            {
                size = fs.Length - start;
            }
            if (size < headerSize) return null;

            if (type == "moov")
            {
                var bodySize = size - headerSize;
                if (bodySize > MaxMoovSize || start + size > fs.Length) return null;
                var body = new byte[bodySize];
                fs.ReadExactly(body);
                return FindComment(body, 0, body.Length, 0);
            }
            fs.Position = start + size;
        }
        return null;
    }

    private static string? FindComment(byte[] b, int start, int end, int depth)
    {
        if (depth > 6) return null;
        var p = start;
        while (p + 8 <= end)
        {
            long size = ReadUInt32(b, p);
            if (size < 8 || p + size > end) return null;
            var type = Encoding.Latin1.GetString(b, p + 4, 4);
            var bodyStart = p + 8;
            var bodyEnd = (int)(p + size);

            switch (type)
            {
                case "udta" or "ilst":
                    if (FindComment(b, bodyStart, bodyEnd, depth + 1) is { } inner) return inner;
                    break;
                case "meta":
                    // meta is a "full box": 4 bytes of version and flags come before its children
                    if (FindComment(b, bodyStart + 4, bodyEnd, depth + 1) is { } meta) return meta;
                    break;
                case "©cmt":
                    return ReadDataAtom(b, bodyStart, bodyEnd);
            }
            p = bodyEnd;
        }
        return null;
    }

    /// <summary>The "data" child of an ilst item: size, "data", type (4), locale (4), then the value.</summary>
    private static string? ReadDataAtom(byte[] b, int start, int end)
    {
        if (start + 16 > end || Encoding.Latin1.GetString(b, start + 4, 4) != "data") return null;
        var valueStart = start + 16;
        var valueEnd = (int)Math.Min(end, start + ReadUInt32(b, start));
        return valueEnd > valueStart ? Encoding.UTF8.GetString(b, valueStart, valueEnd - valueStart) : null;
    }

    private static uint ReadUInt32(byte[] b, int offset) =>
        (uint)((b[offset] << 24) | (b[offset + 1] << 16) | (b[offset + 2] << 8) | b[offset + 3]);
}
