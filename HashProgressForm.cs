using FileInfoViewer.Services;

namespace FileInfoViewer;

/// <summary>Small modeless window with a progress bar and a Cancel button, shown while a large file is hashed.</summary>
public sealed class HashProgressForm : Form
{
    private readonly ProgressBar _bar = new() { Dock = DockStyle.Top, Minimum = 0, Maximum = 1000, Height = 22 };
    private readonly Label _lblFile = new() { AutoSize = true, MaximumSize = new Size(420, 0), Margin = new Padding(0, 0, 0, 8) };
    private readonly Label _lblStatus = new() { AutoSize = true, Margin = new Padding(0, 6, 0, 0), Text = "Starting..." };
    private readonly Button _btnCancel = new() { Text = "Cancel", AutoSize = true, MinimumSize = new Size(88, 28), Anchor = AnchorStyles.Right };
    private bool _finished;

    /// <summary>Raised when the user clicks Cancel, presses Esc or closes the window.</summary>
    public event EventHandler? CancelRequested;

    public HashProgressForm(string fileName)
    {
        Text = "File Info Viewer";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.Manual; // centred in OnShown
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(16);
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        _lblFile.Text = $"Calculating hashes (MD5 and SHA-256) for:\r\n{fileName}";
        _btnCancel.Click += (_, _) => RequestCancel();
        CancelButton = _btnCancel;

        var layout = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Dock = DockStyle.Fill };
        _bar.Width = 420;
        _bar.Margin = new Padding(0);
        layout.Controls.Add(_lblFile);
        layout.Controls.Add(_bar);
        layout.Controls.Add(_lblStatus);
        _btnCancel.Margin = new Padding(0, 12, 0, 0);
        layout.Controls.Add(_btnCancel);
        Controls.Add(layout);

        SettingsService.FixButtonStyles(this);
    }

    public void SetProgress(long done, long total)
    {
        if (total <= 0) return;
        _bar.Value = (int)Math.Min(_bar.Maximum, done * _bar.Maximum / total);
        _lblStatus.Text = $"{done * 100 / total}%  ({FileInfoCollector.FormatSize(done)} of {FileInfoCollector.FormatSize(total)})";
    }

    /// <summary>Closes the window without treating it as a cancel.</summary>
    public void Finish()
    {
        _finished = true;
        Close();
    }

    private void RequestCancel()
    {
        _btnCancel.Enabled = false;
        _lblStatus.Text = "Cancelling...";
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // CenterParent is computed before the auto-sized window has its final size: centre it now instead
        var area = Owner is { WindowState: not FormWindowState.Minimized } owner
            ? owner.Bounds
            : Screen.FromControl(this).WorkingArea;
        var target = new Point(area.Left + (area.Width - Width) / 2, area.Top + (area.Height - Height) / 2);
        var work = Screen.FromRectangle(area).WorkingArea;
        Location = new Point(Math.Clamp(target.X, work.Left, Math.Max(work.Left, work.Right - Width)),
                             Math.Clamp(target.Y, work.Top, Math.Max(work.Top, work.Bottom - Height)));
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_finished && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true; // closing the window means cancel; the caller closes it when hashing has stopped
            if (_btnCancel.Enabled) RequestCancel();
        }
        base.OnFormClosing(e);
    }
}
