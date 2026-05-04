using CivitaiImageDownloader.Tabs;
using System.Diagnostics;

namespace CivitaiImageDownloader;

public partial class MainForm : Form
{
    private const string DefaultTargetFolder = @"D:\AI\CivitAI\";
    private readonly AppMediator _mediator = new();
    private DownloadTabControl? _downloadTab;
    private VideoTabControl? _videoTab;
    private HistoryTabControl? _historyTab;
    private ViewerTabControl? _viewerTab;

    public MainForm()
    {
        InitializeComponent();

        txtTargetFolder.Text = DefaultTargetFolder;
        _mediator.TargetFolder = txtTargetFolder.Text;

        txtTargetFolder.TextChanged += (s, e) => _mediator.TargetFolder = txtTargetFolder.Text;

        // wire mediator events
        _mediator.TabSwitchToDownloadRequested += () => mainTabControl.SelectedTab = DownloadPage;
        _mediator.TabSwitchToVideoRequested += () => mainTabControl.SelectedTab = tabPage2;
        _mediator.UsernamesCopiedToDownload += usernames =>
        {
            mainTabControl.SelectedTab = DownloadPage;
            if (_downloadTab != null) _downloadTab.txtUsernames.Text = usernames;
        };
        _mediator.UsernamesCopiedToVideo += usernames =>
        {
            mainTabControl.SelectedTab = tabPage2;
            _videoTab?.Invoke(() => { });
        };

        // create tab controls
        _downloadTab = new DownloadTabControl(_mediator) { Dock = DockStyle.Fill };
        DownloadPage.Controls.Add(_downloadTab);

        _videoTab = new VideoTabControl(_mediator) { Dock = DockStyle.Fill };
        tabPage2.Controls.Add(_videoTab);

        _historyTab = new HistoryTabControl(_mediator) { Dock = DockStyle.Fill };
        tabPage1.Controls.Add(_historyTab);

        _viewerTab = new ViewerTabControl(_mediator) { Dock = DockStyle.Fill };
        tabPageViewer.Controls.Add(_viewerTab);

        // load data when switching tabs
        mainTabControl.SelectedIndexChanged += (s, e) =>
        {
            if (mainTabControl.SelectedTab == tabPage1)
            {
                _historyTab.LoadActionHistory();
                _ = _historyTab.PopulateUserFolderStatus();
            }
            else if (mainTabControl.SelectedTab == tabPageViewer)
            {
                _ = _viewerTab.PopulateViewerUserList();
            }
        };

        _historyTab.LoadActionHistory();
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    private void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
        List<string> userNames = new();
        List<string> fileNames = new();
        foreach (var path in paths)
        {
            if (Directory.Exists(path))
                userNames.Add(Path.GetFileNameWithoutExtension(path));
            else if (File.Exists(path))
                fileNames.Add(path);
        }
        if (mainTabControl.SelectedTab == tabPage2)
        {
            var text = userNames.Count > 0 ? string.Join(",", userNames) : string.Join(",", fileNames);
            _videoTab?.Invoke(() => _mediator.CopyUsernamesToVideo(text));
        }
        else
        {
            var text = string.Join(",", userNames);
            _downloadTab?.Invoke(() => _mediator.CopyUsernamesToDownload(text));
        }
    }
}
