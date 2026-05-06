namespace CivitaiImageDownloader.Tabs;

partial class DownloadTabControl
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
        btnCompressInfo = new Button();
        btnSetUserNameTextByRating = new Button();
        btnMoveUsersToRating = new Button();
        chb6Star = new CheckBox();
        chb5Star = new CheckBox();
        chb4p5Star = new CheckBox();
        chb4Star = new CheckBox();
        chb3Star = new CheckBox();
        label3 = new Label();
        btnOpenAllUserFolders = new Button();
        btnCopyAllSubdirNames = new Button();
        lblDownloadingCounter = new Label();
        btnStop = new Button();
        btnMarkDeletedFilesNoRedownload = new Button();
        chbDownloadImage = new CheckBox();
        btnOpenFirstUserFolder = new Button();
        btnCopyFailedUrls = new Button();
        btnDeleteInfoFiles = new Button();
        chbDownloadVideo = new CheckBox();
        chbChildLevel = new CheckBox();
        chbNormal = new CheckBox();
        chbMature = new CheckBox();
        chbAlwaysDownloadLatest = new CheckBox();
        chbNsfw = new CheckBox();
        listBoxMessages = new ListBox();
        btnDownload = new Button();
        label1 = new Label();
        txtUsernames = new TextBox();
        btnShowFirstUserInViewer = new Button();
        SuspendLayout();
        // 
        // btnCompressInfo
        // 
        btnCompressInfo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCompressInfo.Location = new Point(2331, 42);
        btnCompressInfo.Name = "btnCompressInfo";
        btnCompressInfo.Size = new Size(272, 33);
        btnCompressInfo.TabIndex = 0;
        btnCompressInfo.Text = "Compress Info Files";
        btnCompressInfo.UseVisualStyleBackColor = true;
        btnCompressInfo.Click += btnCompressInfo_Click;
        // 
        // btnSetUserNameTextByRating
        // 
        btnSetUserNameTextByRating.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSetUserNameTextByRating.Location = new Point(2331, 157);
        btnSetUserNameTextByRating.Name = "btnSetUserNameTextByRating";
        btnSetUserNameTextByRating.Size = new Size(272, 33);
        btnSetUserNameTextByRating.TabIndex = 1;
        btnSetUserNameTextByRating.Text = "Set usernames by rating";
        // 
        // btnMoveUsersToRating
        // 
        btnMoveUsersToRating.Location = new Point(1544, 42);
        btnMoveUsersToRating.Name = "btnMoveUsersToRating";
        btnMoveUsersToRating.Size = new Size(160, 98);
        btnMoveUsersToRating.TabIndex = 2;
        btnMoveUsersToRating.Text = "Move users to rating folder";
        btnMoveUsersToRating.Click += btnMoveUsersToRating_Click;
        // 
        // chb6Star
        // 
        chb6Star.Appearance = Appearance.Button;
        chb6Star.Location = new Point(1324, 96);
        chb6Star.Name = "chb6Star";
        chb6Star.Size = new Size(104, 47);
        chb6Star.TabIndex = 3;
        chb6Star.Text = "6 star";
        // 
        // chb5Star
        // 
        chb5Star.Appearance = Appearance.Button;
        chb5Star.Location = new Point(1214, 96);
        chb5Star.Name = "chb5Star";
        chb5Star.Size = new Size(104, 48);
        chb5Star.TabIndex = 4;
        chb5Star.Text = "5 star";
        // 
        // chb4p5Star
        // 
        chb4p5Star.Appearance = Appearance.Button;
        chb4p5Star.Location = new Point(1434, 42);
        chb4p5Star.Name = "chb4p5Star";
        chb4p5Star.Size = new Size(104, 48);
        chb4p5Star.TabIndex = 5;
        chb4p5Star.Text = "4.5 star";
        // 
        // chb4Star
        // 
        chb4Star.Appearance = Appearance.Button;
        chb4Star.Location = new Point(1324, 42);
        chb4Star.Name = "chb4Star";
        chb4Star.Size = new Size(104, 48);
        chb4Star.TabIndex = 6;
        chb4Star.Text = "4 star";
        // 
        // chb3Star
        // 
        chb3Star.Appearance = Appearance.Button;
        chb3Star.Location = new Point(1214, 42);
        chb3Star.Name = "chb3Star";
        chb3Star.Size = new Size(104, 48);
        chb3Star.TabIndex = 7;
        chb3Star.Text = "3 star";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(1129, 54);
        label3.Name = "label3";
        label3.Size = new Size(79, 24);
        label3.TabIndex = 8;
        label3.Text = "Ratings:";
        // 
        // btnOpenAllUserFolders
        // 
        btnOpenAllUserFolders.Location = new Point(666, 112);
        btnOpenAllUserFolders.Name = "btnOpenAllUserFolders";
        btnOpenAllUserFolders.Size = new Size(195, 64);
        btnOpenAllUserFolders.TabIndex = 9;
        btnOpenAllUserFolders.Text = "Open all users' folders";
        btnOpenAllUserFolders.Click += btnOpenAllUserFolders_Click;
        // 
        // btnCopyAllSubdirNames
        // 
        btnCopyAllSubdirNames.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCopyAllSubdirNames.Location = new Point(2053, 156);
        btnCopyAllSubdirNames.Name = "btnCopyAllSubdirNames";
        btnCopyAllSubdirNames.Size = new Size(272, 34);
        btnCopyAllSubdirNames.TabIndex = 10;
        btnCopyAllSubdirNames.Text = "Copy all subdirectory names";
        btnCopyAllSubdirNames.Click += btnCopyAllSubdirNames_Click;
        // 
        // lblDownloadingCounter
        // 
        lblDownloadingCounter.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        lblDownloadingCounter.AutoSize = true;
        lblDownloadingCounter.Location = new Point(2355, 1235);
        lblDownloadingCounter.Name = "lblDownloadingCounter";
        lblDownloadingCounter.Size = new Size(248, 24);
        lblDownloadingCounter.TabIndex = 11;
        lblDownloadingCounter.Text = "Not downloading anything.";
        // 
        // btnStop
        // 
        btnStop.Location = new Point(528, 42);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(132, 64);
        btnStop.TabIndex = 12;
        btnStop.Text = "Stop";
        btnStop.Click += btnStop_Click;
        // 
        // btnMarkDeletedFilesNoRedownload
        // 
        btnMarkDeletedFilesNoRedownload.Location = new Point(306, 42);
        btnMarkDeletedFilesNoRedownload.Name = "btnMarkDeletedFilesNoRedownload";
        btnMarkDeletedFilesNoRedownload.Size = new Size(216, 64);
        btnMarkDeletedFilesNoRedownload.TabIndex = 13;
        btnMarkDeletedFilesNoRedownload.Text = "Mark Deleted Files No Redownload";
        btnMarkDeletedFilesNoRedownload.Click += btnMarkDeletedFilesNoRedownload_Click;
        // 
        // chbDownloadImage
        // 
        chbDownloadImage.AutoSize = true;
        chbDownloadImage.Location = new Point(101, 146);
        chbDownloadImage.Name = "chbDownloadImage";
        chbDownloadImage.Size = new Size(90, 28);
        chbDownloadImage.TabIndex = 14;
        chbDownloadImage.Text = "Image";
        chbDownloadImage.UseVisualStyleBackColor = true;
        // 
        // btnOpenFirstUserFolder
        // 
        btnOpenFirstUserFolder.Location = new Point(666, 42);
        btnOpenFirstUserFolder.Name = "btnOpenFirstUserFolder";
        btnOpenFirstUserFolder.Size = new Size(195, 64);
        btnOpenFirstUserFolder.TabIndex = 15;
        btnOpenFirstUserFolder.Text = "Open first user folder";
        btnOpenFirstUserFolder.Click += btnOpenFirstUserFolder_Click;
        // 
        // btnCopyFailedUrls
        // 
        btnCopyFailedUrls.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCopyFailedUrls.Location = new Point(2331, 81);
        btnCopyFailedUrls.Name = "btnCopyFailedUrls";
        btnCopyFailedUrls.Size = new Size(272, 34);
        btnCopyFailedUrls.TabIndex = 16;
        btnCopyFailedUrls.Text = "Copy failed URLs";
        btnCopyFailedUrls.Click += btnCopyFailedUrls_Click;
        // 
        // btnDeleteInfoFiles
        // 
        btnDeleteInfoFiles.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnDeleteInfoFiles.Location = new Point(2331, 121);
        btnDeleteInfoFiles.Name = "btnDeleteInfoFiles";
        btnDeleteInfoFiles.Size = new Size(272, 34);
        btnDeleteInfoFiles.TabIndex = 17;
        btnDeleteInfoFiles.Text = "Delete Info Files";
        btnDeleteInfoFiles.Click += btnDeleteInfoFiles_Click;
        // 
        // chbDownloadVideo
        // 
        chbDownloadVideo.AutoSize = true;
        chbDownloadVideo.Checked = true;
        chbDownloadVideo.CheckState = CheckState.Checked;
        chbDownloadVideo.Location = new Point(6, 146);
        chbDownloadVideo.Name = "chbDownloadVideo";
        chbDownloadVideo.Size = new Size(86, 28);
        chbDownloadVideo.TabIndex = 18;
        chbDownloadVideo.Text = "Video";
        chbDownloadVideo.UseVisualStyleBackColor = true;
        // 
        // chbChildLevel
        // 
        chbChildLevel.AutoSize = true;
        chbChildLevel.Location = new Point(313, 112);
        chbChildLevel.Name = "chbChildLevel";
        chbChildLevel.Size = new Size(81, 28);
        chbChildLevel.TabIndex = 19;
        chbChildLevel.Text = "Child";
        chbChildLevel.UseVisualStyleBackColor = true;
        // 
        // chbNormal
        // 
        chbNormal.AutoSize = true;
        chbNormal.Location = new Point(206, 112);
        chbNormal.Name = "chbNormal";
        chbNormal.Size = new Size(101, 28);
        chbNormal.TabIndex = 20;
        chbNormal.Text = "Normal";
        chbNormal.UseVisualStyleBackColor = true;
        // 
        // chbMature
        // 
        chbMature.AutoSize = true;
        chbMature.Location = new Point(101, 112);
        chbMature.Name = "chbMature";
        chbMature.Size = new Size(99, 28);
        chbMature.TabIndex = 21;
        chbMature.Text = "Mature";
        chbMature.UseVisualStyleBackColor = true;
        // 
        // chbAlwaysDownloadLatest
        // 
        chbAlwaysDownloadLatest.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chbAlwaysDownloadLatest.AutoSize = true;
        chbAlwaysDownloadLatest.Location = new Point(2026, 125);
        chbAlwaysDownloadLatest.Name = "chbAlwaysDownloadLatest";
        chbAlwaysDownloadLatest.Size = new Size(299, 28);
        chbAlwaysDownloadLatest.TabIndex = 22;
        chbAlwaysDownloadLatest.Text = "Skip Fetching Latest Index Info";
        chbAlwaysDownloadLatest.UseVisualStyleBackColor = true;
        // 
        // chbNsfw
        // 
        chbNsfw.AutoSize = true;
        chbNsfw.Checked = true;
        chbNsfw.CheckState = CheckState.Checked;
        chbNsfw.Location = new Point(6, 112);
        chbNsfw.Name = "chbNsfw";
        chbNsfw.Size = new Size(89, 28);
        chbNsfw.TabIndex = 23;
        chbNsfw.Text = "NSFW";
        chbNsfw.UseVisualStyleBackColor = true;
        // 
        // listBoxMessages
        // 
        listBoxMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listBoxMessages.Font = new Font("Cascadia Code", 10F);
        listBoxMessages.FormattingEnabled = true;
        listBoxMessages.ItemHeight = 27;
        listBoxMessages.Location = new Point(6, 202);
        listBoxMessages.Name = "listBoxMessages";
        listBoxMessages.Size = new Size(2597, 1030);
        listBoxMessages.TabIndex = 24;
        listBoxMessages.DoubleClick += listBoxMessages_DoubleClick;
        // 
        // btnDownload
        // 
        btnDownload.Location = new Point(6, 43);
        btnDownload.Name = "btnDownload";
        btnDownload.Size = new Size(294, 63);
        btnDownload.TabIndex = 25;
        btnDownload.Text = "Download";
        btnDownload.Click += btnDownload_Click;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(6, 9);
        label1.Name = "label1";
        label1.Size = new Size(96, 24);
        label1.TabIndex = 26;
        label1.Text = "Username";
        // 
        // txtUsernames
        // 
        txtUsernames.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtUsernames.Location = new Point(108, 6);
        txtUsernames.Name = "txtUsernames";
        txtUsernames.Size = new Size(2495, 30);
        txtUsernames.TabIndex = 27;
        // 
        // btnShowFirstUserInViewer
        // 
        btnShowFirstUserInViewer.Location = new Point(867, 42);
        btnShowFirstUserInViewer.Name = "btnShowFirstUserInViewer";
        btnShowFirstUserInViewer.Size = new Size(195, 64);
        btnShowFirstUserInViewer.TabIndex = 28;
        btnShowFirstUserInViewer.Text = "Show first user in Viewer";
        // 
        // DownloadTabControl
        // 
        Controls.Add(btnShowFirstUserInViewer);
        Controls.Add(btnCompressInfo);
        Controls.Add(btnSetUserNameTextByRating);
        Controls.Add(btnMoveUsersToRating);
        Controls.Add(chb6Star);
        Controls.Add(chb5Star);
        Controls.Add(chb4p5Star);
        Controls.Add(chb4Star);
        Controls.Add(chb3Star);
        Controls.Add(label3);
        Controls.Add(btnOpenAllUserFolders);
        Controls.Add(btnCopyAllSubdirNames);
        Controls.Add(lblDownloadingCounter);
        Controls.Add(btnStop);
        Controls.Add(btnMarkDeletedFilesNoRedownload);
        Controls.Add(chbDownloadImage);
        Controls.Add(btnOpenFirstUserFolder);
        Controls.Add(btnCopyFailedUrls);
        Controls.Add(btnDeleteInfoFiles);
        Controls.Add(chbDownloadVideo);
        Controls.Add(chbChildLevel);
        Controls.Add(chbNormal);
        Controls.Add(chbMature);
        Controls.Add(chbAlwaysDownloadLatest);
        Controls.Add(chbNsfw);
        Controls.Add(listBoxMessages);
        Controls.Add(btnDownload);
        Controls.Add(label1);
        Controls.Add(txtUsernames);
        Name = "DownloadTabControl";
        Size = new Size(2606, 1266);
        ResumeLayout(false);
        PerformLayout();
    }

    public Button btnCompressInfo;
    private Button btnSetUserNameTextByRating;
    private Button btnMoveUsersToRating;
    private CheckBox chb6Star;
    private CheckBox chb5Star;
    private CheckBox chb4p5Star;
    private CheckBox chb4Star;
    private CheckBox chb3Star;
    private Label label3;
    private Button btnOpenAllUserFolders;
    private Button btnCopyAllSubdirNames;
    private Label lblDownloadingCounter;
    private Button btnStop;
    private Button btnMarkDeletedFilesNoRedownload;
    private CheckBox chbDownloadImage;
    private Button btnOpenFirstUserFolder;
    private Button btnCopyFailedUrls;
    private Button btnDeleteInfoFiles;
    public CheckBox chbDownloadVideo;
    private CheckBox chbChildLevel;
    private CheckBox chbNormal;
    private CheckBox chbMature;
    private CheckBox chbAlwaysDownloadLatest;
    private CheckBox chbNsfw;
    private ListBox listBoxMessages;
    private Button btnDownload;
    private Label label1;
    internal TextBox txtUsernames;
    private Button btnShowFirstUserInViewer;
}
