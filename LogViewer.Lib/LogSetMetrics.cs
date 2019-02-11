using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib
{
    public class LogSetMetrics
    {
        public Dictionary<string, int> TIDs { get; set; }
        public Dictionary<string, int> Processes { get; set; }
        public Dictionary<string, int> Areas { get; set; }
        public Dictionary<string, int> Categories { get; set; }
        public Dictionary<string, int> EventIDs { get; set; }
        public Dictionary<string, int> Levels { get; set; }
        //public Dictionary<DateTime, int> TimeSlices { get; set; }

        public LogSetMetrics()
        {
            this.TIDs = new Dictionary<string, int>();
            this.Processes = new Dictionary<string, int>();
            this.Areas = new Dictionary<string, int>();
            this.Categories = new Dictionary<string, int>();
            this.EventIDs = new Dictionary<string, int>();
            this.Levels = new Dictionary<string, int>();
            //this.TimeSlices = new Dictionary<DateTime, int>();
        }
    }
}
