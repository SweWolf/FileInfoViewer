using FileInfoViewer.Services;

namespace FileInfoViewer;

/// <summary>Small window that shows the values of a template, each with a Copy button.</summary>
internal sealed class TemplateForm : Form
{
    // Segoe MDL2 Assets glyphs: "Copy" (two overlapping pages) and "CheckMark", shown for a moment after copying
    private const string CopyGlyph = "";
    private const string CopiedGlyph = "";

    private readonly System.Windows.Forms.Timer _resetTimer = new() { Interval = 1200 };
    private readonly ToolTip _toolTip = new();
    private Button? _copiedButton;

    public TemplateForm(string templateName, string filePath, IReadOnlyList<TemplateField> fields)
    {
        Text = $"File Info Viewer - {templateName} - {Path.GetFileName(filePath)}";
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterScreen;
        Padding = new Padding(12);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        // Two columns: the value fills the width, the Copy button keeps its size. Each field is a label row and a value row.
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var row = 0;
        Button? firstCopy = null;
        foreach (var field in fields)
        {
            var label = new Label { Text = field.Label, AutoSize = true, Margin = new Padding(0, 4, 0, 2) };
            layout.Controls.Add(label, 0, row);
            layout.SetColumnSpan(label, 2);
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            row++;

            var found = !string.IsNullOrEmpty(field.Value);
            var box = new TextBox
            {
                Text = field.Value ?? "",
                PlaceholderText = field.Value == null ? "(not found)" : "(empty)",
                ReadOnly = true,
                BackColor = SystemColors.Window, // a read-only TextBox is gray by default; white is easier to read
                BorderStyle = BorderStyle.FixedSingle, // flat, like the file path box in the main window
                Multiline = field.MultiLine,
                ScrollBars = field.MultiLine ? ScrollBars.Vertical : ScrollBars.None,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 4),
                AccessibleName = field.Label,
            };
            var copy = new Button
            {
                Text = CopyGlyph,
                Font = new Font("Segoe MDL2 Assets", 11F),
                Size = new Size(28, 28), // square: the glyph is small; scaled with the font like the rest
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Margin = new Padding(0, 0, 0, 4),
                Enabled = found,
                AccessibleName = $"Copy {field.Label}",
            };
            _toolTip.SetToolTip(copy, $"Copy {field.Label}");
            copy.Click += (_, _) => CopyToClipboard(field.Value!, copy);
            if (found) firstCopy ??= copy;

            layout.Controls.Add(box, 0, row);
            layout.Controls.Add(copy, 1, row);
            // The long texts share the free height; the seed keeps one line
            layout.RowStyles.Add(field.MultiLine ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.AutoSize));
            row++;
        }

        // Percent rows of the multi-line fields: give the prompt more room than the negative prompt
        var multiLineRows = layout.RowStyles.Cast<RowStyle>().Where(r => r.SizeType == SizeType.Percent).ToList();
        if (multiLineRows.Count > 1)
        {
            multiLineRows[0].Height = 60;
            foreach (var r in multiLineRows.Skip(1)) r.Height = 40 / (multiLineRows.Count - 1);
        }

        Controls.Add(layout);
        SettingsService.FixButtonStyles(this);

        _resetTimer.Tick += (_, _) =>
        {
            _resetTimer.Stop();
            if (_copiedButton != null) _copiedButton.Text = CopyGlyph;
            _copiedButton = null;
        };

        ActiveControl = firstCopy;

        ClientSize = new Size(LogicalToDeviceUnits(640), LogicalToDeviceUnits(360));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        MinimumSize = Size; // the designed size is the minimum, after DPI/font scaling
    }

    private void CopyToClipboard(string text, Button button)
    {
        try
        {
            Clipboard.SetText(text);
        }
        catch (Exception ex)
        {
            // The clipboard can be locked by another program for a moment
            MessageBox.Show(this, $"The text could not be copied:\n\n{ex.Message}", "File Info Viewer",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_copiedButton != null && _copiedButton != button) _copiedButton.Text = CopyGlyph;
        _copiedButton = button;
        button.Text = CopiedGlyph;
        _resetTimer.Stop();
        _resetTimer.Start();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) { Close(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _resetTimer.Dispose(); _toolTip.Dispose(); }
        base.Dispose(disposing);
    }
}
