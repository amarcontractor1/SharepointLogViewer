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
    /// Interaction logic for SimpleTextbox.xaml
    /// </summary>
    public partial class SimpleTextbox : Window
    {
        public event EventHandler<SimpleTextboxEventArgs> Updated;

        public SimpleTextbox()
        {
            InitializeComponent();
        }

        public SimpleTextbox(string title, string text)
            : this()
        {
            this.Title = title;
            txtInput.Text = text;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (Updated != null)
            {
                Updated(this, new SimpleTextboxEventArgs() { Text = txtInput.Text });
            }

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
