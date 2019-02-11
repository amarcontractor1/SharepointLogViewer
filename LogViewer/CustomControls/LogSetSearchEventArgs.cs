using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogViewer.Lib;

namespace LogViewer.CustomControls
{
    public class LogSetSearchEventArgs : EventArgs
    {
        public LogSetSearchOptions Options { get; set; }
    }
}
