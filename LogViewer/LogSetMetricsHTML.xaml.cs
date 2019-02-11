using System.IO;
using System.Reflection;
using System.Windows;
using LogViewer.Lib;
using System.Threading.Tasks;
using LogViewer.Lib.Visualization;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for LogSetMetricsHTML.xaml
    /// </summary>
    public partial class LogSetMetricsHTML : Window
    {
        #region Declarations

        private LogSet _logset;
        private string _state;

        #endregion

        #region Constructors

        private LogSetMetricsHTML()
        {
            InitializeComponent();
        }

        public LogSetMetricsHTML(LogSet logset)
            : this()
        {
            _logset = logset;
            _logset.PropertyChanged += logset_PropertyChanged;
            this.Title = GetTitle();
            LoadMetrics();
        }

        #endregion

        #region Actions

        /// <summary>
        /// Allows caller to refresh metrics
        /// </summary>
        public void Refresh()
        {
            LoadMetrics();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Generates metrics and sets browser window contents
        /// </summary>
        private void LoadMetrics()
        {
            SetUserWaitMessage();
            Task.Factory.StartNew(() => { return _logset.GetLogSetMetrics(); })
                .ContinueWith((t) =>
                {
                    if (t.Result != null)
                    {
                        var chart = new GoogleChart(t.Result);
                        this.Left -= 200;
                        this.Top -= 200;
                        SetBrowserContents(chart.HTML, web.Height, web.Width);
                        lblHeading.Content = string.Concat("Total Entries: ", _logset.Entries.Count);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        /// <summary>
        /// Sets the browser contents
        /// </summary>
        /// <param name="html"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        private void SetBrowserContents(string html, double height, double width)
        {
            this.Height = height;
            this.Width = width;
            web.NavigateToString(html);
        }


        /// <summary>
        /// Returns the current title
        /// </summary>
        /// <returns></returns>
        private string GetTitle()
        {
            if (!string.IsNullOrEmpty(_state))
            {
                return string.Concat(_logset.Name, " ", _state);
            }
            else
            {
                return _logset.Name;
            }
        }

        /// <summary>
        /// Sets window to wait message
        /// </summary>
        private void SetUserWaitMessage()
        {
            lblHeading.Content = "Generating log metrics... please wait.";
            web.NavigateToString("<html/>");
            this.Height = 80;
            this.Width = 350;
            _state = string.Empty;
            this.Title = GetTitle();
        }

        #endregion

        #region Events

        void logset_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.ToUpper() == "ENTRIES")
            {
                _state = " (*needs refresh)";
                this.Title = GetTitle();

            }
            else if (e.PropertyName.ToUpper() == "NAME")
            {
                this.Title = GetTitle();
            }
        }

        #endregion
    }
}
