using LogViewer.Lib;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for LogFileDetails.xaml
    /// </summary>
    public partial class LogFileDetails : Window
    {
        #region Declarations and Constructors

        private ILogFileDetails _details;
        private LogFileDetailsTab _currentTab;
        public LogFileDetailsTab CurrentTab
        {
            get
            {
                return _currentTab;
            }
            set
            {
 
                foreach (var item in lbxDetails.Items)
                {
                    var itemValue = ((TextBlock)item).Text;
                    if(itemValue.ToUpper() == value.ToString().ToUpper())
                    {
                        lbxDetails.SelectedItem = item;
                    }
                }
            }
        }

        private LogFileDetails()
        {
            InitializeComponent();

            foreach (var name in Enum.GetNames(typeof(LogFileDetailsTab)))
            {
                var txt = new TextBlock() { Text = name };
                lbxDetails.Items.Add(txt);
            }

            lbxDetails.SelectedIndex = 0;
        }

        public LogFileDetails(ILogFileDetails details)
            : this()
        {
            _details = details;
            lbxDetails.SelectedIndex = 0;

            txtFileCount.Text = _details.FileNames.Length.ToString();
            txtEntriesCount.Text = _details.Count.ToString(); ;
            txtLoadTime.Text = string.Concat(_details.LoadTime, "secs.");
            BindFileNames(false);
            BindErrors();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Binds errors
        /// </summary>
        private void BindErrors()
        {
            lbxErrors.Visibility = Visibility.Collapsed;
            txtNoErrors.Visibility = Visibility.Collapsed;
            if (_details.ParserErrors.Count > 0)
            {
                lbxErrors.Visibility = Visibility.Visible;

                var errors = new List<string>();
                foreach (var err in _details.ParserErrors)
                {
                    var filename = System.IO.Path.GetFileName(err.Key);
                    StringBuilder values = new StringBuilder();
                    foreach (var val in err.Value)
                    {
                        errors.Add(string.Format("{0} : {1}", filename, val));
                    }
                }

                lbxErrors.ItemsSource = errors;
            }
            else
            {
                txtNoErrors.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Binds file names
        /// </summary>
        /// <param name="showFullPath"></param>
        private void BindFileNames(bool showFullPath)
        {
            if (showFullPath)
            {
                lbxFiles.ItemsSource = _details.FileNames;
            }
            else
            {
                lbxFiles.ItemsSource = _details.FileNames.Select(x => System.IO.Path.GetFileName(x)).ToArray();
            }
        }

        /// <summary>
        /// Display the selected tab
        /// </summary>
        /// <param name="name"></param>
        private void SelectTab(LogFileDetailsTab name)
        {
            dockErrors.Visibility = Visibility.Hidden;
            dockFiles.Visibility = Visibility.Hidden;
            stkGeneral.Visibility = Visibility.Hidden;

            if (name == LogFileDetailsTab.General)
            {
                stkGeneral.Visibility = Visibility.Visible;
            }
            else if (name == LogFileDetailsTab.Files)
            {
                dockFiles.Visibility = Visibility.Visible;
            }
            else if (name == LogFileDetailsTab.Errors)
            {
                dockErrors.Visibility = Visibility.Visible;
            }
            _currentTab = name;
        }

        #endregion

        #region Events

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lbxDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var option = ((TextBlock)lbxDetails.SelectedItem).Text.ToUpper();
            LogFileDetailsTab tab;
            if (Enum.TryParse<LogFileDetailsTab>(option, true, out tab))
            {
                SelectTab(tab);
            }
        }

        private void chkFullPath_Checked(object sender, RoutedEventArgs e)
        {
            BindFileNames(chkFullPath.IsChecked ?? false);
        }

        private void NotSelectable(object sender, SelectionChangedEventArgs e)
        {
            var listbox = (ListBox)e.Source;
            listbox.SelectedIndex = -1;
        }

        #endregion
    }
}
