namespace FileInfoViewer.Services;

/// <summary>Reads a file with a template and shows the result window, or a message when that isn't possible.</summary>
internal static class TemplateRunner
{
    private const string Caption = "File Info Viewer";

    /// <param name="owner">The window to show the template window on top of (null: run it as the application's own window).</param>
    public static void Show(int templateNumber, string filePath, Form? owner)
    {
        var template = Templates.Get(templateNumber);
        if (template == null)
        {
            MessageBox.Show(owner, $"Template {templateNumber} does not exist.", Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!File.Exists(filePath))
        {
            MessageBox.Show(owner, $"The file \"{filePath}\" does not exist.", Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = template.Read(filePath);
        if (result.Error != null)
        {
            MessageBox.Show(owner, result.Error, Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new TemplateForm(template.Name, filePath, result.Fields);
        if (owner != null) form.ShowDialog(owner);
        else Application.Run(form);
    }
}
