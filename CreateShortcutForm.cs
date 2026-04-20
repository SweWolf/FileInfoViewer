using System.Windows.Forms;

namespace FileInfoViewer;

public partial class CreateShortcutForm : Form
{
    private readonly string _exePath;
    private readonly string _appName;

    public CreateShortcutForm(string exePath)
    {
        InitializeComponent();
        _exePath = exePath;
        _appName = Application.ProductName ?? System.IO.Path.GetFileNameWithoutExtension(exePath);
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        if (!chkDesktop.Checked && !chkStartMenu.Checked && !chkSendTo.Checked)
        {
            MessageBox.Show(
                "Please select at least one location.",
                "No location selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        bool allUsers = rdoAllUsers.Checked;
        var created = new List<string>();
        var failed  = new List<string>();

        if (chkDesktop.Checked)
        {
            string folder = allUsers
                ? Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
                : Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            TryCreate(System.IO.Path.Combine(folder, $"{_appName}.lnk"), created, failed);
        }

        if (chkStartMenu.Checked)
        {
            string baseFolder = allUsers
                ? Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
                : Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            string programsFolder = System.IO.Path.Combine(baseFolder, "Programs");
            System.IO.Directory.CreateDirectory(programsFolder);
            TryCreate(System.IO.Path.Combine(programsFolder, $"{_appName}.lnk"), created, failed);
        }

        if (chkSendTo.Checked)
        {
            // Send To is always per-user — no "all users" equivalent in Windows
            string sendToFolder = Environment.GetFolderPath(Environment.SpecialFolder.SendTo);
            TryCreate(System.IO.Path.Combine(sendToFolder, $"{_appName}.lnk"), created, failed);
        }

        if (failed.Count > 0)
        {
            string msg = "Could not create the following shortcut(s):\n\n"
                       + string.Join("\n", failed);
            if (allUsers)
                msg += "\n\nCreating shortcuts for all users requires administrator privileges.";
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        if (created.Count > 0)
        {
            MessageBox.Show(
                "Shortcut(s) created successfully:\n\n" + string.Join("\n", created),
                "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void TryCreate(string shortcutPath, List<string> created, List<string> failed)
    {
        try
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell")!;
            dynamic shell  = Activator.CreateInstance(shellType)!;
            dynamic lnk    = shell.CreateShortcut(shortcutPath);
            lnk.TargetPath       = _exePath;
            lnk.WorkingDirectory = System.IO.Path.GetDirectoryName(_exePath);
            lnk.Description      = _appName;
            lnk.IconLocation     = _exePath;
            lnk.Save();
            created.Add(shortcutPath);
        }
        catch (Exception ex)
        {
            failed.Add($"{shortcutPath}\n  ({ex.Message})");
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) { Close(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
