namespace FileInfoViewer
{
    partial class CreateShortcutForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            grpLocation    = new GroupBox();
            chkDesktop     = new CheckBox();
            chkStartMenu   = new CheckBox();
            chkSendTo      = new CheckBox();
            grpFor         = new GroupBox();
            rdoCurrentUser = new RadioButton();
            rdoAllUsers    = new RadioButton();
            lblNote        = new Label();
            lblSendToNote  = new Label();
            btnOK          = new Button();
            btnCancel      = new Button();
            grpLocation.SuspendLayout();
            grpFor.SuspendLayout();
            SuspendLayout();

            // grpLocation
            grpLocation.Controls.Add(chkDesktop);
            grpLocation.Controls.Add(chkStartMenu);
            grpLocation.Controls.Add(chkSendTo);
            grpLocation.Location = new Point(12, 12);
            grpLocation.Name = "grpLocation";
            grpLocation.Size = new Size(356, 107);
            grpLocation.TabIndex = 0;
            grpLocation.TabStop = false;
            grpLocation.Text = "Create shortcut on:";

            // chkDesktop
            chkDesktop.AutoSize = true;
            chkDesktop.Checked = true;
            chkDesktop.CheckState = CheckState.Checked;
            chkDesktop.Location = new Point(15, 28);
            chkDesktop.Name = "chkDesktop";
            chkDesktop.TabIndex = 0;
            chkDesktop.Text = "Desktop";
            chkDesktop.UseVisualStyleBackColor = true;

            // chkStartMenu
            chkStartMenu.AutoSize = true;
            chkStartMenu.Location = new Point(15, 53);
            chkStartMenu.Name = "chkStartMenu";
            chkStartMenu.TabIndex = 1;
            chkStartMenu.Text = "Start Menu (Programs)";
            chkStartMenu.UseVisualStyleBackColor = true;

            // chkSendTo
            chkSendTo.AutoSize = true;
            chkSendTo.Location = new Point(15, 78);
            chkSendTo.Name = "chkSendTo";
            chkSendTo.TabIndex = 2;
            chkSendTo.Text = "Send To menu";
            chkSendTo.UseVisualStyleBackColor = true;

            // grpFor
            grpFor.Controls.Add(rdoCurrentUser);
            grpFor.Controls.Add(rdoAllUsers);
            grpFor.Location = new Point(12, 131);
            grpFor.Name = "grpFor";
            grpFor.Size = new Size(356, 82);
            grpFor.TabIndex = 1;
            grpFor.TabStop = false;
            grpFor.Text = "For:";

            // rdoCurrentUser
            rdoCurrentUser.AutoSize = true;
            rdoCurrentUser.Checked = true;
            rdoCurrentUser.Location = new Point(15, 28);
            rdoCurrentUser.Name = "rdoCurrentUser";
            rdoCurrentUser.TabIndex = 0;
            rdoCurrentUser.TabStop = true;
            rdoCurrentUser.Text = "Current user only";
            rdoCurrentUser.UseVisualStyleBackColor = true;

            // rdoAllUsers
            rdoAllUsers.AutoSize = true;
            rdoAllUsers.Location = new Point(15, 53);
            rdoAllUsers.Name = "rdoAllUsers";
            rdoAllUsers.TabIndex = 1;
            rdoAllUsers.Text = "All users (may require administrator privileges)";
            rdoAllUsers.UseVisualStyleBackColor = true;

            // lblNote
            lblNote.AutoSize = true;
            lblNote.ForeColor = SystemColors.GrayText;
            lblNote.Location = new Point(12, 227);
            lblNote.Name = "lblNote";
            lblNote.TabIndex = 2;
            lblNote.Text = "The shortcut will point to the currently running EXE.";

            // lblSendToNote
            lblSendToNote.AutoSize = true;
            lblSendToNote.ForeColor = SystemColors.GrayText;
            lblSendToNote.Location = new Point(12, 244);
            lblSendToNote.Name = "lblSendToNote";
            lblSendToNote.TabIndex = 3;
            lblSendToNote.Text = "* Send To shortcut is always for current user only.";

            // btnOK
            btnOK.Location = new Point(212, 265);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 28);
            btnOK.TabIndex = 4;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;

            // btnCancel
            btnCancel.Location = new Point(293, 265);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 28);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;

            // CreateShortcutForm
            AcceptButton = btnOK;
            CancelButton = btnCancel;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(380, 305);
            Controls.Add(lblNote);
            Controls.Add(lblSendToNote);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(grpFor);
            Controls.Add(grpLocation);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CreateShortcutForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Create Shortcut";
            grpLocation.ResumeLayout(false);
            grpLocation.PerformLayout();
            grpFor.ResumeLayout(false);
            grpFor.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private GroupBox grpLocation;
        private CheckBox chkDesktop;
        private CheckBox chkStartMenu;
        private CheckBox chkSendTo;
        private GroupBox grpFor;
        private RadioButton rdoCurrentUser;
        private RadioButton rdoAllUsers;
        private Label lblNote;
        private Label lblSendToNote;
        private Button btnOK;
        private Button btnCancel;
    }
}
