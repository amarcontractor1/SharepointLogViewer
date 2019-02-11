using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib.WindowsEvent
{
    public class WindowsEventLogSet : LogSet
    {
        private const string _applicationEventLog = "Application";
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }

        private WindowsEventLogSet() { }

        public WindowsEventLogSet(DateTime startTime, DateTime endTime)
        {
            if (startTime > endTime)
                throw new ArgumentException("The start time must come before the end time.");

            this.StartTime = startTime;
            this.EndTime = EndTime;

            if (EventLog.SourceExists(_applicationEventLog))
            {
                var appEvents = new EventLog() { Source = _applicationEventLog };

                foreach (EventLogEntry entry in appEvents.Entries)
                {
                    if (entry.TimeGenerated >= startTime && entry.TimeGenerated <= endTime)
                    {
                        this.Entries.Add(new WindowsEventEntry()
                        {
                            Area = "Windows Event Log",
                            Category = string.Concat(entry.Category, " (", entry.CategoryNumber, ")"),
                            ComputerName = entry.MachineName,
                            EventID = entry.InstanceId.ToString(),
                            Level = LogEntryLevel.Assert,
                            Message = entry.Message,
                            Process = entry.Source,
                            TID = "(unknown)",
                            Timestamp = entry.TimeGenerated
                        });
                    }
                }
            }
            else
            {
                throw new Exception("Application event log not found!");
            }
        }
    }
}
