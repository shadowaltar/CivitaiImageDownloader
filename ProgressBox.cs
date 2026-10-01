using System.Runtime.InteropServices;
using System.Text;
using CivitaiImageDownloader.Models;

namespace CivitaiImageDownloader;

/// <summary>
/// A compact per-run progress grid: one character per item, updated as items are processed.
/// Each <see cref="BeginRun"/> appends a new block (header + grid); <see cref="Clear"/> resets everything.
/// Pending '.', processing spinner, done tick, skipped 'x', already/no-redownload '○'.
/// </summary>
public sealed class ProgressBox : UserControl
{
    private const int Columns = 60;
    private const int EmGetFirstVisibleLine = 0x00CE;
    private const int EmLineScroll = 0x00B6;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    private static readonly char[] SpinnerFrames = { '-', '\\', '|', '/' };

    private sealed class Block
    {
        public string Header = "";
        public ProgressItemState[] States = [];
    }

    private readonly Label _title;
    private readonly TextBox _text;
    private readonly System.Windows.Forms.Timer _spinner;
    private readonly List<Block> _blocks = new();
    private int _frame;
    private int _processingCount;

    public ProgressBox()
    {
        _title = new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            AutoEllipsis = true,
            Text = "Progress",
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.DimGray,
            BackColor = Color.White,
            Padding = new Padding(4, 0, 0, 0)
        };
        _text = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            WordWrap = false,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Cascadia Code", 10F),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        BackColor = Color.White;
        Controls.Add(_text);
        Controls.Add(_title);

        _spinner = new System.Windows.Forms.Timer { Interval = 500 };
        _spinner.Tick += (_, _) => { _frame++; Render(); };
    }

    /// <summary>Height needed to show all blocks without scrolling (clamped by the caller).</summary>
    public int PreferredHeight
    {
        get
        {
            int lines = 0;
            foreach (var b in _blocks)
                lines += 1 + Math.Max(1, (b.States.Length + Columns - 1) / Columns);
            if (_blocks.Count > 1)
                lines += _blocks.Count - 1; // blank separator between blocks
            if (lines == 0)
                lines = 1;
            return _title.Height + lines * (_text.Font.Height + 2) + 8;
        }
    }

    public void Clear()
    {
        OnUi(() =>
        {
            _spinner.Stop();
            _blocks.Clear();
            _processingCount = 0;
            _text.Text = "";
        });
    }

    /// <summary>Appends a new progress block for one user/task.</summary>
    public void BeginRun(string header, int count)
    {
        OnUi(() =>
        {
            _blocks.Add(new Block { Header = header, States = new ProgressItemState[Math.Max(0, count)] });
            Render();
        });
    }

    public void SetState(int index, ProgressItemState state)
    {
        OnUi(() =>
        {
            if (_blocks.Count == 0) return;
            var block = _blocks[^1];
            if (index < 0 || index >= block.States.Length) return;

            var previous = block.States[index];
            if (previous == state) return;
            block.States[index] = state;

            if (previous == ProgressItemState.Processing) _processingCount--;
            if (state == ProgressItemState.Processing) _processingCount++;
            Render();

            if (_processingCount > 0)
            {
                if (!_spinner.Enabled) _spinner.Start();
            }
            else
            {
                _spinner.Stop();
            }
        });
    }

    // Pending '○' (U+25CB), done '●' (U+25CF), processing spinner, skipped 's', already/no-redownload '='.
    private char CharFor(ProgressItemState state) => state switch
    {
        ProgressItemState.Processing => SpinnerFrames[_frame % SpinnerFrames.Length],
        ProgressItemState.Done => '●',
        ProgressItemState.Skipped => 's',
        ProgressItemState.Already => '=',
        _ => '○'
    };

    private void Render()
    {
        var sb = new StringBuilder();
        for (int bi = 0; bi < _blocks.Count; bi++)
        {
            if (bi > 0)
                sb.Append("\r\n");
            var block = _blocks[bi];
            sb.Append(block.Header);
            sb.Append("\r\n");
            for (int i = 0; i < block.States.Length; i++)
            {
                sb.Append(CharFor(block.States[i]));
                if ((i + 1) % Columns == 0 && i + 1 < block.States.Length)
                    sb.Append("\r\n");
            }
        }

        if (_text.IsHandleCreated)
        {
            // preserve the current scroll position (no auto-tail)
            int firstVisible = SendMessage(_text.Handle, EmGetFirstVisibleLine, IntPtr.Zero, IntPtr.Zero).ToInt32();
            _text.Text = sb.ToString();
            if (firstVisible > 0)
                SendMessage(_text.Handle, EmLineScroll, IntPtr.Zero, (IntPtr)firstVisible);
        }
        else
        {
            _text.Text = sb.ToString();
        }
    }

    private void OnUi(Action action)
    {
        if (InvokeRequired)
            Invoke(action);
        else
            action();
    }
}
