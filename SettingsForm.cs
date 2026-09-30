using FileInfoViewer.Services;

namespace FileInfoViewer;

public partial class SettingsForm : Form
{
    public SettingsForm()
    {
        InitializeComponent();
        // The Designer sets ClientSize before FixedDialog/ControlBox, which leaves the client area too short
        ClientSize = new Size(ClientSize.Width, grpWarnings.Bottom + LogicalToDeviceUnits(12));
        SettingsService.FixButtonStyles(this);
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
        // Number first: setting the checkbox can fire a save that would otherwise store the designer default
        numHashMaxSizeMb.Value    = Math.Clamp(settings.HashMaxSizeMb, (int)numHashMaxSizeMb.Minimum, (int)numHashMaxSizeMb.Maximum);
        chkShowFileHashes.Checked = settings.ShowFileHashes;
        UpdateHashLimitEnabled();

        var tdIndex = cboTextualData.Items.IndexOf(settings.TextualDataDisplay);
        cboTextualData.SelectedIndex = tdIndex >= 0 ? tdIndex : 1; // default: "Formatted"

        chkWebLinksClickable.Checked = settings.WebLinksClickable;
        chkWarnWrongExtension.Checked = settings.WarnWrongExtension;

        var cwIndex = cboContentWidth.Items.IndexOf(settings.ContentMaxWidth);
        cboContentWidth.SelectedIndex = cwIndex >= 0 ? cwIndex : 1; // default: "Normal (1100px)"

        txtCustomContentWidth.Text = settings.CustomContentWidth;
        optCustContWidthPx.Checked   = settings.CustomContentWidthUnit != "%";
        optCustContWidthPerc.Checked = settings.CustomContentWidthUnit == "%";
        UpdateCustomWidthVisibility();

        var styleIndex = cboStyle.Items.IndexOf(settings.AppStyle);
        cboStyle.SelectedIndex = styleIndex >= 0 ? styleIndex : cboStyle.Items.IndexOf("System");
    }

    private void UpdateCustomWidthVisibility()
    {
        bool custom = cboContentWidth.SelectedItem?.ToString() == "Custom";
        txtCustomContentWidth.Visible = custom;
        optCustContWidthPx.Visible    = custom;
        optCustContWidthPerc.Visible  = custom;
    }

    private void UpdateHashLimitEnabled()
    {
        bool on = chkShowFileHashes.Checked;
        lblHashSkip.Enabled = numHashMaxSizeMb.Enabled = lblHashMb.Enabled = lblHashHint.Enabled = on;
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
            HashMaxSizeMb        = (int)numHashMaxSizeMb.Value,
            TextualDataDisplay   = cboTextualData.SelectedItem?.ToString() ?? "Formatted",
            WebLinksClickable    = chkWebLinksClickable.Checked,
            WarnWrongExtension   = chkWarnWrongExtension.Checked,
            ContentMaxWidth          = cboContentWidth.SelectedItem?.ToString() ?? "Normal (1100px)",
            CustomContentWidth       = txtCustomContentWidth.Text.Trim(),
            CustomContentWidthUnit   = optCustContWidthPerc.Checked ? "%" : "px",
            AppStyle                 = cboStyle.SelectedItem?.ToString() ?? "System",
        });
    }

    private void chkShowSeconds_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void cboTimeZone_SelectedIndexChanged(object sender, EventArgs e) => SaveSettings();

    private void cboShowCopyButton_SelectedIndexChanged(object sender, EventArgs e) => SaveSettings();

    private void chkOwner_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void chkFileAttributes_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void chkShowFileHashes_CheckedChanged(object sender, EventArgs e)
    {
        UpdateHashLimitEnabled();
        SaveSettings();
    }

    private void numHashMaxSizeMb_ValueChanged(object sender, EventArgs e) => SaveSettings();

    private void cboTextualData_SelectedIndexChanged(object sender, EventArgs e) => SaveSettings();

    private void chkWebLinksClickable_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void chkWarnWrongExtension_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void cboContentWidth_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateCustomWidthVisibility();
        SaveSettings();
    }

    private void txtCustomContentWidth_TextChanged(object sender, EventArgs e) => SaveSettings();

    private void optCustContWidthUnit_CheckedChanged(object sender, EventArgs e) => SaveSettings();

    private void cboStyle_SelectedIndexChanged(object sender, EventArgs e)
    {
        SaveSettings();
        SettingsService.ApplyStyle(cboStyle.SelectedItem?.ToString() ?? "System");
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            ValidateChildren(); // commits a number that is still being typed
            Close();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
