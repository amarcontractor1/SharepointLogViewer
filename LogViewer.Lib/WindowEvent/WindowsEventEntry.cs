using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib.WindowsEvent
{
    public class WindowsEventEntry : LogEntry
    {
        public string ComputerName { get; set; }
    }
}
