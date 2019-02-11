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
using LogViewer.Lib;
using System.Threading;
using System.ComponentModel;
using System.Collections;

namespace LogViewer.CustomControls
{
    public partial class LogSetGrid : UserControl
    {
        #region Declarations

        private LogSetSearchOptions _searchOptions;
        public LogSet LogSetDataSource { get; private set; }
        public Dictionary<string, bool> SelectedColumns { get; private set; }
        public List<LogEntry> SelectedEntries
        {
            get
            {
                return grid.SelectedItems.Cast<LogEntry>().ToList();
            }
            private set { }
        }
        public int Count { get { return grid.Items.Count; } private set { } }


        public DataGridGridLinesVisibility GridLineVisibility
        {
            get
            {
                return grid.GridLinesVisibility;
            }
            set
            {
                grid.GridLinesVisibility = value;
            }
        }

        #endregion

        #region Constructors & Initializors

        private LogSetGrid()
        {
            InitializeComponent();
        }

        public LogSetGrid(LogSet logset, bool IsLogMonitor = false)
            : this()
        {
            this.LogSetDataSource = logset;

            //init grid
            CreateGridColumns();
            BindGrid();

            if (IsLogMonitor)
            {
                //grid.CellStyle = null;
                grid.RowStyle = null;
                grid.IsReadOnly = true;
            }
        }

        /// <summary>
        /// Creates columns based on LogEntry type
        /// </summary>
        private void CreateGridColumns()
        {
            this.SelectedColumns = new Dictionary<string, bool>();

            foreach (var col in LogEntry.GetColumnNames())
            {
                var column = new DataGridTextColumn
                {
                    Header = col,
                    Binding = new Binding(col)
                };

                grid.Columns.Add(column);
                this.SelectedColumns.Add(col, true);
            }

            grid.Columns[grid.Columns.Count - 1].Width = new DataGridLength(4, DataGridLengthUnitType.Star);

        }

        /// <summary>
        /// Creates databinding using current datasource
        /// </summary>
        public void BindGrid()
        {
            if (this.LogSetDataSource != null)
            {
                var collectionSrc = new CollectionViewSource();
                collectionSrc.Source = this.LogSetDataSource.Entries;
                collectionSrc.Filter += collectionSrc_Filter;
                collectionSrc.SortDescriptions.Add(new SortDescription("Timestamp", ListSortDirection.Descending));

                //data binding
                var myBinding = new Binding()
                {
                    Mode = BindingMode.OneWay,
                    Source = collectionSrc
                };
                BindingOperations.SetBinding(grid, DataGrid.ItemsSourceProperty, myBinding);
                grid.SelectedIndex = -1;
            }
        }

        public LogSetAlert GetAlertContents()
        {
            var lsAlert = new LogSetAlert();
            var panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0),

            };


            if (this.LogSetDataSource.HasErrors)
            {
                lsAlert.HasErrors = true;
                var bmp = (BitmapImage)FindResource("bmpWhAttention16");
                var imgAttention = new Image()
                {
                    Height = 14,
                    Source = bmp,
                    VerticalAlignment = VerticalAlignment.Top,
                    ToolTip = "Log set created with errors. Please review file details for more information.",
                    Margin = new Thickness(0, 0, 5, 0)
                };
    
                var fauxClicked = false;
                imgAttention.MouseLeftButtonDown += ( o, e ) =>
                    {
                        fauxClicked = true;
                    };
                imgAttention.MouseLeftButtonUp += ( o, e) =>
                    {
                        if(fauxClicked)
                        {
                            //handle click event
                            lsAlert.ViewErrors();
                            
                        }
                    };
                imgAttention.MouseLeave += (o, e) =>
                    {
                        fauxClicked = false;
                    };

                panel.Children.Add(imgAttention);

                var seperator = new Rectangle()
                {
                    Width = 1,
                    Opacity = .3,
                    Stroke = Brushes.White,
                    Margin = new Thickness(10, 0, 10, 0)
                };
                panel.Children.Add(seperator);
            }

            var lblTotal = new TextBlock() { Text = string.Concat("Total Entries: ", this.Count) };
            panel.Children.Add(lblTotal);

            lsAlert.Controls = panel;
            return lsAlert;
        }

        #endregion

        #region Events

        void collectionSrc_Filter(object sender, FilterEventArgs e)
        {
            var entry = e.Item as LogEntry;

            if (_searchOptions != null)
            {
                if (!_searchOptions.IsEmptySearch)
                {
                    e.Accepted = entry.Filter(_searchOptions);
                }
            }
        }

        private void dgResults_Standard_Sorting(object sender, DataGridSortingEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            string sortPropertyName = (string)e.Column.Header;
            if (!string.IsNullOrEmpty(sortPropertyName))
            {
                // sorting is cleared when the previous state is Descending
                if (e.Column.SortDirection.HasValue && e.Column.SortDirection.Value == ListSortDirection.Descending)
                {
                    int index = dataGrid.Items.SortDescriptions.GetSortDescriptionIndex(sortPropertyName);
                    if (index != -1)
                    {
                        e.Column.SortDirection = null;

                        // remove the sort description
                        dataGrid.Items.SortDescriptions.RemoveAt(index);
                        dataGrid.Items.Refresh();

                        if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                        {
                            // clear any other sort descriptions for the multisorting case
                            dataGrid.Items.SortDescriptions.Clear();
                            dataGrid.Items.Refresh();
                        }

                        // stop the default sort
                        e.Handled = true;
                    }
                }
            }
        }

        #endregion

        #region Actions

        public void SetColumnVisibility(Dictionary<string, bool> columns)
        {
            foreach (var col in columns)
            {
                var column = grid.Columns.Where(c => (string)c.Header == col.Key).FirstOrDefault();
                if (column != null)
                {
                    if (col.Value)
                    {
                        column.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        column.Visibility = Visibility.Collapsed;
                    }
                }
            }

            this.SelectedColumns = columns;
        }

        public void Search(LogSetSearchOptions options)
        {
            _searchOptions = options;
            BindGrid();
        }

        /// <summary>
        /// Returns the main datagrid
        /// </summary>
        /// <returns></returns>
        public DataGrid GetGrid()
        {
            return grid;
        }

        #endregion
    }
}
