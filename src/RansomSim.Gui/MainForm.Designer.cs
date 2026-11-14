namespace RansomSim.Gui;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        lblHeader = new Label();
        lblBanner = new Label();
        panelLeft = new Panel();
        grpTarget = new GroupBox();
        rdoLocal = new RadioButton();
        rdoRemote = new RadioButton();
        lblPath = new Label();
        txtPath = new TextBox();
        btnBrowse = new Button();
        lblHint = new Label();
        grpOptions = new GroupBox();
        chkRecurse = new CheckBox();
        chkDryRun = new CheckBox();
        chkWriteNote = new CheckBox();
        lblDelay = new Label();
        numDelay = new NumericUpDown();
        lblMaxFiles = new Label();
        numMaxFiles = new NumericUpDown();
        lblMaxSize = new Label();
        numMaxSize = new NumericUpDown();
        lblPass = new Label();
        txtPass = new TextBox();
        lblExclusions = new Label();
        txtExclusions = new TextBox();
        lblGuard = new Label();
        grpActions = new GroupBox();
        btnPreview = new Button();
        btnStart = new Button();
        btnStop = new Button();
        btnRestore = new Button();
        btnOpenFolder = new Button();
        lblCounts = new Label();
        grpLog = new GroupBox();
        txtLog = new TextBox();
        progressBar = new ProgressBar();
        lblStatus = new Label();
        panelLeft.SuspendLayout();
        grpTarget.SuspendLayout();
        grpOptions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numDelay).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numMaxFiles).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numMaxSize).BeginInit();
        grpActions.SuspendLayout();
        grpLog.SuspendLayout();
        SuspendLayout();
        // 
        // lblHeader
        // 
        lblHeader.AutoSize = true;
        lblHeader.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblHeader.Location = new Point(12, 9);
        lblHeader.Name = "lblHeader";
        lblHeader.Size = new Size(392, 25);
        lblHeader.TabIndex = 0;
        lblHeader.Text = "RansomSim - Ransomware Simulation Lab";
        // 
        // lblBanner
        // 
        lblBanner.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblBanner.BackColor = Color.FromArgb(255, 244, 206);
        lblBanner.BorderStyle = BorderStyle.FixedSingle;
        lblBanner.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblBanner.ForeColor = Color.FromArgb(120, 70, 0);
        lblBanner.Location = new Point(12, 40);
        lblBanner.Name = "lblBanner";
        lblBanner.Padding = new Padding(8, 0, 8, 0);
        lblBanner.Size = new Size(1096, 44);
        lblBanner.TabIndex = 1;
        lblBanner.Text = "SIMULATION ONLY - original files are opened read-only and are never modified, renamed or deleted. " +
                         "Encrypted .locked copies are written alongside them and can be reverted with Restore.";
        lblBanner.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // panelLeft
        // 
        panelLeft.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        panelLeft.Controls.Add(grpTarget);
        panelLeft.Controls.Add(grpOptions);
        panelLeft.Controls.Add(grpActions);
        panelLeft.Location = new Point(12, 92);
        panelLeft.Name = "panelLeft";
        panelLeft.Size = new Size(470, 568);
        panelLeft.TabIndex = 2;
        // 
        // grpTarget
        // 
        grpTarget.Controls.Add(rdoLocal);
        grpTarget.Controls.Add(rdoRemote);
        grpTarget.Controls.Add(lblPath);
        grpTarget.Controls.Add(txtPath);
        grpTarget.Controls.Add(btnBrowse);
        grpTarget.Controls.Add(lblHint);
        grpTarget.Location = new Point(0, 0);
        grpTarget.Name = "grpTarget";
        grpTarget.Size = new Size(470, 160);
        grpTarget.TabIndex = 0;
        grpTarget.TabStop = false;
        grpTarget.Text = "1. Target";
        // 
        // rdoLocal
        // 
        rdoLocal.AutoSize = true;
        rdoLocal.Checked = true;
        rdoLocal.Location = new Point(16, 26);
        rdoLocal.Name = "rdoLocal";
        rdoLocal.Size = new Size(105, 19);
        rdoLocal.TabIndex = 0;
        rdoLocal.TabStop = true;
        rdoLocal.Text = "Local computer";
        rdoLocal.UseVisualStyleBackColor = true;
        rdoLocal.CheckedChanged += rdoScope_CheckedChanged;
        // 
        // rdoRemote
        // 
        rdoRemote.AutoSize = true;
        rdoRemote.Location = new Point(200, 26);
        rdoRemote.Name = "rdoRemote";
        rdoRemote.Size = new Size(146, 19);
        rdoRemote.TabIndex = 1;
        rdoRemote.Text = "Remote shared folder";
        rdoRemote.UseVisualStyleBackColor = true;
        rdoRemote.CheckedChanged += rdoScope_CheckedChanged;
        // 
        // lblPath
        // 
        lblPath.AutoSize = true;
        lblPath.Location = new Point(16, 56);
        lblPath.Name = "lblPath";
        lblPath.Size = new Size(42, 15);
        lblPath.TabIndex = 2;
        lblPath.Text = "Folder:";
        // 
        // txtPath
        // 
        txtPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtPath.Location = new Point(16, 76);
        txtPath.Name = "txtPath";
        txtPath.PlaceholderText = @"C:\RansomSimLab";
        txtPath.Size = new Size(330, 23);
        txtPath.TabIndex = 3;
        // 
        // btnBrowse
        // 
        btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowse.Location = new Point(352, 75);
        btnBrowse.Name = "btnBrowse";
        btnBrowse.Size = new Size(100, 25);
        btnBrowse.TabIndex = 4;
        btnBrowse.Text = "Browse...";
        btnBrowse.UseVisualStyleBackColor = true;
        btnBrowse.Click += btnBrowse_Click;
        // 
        // lblHint
        // 
        lblHint.ForeColor = SystemColors.GrayText;
        lblHint.Location = new Point(16, 104);
        lblHint.Name = "lblHint";
        lblHint.Size = new Size(436, 46);
        lblHint.TabIndex = 5;
        lblHint.Text = "Local: any folder on this computer.\r\nRemote: a UNC path such as \\\\fileserver\\lab-share";
        // 
        // grpOptions
        // 
        grpOptions.Controls.Add(chkRecurse);
        grpOptions.Controls.Add(chkDryRun);
        grpOptions.Controls.Add(chkWriteNote);
        grpOptions.Controls.Add(lblDelay);
        grpOptions.Controls.Add(numDelay);
        grpOptions.Controls.Add(lblMaxFiles);
        grpOptions.Controls.Add(numMaxFiles);
        grpOptions.Controls.Add(lblMaxSize);
        grpOptions.Controls.Add(numMaxSize);
        grpOptions.Controls.Add(lblPass);
        grpOptions.Controls.Add(txtPass);
        grpOptions.Controls.Add(lblExclusions);
        grpOptions.Controls.Add(txtExclusions);
        grpOptions.Controls.Add(lblGuard);
        grpOptions.Location = new Point(0, 168);
        grpOptions.Name = "grpOptions";
        grpOptions.Size = new Size(470, 226);
        grpOptions.TabIndex = 1;
        grpOptions.TabStop = false;
        grpOptions.Text = "2. Simulation options";
        // 
        // chkRecurse
        // 
        chkRecurse.AutoSize = true;
        chkRecurse.Checked = true;
        chkRecurse.CheckState = CheckState.Checked;
        chkRecurse.Location = new Point(16, 26);
        chkRecurse.Name = "chkRecurse";
        chkRecurse.Size = new Size(124, 19);
        chkRecurse.TabIndex = 0;
        chkRecurse.Text = "Include subfolders";
        chkRecurse.UseVisualStyleBackColor = true;
        // 
        // chkDryRun
        // 
        chkDryRun.AutoSize = true;
        chkDryRun.Location = new Point(200, 26);
        chkDryRun.Name = "chkDryRun";
        chkDryRun.Size = new Size(160, 19);
        chkDryRun.TabIndex = 1;
        chkDryRun.Text = "Dry run (write nothing)";
        chkDryRun.UseVisualStyleBackColor = true;
        // 
        // chkWriteNote
        // 
        chkWriteNote.AutoSize = true;
        chkWriteNote.Checked = true;
        chkWriteNote.CheckState = CheckState.Checked;
        chkWriteNote.Location = new Point(16, 50);
        chkWriteNote.Name = "chkWriteNote";
        chkWriteNote.Size = new Size(150, 19);
        chkWriteNote.TabIndex = 2;
        chkWriteNote.Text = "Drop ransom note";
        chkWriteNote.UseVisualStyleBackColor = true;
        // 
        // lblDelay
        // 
        lblDelay.AutoSize = true;
        lblDelay.Location = new Point(16, 82);
        lblDelay.Name = "lblDelay";
        lblDelay.Size = new Size(70, 15);
        lblDelay.TabIndex = 3;
        lblDelay.Text = "Throttle ms:";
        // 
        // numDelay
        // 
        numDelay.Location = new Point(120, 80);
        numDelay.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
        numDelay.Name = "numDelay";
        numDelay.Size = new Size(70, 23);
        numDelay.TabIndex = 4;
        // 
        // lblMaxFiles
        // 
        lblMaxFiles.AutoSize = true;
        lblMaxFiles.Location = new Point(210, 82);
        lblMaxFiles.Name = "lblMaxFiles";
        lblMaxFiles.Size = new Size(96, 15);
        lblMaxFiles.TabIndex = 5;
        lblMaxFiles.Text = "Max files (0=all):";
        // 
        // numMaxFiles
        // 
        numMaxFiles.Location = new Point(340, 80);
        numMaxFiles.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        numMaxFiles.Name = "numMaxFiles";
        numMaxFiles.Size = new Size(112, 23);
        numMaxFiles.TabIndex = 6;
        // 
        // lblMaxSize
        // 
        lblMaxSize.AutoSize = true;
        lblMaxSize.Location = new Point(16, 112);
        lblMaxSize.Name = "lblMaxSize";
        lblMaxSize.Size = new Size(98, 15);
        lblMaxSize.TabIndex = 7;
        lblMaxSize.Text = "Max size (MB):";
        // 
        // numMaxSize
        // 
        numMaxSize.Location = new Point(120, 110);
        numMaxSize.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        numMaxSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numMaxSize.Name = "numMaxSize";
        numMaxSize.Size = new Size(70, 23);
        numMaxSize.TabIndex = 8;
        numMaxSize.Value = new decimal(new int[] { 50, 0, 0, 0 });
        // 
        // lblPass
        // 
        lblPass.AutoSize = true;
        lblPass.Location = new Point(210, 112);
        lblPass.Name = "lblPass";
        lblPass.Size = new Size(66, 15);
        lblPass.TabIndex = 9;
        lblPass.Text = "Passphrase:";
        // 
        // txtPass
        // 
        txtPass.Location = new Point(340, 110);
        txtPass.Name = "txtPass";
        txtPass.PlaceholderText = "optional";
        txtPass.Size = new Size(112, 23);
        txtPass.TabIndex = 10;
        txtPass.UseSystemPasswordChar = true;
        // 
        // lblExclusions
        // 
        lblExclusions.AutoSize = true;
        lblExclusions.Location = new Point(16, 142);
        lblExclusions.Name = "lblExclusions";
        lblExclusions.Size = new Size(220, 15);
        lblExclusions.TabIndex = 11;
        lblExclusions.Text = "Extra exclusions (comma separated):";
        // 
        // txtExclusions
        // 
        txtExclusions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtExclusions.Location = new Point(16, 162);
        txtExclusions.Name = "txtExclusions";
        txtExclusions.PlaceholderText = @"e.g. confidential,*.bak";
        txtExclusions.Size = new Size(436, 23);
        txtExclusions.TabIndex = 12;
        // 
        // lblGuard
        // 
        lblGuard.ForeColor = SystemColors.GrayText;
        lblGuard.Location = new Point(16, 190);
        lblGuard.Name = "lblGuard";
        lblGuard.Size = new Size(436, 30);
        lblGuard.TabIndex = 13;
        lblGuard.Text = "Originals are never modified. VM disks, .git, Office lock files and files over the size cap are always skipped.";
        // 
        // grpActions
        // 
        grpActions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        grpActions.Controls.Add(btnPreview);
        grpActions.Controls.Add(btnStart);
        grpActions.Controls.Add(btnStop);
        grpActions.Controls.Add(btnRestore);
        grpActions.Controls.Add(btnOpenFolder);
        grpActions.Controls.Add(lblCounts);
        grpActions.Location = new Point(0, 402);
        grpActions.Name = "grpActions";
        grpActions.Size = new Size(470, 166);
        grpActions.TabIndex = 2;
        grpActions.TabStop = false;
        grpActions.Text = "3. Actions";
        // 
        // btnPreview
        // 
        btnPreview.Location = new Point(16, 28);
        btnPreview.Name = "btnPreview";
        btnPreview.Size = new Size(100, 32);
        btnPreview.TabIndex = 0;
        btnPreview.Text = "Preview";
        btnPreview.UseVisualStyleBackColor = true;
        btnPreview.Click += btnPreview_Click;
        // 
        // btnStart
        // 
        btnStart.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnStart.Location = new Point(122, 28);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(120, 32);
        btnStart.TabIndex = 1;
        btnStart.Text = "Start simulation";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;
        // 
        // btnStop
        // 
        btnStop.Enabled = false;
        btnStop.Location = new Point(248, 28);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(100, 32);
        btnStop.TabIndex = 2;
        btnStop.Text = "Stop";
        btnStop.UseVisualStyleBackColor = true;
        btnStop.Click += btnStop_Click;
        // 
        // btnRestore
        // 
        btnRestore.Location = new Point(16, 70);
        btnRestore.Name = "btnRestore";
        btnRestore.Size = new Size(100, 30);
        btnRestore.TabIndex = 3;
        btnRestore.Text = "Restore...";
        btnRestore.UseVisualStyleBackColor = true;
        btnRestore.Click += btnRestore_Click;
        // 
        // btnOpenFolder
        // 
        btnOpenFolder.Location = new Point(122, 70);
        btnOpenFolder.Name = "btnOpenFolder";
        btnOpenFolder.Size = new Size(120, 30);
        btnOpenFolder.TabIndex = 4;
        btnOpenFolder.Text = "Open folder";
        btnOpenFolder.UseVisualStyleBackColor = true;
        btnOpenFolder.Click += btnOpenFolder_Click;
        // 
        // lblCounts
        // 
        lblCounts.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblCounts.Font = new Font("Consolas", 9F);
        lblCounts.Location = new Point(16, 110);
        lblCounts.Name = "lblCounts";
        lblCounts.Size = new Size(436, 44);
        lblCounts.TabIndex = 5;
        lblCounts.Text = "No run yet.";
        // 
        // grpLog
        // 
        grpLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpLog.Controls.Add(txtLog);
        grpLog.Location = new Point(494, 92);
        grpLog.Name = "grpLog";
        grpLog.Size = new Size(614, 568);
        grpLog.TabIndex = 3;
        grpLog.TabStop = false;
        grpLog.Text = "Activity log";
        // 
        // txtLog
        // 
        txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtLog.BackColor = Color.FromArgb(30, 30, 30);
        txtLog.BorderStyle = BorderStyle.None;
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.ForeColor = Color.Gainsboro;
        txtLog.Location = new Point(10, 22);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Both;
        txtLog.Size = new Size(594, 536);
        txtLog.TabIndex = 0;
        txtLog.WordWrap = false;
        // 
        // progressBar
        // 
        progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(12, 670);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(1096, 18);
        progressBar.TabIndex = 4;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.Location = new Point(12, 694);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(1096, 20);
        lblStatus.TabIndex = 5;
        lblStatus.Text = "Ready.";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1120, 726);
        Controls.Add(lblStatus);
        Controls.Add(progressBar);
        Controls.Add(grpLog);
        Controls.Add(panelLeft);
        Controls.Add(lblBanner);
        Controls.Add(lblHeader);
        MinimumSize = new Size(1000, 640);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "RansomSim - Simulation Only";
        panelLeft.ResumeLayout(false);
        grpTarget.ResumeLayout(false);
        grpTarget.PerformLayout();
        grpOptions.ResumeLayout(false);
        grpOptions.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numDelay).EndInit();
        ((System.ComponentModel.ISupportInitialize)numMaxFiles).EndInit();
        ((System.ComponentModel.ISupportInitialize)numMaxSize).EndInit();
        grpActions.ResumeLayout(false);
        grpLog.ResumeLayout(false);
        grpLog.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label lblHeader;
    private Label lblBanner;
    private Panel panelLeft;
    private GroupBox grpTarget;
    private RadioButton rdoLocal;
    private RadioButton rdoRemote;
    private Label lblPath;
    private TextBox txtPath;
    private Button btnBrowse;
    private Label lblHint;
    private GroupBox grpOptions;
    private CheckBox chkRecurse;
    private CheckBox chkDryRun;
    private CheckBox chkWriteNote;
    private Label lblDelay;
    private NumericUpDown numDelay;
    private Label lblMaxFiles;
    private NumericUpDown numMaxFiles;
    private Label lblMaxSize;
    private NumericUpDown numMaxSize;
    private Label lblPass;
    private TextBox txtPass;
    private Label lblExclusions;
    private TextBox txtExclusions;
    private Label lblGuard;
    private GroupBox grpActions;
    private Button btnPreview;
    private Button btnStart;
    private Button btnStop;
    private Button btnRestore;
    private Button btnOpenFolder;
    private Label lblCounts;
    private GroupBox grpLog;
    private TextBox txtLog;
    private ProgressBar progressBar;
    private Label lblStatus;
}
