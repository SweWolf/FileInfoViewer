namespace FileInfoViewer;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblFilePathLabel;
    private System.Windows.Forms.TextBox txtFilePath;
    private System.Windows.Forms.Button btnBrowse;
    private System.Windows.Forms.Button btnView;
    private System.Windows.Forms.Button btnSettings;
    private System.Windows.Forms.Panel pnlHeader;
    private System.Windows.Forms.Panel pnlMain;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        pnlHeader = new Panel();
        lblTitle = new Label();
        btnSettings = new Button();
        pnlMain = new Panel();
        lblFilePathLabel = new Label();
        txtFilePath = new TextBox();
        btnBrowse = new Button();
        btnView = new Button();
        btnAbout = new Button();
        lblStatus = new Label();
        pnlHeader.SuspendLayout();
        pnlMain.SuspendLayout();
        SuspendLayout();
        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(15, 52, 96);
        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(btnSettings);
        pnlHeader.Controls.Add(btnAbout);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Padding = new Padding(16, 0, 0, 0);
        pnlHeader.Size = new Size(820, 56);
        pnlHeader.TabIndex = 1;
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 14F);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(16, 12);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(173, 25);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "📁  File Info Viewer";
        // 
        // btnSettings
        // 
        btnSettings.BackColor = Color.FromArgb(240, 242, 248);
        btnSettings.Cursor = Cursors.Hand;
        btnSettings.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 220);
        btnSettings.FlatStyle = FlatStyle.Flat;
        btnSettings.ForeColor = Color.FromArgb(40, 40, 80);
        btnSettings.Location = new Point(580, 9);
        btnSettings.Name = "btnSettings";
        btnSettings.Size = new Size(100, 34);
        btnSettings.TabIndex = 5;
        btnSettings.Text = "⚙ Settings";
        btnSettings.UseVisualStyleBackColor = false;
        btnSettings.Click += btnSettings_Click;
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
        pnlMain.Location = new Point(0, 56);
        pnlMain.Name = "pnlMain";
        pnlMain.Padding = new Padding(20, 18, 20, 12);
        pnlMain.Size = new Size(820, 125);
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
        txtFilePath.Font = new Font("Consolas", 9.5F);
        txtFilePath.Location = new Point(82, 20);
        txtFilePath.Name = "txtFilePath";
        txtFilePath.Size = new Size(679, 22);
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
        btnBrowse.Location = new Point(767, 19);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(41, 23);
        btnBrowse.TabIndex = 2;
        btnBrowse.Text = "...";
        btnBrowse.UseVisualStyleBackColor = false;
        btnBrowse.Click += btnBrowse_Click;
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
        // btnAbout
        // 
        btnAbout.BackColor = Color.FromArgb(240, 242, 248);
        btnAbout.Cursor = Cursors.Hand;
        btnAbout.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 220);
        btnAbout.FlatStyle = FlatStyle.Flat;
        btnAbout.ForeColor = Color.FromArgb(40, 40, 80);
        btnAbout.Location = new Point(696, 9);
        btnAbout.Name = "btnAbout";
        btnAbout.Size = new Size(100, 34);
        btnAbout.TabIndex = 6;
        btnAbout.Text = "ℹ About";
        btnAbout.UseVisualStyleBackColor = false;
        btnAbout.Click += btnAbout_Click;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.Font = new Font("Segoe UI", 8.5F);
        lblStatus.ForeColor = Color.FromArgb(100, 100, 120);
        lblStatus.Location = new Point(82, 96);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(726, 23);
        lblStatus.TabIndex = 4;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(820, 181);
        Controls.Add(pnlMain);
        Controls.Add(pnlHeader);
        Font = new Font("Segoe UI", 9.5F);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MaximizeBox = false;
        MaximumSize = new Size(1200, 220);
        MinimumSize = new Size(480, 220);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "File Info Viewer";
        Resize += MainForm_Resize;
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlMain.ResumeLayout(false);
        pnlMain.PerformLayout();
        ResumeLayout(false);
    }

    private Label lblStatus;
    private Button btnAbout;
}
