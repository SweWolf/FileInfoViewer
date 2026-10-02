using System.Text.Json;

namespace FileInfoViewer.Services;

/// <summary>
/// Template 1: reads the prompt, negative prompt and seed that ComfyUI stores in a PNG file.
/// The values come from the PNG text chunk "prompt" (node id to {class_type, inputs}). Node ids and
/// class names differ between workflows, so nothing is looked up by id: the sampler node is found by
/// its seed input and its positive/negative links are followed to the text node.
/// </summary>
internal static class ComfyPromptReader
{
    private const int MaxDepth = 8;

    // Input names that can hold the text, tried in this order (Text Encode, CLIPTextEncode, string/primitive nodes)
    private static readonly string[] TextKeys = ["prompt", "text", "string", "value"];
    private static readonly string[] SeedKeys = ["seed", "noise_seed", "value"];

    public static TemplateResult Read(string filePath)
    {
        if (!string.Equals(Path.GetExtension(filePath), ".png", StringComparison.OrdinalIgnoreCase))
            return TemplateResult.Fail("This template reads ComfyUI data from PNG files only.");

        var chunks = new Dictionary<string, string>();
        try { FileInfoCollector.ReadPngTextChunks(filePath, chunks); }
        catch (Exception ex) { return TemplateResult.Fail($"The file could not be read: {ex.Message}"); }

        if (!chunks.TryGetValue("prompt", out var json))
            return TemplateResult.Fail("No ComfyUI prompt data was found in this file.");

        try
        {
            using var doc = JsonDocument.Parse(json);
            return Extract(doc.RootElement);
        }
        catch (JsonException)
        {
            return TemplateResult.Fail("The ComfyUI prompt data in this file is not valid JSON.");
        }
    }

    private static TemplateResult Extract(JsonElement root)
    {
        string? prompt = null, negative = null, seed = null;

        if (root.ValueKind == JsonValueKind.Object && FindSampler(root) is { } sampler)
        {
            if (ConditioningNode(root, sampler, negative: false) is { } pos) prompt = ReadText(root, pos, false, 0);
            if (ConditioningNode(root, sampler, negative: true) is { } neg) negative = ReadText(root, neg, true, 0);
            seed = ResolveScalar(root, sampler, SeedKeys, 0);
            if (seed == null && sampler.TryGetProperty("noise", out var noiseLink)
                && TryFollow(root, noiseLink, out var noiseNode) && TryInputs(noiseNode, out var noiseInputs))
                seed = ResolveScalar(root, noiseInputs, SeedKeys, 1);
        }

        return new TemplateResult(
        [
            new("Prompt", prompt, MultiLine: true),
            new("Negative Prompt", negative, MultiLine: true),
            new("Seed", seed, MultiLine: false),
        ]);
    }

    /// <summary>Finds the inputs of the sampler node: the first one with a seed and prompt links, else the first with any seed.</summary>
    private static JsonElement? FindSampler(JsonElement root)
    {
        JsonElement? withSeedOnly = null;
        foreach (var node in root.EnumerateObject())
        {
            if (!TryInputs(node.Value, out var inputs)) continue;
            if (!inputs.TryGetProperty("seed", out _) && !inputs.TryGetProperty("noise_seed", out _)
                && !inputs.TryGetProperty("noise", out _)) continue;
            if (inputs.TryGetProperty("positive", out _) || inputs.TryGetProperty("guider", out _)) return inputs;
            withSeedOnly ??= inputs;
        }
        return withSeedOnly;
    }

    /// <summary>Follows the sampler's positive/negative link (or, for guider-based samplers, the guider's) to the conditioning node.</summary>
    private static JsonElement? ConditioningNode(JsonElement root, JsonElement samplerInputs, bool negative)
    {
        string[] keys = negative ? ["negative"] : ["positive", "conditioning"];

        if (samplerInputs.TryGetProperty(negative ? "negative" : "positive", out var link) && TryFollow(root, link, out var node))
            return node;

        if (samplerInputs.TryGetProperty("guider", out var guiderLink) && TryFollow(root, guiderLink, out var guider)
            && TryInputs(guider, out var guiderInputs))
        {
            foreach (var key in keys)
                if (guiderInputs.TryGetProperty(key, out var l) && TryFollow(root, l, out var n))
                    return n;
        }
        return null;
    }

    private static string? ReadText(JsonElement root, JsonElement node, bool negative, int depth)
    {
        if (depth > MaxDepth || !TryInputs(node, out var inputs)) return null;

        // "Zero out" is how a workflow says "no negative prompt" without typing an empty one
        if (node.TryGetProperty("class_type", out var classType) && classType.GetString() == "ConditioningZeroOut")
            return "";

        // A node that holds both texts (Text Encode Qwen Image): which one is wanted depends on the role of the link
        if (inputs.TryGetProperty("negative_prompt", out _))
            return ResolveScalar(root, inputs, [negative ? "negative_prompt" : "prompt"], depth);

        var text = ResolveScalar(root, inputs, TextKeys, depth);
        if (text != null) return text;

        // Pass-through nodes (e.g. FluxGuidance) keep the text one step further back
        if (inputs.TryGetProperty("conditioning", out var link) && TryFollow(root, link, out var next))
            return ReadText(root, next, negative, depth + 1);
        return null;
    }

    /// <summary>Returns the first of <paramref name="keys"/> that has a text or number value, following links to string/primitive nodes.</summary>
    private static string? ResolveScalar(JsonElement root, JsonElement inputs, string[] keys, int depth)
    {
        foreach (var key in keys)
        {
            if (!inputs.TryGetProperty(key, out var value)) continue;
            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    return value.GetString();
                case JsonValueKind.Number:
                    return value.GetRawText();
                case JsonValueKind.Array when depth < MaxDepth && TryFollow(root, value, out var target) && TryInputs(target, out var targetInputs):
                    var resolved = ResolveScalar(root, targetInputs, keys, depth + 1);
                    if (resolved != null) return resolved;
                    break;
            }
        }
        return null;
    }

    /// <summary>A link in the prompt JSON is ["node id", output index].</summary>
    private static bool TryFollow(JsonElement root, JsonElement link, out JsonElement node)
    {
        node = default;
        if (link.ValueKind != JsonValueKind.Array || link.GetArrayLength() != 2) return false;
        var idElement = link[0];
        var id = idElement.ValueKind == JsonValueKind.String ? idElement.GetString() : idElement.GetRawText();
        return id != null && root.TryGetProperty(id, out node) && node.ValueKind == JsonValueKind.Object;
    }

    private static bool TryInputs(JsonElement node, out JsonElement inputs)
    {
        inputs = default;
        return node.ValueKind == JsonValueKind.Object
            && node.TryGetProperty("inputs", out inputs) && inputs.ValueKind == JsonValueKind.Object;
    }
}
