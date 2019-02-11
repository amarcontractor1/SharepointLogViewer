using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LogViewer.Controls
{
    public class ColumnSettings
    {
        public string Name { get; set; }
        public bool Visibile { get; set; }
        public double FontSize { get; set; }
        public Brush Background { get; set; }
    }
}
