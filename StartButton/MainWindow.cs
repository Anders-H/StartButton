using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace StartButton
{
    public partial class MainWindow : Form
    {
        private string _filename;
        private List<StartConfiguration> StartConfigurations { get; set; }

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

            StartConfigurations = startConfiguration;
            RefreshList();
        }

        public List<StartConfiguration> LoadShortcuts(string filename)
        {
            var result = new List<StartConfiguration>();
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
                var processes = shortCut.SelectSingleNode("processList")?.SelectNodes("process");

                if (processes == null)
                    continue;

                var p = (from XmlElement process in processes select process.InnerText).ToList();
                var sc = new StartConfiguration();
                var parsedShortCut = new ShortCut(name);

                foreach (var path in p)
                    parsedShortCut.ProcessList.Add(new Process(path));

                sc.ShortCuts.Add(parsedShortCut);
                result.Add(sc);
            }

            return result;
        }

        private void RefreshList()
        {
            foreach (Control control in Controls)
                control.Click -= LinkClick;

            while (Controls.Count > 0)
                Controls.RemoveAt(Controls.Count - 1);

            foreach (var startConfiguration in StartConfigurations)
            {
                
            }
        }

        private void LinkClick(object sender, EventArgs e)
        {

        }
    }
}