namespace CivitaiImageDownloader.Tabs;

partial class ViewerTabControl
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
        splitContainerViewer = new SplitContainer();
        listBoxViewerUsers = new ListBox();
        flowLayoutPanelViewer = new FlowLayoutPanel();
        progressBarViewer = new ProgressBar();
        ((System.ComponentModel.ISupportInitialize)splitContainerViewer).BeginInit();
        splitContainerViewer.Panel1.SuspendLayout();
        splitContainerViewer.Panel2.SuspendLayout();
        SuspendLayout();

        splitContainerViewer.Dock = DockStyle.Fill;
        splitContainerViewer.Location = new Point(0, 0);
        splitContainerViewer.Name = "splitContainerViewer";
        splitContainerViewer.Orientation = Orientation.Vertical;
        splitContainerViewer.Size = new Size(1617, 748);
        splitContainerViewer.SplitterDistance = 350;
        splitContainerViewer.SplitterWidth = 4;
        splitContainerViewer.TabIndex = 0;

        listBoxViewerUsers.Dock = DockStyle.Fill;
        listBoxViewerUsers.Font = new Font("Cascadia Code", 10F);
        listBoxViewerUsers.IntegralHeight = false;
        listBoxViewerUsers.ItemHeight = 27;
        listBoxViewerUsers.Location = new Point(0, 0);
        listBoxViewerUsers.Name = "listBoxViewerUsers";
        listBoxViewerUsers.Size = new Size(346, 748);
        listBoxViewerUsers.TabIndex = 0;

        flowLayoutPanelViewer.AutoScroll = true;
        flowLayoutPanelViewer.BackColor = SystemColors.ControlDark;
        flowLayoutPanelViewer.Dock = DockStyle.Fill;
        flowLayoutPanelViewer.Location = new Point(0, 0);
        flowLayoutPanelViewer.Name = "flowLayoutPanelViewer";
        flowLayoutPanelViewer.Padding = new Padding(2);
        flowLayoutPanelViewer.Size = new Size(1263, 744);
        flowLayoutPanelViewer.TabIndex = 0;

        progressBarViewer.Dock = DockStyle.Bottom;
        progressBarViewer.Height = 4;
        progressBarViewer.Location = new Point(0, 744);
        progressBarViewer.Name = "progressBarViewer";
        progressBarViewer.Size = new Size(1263, 4);
        progressBarViewer.Style = ProgressBarStyle.Continuous;
        progressBarViewer.TabIndex = 1;
        progressBarViewer.Visible = false;

        splitContainerViewer.Panel1.Controls.Add(listBoxViewerUsers);
        splitContainerViewer.Panel2.Controls.Add(flowLayoutPanelViewer);
        splitContainerViewer.Panel2.Controls.Add(progressBarViewer);

        Controls.Add(splitContainerViewer);
        Name = "ViewerTabControl";
        Size = new Size(1617, 748);

        splitContainerViewer.Panel1.ResumeLayout(false);
        splitContainerViewer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainerViewer).EndInit();
        ResumeLayout(false);
    }

    private SplitContainer splitContainerViewer;
    private ListBox listBoxViewerUsers;
    private FlowLayoutPanel flowLayoutPanelViewer;
    private ProgressBar progressBarViewer;
}
