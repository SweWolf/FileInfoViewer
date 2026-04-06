using FileInfoViewer.Services;

namespace FileInfoViewer;

public partial class SettingsForm : Form
{
    public SettingsForm()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = SettingsService.Current;
        chkShowSeconds.Checked = settings.ShowSeconds;

        var tzIndex = cboTimeZone.Items.IndexOf(settings.TimeZoneDisplay);
        cboTimeZone.SelectedIndex = tzIndex >= 0 ? tzIndex : 0;

        var copyIndex = cboShowCopyButton.Items.IndexOf(settings.CopyButtonDisplay);
        cboShowCopyButton.SelectedIndex = copyIndex >= 0 ? copyIndex : 0;

        chkOwner.Checked          = settings.ShowOwner;
        chkFileAttributes.Checked = settings.ShowFileAttributes;
        chkShowFileHashes.Checked = settings.ShowFileHashes;
    }

    private void SaveSettings()
    {
        SettingsService.Save(new Models.AppSettings
        {
            ShowSeconds       = chkShowSeconds.Checked,
            TimeZoneDisplay   = cboTimeZone.SelectedItem?.ToString() ?? "Local",
            CopyButtonDisplay = cboShowCopyButton.SelectedItem?.ToString() ?? "No",
            ShowOwner         = chkOwner.Checked,
            ShowFileAttributes = chkFileAttributes.Checked,
            ShowFileHashes    = chkShowFileHashes.Checked,
        });
    }

    private void chkShowSeconds_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void cboTimeZone_SelectedIndexChanged(object sender, EventArgs e) => SaveSettings();

    private void cboShowCopyButton_SelectedIndexChanged(object sender, EventArgs e) => SaveSettings();

    private void chkOwner_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void chkFileAttributes_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void chkShowFileHashes_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) { Close(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
