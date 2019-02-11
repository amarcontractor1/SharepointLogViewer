using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for SharePointLogMonitorSettings.xaml
    /// </summary>
    public partial class SPLogMonitorSettings : Window
    {
        #region Declarations and Constructors

        private string _monitorDirectory;
        public string MonitorDirectory
        {
            get { return _monitorDirectory; }
            set { SetDirectoryPath(value); }
        }

        public int? Interval
        {
            get { return updnInterval.Value; }
            set
            {
                if (value < 30)
                {
                    throw new ArgumentOutOfRangeException("Internval must be greater or equal to 30!");
                }
                else
                {
                    updnInterval.Value = value;
                }

            }
        }

        public SPLogMonitorSettings()
        {
            InitializeComponent();
            _monitorDirectory = string.Empty;
        }

        #endregion

        #region Helpers

        private void SetDirectoryPath(string directory)
        {
            //do nothing if blank
            if (string.IsNullOrEmpty(directory))
                return;

            if (Directory.Exists(directory))
            {
                _monitorDirectory = directory;
                txtDir.Text = directory;
                txtDir.ToolTip = directory;
            }
            else
            {
                throw new DirectoryNotFoundException("Directory not found!");
            }
        }

        #endregion

        #region Events

        private void btnSelectDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = false,
                Description = "Select directory to monitor..."
            };

            //set existing path if there
            if (!string.IsNullOrEmpty(_monitorDirectory))
                dialog.SelectedPath = _monitorDirectory;


            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SetDirectoryPath(dialog.SelectedPath);
                txtDir.Background = null;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_monitorDirectory))
            {
                txtDir.Background = Brushes.MistyRose;
                return;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #endregion
    }
}
