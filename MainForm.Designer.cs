namespace FileInfoViewer;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Label lblFilePathLabel;
    private System.Windows.Forms.TextBox txtFilePath;
    private System.Windows.Forms.Button btnBrowse;
    private System.Windows.Forms.Button btnView;
    private System.Windows.Forms.Panel pnlMain;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        toolTip = new ToolTip(components);
        menuStrip = new MenuStrip();
        toolsToolStripMenuItem = new ToolStripMenuItem();
        createShortcutToolStripMenuItem = new ToolStripMenuItem();
        menuSettings = new ToolStripMenuItem();
        helpToolStripMenuItem = new ToolStripMenuItem();
        menuViewHelp = new ToolStripMenuItem();
        menuViewHelpSeparator = new ToolStripSeparator();
        aboutToolStripMenuItem = new ToolStripMenuItem();
        menuNewVersion = new ToolStripMenuItem();
        pnlMain = new Panel();
        lblFilePathLabel = new Label();
        txtFilePath = new TextBox();
        btnBrowse = new Button();
        btnView = new Button();
        lblStatus = new Label();
        menuStrip.SuspendLayout();
        pnlMain.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip
        // 
        menuStrip.Items.AddRange(new ToolStripItem[] { toolsToolStripMenuItem, helpToolStripMenuItem, menuNewVersion });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(820, 24);
        menuStrip.TabIndex = 2;
        // 
        // toolsToolStripMenuItem
        // 
        toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { createShortcutToolStripMenuItem, menuSettings });
        toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
        toolsToolStripMenuItem.Size = new Size(47, 20);
        toolsToolStripMenuItem.Text = "Tools";
        // 
        // createShortcutToolStripMenuItem
        // 
        createShortcutToolStripMenuItem.Name = "createShortcutToolStripMenuItem";
        createShortcutToolStripMenuItem.Size = new Size(165, 22);
        createShortcutToolStripMenuItem.Text = "Create Shortcut...";
        createShortcutToolStripMenuItem.Click += btnCreateShortcut_Click;
        // 
        // menuSettings
        // 
        menuSettings.Name = "menuSettings";
        menuSettings.Size = new Size(165, 22);
        menuSettings.Text = "Settings";
        menuSettings.Click += menuSettings_Click;
        // 
        // helpToolStripMenuItem
        // 
        helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { menuViewHelp, menuViewHelpSeparator, aboutToolStripMenuItem });
        helpToolStripMenuItem.Name = "helpToolStripMenuItem";
        helpToolStripMenuItem.Size = new Size(44, 20);
        helpToolStripMenuItem.Text = "Help";
        // 
        // menuViewHelp
        // 
        menuViewHelp.Name = "menuViewHelp";
        menuViewHelp.ShortcutKeys = Keys.F1;
        menuViewHelp.Size = new Size(201, 22);
        menuViewHelp.Text = "File Info Viewer Help";
        menuViewHelp.Click += menuViewHelp_Click;
        // 
        // menuViewHelpSeparator
        // 
        menuViewHelpSeparator.Name = "menuViewHelpSeparator";
        menuViewHelpSeparator.Size = new Size(198, 6);
        // 
        // aboutToolStripMenuItem
        // 
        aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
        aboutToolStripMenuItem.Size = new Size(201, 22);
        aboutToolStripMenuItem.Text = "About";
        aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
        // 
        // menuNewVersion
        //
        menuNewVersion.Alignment = ToolStripItemAlignment.Right;
        menuNewVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        menuNewVersion.ForeColor = Color.Green;
        menuNewVersion.Name = "menuNewVersion";
        menuNewVersion.Size = new Size(142, 20);
        menuNewVersion.Text = "New Version Available";
        menuNewVersion.Visible = false;
        menuNewVersion.Click += menuNewVersion_Click;
        //
        // pnlMain
        // 
        pnlMain.BackColor = Color.White;
        pnlMain.Controls.Add(lblFilePathLabel);
        pnlMain.Controls.Add(txtFilePath);
        pnlMain.Controls.Add(btnBrowse);
        pnlMain.Controls.Add(btnView);
        pnlMain.Controls.Add(lblStatus);
        pnlMain.Dock = DockStyle.Fill;
        pnlMain.Location = new Point(0, 24);
        pnlMain.Name = "pnlMain";
        pnlMain.Padding = new Padding(20, 18, 20, 12);
        pnlMain.Size = new Size(820, 133);
        pnlMain.TabIndex = 0;
        // 
        // lblFilePathLabel
        // 
        lblFilePathLabel.AutoSize = true;
        lblFilePathLabel.ForeColor = Color.FromArgb(70, 70, 70);
        lblFilePathLabel.Location = new Point(20, 24);
        lblFilePathLabel.Name = "lblFilePathLabel";
        lblFilePathLabel.Size = new Size(59, 17);
        lblFilePathLabel.TabIndex = 0;
        lblFilePathLabel.Text = "File Path:";
        // 
        // txtFilePath
        // 
        txtFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtFilePath.BackColor = Color.FromArgb(248, 249, 252);
        txtFilePath.BorderStyle = BorderStyle.FixedSingle;
        txtFilePath.Font = new Font("Consolas", 12F);
        txtFilePath.ForeColor = Color.FromArgb(30, 30, 30);
        txtFilePath.Location = new Point(82, 20);
        txtFilePath.Name = "txtFilePath";
        txtFilePath.Size = new Size(680, 26);
        txtFilePath.TabIndex = 1;
        txtFilePath.KeyDown += txtFilePath_KeyDown;
        // 
        // btnBrowse
        // 
        btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowse.BackColor = Color.FromArgb(240, 242, 248);
        btnBrowse.Cursor = Cursors.Hand;
        btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 220);
        btnBrowse.FlatStyle = FlatStyle.Flat;
        btnBrowse.ForeColor = Color.FromArgb(40, 40, 80);
        btnBrowse.Location = new Point(768, 19);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(30, 27);
        btnBrowse.TabIndex = 2;
        btnBrowse.Text = "...";
        btnBrowse.UseVisualStyleBackColor = false;
        btnBrowse.Click += btnBrowse_Click;
        toolTip.SetToolTip(btnBrowse, "Browse for a file");
        // 
        // btnView
        // 
        btnView.BackColor = Color.FromArgb(15, 52, 96);
        btnView.Cursor = Cursors.Hand;
        btnView.FlatAppearance.BorderSize = 0;
        btnView.FlatStyle = FlatStyle.Flat;
        btnView.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnView.ForeColor = Color.White;
        btnView.Location = new Point(82, 55);
        btnView.Name = "btnView";
        btnView.Size = new Size(140, 34);
        btnView.TabIndex = 3;
        btnView.Text = "View File Info";
        btnView.UseVisualStyleBackColor = false;
        btnView.Click += btnView_Click;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.Font = new Font("Segoe UI", 10F);
        lblStatus.ForeColor = Color.FromArgb(100, 100, 120);
        lblStatus.Location = new Point(82, 96);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(716, 23);
        lblStatus.TabIndex = 4;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(820, 157);
        Controls.Add(pnlMain);
        Controls.Add(menuStrip);
        Font = new Font("Segoe UI", 9.5F);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MainMenuStrip = menuStrip;
        MaximizeBox = false;
        MaximumSize = new Size(1600, 196);
        MinimumSize = new Size(480, 196);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "File Info Viewer";
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        pnlMain.ResumeLayout(false);
        pnlMain.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private Label lblStatus;
    private ToolTip toolTip;
    private MenuStrip menuStrip;
    private ToolStripMenuItem toolsToolStripMenuItem;
    private ToolStripMenuItem createShortcutToolStripMenuItem;
    private ToolStripMenuItem menuSettings;
    private ToolStripMenuItem helpToolStripMenuItem;
    private ToolStripMenuItem menuViewHelp;
    private ToolStripSeparator menuViewHelpSeparator;
    private ToolStripMenuItem aboutToolStripMenuItem;
    private ToolStripMenuItem menuNewVersion;
}
