using LogViewer.Lib;
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
using System.Windows.Shapes;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for ColumnsSelector.xaml
    /// </summary>
    public partial class ColumnsSelector : Window
    {

        public Dictionary<string, bool> Selected { get; private set; }
        public event EventHandler<SelectedColumnsEventArgs> Updated;

        public ColumnsSelector(Dictionary<string, bool> columns)
        {
            InitializeComponent();
            this.Selected = columns;

            foreach (var col in this.Selected)
            {
                //create label
                gridMain.RowDefinitions.Add(new RowDefinition());
                var label = new Label()
                {
                    Content = col.Key,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0, 3, 3, 3)
                };
                gridMain.Children.Add(label);
                Grid.SetColumn(label, 0);
                Grid.SetRow(label, gridMain.RowDefinitions.Count - 1);

                //create checkbox
                var checkbox = new CheckBox()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    CommandParameter = col.Key
                };
                checkbox.IsChecked = col.Value;
                checkbox.Checked += checkbox_HandleCheck;
                checkbox.Unchecked += checkbox_HandleCheck;               
                gridMain.Children.Add(checkbox);
                Grid.SetColumn(checkbox, 1);
                Grid.SetRow(checkbox, gridMain.RowDefinitions.Count - 1);        
            }

            gridMain.RowDefinitions.Add(new RowDefinition());
            var btnOK = new Button()
            {
                Content = "OK",
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 60,
                Padding = new Thickness(3, 2, 3, 2),
                Margin = new Thickness(0, 10, 5, 0)
            };
            btnOK.Click += (s, e) =>
            {
                if (Updated != null)
                {
                    var args = new SelectedColumnsEventArgs()
                    {
                        Selected = this.Selected
                    };
                    Updated(this, args);
                }
                this.Close();
            };
            gridMain.Children.Add(btnOK);
            Grid.SetColumn(btnOK, 0);
            Grid.SetRow(btnOK, gridMain.RowDefinitions.Count - 1);

            var btnCancel = new Button()
            {
                Content = "Cancel",
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 60,
                Padding = new Thickness(3, 2, 3, 2),
                Margin = new Thickness(5, 10, 0, 0)
            };
            btnCancel.Click += (s, e) =>
            {
                this.Close();
            };
            gridMain.Children.Add(btnCancel);
            Grid.SetColumn(btnCancel, 1);
            Grid.SetRow(btnCancel, gridMain.RowDefinitions.Count - 1);
        }



        void checkbox_HandleCheck(object sender, RoutedEventArgs e)
        {
            var cb = e.Source as CheckBox;
            this.Selected[(string)cb.CommandParameter] =
                (cb.IsChecked.HasValue) ? cb.IsChecked.Value : false;

        }
    }
}
