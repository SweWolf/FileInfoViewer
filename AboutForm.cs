using System.Reflection;

namespace FileInfoViewer;

public partial class AboutForm : Form
{
    public AboutForm()
    {
        InitializeComponent();

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
