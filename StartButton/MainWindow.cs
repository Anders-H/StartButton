#nullable enable
using StartButton.ObjectModel;
using StartButton.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StartButton;

public partial class MainWindow : Form
{
    private string? _filename;
    private StartConfiguration? StartConfiguration { get; set; }
    private readonly ShortcutsLoader _shortcutLoader;
    private readonly ConfigurationFileService _configurationFileService;

    public MainWindow()
    {
        _shortcutLoader = new ShortcutsLoader();
        _configurationFileService = new ConfigurationFileService();
        InitializeComponent();
    }

    private void MainWindow_Load(object sender, EventArgs e)
    {
        _filename = _configurationFileService.GetSuggestedFilename();
    }

    private void MainWindow_Shown(object sender, EventArgs e)
    {
        Refresh();
        var originalFilename = _filename ?? "";
        var fi = new FileInfo(_filename ?? "");
        var dir = _configurationFileService.TryGetExecutingDirectory();

        if (!fi.Exists && dir == null)
        {
            MessageBox.Show(this, originalFilename, @"File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        fi = new FileInfo(Path.Combine(dir?.Parent?.Parent?.Parent?.FullName ?? "", "StartButtonConfiguration.xml"));
        _filename = fi.FullName;

        if (!fi.Exists)
        {
            MessageBox.Show(this, originalFilename, @"File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        var startConfiguration = _shortcutLoader.LoadShortcuts(this, _filename);

        if (startConfiguration == null)
        {
            MessageBox.Show(this, originalFilename, @"Load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        StartConfiguration = startConfiguration;
        RefreshList();
    }

    private void RefreshList()
    {
        if (StartConfiguration == null)
            return;

        foreach (Control control in Controls)
            control.Click -= LinkClick;

        while (Controls.Count > 0)
            Controls.RemoveAt(Controls.Count - 1);

        var top = 4;

        foreach (var b in StartConfiguration.ShortCuts.Select(shortCut => new Button
        {
            Text = shortCut.Name,
            Top = top,
            Left = 4,
            Width = ClientRectangle.Width - 8,
            Height = 22,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Tag = shortCut,
        }))
        {
            b.Click += LinkClick;
            Controls.Add(b);
            top += 26;
        }
    }

    private void LinkClick(object sender, EventArgs e)
    {
        if (!((sender as Button)?.Tag is ShortCut shortCut))
            return;

        if (shortCut.Warn && MessageBox.Show(this, $@"Open ""{shortCut.Name}"" ({shortCut.ProcessList.Count} processes)?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        var fails = new List<string>();

        foreach (var process in shortCut.ProcessList)
        {
            try
            {
                System.Diagnostics.Process.Start(process.ProcessPath);
            }
            catch (Exception ex)
            {
                fails.Add($@"{ex.GetType().Name}: {ex.Message}");
            }
        }

        if (fails.Count == 1)
        {
            MessageBox.Show(this, fails.First(), @"One error occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else if (fails.Count > 1)
        {
            var s = new StringBuilder();

            foreach (var fail in fails)
                s.AppendLine(fail);

            MessageBox.Show(this, s.ToString().Trim(), $@"{fails.Count} errors occurred", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}