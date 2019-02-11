using LogViewer.CustomControls;
using LogViewer.CustomControls.Printing;
using LogViewer.Lib;
using LogViewer.Lib.CommonLog;
using LogViewer.Lib.SharePoint;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declarations & Constructors

        private LogSetTabItem ActiveTab
        {
            get
            {
                return tcLogSets.SelectedItem as LogSetTabItem;
            }
            set
            {
                tcLogSets.SelectedItem = value;
            }
        }

        private const string _statusReadyMsg = "Ready";
        private int _currentTabCount;
        private GridLength _searchColumnWidth;

        private SharePointLogMonitor _spLogMonitor;
        private SharePointLogMonitorSettings _spLogMonitorSettings;
        private SPLogMonitor _spLogMonitorWin;

        public MainWindow()
        {
            InitializeComponent();
            EnableLogSetControls(false);
            SetMainWindowStatusMessage(_statusReadyMsg);
            _currentTabCount = 0;
            _searchColumnWidth = new GridLength(250);

            //testing
            var files = new string[] { @"C:\Users\Amar Contractor\source\repos\LogViewer\_project resources\testing\logviewer test\PLYSOSP44ZP-20130717-1648.log" };
            ProcessLogFiles(files);


        }

        #endregion

        #region Events

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown(110);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var drops = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (drops == null)
                return;

            var files = new List<string>();
            foreach (var d in drops)
            {
                var ext = System.IO.Path.GetExtension(d).ToUpper();
                if (ext == ".LOG" || ext == ".TXT" || ext == ".CSV")
                {
                    files.Add(d);
                }
            }

            ProcessLogFiles(files.ToArray());
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var command = ((string)((MenuItem)e.Source).CommandParameter).ToUpper();

            if (command == "NEW")
            {
                var files = ShowSelectFilesDialog();
                ProcessLogFiles(files);
            }
            else if (command == "NEWEMPTY")
            {
                CreateLogSetTabItem(new LogSet() { HasErrors = false });
                EnableLogSetControls(true);
            }
            else if (command == "SAVE")
            {
                var dlg = new OpenFileDialog()
                {
                    CheckFileExists = false,
                    Multiselect = false,
                    Title = "Save",
                    DefaultExt = "csv",
                    AddExtension = true,
                };
                var result = dlg.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    using (var file = File.CreateText(dlg.FileName))
                    {
                        this.ActiveTab.Content.LogSetDataSource.Save(file);
                    }
                }

                SetMainWindowStatusMessageWithReset(5000, "File saved!");

            }
            else if (command == "PRINT")
            {
                var printDialog = new PrintDialog();
                var pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                var paginator = new LogSetGridPaginator(this.ActiveTab.Content.GetGrid(), this.ActiveTab.Header.ToString(), pageSize, new Thickness(30, 20, 30, 20));
                printDialog.PrintDocument(paginator, this.ActiveTab.Header.ToString());

            }
            else if (command == "LOGMONITORSETTINGS")
            {
                var settings = GetSPLogMonitorDirectory(out _spLogMonitorSettings);

                if (settings && _spLogMonitor != null)
                {
                    _spLogMonitor.MonitorDirectory = _spLogMonitorSettings.Directory;
                    _spLogMonitor.Interval = _spLogMonitorSettings.Interval.Value;
                }
            }
            else if (command == "EXIT")
            {
                Application.Current.Shutdown(110);
            }
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;

            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void ToolbarButton_Click(object sender, RoutedEventArgs e)
        {
            var command = ((string)((Button)e.Source).CommandParameter).ToUpper();

            if (command == "NEW")
            {
                var files = ShowSelectFilesDialog();
                ProcessLogFiles(files);
            }
            else if (command == "SEARCH")
            {
                ToggleSearchPanelVisbility();
            }
            else if (command == "METRICS")
            {
                if (this.ActiveTab.MetricsWindow != null)
                {
                    //check if window has been closed
                    if (!this.ActiveTab.MetricsWindow.IsLoaded)
                    {
                        var metrics = new LogSetMetricsHTML(this.ActiveTab.Content.LogSetDataSource);
                        metrics.Owner = this;
                        this.ActiveTab.MetricsWindow = metrics;
                        metrics.Show();
                    }
                    else
                    {
                        ((LogSetMetricsHTML)this.ActiveTab.MetricsWindow).Refresh();
                    }
                }
                else
                {
                    var metrics = new LogSetMetricsHTML(this.ActiveTab.Content.LogSetDataSource);
                    metrics.Owner = this;
                    this.ActiveTab.MetricsWindow = metrics;
                    metrics.Show();
                }
            }
            else if (command == "MONITOR")
            {
                //get settings if none exists, exit if user cancels otherwise continue and start monitor
                if (_spLogMonitorSettings == null)
                {
                    var settings = GetSPLogMonitorDirectory(out _spLogMonitorSettings);
                    if (!settings)
                        return;
                }

                //if settings exist already, start monitor
                if (_spLogMonitorSettings != null)
                {
                    if (_spLogMonitor == null)
                    {
                        CreateSPLogMonitor();
                    }

                    if (!_spLogMonitor.IsRunning)
                    {
                        _spLogMonitor.Start();
                        imgMonitoring.Source = (BitmapImage)FindResource("bmpStop16");
                        btnMonitoring.ToolTip = "Stop log monitoring.";

                        if (_spLogMonitorWin == null)
                        {
                            _spLogMonitorWin = new SPLogMonitor(_spLogMonitor.LogSet);
                        }

                        _spLogMonitorWin.Show();
                    }
                    else
                    {
                        _spLogMonitor.Stop();
                        imgMonitoring.Source = (BitmapImage)FindResource("bmpPlay16");
                        btnMonitoring.ToolTip = "Start log monitoring...";
                    }
                }
            }
        }

        void logsetSearch_Cancelled(object sender, EventArgs e)
        {
            ToggleSearchPanelVisbility();
        }

        private void mainmenu_MouseEnter(object sender, MouseEventArgs e)
        {
            if (tcLogSets.Items.Count > 0)
            {
                var hdr = string.Format("'{0}'...", this.ActiveTab.Header);
                menuSave.Header = string.Concat("Save ", hdr);
                menuPrint.Header = string.Concat("Print ", hdr);
                SetControlIsEnabledAndOpacity(true, menuSave, menuPrint);

            }
            else
            {
                menuSave.Header = "Save";
                menuPrint.Header = "Print";
                SetControlIsEnabledAndOpacity(false, menuSave, menuPrint);
            }

        }

        private void tcLogSets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ActiveTab != null)
            {
                logsetSearch.LoadSearchOptions(this.ActiveTab.SearchOptions);
                SetSearchPanelUserMeessage();
                var alertContents = this.ActiveTab.Content.GetAlertContents();
                alertContents.OnViewErrors += (o, a) =>
                    {
                        var fileDetails = new LogFileDetails((ILogFileDetails)this.ActiveTab.Content.LogSetDataSource);
                        fileDetails.Owner = this;
                        fileDetails.CurrentTab = LogFileDetailsTab.Errors;
                        fileDetails.ShowDialog();
                    };
                Alert.Content = alertContents.Controls;
            }
            else
            {
                Alert.Content = null;
            }
        }

        private void logsetSearch_DoSearch(object sender, LogSetSearchEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                this.ActiveTab.SearchOptions = e.Options;
                if (!e.Options.IsEmptySearch)
                {
                    SetSearchPanelUserMeessage();
                }
                else
                {
                    logsetSearch.SetUserInfoMessage(string.Empty);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

        }

        #endregion

        #region Helpers

        /// <summary>
        /// Toggles search panel visibility
        /// </summary>
        private void ToggleSearchPanelVisbility()
        {
            if (mainGridRow2.Width.Value == 0)
            {
                //show panel
                mainGridRow2.Width = _searchColumnWidth;
                spltSearch.Width = 1;
            }
            else
            {
                //hide panel
                _searchColumnWidth = mainGridRow2.Width;
                mainGridRow2.Width = new GridLength(0);
                spltSearch.Width = 0;
            }
        }

        /// <summary>
        /// Sets the message displayed to user in the search panel. Right now this only displays the search results.
        /// </summary>
        private void SetSearchPanelUserMeessage()
        {
            if (this.ActiveTab.SearchOptions.IsEmptySearch)
            {
                logsetSearch.SetUserInfoMessage(string.Empty);
            }
            else
            {
                var msg = string.Concat(this.ActiveTab.Content.Count, " entries returned (", Math.Round(this.ActiveTab.SearchOptions.Duration, 3), " seconds)");
                logsetSearch.SetUserInfoMessage(msg);
            }
        }

        /// <summary>
        /// Chesks for permissions and creates a new SharePoing log monitor. 
        /// Throws SecurityException if cannot create monitor.
        /// </summary>
        private void CreateSPLogMonitor()
        {
            try
            {
                var perms = new PermissionSet(PermissionState.Unrestricted);
                perms.Demand();
                _spLogMonitor = new SharePointLogMonitor()
                {
                    MonitorDirectory = _spLogMonitorSettings.Directory,
                    Interval = _spLogMonitorSettings.Interval.Value
                };

                var path = _spLogMonitorSettings.Directory;
                if (_spLogMonitorSettings.Directory.Length > 20)
                {
                    path = string.Concat(_spLogMonitorSettings.Directory.Substring(0, 18), "...");
                }
            }
            catch (SecurityException)
            {
                MessageBox.Show("You do not have the required systems permissions to start log monitoring! Please restart the utility using an Administrator account.",
                    "Permissions Error!!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private bool GetSPLogMonitorDirectory(out SharePointLogMonitorSettings settings)
        {
            var sets = new SPLogMonitorSettings()
            {
                MonitorDirectory = (_spLogMonitorSettings != null) ? _spLogMonitorSettings.Directory : string.Empty
            };
            sets.Owner = this;
            var result = sets.ShowDialog();

            if (result.Value)
            {
                settings = new SharePointLogMonitorSettings()
                {
                    Directory = sets.MonitorDirectory,
                    Interval = sets.Interval
                };
                return true;
            }
            else
            {
                settings = null;
                return false;
            }

        }

        /// <summary>
        /// Enables/Disables control
        /// </summary>
        /// <param name="control"></param>
        /// <param name="enabled"></param>
        private void SetControlIsEnabledAndOpacity(bool enabled, params Control[] controls)
        {
            foreach (var control in controls)
            {
                control.IsEnabled = enabled;

                if (enabled)
                {
                    control.Opacity = 1;
                }
                else
                {
                    control.Opacity = .2;
                }
            }
        }

        /// <summary>
        /// Shows file select window
        /// </summary>
        private string[] ShowSelectFilesDialog()
        {
            var results = new string[0];
            var active = true;

            while (active)
            {
                var dlg = new OpenFileDialog()
                {
                    Filter = "SharePoint Log (*.log)|*.log| LogViewer CSV (*.csv)|*.csv",
                    Multiselect = true,
                    Title = "Open Log File(s)",
                };

                var userResult = dlg.ShowDialog();
                if (userResult.HasValue && userResult.Value)
                {
                    var exts = new HashSet<string>();
                    foreach (var fi in dlg.FileNames)
                    {
                        exts.Add(Path.GetExtension(fi));
                    }

                    if (exts.Count > 1)
                    {
                        MessageBox.Show("Each newly opened log set must contain the same file type (SharePoint, LogViewer common, etc).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        active = false;
                        results = dlg.FileNames;
                    }
                }
                else
                {
                    active = false;
                }
            }

            return results;
        }

        /// <summary>
        /// Process log file
        /// </summary>
        /// <param name="filenames"></param>
        private void ProcessLogFiles(params string[] filenames)
        {
            if (filenames.Length > 0)
            {
                try
                {
                    var includeWinLog = optIncludeWinLog.IsChecked;
                    var errorLog = optErrorLog.IsChecked;

                    SetMainWindowStatusMessage("Loading files...");
                    var startTime = DateTime.Now;
                    Task<LogSet>.Factory.StartNew(() =>
                        {
                            var ext = Path.GetExtension(filenames[0]).ToUpper();
                            if (ext == ".LOG")
                            {
                                return new SharePointLogSet(filenames, includeWinLog, errorLog);
                            }
                            else if (ext == ".CSV")
                            {
                                return new CommonLogSet(filenames);
                            }

                            return new LogSet() { HasErrors = false };

                        }).ContinueWith((t) =>
                        {
                            var logset = t.Result;

                            //if logset implements ILogFileDetails use LoadTime, otherwise calculate 
                            var totalTime = 0d;
                            if (logset is ILogFileDetails)
                            {
                                totalTime = ((ILogFileDetails)logset).LoadTime;
                            }
                            else
                            {
                                totalTime = Math.Round((DateTime.Now - startTime).TotalSeconds, 2);
                            }

                            CreateLogSetTabItem(logset);
                            EnableLogSetControls(true);

                            var msg = (t.Result.HasErrors) ? string.Format("Files processed with errors. ({0} seconds)", totalTime) :
                                string.Format("{0} file(s) processed. ({1} seconds)", filenames.Length, totalTime);
                            SetMainWindowStatusMessageWithReset(10000, "Complete!", msg);

                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException)
                    {
                        var aggEx = ex as AggregateException;
                        aggEx.Handle((x) =>
                        {
                            ex.LogException("MainWindow.btnOpenNewSet_Click");
                            return true;
                        });
                    }
                    else
                    {
                        ex.LogException("MainWindow.btnOpenNewSet_Click");
                    }

                    SetMainWindowStatusMessageWithReset(10000, "Error");
                }
            }
        }

        /// <summary>
        /// Creates a LogSetTabItem
        /// </summary>
        /// <param name="logset"></param>
        /// <returns></returns>
        private void CreateLogSetTabItem(LogSet logset)
        {
            Interlocked.Increment(ref _currentTabCount);
            var heading = string.Format("Set [{0}]", _currentTabCount);
            logset.Name = heading;
            var tabitem = new LogSetTabItem()
            {
                Header = heading,
                Content = new LogSetGrid(logset),
                SearchOptions = new LogSetSearchOptions() { IsEmptySearch = true },
                OwnerWindow = this
            };
            tabitem.Close += (sen, arg) =>
            {
                tcLogSets.RemoveLogSetTab(sen);
                if (tcLogSets.Items.Count == 0)
                {
                    EnableLogSetControls(false);
                }
            };

            tcLogSets.AddLogSetTab(tabitem);
        }

        /// <summary>
        /// Enables/Disables log set UI controls
        /// </summary>
        /// <param name="enable"></param>
        private void EnableLogSetControls(bool enable)
        {
            SetControlIsEnabledAndOpacity(enable, btnSearch, btnMetrics);

            if (enable)
            {
                mainGrid.Visibility = Visibility.Visible;
            }
            else
            {
                mainGrid.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Sets status bar message
        /// </summary>
        /// <param name="status"></param>
        /// <param name="info"></param>
        private void SetMainWindowStatusMessage(string status, string info = "")
        {
            statusbarLeft.Text = status;
            statusbarRight.Text = info;

            if (!string.IsNullOrEmpty(statusbarLeft.Text) && !string.IsNullOrEmpty(statusbarRight.Text))
            {
                statusSeperator.Visibility = Visibility.Visible;
            }
            else
            {
                statusSeperator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Sets initial status bar message and then resets after specified timeperioud
        /// </summary>
        /// <param name="resetDelay">reset delay in milliseconds</param>
        /// <param name="status"></param>
        /// <param name="info"></param>
        private void SetMainWindowStatusMessageWithReset(int resetDelay, string status, string info = "")
        {
            SetMainWindowStatusMessage(status, info);
            Task.Delay(resetDelay).ContinueWith((t, o) =>
                {
                    SetMainWindowStatusMessage(_statusReadyMsg);
                }, CancellationToken.None, TaskScheduler.FromCurrentSynchronizationContext());
        }


        #endregion
    }
}
