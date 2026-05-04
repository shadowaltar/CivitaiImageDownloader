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
        SuspendLayout();
        // 
        // listBoxVideoProcessingMessages
        // 
        listBoxVideoProcessingMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listBoxVideoProcessingMessages.Font = new Font("Cascadia Code", 10F);
        listBoxVideoProcessingMessages.FormattingEnabled = true;
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
        // VideoTabControl
        // 
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
}
