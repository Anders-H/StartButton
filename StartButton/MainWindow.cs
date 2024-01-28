using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace StartButton
{
    public partial class MainWindow : Form
    {
        private string _filename;
        private StartConfiguration StartConfiguration { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            _filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StartButtonUserDataFolder");

            var folder = new DirectoryInfo(_filename);

            if (!folder.Exists)
                folder.Create();

            _filename = Path.Combine(_filename, "StartButtonConfiguration.xml");
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            Refresh();
            var originalFilename = _filename;
            System.Diagnostics.Debug.WriteLine(_filename);
            var fi = new FileInfo(_filename);

            if (!fi.Exists)
            {
                var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                var exe = new FileInfo(executingAssembly.Location);
                var dir = exe.Directory;

                if (dir == null)
                {
                    MessageBox.Show(this, originalFilename, @"File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                fi = new FileInfo(Path.Combine(dir.Parent?.Parent?.Parent?.FullName ?? "", "StartButtonConfiguration.xml"));
                _filename = fi.FullName;
                System.Diagnostics.Debug.WriteLine(_filename);
            }

            if (!fi.Exists)
            {
                MessageBox.Show(this, originalFilename, @"File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            var startConfiguration = LoadShortcuts(_filename);

            if (startConfiguration == null)
            {
                MessageBox.Show(this, originalFilename, @"Load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            StartConfiguration = startConfiguration;
            RefreshList();
        }

        public StartConfiguration LoadShortcuts(string filename)
        {
            var result = new StartConfiguration();
            string source;

            using (var sr = new StreamReader(filename))
            {
                source = sr.ReadToEnd();
                sr.Close();
            }

            var dom = new XmlDocument();
            dom.LoadXml(source);
            var shortCuts = dom.DocumentElement?.SelectNodes("shortCut");

            if (shortCuts == null)
            {
                MessageBox.Show(
                    this,
                    $@"XML document ({filename}) did not contain shortCuts/shortCut.",
                    Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Close();
                return null;
            }

            foreach (XmlElement shortCut in shortCuts)
            {
                var name = shortCut.SelectSingleNode("name")?.InnerText ?? "";
                var warn = (shortCut.Attributes.GetNamedItem("warn")?.Value?.ToLower().Trim() ?? "") == "true";
                var processes = shortCut.SelectSingleNode("processList")?.SelectNodes("process");

                if (processes == null)
                    continue;

                var p = (from XmlElement process in processes select process.InnerText).ToList();
                var parsedShortCut = new ShortCut(name, warn);

                foreach (var path in p)
                    parsedShortCut.ProcessList.Add(new Process(path));

                result.ShortCuts.Add(parsedShortCut);
            }

            return result;
        }

        private void RefreshList()
        {
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
                        }
                    )
                )
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
}