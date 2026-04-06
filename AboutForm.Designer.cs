namespace FileInfoViewer
{
    partial class AboutForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlHeader = new Panel();
            picIcon = new PictureBox();
            lblAppName = new Label();
            lblVersion = new Label();
            pnlContent = new Panel();
            lblDescription = new Label();
            lblCopyright = new Label();
            lblBuiltWithHeader = new Label();
            lblBuiltWith = new Label();
            btnClose = new Button();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picIcon).BeginInit();
            pnlContent.SuspendLayout();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.BackColor = Color.FromArgb(15, 52, 96);
            pnlHeader.Controls.Add(picIcon);
            pnlHeader.Controls.Add(lblAppName);
            pnlHeader.Controls.Add(lblVersion);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(440, 88);
            pnlHeader.TabIndex = 0;
            //
            // picIcon
            //
            picIcon.Location = new Point(20, 16);
            picIcon.Name = "picIcon";
            picIcon.Size = new Size(52, 52);
            picIcon.SizeMode = PictureBoxSizeMode.Zoom;
            picIcon.TabIndex = 0;
            picIcon.TabStop = false;
            //
            // lblAppName
            //
            lblAppName.AutoSize = true;
            lblAppName.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblAppName.ForeColor = Color.White;
            lblAppName.Location = new Point(84, 16);
            lblAppName.Name = "lblAppName";
            lblAppName.TabIndex = 1;
            lblAppName.Text = "File Info Viewer";
            //
            // lblVersion
            //
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Segoe UI", 9F);
            lblVersion.ForeColor = Color.FromArgb(160, 195, 225);
            lblVersion.Location = new Point(86, 54);
            lblVersion.Name = "lblVersion";
            lblVersion.TabIndex = 2;
            lblVersion.Text = "Version 1.0.0";
            //
            // pnlContent
            //
            pnlContent.BackColor = Color.White;
            pnlContent.Controls.Add(lblDescription);
            pnlContent.Controls.Add(lblCopyright);
            pnlContent.Controls.Add(lblBuiltWithHeader);
            pnlContent.Controls.Add(lblBuiltWith);
            pnlContent.Controls.Add(btnClose);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 88);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(440, 232);
            pnlContent.TabIndex = 1;
            //
            // lblDescription
            //
            lblDescription.Font = new Font("Segoe UI", 9.5F);
            lblDescription.ForeColor = Color.FromArgb(50, 50, 50);
            lblDescription.Location = new Point(20, 18);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(400, 36);
            lblDescription.TabIndex = 0;
            lblDescription.Text = "Collects and displays detailed metadata about files in a rich, self-contained HTML report.";
            //
            // lblCopyright
            //
            lblCopyright.AutoSize = true;
            lblCopyright.Font = new Font("Segoe UI", 9F);
            lblCopyright.ForeColor = Color.FromArgb(120, 120, 120);
            lblCopyright.Location = new Point(20, 62);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.TabIndex = 1;
            lblCopyright.Text = "SweWolf Software";
            //
            // lblBuiltWithHeader
            //
            lblBuiltWithHeader.AutoSize = true;
            lblBuiltWithHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblBuiltWithHeader.ForeColor = Color.FromArgb(50, 50, 50);
            lblBuiltWithHeader.Location = new Point(20, 96);
            lblBuiltWithHeader.Name = "lblBuiltWithHeader";
            lblBuiltWithHeader.TabIndex = 2;
            lblBuiltWithHeader.Text = "Built with:";
            //
            // lblBuiltWith
            //
            lblBuiltWith.Font = new Font("Segoe UI", 9F);
            lblBuiltWith.ForeColor = Color.FromArgb(80, 80, 80);
            lblBuiltWith.Location = new Point(20, 116);
            lblBuiltWith.Name = "lblBuiltWith";
            lblBuiltWith.Size = new Size(400, 52);
            lblBuiltWith.TabIndex = 3;
            lblBuiltWith.Text = "• MetadataExtractor 2.9.0\r\n• TagLibSharp 2.3.0\r\n• .NET 9 / Windows Forms";
            //
            // btnClose
            //
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.BackColor = Color.FromArgb(15, 52, 96);
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(340, 190);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(80, 28);
            btnClose.TabIndex = 4;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            //
            // AboutForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(440, 320);
            Controls.Add(pnlContent);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "About File Info Viewer";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picIcon).EndInit();
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            ResumeLayout(false);
        }

        private Panel pnlHeader;
        private PictureBox picIcon;
        private Label lblAppName;
        private Label lblVersion;
        private Panel pnlContent;
        private Label lblDescription;
        private Label lblCopyright;
        private Label lblBuiltWithHeader;
        private Label lblBuiltWith;
        private Button btnClose;
    }
}
