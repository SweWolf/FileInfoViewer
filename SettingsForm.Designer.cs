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
            cboTextualData = new ComboBox();
            label3 = new Label();
            cboShowCopyButton = new ComboBox();
            label2 = new Label();
            grpLayout = new GroupBox();
            cboContentWidth = new ComboBox();
            label4 = new Label();
            groupBox1 = new GroupBox();
            chkWebLinksClickable = new CheckBox();
            txtCustomContentWidth = new TextBox();
            optCustContWidthPx = new RadioButton();
            optCustContWidthPerc = new RadioButton();
            grpFileDate.SuspendLayout();
            grpShow.SuspendLayout();
            grpLayout.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // grpFileDate
            // 
            grpFileDate.Controls.Add(cboTimeZone);
            grpFileDate.Controls.Add(label1);
            grpFileDate.Controls.Add(chkShowSeconds);
            grpFileDate.Location = new Point(12, 12);
            grpFileDate.Name = "grpFileDate";
            grpFileDate.Size = new Size(360, 101);
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
            grpShow.Controls.Add(cboTextualData);
            grpShow.Controls.Add(label3);
            grpShow.Controls.Add(cboShowCopyButton);
            grpShow.Controls.Add(label2);
            grpShow.Location = new Point(12, 128);
            grpShow.Name = "grpShow";
            grpShow.Size = new Size(360, 187);
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
            // cboTextualData
            // 
            cboTextualData.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTextualData.FormattingEnabled = true;
            cboTextualData.Items.AddRange(new object[] { "None", "Formatted", "Raw data", "Both Formatted and Raw Data" });
            cboTextualData.Location = new Point(108, 150);
            cboTextualData.Name = "cboTextualData";
            cboTextualData.Size = new Size(200, 23);
            cboTextualData.TabIndex = 5;
            cboTextualData.SelectedIndexChanged += cboTextualData_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 153);
            label3.Name = "label3";
            label3.Size = new Size(71, 15);
            label3.TabIndex = 1;
            label3.Text = "Textual Data";
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
            // grpLayout
            // 
            grpLayout.Controls.Add(optCustContWidthPerc);
            grpLayout.Controls.Add(optCustContWidthPx);
            grpLayout.Controls.Add(txtCustomContentWidth);
            grpLayout.Controls.Add(cboContentWidth);
            grpLayout.Controls.Add(label4);
            grpLayout.Location = new Point(14, 396);
            grpLayout.Name = "grpLayout";
            grpLayout.Size = new Size(360, 114);
            grpLayout.TabIndex = 2;
            grpLayout.TabStop = false;
            grpLayout.Text = "Layout";
            // 
            // cboContentWidth
            // 
            cboContentWidth.DropDownStyle = ComboBoxStyle.DropDownList;
            cboContentWidth.FormattingEnabled = true;
            cboContentWidth.Items.AddRange(new object[] { "Narrow (800px)", "Normal (1100px)", "Wide (1400px)", "Very wide (1800px)", "Full width", "Custom" });
            cboContentWidth.Location = new Point(106, 19);
            cboContentWidth.Name = "cboContentWidth";
            cboContentWidth.Size = new Size(202, 23);
            cboContentWidth.TabIndex = 0;
            cboContentWidth.SelectedIndexChanged += cboContentWidth_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(20, 22);
            label4.Name = "label4";
            label4.Size = new Size(85, 15);
            label4.TabIndex = 1;
            label4.Text = "Content Width";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(chkWebLinksClickable);
            groupBox1.Location = new Point(14, 330);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(362, 54);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Web Links";
            // 
            // chkWebLinksClickable
            // 
            chkWebLinksClickable.AutoSize = true;
            chkWebLinksClickable.Checked = true;
            chkWebLinksClickable.CheckState = CheckState.Checked;
            chkWebLinksClickable.Location = new Point(110, 22);
            chkWebLinksClickable.Name = "chkWebLinksClickable";
            chkWebLinksClickable.Size = new Size(74, 19);
            chkWebLinksClickable.TabIndex = 0;
            chkWebLinksClickable.Text = "Clickable";
            chkWebLinksClickable.UseVisualStyleBackColor = true;
            chkWebLinksClickable.CheckedChanged += chkWebLinksClickable_CheckedChanged;
            // 
            // txtCustomContentWidth
            // 
            txtCustomContentWidth.Location = new Point(106, 57);
            txtCustomContentWidth.Name = "txtCustomContentWidth";
            txtCustomContentWidth.Size = new Size(202, 23);
            txtCustomContentWidth.TabIndex = 2;
            txtCustomContentWidth.Visible = false;
            txtCustomContentWidth.TextChanged += txtCustomContentWidth_TextChanged;
            // 
            // optCustContWidthPx
            // 
            optCustContWidthPx.AutoSize = true;
            optCustContWidthPx.Checked = true;
            optCustContWidthPx.Location = new Point(111, 86);
            optCustContWidthPx.Name = "optCustContWidthPx";
            optCustContWidthPx.Size = new Size(54, 19);
            optCustContWidthPx.TabIndex = 3;
            optCustContWidthPx.TabStop = true;
            optCustContWidthPx.Text = "Pixels";
            optCustContWidthPx.UseVisualStyleBackColor = true;
            optCustContWidthPx.Visible = false;
            optCustContWidthPx.CheckedChanged += optCustContWidthUnit_CheckedChanged;
            // 
            // optCustContWidthPerc
            // 
            optCustContWidthPerc.AutoSize = true;
            optCustContWidthPerc.Location = new Point(188, 86);
            optCustContWidthPerc.Name = "optCustContWidthPerc";
            optCustContWidthPerc.Size = new Size(65, 19);
            optCustContWidthPerc.TabIndex = 3;
            optCustContWidthPerc.Text = "Percent";
            optCustContWidthPerc.UseVisualStyleBackColor = true;
            optCustContWidthPerc.Visible = false;
            optCustContWidthPerc.CheckedChanged += optCustContWidthUnit_CheckedChanged;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(388, 522);
            Controls.Add(groupBox1);
            Controls.Add(grpLayout);
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
            grpLayout.ResumeLayout(false);
            grpLayout.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
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
        private ComboBox cboTextualData;
        private Label label3;
        private GroupBox grpLayout;
        private ComboBox cboContentWidth;
        private Label label4;
        private GroupBox groupBox1;
        private CheckBox chkWebLinksClickable;
        private RadioButton optCustContWidthPx;
        private TextBox txtCustomContentWidth;
        private RadioButton optCustContWidthPerc;
    }
}
