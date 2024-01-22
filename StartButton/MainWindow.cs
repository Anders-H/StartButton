using System;
using System.IO;
using System.Windows.Forms;

namespace StartButton
{
    public partial class MainWindow : Form
    {
        private string _filename;

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
                var exe = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                fi = new FileInfo(Path.Combine(exe.Directory.Parent.Parent.Parent.FullName, "StartButtonConfiguration.xml"));
                _filename = fi.FullName;
                System.Diagnostics.Debug.WriteLine(_filename);
            }

            if (!fi.Exists)
            {
                MessageBox.Show(this, originalFilename, @"File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
    }
}
