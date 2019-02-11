using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib
{
    public class LogEntryError
    {
        public Exception Exception { get; set; }
        public string Source { get; set; }
        public LogEntryErrorType Type { get; set; }
    }
}
