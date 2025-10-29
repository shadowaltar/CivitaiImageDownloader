namespace CivitaiImageDownloader;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        txtUsername = new TextBox();
        label1 = new Label();
        btnDownload = new Button();
        listBoxMessages = new ListBox();
        txtTargetFolder = new TextBox();
        label2 = new Label();
        chbNsfw = new CheckBox();
        chbMature = new CheckBox();
        chbNormal = new CheckBox();
        chbChildLevel = new CheckBox();
        chbDownloadVideo = new CheckBox();
        btnDeleteInfoFiles = new Button();
        btnCopyFailedUrls = new Button();
        btnOpenFirstUserFolder = new Button();
        chbDownloadImage = new CheckBox();
        btnMarkDeletedFilesNoRedownload = new Button();
        SuspendLayout();
        // 
        // txtUsername
        // 
        txtUsername.Location = new Point(120, 12);
        txtUsername.Name = "txtUsername";
        txtUsername.Size = new Size(799, 30);
        txtUsername.TabIndex = 0;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(13, 14);
        label1.Name = "label1";
        label1.Size = new Size(96, 24);
        label1.TabIndex = 1;
        label1.Text = "Username";
        // 
        // btnDownload
        // 
        btnDownload.Location = new Point(13, 114);
        btnDownload.Name = "btnDownload";
        btnDownload.Size = new Size(272, 33);
        btnDownload.TabIndex = 2;
        btnDownload.Text = "Download";
        btnDownload.UseVisualStyleBackColor = true;
        btnDownload.Click += btnDownload_Click;
        // 
        // listBoxMessages
        // 
        listBoxMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listBoxMessages.Font = new Font("Cascadia Code", 10F);
        listBoxMessages.FormattingEnabled = true;
        listBoxMessages.ItemHeight = 27;
        listBoxMessages.Location = new Point(13, 235);
        listBoxMessages.Name = "listBoxMessages";
        listBoxMessages.Size = new Size(1531, 490);
        listBoxMessages.TabIndex = 3;
        listBoxMessages.DoubleClick += listBoxMessages_DoubleClick;
        // 
        // txtTargetFolder
        // 
        txtTargetFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtTargetFolder.Location = new Point(1061, 12);
        txtTargetFolder.Name = "txtTargetFolder";
        txtTargetFolder.Size = new Size(485, 30);
        txtTargetFolder.TabIndex = 4;
        // 
        // label2
        // 
        label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        label2.AutoSize = true;
        label2.Location = new Point(925, 14);
        label2.Name = "label2";
        label2.Size = new Size(155, 24);
        label2.TabIndex = 5;
        label2.Text = "ParentOutputDir";
        // 
        // chbNsfw
        // 
        chbNsfw.AutoSize = true;
        chbNsfw.Checked = true;
        chbNsfw.CheckState = CheckState.Checked;
        chbNsfw.Location = new Point(13, 66);
        chbNsfw.Name = "chbNsfw";
        chbNsfw.Size = new Size(89, 28);
        chbNsfw.TabIndex = 6;
        chbNsfw.Text = "NSFW";
        chbNsfw.UseVisualStyleBackColor = true;
        // 
        // chbMature
        // 
        chbMature.AutoSize = true;
        chbMature.Location = new Point(108, 66);
        chbMature.Name = "chbMature";
        chbMature.Size = new Size(99, 28);
        chbMature.TabIndex = 7;
        chbMature.Text = "Mature";
        chbMature.UseVisualStyleBackColor = true;
        // 
        // chbNormal
        // 
        chbNormal.AutoSize = true;
        chbNormal.Location = new Point(213, 66);
        chbNormal.Name = "chbNormal";
        chbNormal.Size = new Size(101, 28);
        chbNormal.TabIndex = 8;
        chbNormal.Text = "Normal";
        chbNormal.UseVisualStyleBackColor = true;
        // 
        // chbChildLevel
        // 
        chbChildLevel.AutoSize = true;
        chbChildLevel.Location = new Point(320, 66);
        chbChildLevel.Name = "chbChildLevel";
        chbChildLevel.Size = new Size(124, 28);
        chbChildLevel.TabIndex = 9;
        chbChildLevel.Text = "ChildLevel";
        chbChildLevel.UseVisualStyleBackColor = true;
        // 
        // chbDownloadVideo
        // 
        chbDownloadVideo.AutoSize = true;
        chbDownloadVideo.Checked = true;
        chbDownloadVideo.CheckState = CheckState.Checked;
        chbDownloadVideo.Location = new Point(525, 66);
        chbDownloadVideo.Name = "chbDownloadVideo";
        chbDownloadVideo.Size = new Size(86, 28);
        chbDownloadVideo.TabIndex = 10;
        chbDownloadVideo.Text = "Video";
        chbDownloadVideo.UseVisualStyleBackColor = true;
        // 
        // btnDeleteInfoFiles
        // 
        btnDeleteInfoFiles.Location = new Point(291, 114);
        btnDeleteInfoFiles.Name = "btnDeleteInfoFiles";
        btnDeleteInfoFiles.Size = new Size(272, 33);
        btnDeleteInfoFiles.TabIndex = 11;
        btnDeleteInfoFiles.Text = "Delete Info Files";
        btnDeleteInfoFiles.UseVisualStyleBackColor = true;
        btnDeleteInfoFiles.Click += btnDeleteInfoFiles_Click;
        // 
        // btnCopyFailedUrls
        // 
        btnCopyFailedUrls.Location = new Point(569, 114);
        btnCopyFailedUrls.Name = "btnCopyFailedUrls";
        btnCopyFailedUrls.Size = new Size(272, 33);
        btnCopyFailedUrls.TabIndex = 12;
        btnCopyFailedUrls.Text = "Copy Failed Urls";
        btnCopyFailedUrls.UseVisualStyleBackColor = true;
        btnCopyFailedUrls.Click += btnCopyFailedUrls_Click;
        // 
        // btnOpenFirstUserFolder
        // 
        btnOpenFirstUserFolder.Location = new Point(12, 153);
        btnOpenFirstUserFolder.Name = "btnOpenFirstUserFolder";
        btnOpenFirstUserFolder.Size = new Size(272, 33);
        btnOpenFirstUserFolder.TabIndex = 13;
        btnOpenFirstUserFolder.Text = "Open First User Folder";
        btnOpenFirstUserFolder.UseVisualStyleBackColor = true;
        btnOpenFirstUserFolder.Click += btnOpenFirstUserFolder_Click;
        // 
        // chbDownloadImage
        // 
        chbDownloadImage.AutoSize = true;
        chbDownloadImage.Checked = true;
        chbDownloadImage.CheckState = CheckState.Checked;
        chbDownloadImage.Location = new Point(617, 66);
        chbDownloadImage.Name = "chbDownloadImage";
        chbDownloadImage.Size = new Size(90, 28);
        chbDownloadImage.TabIndex = 14;
        chbDownloadImage.Text = "Image";
        chbDownloadImage.UseVisualStyleBackColor = true;
        // 
        // btnMarkDeletedFilesNoRedownload
        // 
        btnMarkDeletedFilesNoRedownload.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnMarkDeletedFilesNoRedownload.Location = new Point(1274, 114);
        btnMarkDeletedFilesNoRedownload.Name = "btnMarkDeletedFilesNoRedownload";
        btnMarkDeletedFilesNoRedownload.Size = new Size(272, 72);
        btnMarkDeletedFilesNoRedownload.TabIndex = 15;
        btnMarkDeletedFilesNoRedownload.Text = "Mark Deleted Files No Redownload";
        btnMarkDeletedFilesNoRedownload.UseVisualStyleBackColor = true;
        btnMarkDeletedFilesNoRedownload.Click += btnMarkDeletedFilesNoRedownload_ClickAsync;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(11F, 24F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1558, 783);
        Controls.Add(btnMarkDeletedFilesNoRedownload);
        Controls.Add(chbDownloadImage);
        Controls.Add(btnOpenFirstUserFolder);
        Controls.Add(btnCopyFailedUrls);
        Controls.Add(btnDeleteInfoFiles);
        Controls.Add(chbDownloadVideo);
        Controls.Add(chbChildLevel);
        Controls.Add(chbNormal);
        Controls.Add(chbMature);
        Controls.Add(chbNsfw);
        Controls.Add(label2);
        Controls.Add(txtTargetFolder);
        Controls.Add(listBoxMessages);
        Controls.Add(btnDownload);
        Controls.Add(label1);
        Controls.Add(txtUsername);
        Name = "Form1";
        Text = "Form1";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TextBox txtUsername;
    private Label label1;
    private Button btnDownload;
    private ListBox listBoxMessages;
    private TextBox txtTargetFolder;
    private Label label2;
    private CheckBox chbNsfw;
    private CheckBox chbMature;
    private CheckBox chbNormal;
    private CheckBox chbChildLevel;
    private CheckBox chbDownloadVideo;
    private Button btnDeleteInfoFiles;
    private Button btnCopyFailedUrls;
    private Button btnOpenFirstUserFolder;
    private CheckBox chbDownloadImage;
    private Button btnMarkDeletedFilesNoRedownload;
}
