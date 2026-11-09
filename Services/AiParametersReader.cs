using System.Text.Json;
using System.Text.RegularExpressions;

namespace FileInfoViewer.Services;

/// <summary>
/// The "parameters" text chunk of a PNG from Fooocus with "Save Metadata to Images" on and the scheme "fooocus": one JSON object
/// (a second chunk, "fooocus_scheme", says "fooocus"). Keys used: prompt, negative_prompt, seed (a number or a text, kept as text:
/// it can have 19 digits); the rest (models, sampler, styles, the expanded prompts ...) is not shown. Text that is not a JSON
/// object gives null, so the Automatic1111 reader gets its turn (Fooocus' "a1111" scheme).
/// Copied from SweWolf File Explorer's Helpers/AiInfoReader.cs; the apps share no code, so a fix in one may belong in the other.
/// When copying: change the namespace and the result type (here <see cref="TemplateResult"/>).
/// History:
///   2026-11-09  First version (SweWolf File Explorer); copied into File Info Viewer.
/// </summary>
internal static class FooocusReader
{
    public static TemplateResult? Parse(string text)
    {
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed[0] != '{') return null;
        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return null;

            string? Text(string name)
            {
                if (!root.TryGetProperty(name, out var v)) return null;
                var s = v.ValueKind switch
                {
                    JsonValueKind.String => v.GetString(),
                    JsonValueKind.Number => v.GetRawText(),
                    _ => null,
                };
                return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
            }

            return AiParameters.Result(Text("prompt"), Text("negative_prompt"), Text("seed"));
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>
/// The "parameters" text chunk that Automatic1111's web UI (and Forge, SD.Next, Fooocus' "a1111" scheme ...) writes into a PNG:
///   the prompt (any number of lines)
///   Negative prompt: the negative prompt (any number of lines)
///   Steps: 20, Sampler: Euler a, CFG scale: 7, Seed: 1234, Size: 512x512, Model hash: ...
/// The settings line is the last line that starts with "Steps: "; the seed is read from it.
/// Copied from SweWolf File Explorer's Helpers/AiInfoReader.cs; when copying: change the namespace and the result type.
/// History:
///   2026-10-24  First version (SweWolf File Explorer).
///   2026-11-09  Fooocus: "Raw prompt" / "Raw negative prompt" of the settings line replace the expanded prompts.
///   2026-11-09  Copied into File Info Viewer.
/// </summary>
internal static class Automatic1111Reader
{
    private static readonly Regex SeedPattern = new(@"(?:^|,\s*)Seed:\s*(\d+)", RegexOptions.Compiled);

    public static TemplateResult? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var lines = text.Replace("\r\n", "\n").Split('\n');

        // The settings line: the last one that starts with "Steps: " (a prompt may mention "Steps:" higher up).
        int settings = -1;
        for (int i = lines.Length - 1; i >= 0; i--)
            if (lines[i].StartsWith("Steps: ", StringComparison.Ordinal)) { settings = i; break; }

        int bodyEnd = settings >= 0 ? settings : lines.Length;
        int negativeAt = -1;
        for (int i = 0; i < bodyEnd; i++)
            if (lines[i].StartsWith("Negative prompt:", StringComparison.Ordinal)) { negativeAt = i; break; }

        string Join(int from, int to) => string.Join("\n", lines.Skip(from).Take(to - from)).Trim();

        string prompt = Join(0, negativeAt >= 0 ? negativeAt : bodyEnd);
        string? negative = null;
        if (negativeAt >= 0)
        {
            var first = lines[negativeAt]["Negative prompt:".Length..].TrimStart();
            negative = (first + "\n" + Join(negativeAt + 1, bodyEnd)).Trim();
        }

        string? seed = null;
        if (settings >= 0 && SeedPattern.Match(lines[settings]) is { Success: true } m) seed = m.Groups[1].Value;

        // Fooocus' "a1111" scheme writes the prompt after its expansion (styles and extra words added) and keeps what the
        // user typed in the settings line as: Raw prompt: "...", Raw negative prompt: "..." (JSON-quoted text).
        if (settings >= 0)
        {
            prompt = RawValue(lines[settings], "Raw prompt") ?? prompt;
            negative = RawValue(lines[settings], "Raw negative prompt") ?? negative;
        }

        return AiParameters.Result(prompt, negative, seed);
    }

    /// <summary>The quoted value of <paramref name="key"/> in an A1111 settings line (a JSON string, so \" and \\ are understood), or null.</summary>
    private static string? RawValue(string settingsLine, string key)
    {
        var m = Regex.Match(settingsLine, "(?:^|,\\s*)" + Regex.Escape(key) + ":\\s*(\"(?:[^\"\\\\]|\\\\.)*\")");
        if (!m.Success) return null;
        try { return JsonSerializer.Deserialize<string>(m.Groups[1].Value)?.Trim() is { Length: > 0 } s ? s : null; }
        catch (JsonException) { return null; }
    }
}

internal static class AiParameters
{
    /// <summary>The template fields for a prompt, negative prompt and seed; null when nothing was found.</summary>
    public static TemplateResult? Result(string? prompt, string? negative, string? seed)
    {
        if (string.IsNullOrEmpty(prompt)) prompt = null;
        if (string.IsNullOrEmpty(negative)) negative = null;
        if (prompt == null && negative == null && seed == null) return null;
        return new TemplateResult(
        [
            new("Prompt", prompt, MultiLine: true),
            new("Negative Prompt", negative, MultiLine: true),
            new("Seed", seed, MultiLine: false),
        ]);
    }
}
