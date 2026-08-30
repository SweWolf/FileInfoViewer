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

        var tdIndex = cboTextualData.Items.IndexOf(settings.TextualDataDisplay);
        cboTextualData.SelectedIndex = tdIndex >= 0 ? tdIndex : 1; // default: "Formatted"

        chkWebLinksClickable.Checked = settings.WebLinksClickable;

        var cwIndex = cboContentWidth.Items.IndexOf(settings.ContentMaxWidth);
        cboContentWidth.SelectedIndex = cwIndex >= 0 ? cwIndex : 1; // default: "Normal (1100px)"

        txtCustomContentWidth.Text = settings.CustomContentWidth;
        optCustContWidthPx.Checked   = settings.CustomContentWidthUnit != "%";
        optCustContWidthPerc.Checked = settings.CustomContentWidthUnit == "%";
        UpdateCustomWidthVisibility();
    }

    private void UpdateCustomWidthVisibility()
    {
        bool custom = cboContentWidth.SelectedItem?.ToString() == "Custom";
        txtCustomContentWidth.Visible = custom;
        optCustContWidthPx.Visible    = custom;
        optCustContWidthPerc.Visible  = custom;
    }

    private void SaveSettings()
    {
        SettingsService.Save(new Models.AppSettings
        {
            ShowSeconds          = chkShowSeconds.Checked,
            TimeZoneDisplay      = cboTimeZone.SelectedItem?.ToString() ?? "Local",
            CopyButtonDisplay    = cboShowCopyButton.SelectedItem?.ToString() ?? "No",
            ShowOwner            = chkOwner.Checked,
            ShowFileAttributes   = chkFileAttributes.Checked,
            ShowFileHashes       = chkShowFileHashes.Checked,
            TextualDataDisplay   = cboTextualData.SelectedItem?.ToString() ?? "Formatted",
            WebLinksClickable    = chkWebLinksClickable.Checked,
            ContentMaxWidth          = cboContentWidth.SelectedItem?.ToString() ?? "Normal (1100px)",
            CustomContentWidth       = txtCustomContentWidth.Text.Trim(),
            CustomContentWidthUnit   = optCustContWidthPerc.Checked ? "%" : "px",
        });
    }

    private void chkShowSeconds_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void cboTimeZone_SelectedIndexChanged(object sender, EventArgs e) => SaveSettings();

    private void cboShowCopyButton_SelectedIndexChanged(object sender, EventArgs e) => SaveSettings();

    private void chkOwner_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void chkFileAttributes_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void chkShowFileHashes_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void cboTextualData_SelectedIndexChanged(object sender, EventArgs e) => SaveSettings();

    private void chkWebLinksClickable_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void cboContentWidth_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateCustomWidthVisibility();
        SaveSettings();
    }

    private void txtCustomContentWidth_TextChanged(object sender, EventArgs e) => SaveSettings();

    private void optCustContWidthUnit_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) { Close(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
