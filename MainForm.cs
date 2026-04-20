using FileInfoViewer.Services;

namespace FileInfoViewer;

public partial class MainForm : Form
{
    public MainForm()
    {
        // Check for updates in the background — does not block startup
        _ = Common.CheckForUpdatesAsync("SweWolf", "FileInfoViewer");

        InitializeComponent();
        AllowDrop = true;
        DragEnter += MainForm_DragEnter;
        DragDrop += MainForm_DragDrop;
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

    private void ProcessFile(string path)
    {
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
        Application.DoEvents();

        try
        {
            var model = FileInfoCollector.Collect(path);
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

    private void btnSettings_Click(object sender, EventArgs e)
    {
        using var form = new SettingsForm();
        form.ShowDialog(this);
    }

    private void btnAbout_Click(object sender, EventArgs e)
    {
        using var form = new AboutForm();
        form.ShowDialog(this);
    }

    private void btnCreateShortcut_Click(object sender, EventArgs e)
    {
        using var form = new CreateShortcutForm(Application.ExecutablePath);
        form.ShowDialog(this);
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        txtFilePath.Width = ClientSize.Width - 82 - 20 - 92;
        btnBrowse.Left   = ClientSize.Width - 20 - 80;
        lblStatus.Width  = ClientSize.Width - 82 - 20;
    }

    private void SetStatus(string message, bool error)
    {
        lblStatus.Text = message;
        lblStatus.ForeColor = error ? Color.Crimson : Color.FromArgb(0, 120, 80);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) { Application.Exit(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
