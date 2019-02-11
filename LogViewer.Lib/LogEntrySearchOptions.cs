using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogViewer.Lib
{
    public class LogEntrySearchOptions
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string Process { get; set; }     
        public string TID { get; set; }
        public string Area { get; set; }
        public string Category { get; set; }
        public string EventID { get; set; }
        public LogEntryLevel? Level { get; set; }
        public ComparisonOperator LevelCompareType { get; set; }
        public string Message { get; set; }   

        public LogEntrySearchOptions()
        {
            LevelCompareType = ComparisonOperator.Equal;//default
        }
    }
}
