namespace CivitaiImageDownloader;

partial class MainForm
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
        mainTabControl = new TabControl();
        DownloadPage = new TabPage();
        tabPage2 = new TabPage();
        tabPage1 = new TabPage();
        tabPageViewer = new TabPage();
        label2 = new Label();
        txtTargetFolder = new TextBox();
        mainTabControl.SuspendLayout();
        SuspendLayout();
        // 
        // mainTabControl
        // 
        mainTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        mainTabControl.Controls.Add(DownloadPage);
        mainTabControl.Controls.Add(tabPage2);
        mainTabControl.Controls.Add(tabPage1);
        mainTabControl.Controls.Add(tabPageViewer);
        mainTabControl.Location = new Point(12, 12);
        mainTabControl.Name = "mainTabControl";
        mainTabControl.SelectedIndex = 0;
        mainTabControl.Size = new Size(2254, 1008);
        mainTabControl.TabIndex = 21;
        // 
        // DownloadPage
        // 
        DownloadPage.Location = new Point(4, 33);
        DownloadPage.Name = "DownloadPage";
        DownloadPage.Padding = new Padding(3);
        DownloadPage.Size = new Size(2246, 971);
        DownloadPage.TabIndex = 0;
        DownloadPage.Text = "Download";
        // 
        // tabPage2
        // 
        tabPage2.Location = new Point(4, 33);
        tabPage2.Name = "tabPage2";
        tabPage2.Padding = new Padding(3);
        tabPage2.Size = new Size(2246, 941);
        tabPage2.TabIndex = 1;
        tabPage2.Text = "Video";
        // 
        // tabPage1
        // 
        tabPage1.Location = new Point(4, 33);
        tabPage1.Name = "tabPage1";
        tabPage1.Padding = new Padding(3);
        tabPage1.Size = new Size(2246, 941);
        tabPage1.TabIndex = 2;
        tabPage1.Text = "History";
        // 
        // tabPageViewer
        // 
        tabPageViewer.Location = new Point(4, 33);
        tabPageViewer.Name = "tabPageViewer";
        tabPageViewer.Padding = new Padding(3);
        tabPageViewer.Size = new Size(2246, 941);
        tabPageViewer.TabIndex = 3;
        tabPageViewer.Text = "Viewer";
        // 
        // label2
        // 
        label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        label2.AutoSize = true;
        label2.Location = new Point(1570, 12);
        label2.Name = "label2";
        label2.Size = new Size(198, 24);
        label2.TabIndex = 28;
        label2.Text = "Parent Output Folder:";
        // 
        // txtTargetFolder
        // 
        txtTargetFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        txtTargetFolder.Location = new Point(1774, 9);
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
        ResumeLayout(false);
        PerformLayout();
    }

    private TabControl mainTabControl;
    private TabPage DownloadPage;
    private TabPage tabPage2;
    private TabPage tabPage1;
    private TabPage tabPageViewer;
    private Label label2;
    private TextBox txtTargetFolder;
}
