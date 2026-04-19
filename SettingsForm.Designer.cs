namespace FileInfoViewer
{
	partial class SettingsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            grpFileDate = new GroupBox();
            cboTimeZone = new ComboBox();
            label1 = new Label();
            chkShowSeconds = new CheckBox();
            grpShow = new GroupBox();
            chkShowFileHashes = new CheckBox();
            chkFileAttributes = new CheckBox();
            chkOwner = new CheckBox();
            cboShowCopyButton = new ComboBox();
            label2 = new Label();
            grpFileDate.SuspendLayout();
            grpShow.SuspendLayout();
            SuspendLayout();
            // 
            // grpFileDate
            // 
            grpFileDate.Controls.Add(cboTimeZone);
            grpFileDate.Controls.Add(label1);
            grpFileDate.Controls.Add(chkShowSeconds);
            grpFileDate.Location = new Point(12, 12);
            grpFileDate.Name = "grpFileDate";
            grpFileDate.Size = new Size(360, 110);
            grpFileDate.TabIndex = 0;
            grpFileDate.TabStop = false;
            grpFileDate.Text = "File Date";
            // 
            // cboTimeZone
            // 
            cboTimeZone.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTimeZone.FormattingEnabled = true;
            cboTimeZone.Items.AddRange(new object[] { "Local", "UTC", "Both" });
            cboTimeZone.Location = new Point(108, 26);
            cboTimeZone.Name = "cboTimeZone";
            cboTimeZone.Size = new Size(200, 23);
            cboTimeZone.TabIndex = 0;
            cboTimeZone.SelectedIndexChanged += cboTimeZone_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 29);
            label1.Name = "label1";
            label1.Size = new Size(62, 15);
            label1.TabIndex = 1;
            label1.Text = "Time zone";
            // 
            // chkShowSeconds
            // 
            chkShowSeconds.AutoSize = true;
            chkShowSeconds.Location = new Point(108, 69);
            chkShowSeconds.Name = "chkShowSeconds";
            chkShowSeconds.Size = new Size(102, 19);
            chkShowSeconds.TabIndex = 1;
            chkShowSeconds.Text = "Show Seconds";
            chkShowSeconds.UseVisualStyleBackColor = true;
            chkShowSeconds.CheckedChanged += chkShowSeconds_CheckedChanged;
            // 
            // grpShow
            // 
            grpShow.Controls.Add(chkShowFileHashes);
            grpShow.Controls.Add(chkFileAttributes);
            grpShow.Controls.Add(chkOwner);
            grpShow.Controls.Add(cboShowCopyButton);
            grpShow.Controls.Add(label2);
            grpShow.Location = new Point(12, 137);
            grpShow.Name = "grpShow";
            grpShow.Size = new Size(360, 155);
            grpShow.TabIndex = 1;
            grpShow.TabStop = false;
            grpShow.Text = "Show";
            // 
            // chkShowFileHashes
            // 
            chkShowFileHashes.AutoSize = true;
            chkShowFileHashes.Checked = true;
            chkShowFileHashes.CheckState = CheckState.Checked;
            chkShowFileHashes.Location = new Point(108, 115);
            chkShowFileHashes.Name = "chkShowFileHashes";
            chkShowFileHashes.Size = new Size(85, 19);
            chkShowFileHashes.TabIndex = 4;
            chkShowFileHashes.Text = "File Hashes";
            chkShowFileHashes.UseVisualStyleBackColor = true;
            chkShowFileHashes.CheckedChanged += chkShowFileHashes_CheckedChanged;
            // 
            // chkFileAttributes
            // 
            chkFileAttributes.AutoSize = true;
            chkFileAttributes.Location = new Point(108, 90);
            chkFileAttributes.Name = "chkFileAttributes";
            chkFileAttributes.Size = new Size(99, 19);
            chkFileAttributes.TabIndex = 3;
            chkFileAttributes.Text = "File Attributes";
            chkFileAttributes.UseVisualStyleBackColor = true;
            chkFileAttributes.CheckedChanged += chkFileAttributes_CheckedChanged;
            // 
            // chkOwner
            // 
            chkOwner.AutoSize = true;
            chkOwner.Location = new Point(108, 65);
            chkOwner.Name = "chkOwner";
            chkOwner.Size = new Size(61, 19);
            chkOwner.TabIndex = 2;
            chkOwner.Text = "Owner";
            chkOwner.UseVisualStyleBackColor = true;
            chkOwner.CheckedChanged += chkOwner_CheckedChanged;
            // 
            // cboShowCopyButton
            // 
            cboShowCopyButton.DropDownStyle = ComboBoxStyle.DropDownList;
            cboShowCopyButton.FormattingEnabled = true;
            cboShowCopyButton.Items.AddRange(new object[] { "No", "Yes", "Yes on hover over" });
            cboShowCopyButton.Location = new Point(108, 26);
            cboShowCopyButton.Name = "cboShowCopyButton";
            cboShowCopyButton.Size = new Size(200, 23);
            cboShowCopyButton.TabIndex = 0;
            cboShowCopyButton.SelectedIndexChanged += cboShowCopyButton_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(20, 29);
            label2.Name = "label2";
            label2.Size = new Size(74, 15);
            label2.TabIndex = 1;
            label2.Text = "Copy Button";
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(388, 307);
            Controls.Add(grpShow);
            Controls.Add(grpFileDate);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            grpFileDate.ResumeLayout(false);
            grpFileDate.PerformLayout();
            grpShow.ResumeLayout(false);
            grpShow.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpFileDate;
		private CheckBox chkShowSeconds;
		private ComboBox cboTimeZone;
		private Label label1;
        private GroupBox grpShow;
        private ComboBox cboShowCopyButton;
        private Label label2;
        private CheckBox chkFileAttributes;
        private CheckBox chkOwner;
        private CheckBox chkShowFileHashes;
    }
}
