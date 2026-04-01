namespace CivitaiImageDownloader;

partial class MainForm
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
        mainTabControl = new TabControl();
        DownloadPage = new TabPage();
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
        tabPage2 = new TabPage();
        listBoxVideoProcessingMessages = new ListBox();
        btnCopyFromDownloadTab = new Button();
        label4 = new Label();
        txtVideoProcessingUsers = new TextBox();
        btnCompressVideo = new Button();
        label2 = new Label();
        txtTargetFolder = new TextBox();
        mainTabControl.SuspendLayout();
        DownloadPage.SuspendLayout();
        tabPage2.SuspendLayout();
        SuspendLayout();
        // 
        // mainTabControl
        // 
        mainTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        mainTabControl.Controls.Add(DownloadPage);
        mainTabControl.Controls.Add(tabPage2);
        mainTabControl.Location = new Point(12, 12);
        mainTabControl.Name = "mainTabControl";
        mainTabControl.SelectedIndex = 0;
        mainTabControl.Size = new Size(2254, 1008);
        mainTabControl.TabIndex = 21;
        // 
        // DownloadPage
        // 
        DownloadPage.Controls.Add(btnSetUserNameTextByRating);
        DownloadPage.Controls.Add(btnMoveUsersToRating);
        DownloadPage.Controls.Add(chb6Star);
        DownloadPage.Controls.Add(chb5Star);
        DownloadPage.Controls.Add(chb4p5Star);
        DownloadPage.Controls.Add(chb4Star);
        DownloadPage.Controls.Add(chb3Star);
        DownloadPage.Controls.Add(label3);
        DownloadPage.Controls.Add(btnOpenAllUserFolders);
        DownloadPage.Controls.Add(btnCopyAllSubdirNames);
        DownloadPage.Controls.Add(lblDownloadingCounter);
        DownloadPage.Controls.Add(btnStop);
        DownloadPage.Controls.Add(btnMarkDeletedFilesNoRedownload);
        DownloadPage.Controls.Add(chbDownloadImage);
        DownloadPage.Controls.Add(btnOpenFirstUserFolder);
        DownloadPage.Controls.Add(btnCopyFailedUrls);
        DownloadPage.Controls.Add(btnDeleteInfoFiles);
        DownloadPage.Controls.Add(chbDownloadVideo);
        DownloadPage.Controls.Add(chbChildLevel);
        DownloadPage.Controls.Add(chbNormal);
        DownloadPage.Controls.Add(chbMature);
        DownloadPage.Controls.Add(chbAlwaysDownloadLatest);
        DownloadPage.Controls.Add(chbNsfw);
        DownloadPage.Controls.Add(listBoxMessages);
        DownloadPage.Controls.Add(btnDownload);
        DownloadPage.Controls.Add(label1);
        DownloadPage.Controls.Add(txtUsernames);
        DownloadPage.Location = new Point(4, 33);
        DownloadPage.Name = "DownloadPage";
        DownloadPage.Padding = new Padding(3);
        DownloadPage.Size = new Size(2246, 971);
        DownloadPage.TabIndex = 0;
        DownloadPage.Text = "Download";
        DownloadPage.UseVisualStyleBackColor = true;
        // 
        // btnSetUserNameTextByRating
        // 
        btnSetUserNameTextByRating.Location = new Point(1241, 42);
        btnSetUserNameTextByRating.Name = "btnSetUserNameTextByRating";
        btnSetUserNameTextByRating.Size = new Size(200, 62);
        btnSetUserNameTextByRating.TabIndex = 57;
        btnSetUserNameTextByRating.Text = "Populate Usernames by Selected Rating";
        btnSetUserNameTextByRating.UseVisualStyleBackColor = true;
        // 
        // btnMoveUsersToRating
        // 
        btnMoveUsersToRating.Location = new Point(1035, 42);
        btnMoveUsersToRating.Name = "btnMoveUsersToRating";
        btnMoveUsersToRating.Size = new Size(200, 62);
        btnMoveUsersToRating.TabIndex = 56;
        btnMoveUsersToRating.Text = "Move Users to Selected Rating";
        btnMoveUsersToRating.UseVisualStyleBackColor = true;
        btnMoveUsersToRating.Click += btnMoveUsersToRating_Click;
        // 
        // chb6Star
        // 
        chb6Star.Appearance = Appearance.Button;
        chb6Star.Location = new Point(957, 42);
        chb6Star.Name = "chb6Star";
        chb6Star.Size = new Size(72, 62);
        chb6Star.TabIndex = 55;
        chb6Star.Text = "6 ★";
        chb6Star.TextAlign = ContentAlignment.MiddleCenter;
        chb6Star.UseVisualStyleBackColor = true;
        // 
        // chb5Star
        // 
        chb5Star.Appearance = Appearance.Button;
        chb5Star.Location = new Point(879, 42);
        chb5Star.Name = "chb5Star";
        chb5Star.Size = new Size(72, 62);
        chb5Star.TabIndex = 54;
        chb5Star.Text = "5 ★";
        chb5Star.TextAlign = ContentAlignment.MiddleCenter;
        chb5Star.UseVisualStyleBackColor = true;
        // 
        // chb4p5Star
        // 
        chb4p5Star.Appearance = Appearance.Button;
        chb4p5Star.Location = new Point(801, 42);
        chb4p5Star.Name = "chb4p5Star";
        chb4p5Star.Size = new Size(72, 62);
        chb4p5Star.TabIndex = 53;
        chb4p5Star.Text = "4.5 ★";
        chb4p5Star.TextAlign = ContentAlignment.MiddleCenter;
        chb4p5Star.UseVisualStyleBackColor = true;
        // 
        // chb4Star
        // 
        chb4Star.Appearance = Appearance.Button;
        chb4Star.Location = new Point(723, 42);
        chb4Star.Name = "chb4Star";
        chb4Star.Size = new Size(72, 62);
        chb4Star.TabIndex = 52;
        chb4Star.Text = "4 ★";
        chb4Star.TextAlign = ContentAlignment.MiddleCenter;
        chb4Star.UseVisualStyleBackColor = true;
        // 
        // chb3Star
        // 
        chb3Star.Appearance = Appearance.Button;
        chb3Star.Location = new Point(645, 42);
        chb3Star.Name = "chb3Star";
        chb3Star.Size = new Size(72, 62);
        chb3Star.TabIndex = 51;
        chb3Star.Text = "3 ★";
        chb3Star.TextAlign = ContentAlignment.MiddleCenter;
        chb3Star.UseVisualStyleBackColor = true;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(560, 61);
        label3.Name = "label3";
        label3.Size = new Size(79, 24);
        label3.TabIndex = 50;
        label3.Text = "Ratings:";
        // 
        // btnOpenAllUserFolders
        // 
        btnOpenAllUserFolders.Location = new Point(923, 110);
        btnOpenAllUserFolders.Name = "btnOpenAllUserFolders";
        btnOpenAllUserFolders.Size = new Size(272, 64);
        btnOpenAllUserFolders.TabIndex = 44;
        btnOpenAllUserFolders.Text = "Open All User Folders";
        btnOpenAllUserFolders.UseVisualStyleBackColor = true;
        btnOpenAllUserFolders.Click += btnOpenAllUserFolders_Click;
        // 
        // btnCopyAllSubdirNames
        // 
        btnCopyAllSubdirNames.Location = new Point(1968, 190);
        btnCopyAllSubdirNames.Name = "btnCopyAllSubdirNames";
        btnCopyAllSubdirNames.Size = new Size(272, 33);
        btnCopyAllSubdirNames.TabIndex = 43;
        btnCopyAllSubdirNames.Text = "Copy All Subdir Names";
        btnCopyAllSubdirNames.UseVisualStyleBackColor = true;
        btnCopyAllSubdirNames.Click += btnCopyAllSubdirNames_Click;
        // 
        // lblDownloadingCounter
        // 
        lblDownloadingCounter.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        lblDownloadingCounter.AutoSize = true;
        lblDownloadingCounter.Location = new Point(2177, 944);
        lblDownloadingCounter.Name = "lblDownloadingCounter";
        lblDownloadingCounter.Size = new Size(63, 24);
        lblDownloadingCounter.TabIndex = 40;
        lblDownloadingCounter.Text = "label3";
        lblDownloadingCounter.TextAlign = ContentAlignment.MiddleRight;
        // 
        // btnStop
        // 
        btnStop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnStop.Location = new Point(1690, 190);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(272, 33);
        btnStop.TabIndex = 39;
        btnStop.Text = "STOP";
        btnStop.UseVisualStyleBackColor = true;
        btnStop.Click += btnStop_Click;
        // 
        // btnMarkDeletedFilesNoRedownload
        // 
        btnMarkDeletedFilesNoRedownload.Location = new Point(284, 110);
        btnMarkDeletedFilesNoRedownload.Name = "btnMarkDeletedFilesNoRedownload";
        btnMarkDeletedFilesNoRedownload.Size = new Size(355, 64);
        btnMarkDeletedFilesNoRedownload.TabIndex = 38;
        btnMarkDeletedFilesNoRedownload.Text = "Mark Deleted Files No Redownload";
        btnMarkDeletedFilesNoRedownload.UseVisualStyleBackColor = true;
        btnMarkDeletedFilesNoRedownload.Click += btnMarkDeletedFilesNoRedownload_Click;
        // 
        // chbDownloadImage
        // 
        chbDownloadImage.AutoSize = true;
        chbDownloadImage.Checked = true;
        chbDownloadImage.CheckState = CheckState.Checked;
        chbDownloadImage.Location = new Point(391, 76);
        chbDownloadImage.Name = "chbDownloadImage";
        chbDownloadImage.Size = new Size(90, 28);
        chbDownloadImage.TabIndex = 37;
        chbDownloadImage.Text = "Image";
        chbDownloadImage.UseVisualStyleBackColor = true;
        // 
        // btnOpenFirstUserFolder
        // 
        btnOpenFirstUserFolder.Location = new Point(645, 110);
        btnOpenFirstUserFolder.Name = "btnOpenFirstUserFolder";
        btnOpenFirstUserFolder.Size = new Size(272, 64);
        btnOpenFirstUserFolder.TabIndex = 36;
        btnOpenFirstUserFolder.Text = "Open First User Folder";
        btnOpenFirstUserFolder.UseVisualStyleBackColor = true;
        btnOpenFirstUserFolder.Click += btnOpenFirstUserFolder_Click;
        // 
        // btnCopyFailedUrls
        // 
        btnCopyFailedUrls.Location = new Point(1968, 151);
        btnCopyFailedUrls.Name = "btnCopyFailedUrls";
        btnCopyFailedUrls.Size = new Size(272, 33);
        btnCopyFailedUrls.TabIndex = 35;
        btnCopyFailedUrls.Text = "Copy Failed Urls";
        btnCopyFailedUrls.UseVisualStyleBackColor = true;
        btnCopyFailedUrls.Click += btnCopyFailedUrls_Click;
        // 
        // btnDeleteInfoFiles
        // 
        btnDeleteInfoFiles.Location = new Point(1968, 112);
        btnDeleteInfoFiles.Name = "btnDeleteInfoFiles";
        btnDeleteInfoFiles.Size = new Size(272, 33);
        btnDeleteInfoFiles.TabIndex = 34;
        btnDeleteInfoFiles.Text = "Delete Info Files";
        btnDeleteInfoFiles.UseVisualStyleBackColor = true;
        btnDeleteInfoFiles.Click += btnDeleteInfoFiles_Click;
        // 
        // chbDownloadVideo
        // 
        chbDownloadVideo.AutoSize = true;
        chbDownloadVideo.Checked = true;
        chbDownloadVideo.CheckState = CheckState.Checked;
        chbDownloadVideo.Location = new Point(391, 42);
        chbDownloadVideo.Name = "chbDownloadVideo";
        chbDownloadVideo.Size = new Size(86, 28);
        chbDownloadVideo.TabIndex = 33;
        chbDownloadVideo.Text = "Video";
        chbDownloadVideo.UseVisualStyleBackColor = true;
        // 
        // chbChildLevel
        // 
        chbChildLevel.AutoSize = true;
        chbChildLevel.Location = new Point(242, 76);
        chbChildLevel.Name = "chbChildLevel";
        chbChildLevel.Size = new Size(124, 28);
        chbChildLevel.TabIndex = 32;
        chbChildLevel.Text = "ChildLevel";
        chbChildLevel.UseVisualStyleBackColor = true;
        // 
        // chbNormal
        // 
        chbNormal.AutoSize = true;
        chbNormal.Location = new Point(108, 76);
        chbNormal.Name = "chbNormal";
        chbNormal.Size = new Size(101, 28);
        chbNormal.TabIndex = 31;
        chbNormal.Text = "Normal";
        chbNormal.UseVisualStyleBackColor = true;
        // 
        // chbMature
        // 
        chbMature.AutoSize = true;
        chbMature.Location = new Point(242, 42);
        chbMature.Name = "chbMature";
        chbMature.Size = new Size(99, 28);
        chbMature.TabIndex = 30;
        chbMature.Text = "Mature";
        chbMature.UseVisualStyleBackColor = true;
        // 
        // chbAlwaysDownloadLatest
        // 
        chbAlwaysDownloadLatest.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        chbAlwaysDownloadLatest.AutoSize = true;
        chbAlwaysDownloadLatest.Location = new Point(1958, 78);
        chbAlwaysDownloadLatest.Name = "chbAlwaysDownloadLatest";
        chbAlwaysDownloadLatest.Size = new Size(282, 28);
        chbAlwaysDownloadLatest.TabIndex = 29;
        chbAlwaysDownloadLatest.Text = "Redownload User Index Files";
        chbAlwaysDownloadLatest.UseVisualStyleBackColor = true;
        // 
        // chbNsfw
        // 
        chbNsfw.AutoSize = true;
        chbNsfw.Checked = true;
        chbNsfw.CheckState = CheckState.Checked;
        chbNsfw.Location = new Point(108, 42);
        chbNsfw.Name = "chbNsfw";
        chbNsfw.Size = new Size(89, 28);
        chbNsfw.TabIndex = 28;
        chbNsfw.Text = "NSFW";
        chbNsfw.UseVisualStyleBackColor = true;
        // 
        // listBoxMessages
        // 
        listBoxMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listBoxMessages.Font = new Font("Cascadia Code", 10F);
        listBoxMessages.FormattingEnabled = true;
        listBoxMessages.ItemHeight = 27;
        listBoxMessages.Location = new Point(6, 229);
        listBoxMessages.Name = "listBoxMessages";
        listBoxMessages.Size = new Size(2234, 679);
        listBoxMessages.TabIndex = 24;
        // 
        // btnDownload
        // 
        btnDownload.Location = new Point(6, 110);
        btnDownload.Name = "btnDownload";
        btnDownload.Size = new Size(272, 64);
        btnDownload.TabIndex = 23;
        btnDownload.Text = "Download";
        btnDownload.UseVisualStyleBackColor = true;
        btnDownload.Click += btnDownload_Click;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(6, 9);
        label1.Name = "label1";
        label1.Size = new Size(96, 24);
        label1.TabIndex = 22;
        label1.Text = "Username";
        // 
        // txtUsernames
        // 
        txtUsernames.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtUsernames.Location = new Point(108, 6);
        txtUsernames.Name = "txtUsernames";
        txtUsernames.Size = new Size(2132, 30);
        txtUsernames.TabIndex = 21;
        // 
        // tabPage2
        // 
        tabPage2.Controls.Add(listBoxVideoProcessingMessages);
        tabPage2.Controls.Add(btnCopyFromDownloadTab);
        tabPage2.Controls.Add(label4);
        tabPage2.Controls.Add(txtVideoProcessingUsers);
        tabPage2.Controls.Add(btnCompressVideo);
        tabPage2.Location = new Point(4, 33);
        tabPage2.Name = "tabPage2";
        tabPage2.Padding = new Padding(3);
        tabPage2.Size = new Size(2246, 971);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "Video Download";
        tabPage2.UseVisualStyleBackColor = true;
        // 
        // listBoxVideoProcessingMessages
        // 
        listBoxVideoProcessingMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listBoxVideoProcessingMessages.Font = new Font("Cascadia Code", 10F);
        listBoxVideoProcessingMessages.FormattingEnabled = true;
        listBoxVideoProcessingMessages.ItemHeight = 27;
        listBoxVideoProcessingMessages.Location = new Point(6, 120);
        listBoxVideoProcessingMessages.Name = "listBoxVideoProcessingMessages";
        listBoxVideoProcessingMessages.Size = new Size(2234, 787);
        listBoxVideoProcessingMessages.TabIndex = 46;
        // 
        // btnCopyFromDownloadTab
        // 
        btnCopyFromDownloadTab.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCopyFromDownloadTab.Location = new Point(6, 42);
        btnCopyFromDownloadTab.Name = "btnCopyFromDownloadTab";
        btnCopyFromDownloadTab.Size = new Size(216, 72);
        btnCopyFromDownloadTab.TabIndex = 45;
        btnCopyFromDownloadTab.Text = "Copy usernames from Download Tab";
        btnCopyFromDownloadTab.UseVisualStyleBackColor = true;
        btnCopyFromDownloadTab.Click += btnCopyFromDownloadTab_Click;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new Point(6, 9);
        label4.Name = "label4";
        label4.Size = new Size(96, 24);
        label4.TabIndex = 44;
        label4.Text = "Username";
        // 
        // txtVideoProcessingUsers
        // 
        txtVideoProcessingUsers.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtVideoProcessingUsers.Location = new Point(108, 6);
        txtVideoProcessingUsers.Name = "txtVideoProcessingUsers";
        txtVideoProcessingUsers.Size = new Size(1800, 30);
        txtVideoProcessingUsers.TabIndex = 43;
        // 
        // btnCompressVideo
        // 
        btnCompressVideo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCompressVideo.Location = new Point(228, 42);
        btnCompressVideo.Name = "btnCompressVideo";
        btnCompressVideo.Size = new Size(218, 72);
        btnCompressVideo.TabIndex = 42;
        btnCompressVideo.Text = "Compress Video";
        btnCompressVideo.UseVisualStyleBackColor = true;
        btnCompressVideo.Click += btnCompressVideo_Click;
        // 
        // label2
        // 
        label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        label2.AutoSize = true;
        label2.Location = new Point(1570, 9);
        label2.Name = "label2";
        label2.Size = new Size(198, 24);
        label2.TabIndex = 28;
        label2.Text = "Parent Output Folder:";
        // 
        // txtTargetFolder
        // 
        txtTargetFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtTargetFolder.Location = new Point(1774, 7);
        txtTargetFolder.Name = "txtTargetFolder";
        txtTargetFolder.Size = new Size(485, 30);
        txtTargetFolder.TabIndex = 27;
        // 
        // MainForm
        // 
        AllowDrop = true;
        AutoScaleDimensions = new SizeF(11F, 24F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(2278, 1032);
        Controls.Add(label2);
        Controls.Add(txtTargetFolder);
        Controls.Add(mainTabControl);
        Name = "MainForm";
        Text = "CivitAI Worker";
        DragDrop += MainForm_DragDrop;
        DragEnter += MainForm_DragEnter;
        mainTabControl.ResumeLayout(false);
        DownloadPage.ResumeLayout(false);
        DownloadPage.PerformLayout();
        tabPage2.ResumeLayout(false);
        tabPage2.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TabControl mainTabControl;
    private TabPage DownloadPage;
    private Button btnCopyAllSubdirNames;
    private Label lblDownloadingCounter;
    private Button btnStop;
    private Button btnMarkDeletedFilesNoRedownload;
    private CheckBox chbDownloadImage;
    private Button btnOpenFirstUserFolder;
    private Button btnCopyFailedUrls;
    private Button btnDeleteInfoFiles;
    private CheckBox chbDownloadVideo;
    private CheckBox chbChildLevel;
    private CheckBox chbNormal;
    private CheckBox chbMature;
    private CheckBox chbAlwaysDownloadLatest;
    private CheckBox chbNsfw;
    private ListBox listBoxMessages;
    private Button btnDownload;
    private Label label1;
    private TextBox txtUsernames;
    private TabPage tabPage2;
    private Button btnCompressVideo;
    private Label label4;
    private TextBox txtVideoProcessingUsers;
    private Button btnCopyFromDownloadTab;
    private ListBox listBoxVideoProcessingMessages;
    private Label label2;
    private TextBox txtTargetFolder;
    private Button btnOpenAllUserFolders;
    private Label label3;
    private CheckBox chb6Star;
    private CheckBox chb5Star;
    private CheckBox chb4p5Star;
    private CheckBox chb4Star;
    private CheckBox chb3Star;
    private Button btnSetUserNameTextByRating;
    private Button btnMoveUsersToRating;
}
