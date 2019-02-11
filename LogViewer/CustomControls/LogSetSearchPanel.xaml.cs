using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using LogViewer.Lib;

namespace LogViewer.CustomControls
{
    /// <summary>
    /// Interaction logic for LogEntryFilterPanel.xaml
    /// </summary>
    public partial class LogSetSearchPanel : UserControl
    {

        #region Declarations and Constructors

        readonly string[] _operators = new string[] { ">", ">=", "=", "<=", "<" };
        const string _optionNone = "-none-";
        bool _hasSearched = false; //used for better reset field functionality

        public event EventHandler<LogSetSearchEventArgs> DoSearch;
        public event EventHandler<LogSetSearchEventArgs> ResetSearch;
        public event EventHandler<EventArgs> Cancelled;

        public LogSetSearchPanel()
        {
            InitializeComponent();

            var levels = Enum.GetNames(typeof(LogEntryLevel));
            var src = new string[levels.Length + 1];
            src[0] = _optionNone;
            for (int i = 1; i < src.Length - 1; i++)
            {
                src[i] = levels[i];
            }
            cmbLevel.ItemsSource = src;
            cmbLevelOperator.ItemsSource = _operators;
            cmbLevelOperator.SelectedIndex = 2;
        }

        #endregion

        #region Actions

        public void LoadSearchOptions(LogSetSearchOptions options)
        {
            if (options != null)
            {
                txtArea.Text = options.Area;
                txtCategory.Text = options.Category;
                dtEnd.Value = options.EndDateTime;
                txtEventID.Text = options.EventID;
                txtMessage.Text = options.Message;
                txtProcess.Text = options.Process;
                dtStart.Value = options.StartDateTime;
                txtThreadId.Text = options.TID;

                if (options.Level != null)
                {
                    cmbLevel.Text = Enum.GetName(typeof(LogEntryLevel), options.Level);
                }
                else
                {
                    cmbLevel.SelectedIndex = 0;
                }
            }
        }

        public void SetUserInfoMessage(string message)
        {
            txtResults.Text = message;
        }

        public void ResetFields()
        {
            txtResults.Text = string.Empty;
            var options = new LogSetSearchOptions() { IsEmptySearch = true };
            LoadSearchOptions(options);

            if (ResetSearch != null)
            {
                ResetSearch(this, new LogSetSearchEventArgs() { Options = options });
            }
        }

        #endregion

        #region Events

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if(Cancelled != null)
            {
                if(!_hasSearched)
                {
                    //only reset fields if user has NOT searched
                    ResetFields();
                }

                Cancelled(this, new EventArgs());
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var search = new LogSetSearchOptions()
            {
                Area = txtArea.Text,
                Category = txtCategory.Text,
                EndDateTime = (dtEnd.Value.HasValue) ? dtEnd.Value.Value.AddMilliseconds(999d) : new Nullable<DateTime>(),//search friendly
                EventID = txtEventID.Text,
                Message = txtMessage.Text,
                Process = txtProcess.Text,
                StartDateTime = dtStart.Value,
                TID = txtThreadId.Text
            };

            if (!string.IsNullOrEmpty(cmbLevel.Text) && cmbLevel.Text != _optionNone)
            {
                search.Level = (LogEntryLevel?)Enum.Parse(typeof(LogEntryLevel), cmbLevel.Text);
            }
            else
            {
                search.Level = null;
            }

            switch (cmbLevelOperator.Text)
            {
                case "=":
                    search.LevelCompareType = ComparisonOperator.Equal;
                    break;
                case ">":
                    search.LevelCompareType = ComparisonOperator.GreaterThan;
                    break;
                case ">=":
                    search.LevelCompareType = ComparisonOperator.GreaterThanOrEqual;
                    break;
                case "<":
                    search.LevelCompareType = ComparisonOperator.LessThan;
                    break;
                case "<=":
                    search.LevelCompareType = ComparisonOperator.LessThanOrEqual;
                    break;
            }

            if (DoSearch != null)
            {
                DoSearch(this, new LogSetSearchEventArgs() { Options = search });
            }

            _hasSearched = true;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetFields();
        }

        #endregion
    }
}
