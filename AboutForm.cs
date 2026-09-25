using System.Reflection;

namespace FileInfoViewer;

public partial class AboutForm : Form
{
    public AboutForm()
    {
        InitializeComponent();
        Services.SettingsService.FixButtonStyles(this);

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        lblVersion.Text = version != null
            ? $"Version {version.Major}.{version.Minor}.{version.Build}"
            : "Version 1.0.0";

        try
        {
            var stream = System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("FileInfoViewer.Resources.FileInfoViewer.png");
            if (stream != null)
                picIcon.Image = Image.FromStream(stream);
        }
        catch { }

        Shown += AboutForm_Shown;
    }

    private async void AboutForm_Shown(object? sender, EventArgs e)
    {
        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        var result = await GitHubUpdateChecker.CheckAsync("SweWolf", "FileInfoViewer", currentVersion);

        if (result == null || IsDisposed) return; // network error or form already closed

        if (result.IsUpdateAvailable)
        {
            lblUpdateStatus.Text = $"↑ Version {result.LatestVersion} available";
            lblUpdateStatus.ForeColor = Color.FromArgb(255, 210, 80); // warm yellow
        }
        else
        {
            lblUpdateStatus.Text = "✓ This is the latest version";
            lblUpdateStatus.ForeColor = Color.FromArgb(120, 210, 120); // light green
        }
    }

    private void btnClose_Click(object sender, EventArgs e) => Close();

    private void lnkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://github.com/SweWolf/FileInfoViewer",
            UseShellExecute = true,
        });
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) { Close(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
