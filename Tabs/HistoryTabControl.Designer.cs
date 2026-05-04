namespace CivitaiImageDownloader.Tabs;

partial class HistoryTabControl
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
        UserName = new DataGridViewTextBoxColumn();
        FileCount = new DataGridViewTextBoxColumn();
        FolderSize = new DataGridViewTextBoxColumn();
        ParentFolder = new DataGridViewTextBoxColumn();
        dgvUserHistory = new DataGridView();
        btnCopyToVideoTab = new Button();
        btnCopyToDownloadTab = new Button();
        listBoxActionHistory = new ListBox();
        label5 = new Label();
        label6 = new Label();
        btnReloadExistingUserList = new Button();
        ((System.ComponentModel.ISupportInitialize)dgvUserHistory).BeginInit();
        SuspendLayout();
        // 
        // UserName
        // 
        UserName.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        UserName.HeaderText = "User Name";
        UserName.MinimumWidth = 8;
        UserName.Name = "UserName";
        UserName.ReadOnly = true;
        UserName.Width = 141;
        // 
        // FileCount
        // 
        FileCount.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        FileCount.HeaderText = "File Count";
        FileCount.MinimumWidth = 8;
        FileCount.Name = "FileCount";
        FileCount.ReadOnly = true;
        FileCount.Width = 133;
        // 
        // FolderSize
        // 
        FolderSize.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        FolderSize.HeaderText = "Folder Size";
        FolderSize.MinimumWidth = 8;
        FolderSize.Name = "FolderSize";
        FolderSize.ReadOnly = true;
        FolderSize.Width = 140;
        // 
        // ParentFolder
        // 
        ParentFolder.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        ParentFolder.HeaderText = "Parent Folder";
        ParentFolder.MinimumWidth = 8;
        ParentFolder.Name = "ParentFolder";
        ParentFolder.ReadOnly = true;
        ParentFolder.Width = 162;
        // 
        // dgvUserHistory
        // 
        dgvUserHistory.AllowUserToAddRows = false;
        dgvUserHistory.AllowUserToDeleteRows = false;
        dgvUserHistory.AllowUserToOrderColumns = true;
        dgvUserHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        dgvUserHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvUserHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvUserHistory.Columns.AddRange(new DataGridViewColumn[] { UserName, FileCount, FolderSize, ParentFolder });
        dgvUserHistory.Location = new Point(6, 86);
        dgvUserHistory.Name = "dgvUserHistory";
        dgvUserHistory.RowHeadersWidth = 42;
        dgvUserHistory.Size = new Size(1031, 683);
        dgvUserHistory.TabIndex = 3;
        // 
        // btnCopyToVideoTab
        // 
        btnCopyToVideoTab.Location = new Point(1043, 46);
        btnCopyToVideoTab.Name = "btnCopyToVideoTab";
        btnCopyToVideoTab.Size = new Size(325, 34);
        btnCopyToVideoTab.TabIndex = 2;
        btnCopyToVideoTab.Text = "Copy to Video Tab";
        btnCopyToVideoTab.UseVisualStyleBackColor = true;
        // 
        // btnCopyToDownloadTab
        // 
        btnCopyToDownloadTab.Location = new Point(1043, 6);
        btnCopyToDownloadTab.Name = "btnCopyToDownloadTab";
        btnCopyToDownloadTab.Size = new Size(325, 34);
        btnCopyToDownloadTab.TabIndex = 3;
        btnCopyToDownloadTab.Text = "Copy to Download Tab";
        btnCopyToDownloadTab.UseVisualStyleBackColor = true;
        // 
        // listBoxActionHistory
        // 
        listBoxActionHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listBoxActionHistory.Font = new Font("Cascadia Code", 10F);
        listBoxActionHistory.FormattingEnabled = true;
        listBoxActionHistory.ItemHeight = 27;
        listBoxActionHistory.Location = new Point(1043, 86);
        listBoxActionHistory.Name = "listBoxActionHistory";
        listBoxActionHistory.Size = new Size(687, 679);
        listBoxActionHistory.TabIndex = 4;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new Point(1043, 6);
        label5.Name = "label5";
        label5.Size = new Size(133, 24);
        label5.TabIndex = 5;
        label5.Text = "Action History";
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Location = new Point(6, 0);
        label6.Name = "label6";
        label6.Size = new Size(128, 24);
        label6.TabIndex = 1;
        label6.Text = "Existing Users";
        // 
        // btnReloadExistingUserList
        // 
        btnReloadExistingUserList.Location = new Point(6, 33);
        btnReloadExistingUserList.Name = "btnReloadExistingUserList";
        btnReloadExistingUserList.Size = new Size(325, 34);
        btnReloadExistingUserList.TabIndex = 0;
        btnReloadExistingUserList.Text = "Reload";
        btnReloadExistingUserList.UseVisualStyleBackColor = true;
        // 
        // HistoryTabControl
        // 
        Controls.Add(btnReloadExistingUserList);
        Controls.Add(label6);
        Controls.Add(btnCopyToVideoTab);
        Controls.Add(btnCopyToDownloadTab);
        Controls.Add(listBoxActionHistory);
        Controls.Add(label5);
        Controls.Add(dgvUserHistory);
        Name = "HistoryTabControl";
        Size = new Size(1733, 772);
        ((System.ComponentModel.ISupportInitialize)dgvUserHistory).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private DataGridView dgvUserHistory;
    private Button btnCopyToVideoTab;
    private Button btnCopyToDownloadTab;
    private ListBox listBoxActionHistory;
    private Label label5;
    private Label label6;
    private Button btnReloadExistingUserList;
    private DataGridViewTextBoxColumn UserName;
    private DataGridViewTextBoxColumn FileCount;
    private DataGridViewTextBoxColumn FolderSize;
    private DataGridViewTextBoxColumn ParentFolder;
}
