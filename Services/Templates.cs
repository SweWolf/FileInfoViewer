namespace FileInfoViewer.Services;

/// <summary>One value a template shows. <see cref="Value"/> is null when it was not found, empty when it is set but empty.</summary>
internal sealed record TemplateField(string Label, string? Value, bool MultiLine);

/// <summary>What a template found in a file: the fields to show, or an error message when the file can't be used.</summary>
internal sealed record TemplateResult(IReadOnlyList<TemplateField> Fields, string? Error = null)
{
    public static TemplateResult Fail(string error) => new([], error);
}

/// <summary>A named template that picks a few values out of a file (started with -T&lt;Number&gt; on the command line).</summary>
internal sealed record Template(int Number, string Name, Func<string, TemplateResult> Read);

internal static class Templates
{
    public static readonly IReadOnlyList<Template> All =
    [
        new(1, "Prompt and Seed", ComfyPromptReader.Read),
    ];

    public static Template? Get(int number) => All.FirstOrDefault(t => t.Number == number);
}
