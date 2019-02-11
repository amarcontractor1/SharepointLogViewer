using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib
{
    public class LogSetSearchOptions : LogEntrySearchOptions
    {
        public double Duration { get; set; }
        public int ResultsCount { get; set; }

        //This was added to remove NULL checks. Setting this to true means all entries match by default.
        public bool IsEmptySearch { get; set; }
    }
}
