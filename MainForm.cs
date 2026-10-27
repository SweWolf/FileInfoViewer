using FileInfoViewer.Services;

namespace FileInfoViewer;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        SettingsService.FixButtonStyles(this);
        AllowDrop = true;
        DragEnter += MainForm_DragEnter;
        DragDrop += MainForm_DragDrop;

        // Check for updates in the background (does not block startup)
        Shown += (_, _) =>
        {
            if (SettingsService.Current.CheckForUpdatesOnStartup)
                _ = CheckForUpdatesAsync();
        };
    }

    private string? _updateReleasePageUrl;

    private async Task CheckForUpdatesAsync()
    {
        var currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        var result = await GitHubUpdateChecker.CheckAsync("SweWolf", "FileInfoViewer", currentVersion);

        if (result is { IsUpdateAvailable: true } && !IsDisposed)
        {
            _updateReleasePageUrl = result.ReleasePageUrl;
            menuNewVersion.Visible = true;
        }
    }

    private void menuNewVersion_Click(object? sender, EventArgs e)
    {
        if (_updateReleasePageUrl != null)
            Common.OpenWithDefaultApp(_updateReleasePageUrl);
    }

    public MainForm(string filePath) : this()
    {
        txtFilePath.Text = filePath;
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Select a file to inspect",
            Filter = "All Files (*.*)|*.*",
            CheckFileExists = true,
        };

        if (!string.IsNullOrWhiteSpace(txtFilePath.Text) &&
            Directory.Exists(Path.GetDirectoryName(txtFilePath.Text)))
        {
            dlg.InitialDirectory = Path.GetDirectoryName(txtFilePath.Text);
        }

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = dlg.FileName;
            ProcessFile(dlg.FileName);
        }
    }

    private void btnView_Click(object sender, EventArgs e)
    {
        ProcessFile(txtFilePath.Text.Trim());
    }

    private void txtFilePath_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            ProcessFile(txtFilePath.Text.Trim());
        }
    }

    private void MainForm_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
    }

    private void MainForm_DragDrop(object? sender, DragEventArgs e)
    {
        var files = (string[]?)e.Data?.GetData(DataFormats.FileDrop);
        if (files?.Length > 0)
        {
            txtFilePath.Text = files[0];
            ProcessFile(files[0]);
        }
    }

    /// <summary>Removes the quotes that "Copy as path" in File Explorer puts around a path, and any spaces outside them.</summary>
    private static string CleanPath(string text) => text.Trim().Trim('"').Trim();

    private async void ProcessFile(string path)
    {
        path = CleanPath(path);
        if (txtFilePath.Text != path) txtFilePath.Text = path;

        if (string.IsNullOrWhiteSpace(path))
        {
            SetStatus("Please enter or browse to a file path.", error: true);
            return;
        }

        if (!File.Exists(path))
        {
            SetStatus($"File not found: {path}", error: true);
            return;
        }

        SetStatus("Collecting file information...", error: false);
        btnView.Enabled = false;

        try
        {
            var model = await CollectRunner.RunAsync(path, this);
            var htmlPath = HtmlReportGenerator.Generate(model);
            BrowserLauncher.OpenInBrowser(htmlPath);
            SetStatus($"Report opened in browser. ({model.Warnings.Count} warning(s))", error: false);
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", error: true);
        }
        finally
        {
            btnView.Enabled = true;
        }
    }

    private void menuSettings_Click(object? sender, EventArgs e)
    {
        using var form = new SettingsForm();
        form.ShowDialog(this);
    }

    private void aboutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using var form = new AboutForm();
        form.ShowDialog(this);
    }

    private void menuViewHelp_Click(object? sender, EventArgs e) => HelpFiles.OpenHelp(Common.AppName);

    private void btnCreateShortcut_Click(object sender, EventArgs e)
    {
        using var form = new CreateShortcutForm(Application.ExecutablePath);
        form.ShowDialog(this);
    }

    private void SetStatus(string message, bool error)
    {
        lblStatus.Text = message;
        lblStatus.ForeColor = error ? Color.Crimson : Color.FromArgb(0, 120, 80);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Escape:
                Application.Exit();
                return true;
            case Keys.F6:
                txtFilePath.Focus();
                txtFilePath.SelectAll();
                return true;
            case Keys.Alt | Keys.B:
                if (btnBrowse.Enabled) btnBrowse.PerformClick();
                return true;
            case Keys.Control | Keys.E:
                if (btnView.Enabled) btnView.PerformClick();
                return true;
            case Keys.Control | Keys.V:
                // A file copied in File Explorer is used like a dropped file; text is pasted as usual
                try
                {
                    if (Clipboard.ContainsFileDropList() && Clipboard.GetFileDropList() is { Count: > 0 } list)
                    {
                        txtFilePath.Text = list[0]!;
                        ProcessFile(list[0]!);
                        return true;
                    }
                }
                catch (System.Runtime.InteropServices.ExternalException) { } // clipboard busy: paste as usual
                break;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
