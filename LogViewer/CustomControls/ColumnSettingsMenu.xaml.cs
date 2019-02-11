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

namespace LogViewer.Controls
{
    /// <summary>
    /// Interaction logic for ColumnSettingsMenu.xaml
    /// </summary>
    public partial class ColumnSettingsMenu : UserControl
    {
        List<ColumnSettings> _settings;

        public ColumnSettingsMenu()
        {
            InitializeComponent();
        }

        public void LoadSettings(List<ColumnSettings> settings)
        {
            _settings = settings;
            CreateGrid();

        }

        private void CreateGrid()
        {
            var lbl_0 = new Label() { Content = "Column" };
            var lbl_1 = new Label() { Content = "Visibility" };
            var lbl_2 = new Label() { Content = "Font-Size" };
            var lbl_3 = new Label() { Content = "Background" };
            Grid.SetRow(lbl_0, 0);
            Grid.SetColumn(lbl_0, 0);
            gridSettings.Children.Add(lbl_0);
            Grid.SetRow(lbl_1, 0);
            Grid.SetColumn(lbl_1, 1);
            gridSettings.Children.Add(lbl_1);
            Grid.SetRow(lbl_2, 0);
            Grid.SetColumn(lbl_2, 2);
            gridSettings.Children.Add(lbl_2);
            Grid.SetRow(lbl_3, 0);
            Grid.SetColumn(lbl_3, 3);
            gridSettings.Children.Add(lbl_3);

            //TODO: add color picker 
            var rowIndex = 1;
            foreach (var set in _settings)
            {
                //add new row
                gridSettings.RowDefinitions.Add(new RowDefinition());

                //create controls
                var lbl_colName = new Label() { Name = string.Format("lbl_{0}", set.Name), Content = set.Name };
                var cb_visibility = new CheckBox() { Name = string.Format("cb_{0}", set.Name), IsChecked = set.Visibile };
                var txt_fontSize = new TextBox() { Name = string.Format("txt_{0}", set.Name), Text = set.FontSize.ToString(), Width = 50d };

                //set index
                Grid.SetRow(lbl_colName, rowIndex);
                Grid.SetColumn(lbl_colName, 0);
                gridSettings.Children.Add(lbl_colName);
                Grid.SetRow(cb_visibility, rowIndex);
                Grid.SetColumn(cb_visibility, 1);
                gridSettings.Children.Add(cb_visibility);
                Grid.SetRow(txt_fontSize, rowIndex);
                Grid.SetColumn(txt_fontSize, 2);
                gridSettings.Children.Add(txt_fontSize);

                rowIndex++;
            }
        }
    }
}
