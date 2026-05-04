using CivitaiImageDownloader.Models;
using CivitaiImageDownloader.Util;
using System.Diagnostics;

namespace CivitaiImageDownloader.Tabs;

public partial class VideoTabControl : UserControl
{
    private readonly AppMediator _mediator;
    private VideoCompressor? _videoCompressor;

    public VideoTabControl(AppMediator mediator)
    {
        _mediator = mediator;
        InitializeComponent();
        _mediator.UsernamesCopiedToVideo += usernames => txtVideoProcessingUsers.Text = usernames;
        _mediator.Stopping = false;
    }

    private void btnCopyFromDownloadTab_Click(object sender, EventArgs e)
    {
        txtVideoProcessingUsers.Text = _mediator.DownloadUsernames;
    }

    private async void btnCompressVideo_Click(object sender, EventArgs e)
    {
        Invoke(listBoxVideoProcessingMessages.Items.Clear);

        _videoCompressor = null;

        List<string> names = txtVideoProcessingUsers.ParseUserNames();
        if (names.Count == 0) { return; }
        if (txtVideoProcessingUsers.Text.Length == 0 && names.Count == 0 && !Path.Exists(_mediator.TargetFolder))
        {
            var result = MessageBox.Show("Do you want to compress everything in " + _mediator.TargetFolder + "?", "Warning", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                names = [_mediator.TargetFolder];
            }
        }
        foreach (var name in names)
        {
            var mode = VideoProcessInputMode.UserName;
            if (Path.Exists(name))
            {
                mode = VideoProcessInputMode.FilePath;
            }
            if (_mediator.Stopping && _videoCompressor != null)
            {
                _videoCompressor.ShouldStop = true;
                _mediator.LogMessage("Video compression stopped.");
                break;
            }
            _videoCompressor = new VideoCompressor(_mediator.TargetFolder, name, mode);
            _videoCompressor.RaiseAddMessage += AddVideoProcessingMessage;
            _videoCompressor.RaiseAppendMessage += AppendVideoProcessingMessage;
            await _videoCompressor.Run();
            _videoCompressor.RaiseAddMessage -= AddVideoProcessingMessage;
            _videoCompressor.RaiseAppendMessage -= AppendVideoProcessingMessage;
            _videoCompressor.Dispose();
        }
        AddVideoProcessingMessage("ALL DONE!");

        _mediator.RecordVideoHistory(txtVideoProcessingUsers.Text.Trim());
    }

    private void listBoxVideoProcessingMessages_DoubleClick(object sender, EventArgs e)
    {
        if (_videoCompressor == null || string.IsNullOrWhiteSpace(_videoCompressor.UserName))
            return;
        _mediator.CurrentUserFolder = FolderHelper.GetFolder(_mediator.TargetFolder, _videoCompressor.UserName);
        if (listBoxVideoProcessingMessages.SelectedIndex != -1 && Directory.Exists(_mediator.CurrentUserFolder))
        {
            string selectedItemText = listBoxVideoProcessingMessages.SelectedItem?.ToString() ?? "";
            if (selectedItemText.Contains("Compress video:"))
            {
                var parts = selectedItemText.Split("Compress video:");
                if (parts.Length >= 2)
                {
                    parts = parts[1].Split(" ... ");
                    if (parts.Length >= 2)
                    {
                        var candidate = parts[0].Trim();
                        var path = Path.Combine(_mediator.CurrentUserFolder, candidate);
                        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                        Clipboard.SetText(path);
                    }
                }
            }
            else
            {
                Clipboard.SetText(selectedItemText);
            }
        }
    }

    private void AddVideoProcessingMessage(string message)
    {
        Invoke(() => listBoxVideoProcessingMessages.Items.Insert(0, message));
    }

    private void AppendVideoProcessingMessage(string message)
    {
        Invoke(() =>
        {
            try
            {
                var item = (string)listBoxVideoProcessingMessages.Items[0];
                listBoxVideoProcessingMessages.Items[0] = item + message;
            }
            catch { }
        });
    }
}
