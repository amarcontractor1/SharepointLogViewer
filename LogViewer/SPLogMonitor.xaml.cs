using LogViewer.CustomControls;
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
    /// Interaction logic for SPLogMonitor.xaml
    /// </summary>
    public partial class SPLogMonitor : Window
    {

        private SPLogMonitor()
        {
            InitializeComponent();
        }

        public SPLogMonitor(LogSet logset)
            : this()
        {
            var grid = new LogSetGrid(logset, true);
            Grid.SetColumn(grid, 0);
            Grid.SetRow(grid, 1);
            main.Children.Add(grid);

            this.Closing += SPLogMonitor_Closing;
        }

        void SPLogMonitor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
