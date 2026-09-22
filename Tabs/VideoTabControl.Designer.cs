namespace CivitaiImageDownloader.Tabs;

partial class VideoTabControl
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
        listBoxVideoProcessingMessages = new ListBox();
        btnCopyFromDownloadTab = new Button();
        label4 = new Label();
        txtVideoProcessingUsers = new TextBox();
        btnCompressVideo = new Button();
        btnEnhanceFrameRate = new Button();
        btnSelectFolder = new Button();
        lblMinFps = new Label();
        txtMinFps = new TextBox();
        lblTargetFps = new Label();
        txtTargetFps = new TextBox();
        SuspendLayout();
        // 
        // listBoxVideoProcessingMessages
        // 
        listBoxVideoProcessingMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listBoxVideoProcessingMessages.Font = new Font("Cascadia Code", 10F);
        listBoxVideoProcessingMessages.FormattingEnabled = true;
        listBoxVideoProcessingMessages.IntegralHeight = false;
        listBoxVideoProcessingMessages.ItemHeight = 27;
        listBoxVideoProcessingMessages.Location = new Point(6, 120);
        listBoxVideoProcessingMessages.Name = "listBoxVideoProcessingMessages";
        listBoxVideoProcessingMessages.Size = new Size(3687, 1300);
        listBoxVideoProcessingMessages.TabIndex = 0;
        listBoxVideoProcessingMessages.DoubleClick += listBoxVideoProcessingMessages_DoubleClick;
        // 
        // btnCopyFromDownloadTab
        // 
        btnCopyFromDownloadTab.Location = new Point(6, 6);
        btnCopyFromDownloadTab.Name = "btnCopyFromDownloadTab";
        btnCopyFromDownloadTab.Size = new Size(350, 34);
        btnCopyFromDownloadTab.TabIndex = 1;
        btnCopyFromDownloadTab.Text = "Copy usernames from Download Tab";
        btnCopyFromDownloadTab.UseVisualStyleBackColor = true;
        btnCopyFromDownloadTab.Click += btnCopyFromDownloadTab_Click;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new Point(6, 50);
        label4.Name = "label4";
        label4.Size = new Size(96, 24);
        label4.TabIndex = 2;
        label4.Text = "Username";
        // 
        // txtVideoProcessingUsers
        // 
        txtVideoProcessingUsers.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtVideoProcessingUsers.Location = new Point(99, 47);
        txtVideoProcessingUsers.Name = "txtVideoProcessingUsers";
        txtVideoProcessingUsers.Size = new Size(3594, 30);
        txtVideoProcessingUsers.TabIndex = 3;
        // 
        // btnCompressVideo
        // 
        btnCompressVideo.Location = new Point(6, 80);
        btnCompressVideo.Name = "btnCompressVideo";
        btnCompressVideo.Size = new Size(272, 34);
        btnCompressVideo.TabIndex = 4;
        btnCompressVideo.Text = "Compress Video";
        btnCompressVideo.UseVisualStyleBackColor = true;
        btnCompressVideo.Click += btnCompressVideo_Click;
        // 
        // btnEnhanceFrameRate
        // 
        btnEnhanceFrameRate.Location = new Point(284, 80);
        btnEnhanceFrameRate.Name = "btnEnhanceFrameRate";
        btnEnhanceFrameRate.Size = new Size(240, 34);
        btnEnhanceFrameRate.TabIndex = 5;
        btnEnhanceFrameRate.Text = "Enhance Frame Rate";
        btnEnhanceFrameRate.UseVisualStyleBackColor = true;
        btnEnhanceFrameRate.Click += btnEnhanceFrameRate_Click;
        // 
        // btnSelectFolder
        // 
        btnSelectFolder.Location = new Point(530, 80);
        btnSelectFolder.Name = "btnSelectFolder";
        btnSelectFolder.Size = new Size(220, 34);
        btnSelectFolder.TabIndex = 6;
        btnSelectFolder.Text = "Select Folder...";
        btnSelectFolder.UseVisualStyleBackColor = true;
        btnSelectFolder.Click += btnSelectFolder_Click;
        // 
        // lblMinFps
        // 
        lblMinFps.AutoSize = true;
        lblMinFps.Location = new Point(758, 86);
        lblMinFps.Name = "lblMinFps";
        lblMinFps.Size = new Size(69, 24);
        lblMinFps.TabIndex = 7;
        lblMinFps.Text = "Min FPS";
        // 
        // txtMinFps
        // 
        txtMinFps.Location = new Point(833, 80);
        txtMinFps.Name = "txtMinFps";
        txtMinFps.Size = new Size(50, 30);
        txtMinFps.TabIndex = 8;
        txtMinFps.Text = "24";
        // 
        // lblTargetFps
        // 
        lblTargetFps.AutoSize = true;
        lblTargetFps.Location = new Point(891, 86);
        lblTargetFps.Name = "lblTargetFps";
        lblTargetFps.Size = new Size(86, 24);
        lblTargetFps.TabIndex = 9;
        lblTargetFps.Text = "Target FPS";
        // 
        // txtTargetFps
        // 
        txtTargetFps.Location = new Point(983, 80);
        txtTargetFps.Name = "txtTargetFps";
        txtTargetFps.Size = new Size(50, 30);
        txtTargetFps.TabIndex = 10;
        txtTargetFps.Text = "30";
        // 
        // VideoTabControl
        // 
        Controls.Add(txtTargetFps);
        Controls.Add(lblTargetFps);
        Controls.Add(txtMinFps);
        Controls.Add(lblMinFps);
        Controls.Add(btnSelectFolder);
        Controls.Add(btnEnhanceFrameRate);
        Controls.Add(btnCompressVideo);
        Controls.Add(txtVideoProcessingUsers);
        Controls.Add(label4);
        Controls.Add(btnCopyFromDownloadTab);
        Controls.Add(listBoxVideoProcessingMessages);
        Name = "VideoTabControl";
        Size = new Size(1603, 708);
        ResumeLayout(false);
        PerformLayout();
    }

    private ListBox listBoxVideoProcessingMessages;
    private Button btnCompressVideo;
    private Label label4;
    private TextBox txtVideoProcessingUsers;
    private Button btnCopyFromDownloadTab;
    private Button btnEnhanceFrameRate;
    private Button btnSelectFolder;
    private Label lblMinFps;
    private TextBox txtMinFps;
    private Label lblTargetFps;
    private TextBox txtTargetFps;
}
