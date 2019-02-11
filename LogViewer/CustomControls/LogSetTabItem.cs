using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LogViewer.Lib;

namespace LogViewer.CustomControls
{
    class LogSetTabItem : TabItem
    {
        #region Declarations

        public event EventHandler Close;
        private MenuItem _optDetails;
        public string InteralID { get; set; }
        public Window OwnerWindow { get; set; }
        public Window MetricsWindow { get; set; }

        private LogSetSearchOptions _searchOptions;
        public LogSetSearchOptions SearchOptions
        {
            get
            {
                return _searchOptions;
            }
            set
            {
                var startTime = DateTime.Now;
                _searchOptions = value;
                this.Content.Search(_searchOptions);
                _searchOptions.Duration = (DateTime.Now - startTime).TotalSeconds;
                _searchOptions.ResultsCount = this.Content.Count;
            }
        }

        public new object Header
        {
            get
            {
                var label = base.Header as Label;
                return label.Content;
            }
            set
            {
                var label = new Label()
                {
                    Padding = new Thickness(5, 0, 5, 0),
                    Content = value
                };
                base.Header = label;
            }
        }

        public new LogSetGrid Content
        {
            get
            {
                return (LogSetGrid)base.Content;
            }
            set
            {
                _optDetails.IsEnabled = (value.LogSetDataSource is ILogFileDetails);
                base.Content = value;
            }
        }


        #endregion

        #region Constructors and Initializors

        //private LogSetTabItem() { }

        public LogSetTabItem()
            : base()
        {
            this.Margin = new Thickness(0, 0, 0, 0);
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.ContextMenu = CreateContextMenu();
        }

        /// <summary>
        /// Creates context menu for tab item
        /// </summary>
        /// <returns></returns>
        private ContextMenu CreateContextMenu()
        {
            var menu = new ContextMenu();
            menu.Opened += menu_Opened;

            //merge results with another 
            var optMerge = new MenuItem() { Header = "Merge log set with..." };
            optMerge.Click += menuOpt_Merge;
            optMerge.Icon = new Image
            {
                Source = (BitmapImage)FindResource("bmpMerge16")
            };
            menu.Items.Insert(0, optMerge);

            //copy selected items 
            var optCopy = new MenuItem() { Header = "Copy seletected item(s) to..." };
            optCopy.Click += menuOpt_Copy;
            optCopy.Icon = new Image
            {
                Source = (BitmapImage)FindResource("bmpCopy16")
            };
            menu.Items.Insert(1, optCopy);

            //seperator
            menu.Items.Add(new Separator());

            //rename logset
            var optRename = new MenuItem() { Header = "Rename log set..." };
            optRename.Click += menuOpt_Rename;
            menu.Items.Add(optRename);

            //column visbility
            var optColumns = new MenuItem() { Header = "Select columns..." };
            optColumns.Click += menuOpt_Columns;
            menu.Items.Add(optColumns);

            //seperator
            menu.Items.Add(new Separator());

            //view log file details
            _optDetails = new MenuItem() { Header = "View file(s) details" };
            _optDetails.Click += (s, e) =>
            {
                var logset = this.Content.LogSetDataSource;
                var fileDetails = new LogFileDetails((ILogFileDetails)logset);
                fileDetails.Owner = this.OwnerWindow;
                fileDetails.ShowDialog();
            };
            menu.Items.Add(_optDetails);
            menu.Items.Add(new Separator());

            //menu option to close set
            var optionClose = new MenuItem() { Header = "Close" };
            optionClose.Click += (s, e) =>
            {
                if (Close != null)
                    Close(this, new EventArgs());
            };
            menu.Items.Add(optionClose);

            return menu;
        }

        #endregion

        #region Menu Events

        void menu_Opened(object sender, RoutedEventArgs e)
        {
            var optionMerge = this.ContextMenu.Items[0] as MenuItem;
            optionMerge.Items.Clear();
            var optionCopy = this.ContextMenu.Items[1] as MenuItem;
            optionCopy.Items.Clear();

            foreach (var item in GetOpenTabMenuItems())
            {
                optionMerge.Items.Add(item);
            }

            foreach (var item in GetOpenTabMenuItems())
            {
                optionCopy.Items.Add(item);
            }

            optionMerge.IsEnabled = optionMerge.Items.Count > 0;
            optionCopy.IsEnabled = (optionMerge.Items.Count > 0 && this.Content.SelectedEntries.Count > 0);
        }

        void menuOpt_Copy(object sender, RoutedEventArgs e)
        {

            var optionMerge = e.Source as MenuItem;
            var internalID = optionMerge.CommandParameter.ToString();
            var tabControl = this.Parent as LogSetTabControl;

            foreach (var tab in tabControl.Items)
            {
                var obj = tab as LogSetTabItem;
                if (obj.InteralID == internalID)
                {
                    var targetGrid = obj.Content as LogSetGrid;
                    targetGrid.LogSetDataSource.Merge(this.Content.SelectedEntries, true);
                    break;
                }
            }

        }

        void menuOpt_Rename(object sender, RoutedEventArgs e)
        {
            var simpleTxt = new SimpleTextbox("Rename logset...", (string)this.Header);
            simpleTxt.Owner = this.OwnerWindow;
            simpleTxt.Updated += (s, ev) =>
            {
                this.Header = ev.Text;
                this.Content.LogSetDataSource.Name = ev.Text;
            };
            simpleTxt.ShowDialog();
        }

        void menuOpt_Columns(object sender, RoutedEventArgs e)
        {
            var selectedColumns = this.Content.SelectedColumns;
            var columnsSeletor = new ColumnsSelector(selectedColumns);
            columnsSeletor.Owner = this.OwnerWindow;
            var title = this.Header;
            columnsSeletor.Title = string.Concat(title, " Columns");
            columnsSeletor.Updated += (s, ev) =>
            {
                var selector = s as ColumnsSelector;
                this.Content.SetColumnVisibility(ev.Selected);
            };

            columnsSeletor.ShowDialog();
        }

        void menuOpt_Merge(object sender, RoutedEventArgs e)
        {
            var optionMerge = e.Source as MenuItem;
            var internalID = optionMerge.CommandParameter.ToString();
            var tabControl = this.Parent as LogSetTabControl;

            var msg = string.Format("Press OK if you would like to merge log set '{0}' with '{1}'? Please note that '{0}' will be closed.", optionMerge.Header, this.Header);
            if (MessageBox.Show(msg, "Confirm Merge", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                foreach (var tab in tabControl.Items)
                {
                    var obj = tab as LogSetTabItem;
                    if (obj.InteralID == internalID)
                    {
                        var grid = obj.Content as LogSetGrid;
                        this.Content.LogSetDataSource.Merge(grid.LogSetDataSource, true);
                        tabControl.RemoveLogSetTab(tab);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Helpers

        private List<MenuItem> GetOpenTabMenuItems()
        {
            var menuitems = new List<MenuItem>();

            var parentTabs = ((LogSetTabControl)this.Parent).Items;
            foreach (var item in parentTabs)
            {
                var obj = item as LogSetTabItem;

                if (obj.InteralID != this.InteralID)
                {
                    var mergeItem = new MenuItem()
                    {
                        Header = obj.Header.ToString(),
                        CommandParameter = obj.InteralID
                    };

                    menuitems.Add(mergeItem);
                }
            }

            return menuitems;
        }

        #endregion
    }
}
