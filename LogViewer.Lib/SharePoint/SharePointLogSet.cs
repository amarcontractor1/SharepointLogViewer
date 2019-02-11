using LogViewer.Lib.WindowsEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogViewer.Lib.SharePoint
{
    public class SharePointLogSet : LogSet, ILogFileDetails
    {

        #region Declarations

        public bool IncludeWindowsAppLogEvents { get; private set; }
        public string[] FileNames { get; set; }
        public int Count
        {
            get
            {
                return this.Entries.Count;
            }
        }
        public Dictionary<string, string[]> ParserErrors { get; set; }
        public double LoadTime { get; set; }

        #endregion

        #region Constructors

        public SharePointLogSet()
        {
            
        }

        public SharePointLogSet(string[] files, bool includeWindowAppLogEvents = false, bool dumpErrors = false)
            : this()
        {
            var startTime = DateTime.Now;
            this.IncludeWindowsAppLogEvents = includeWindowAppLogEvents;
            this.FileNames = files;
            var runningTasks = new List<Task<SharePointLogFile>>(files.Length + 1);
            var logs = new List<SharePointLogFile>();

            foreach (var fi in this.FileNames)
            {
                var t = Task<SharePointLogFile>.Factory.StartNew(() => { return new SharePointLogFile(fi); });
                runningTasks.Add(t);
            }

            //wait for tasks to finish
            Task.WaitAll(runningTasks.ToArray());
            runningTasks.ForEach(t => logs.Add(t.Result));

            //create list of  entries and Sort to get min/max dates
            var capacity = logs.Sum(x => x.LogEntries.Count);

            //combine results
            foreach (var log in logs)
            {
                foreach (var entry in log.LogEntries)
                {
                    this.Entries.Add(entry);
                }
            }

            //errors
            logs.ForEach(l => this.Errors.AddRange(l.LogParseErrors));
            this.HasErrors = this.Errors.Count > 0;

            //ILogFileDetails error
            this.ParserErrors = new Dictionary<string, string[]>();
            var grpErrors = from err in this.Errors
                             group err by err.Source into file
                             select new { FileName = file.Key, Errors = file.ToList() };

            foreach (var grp in grpErrors)
            {
                var fileErrors = new List<string>();
                foreach (var er in grp.Errors)
                {
                    if (er.Type == LogEntryErrorType.Parser)
                    {
                        fileErrors.Add(er.Exception.Message);
                    }
                }
                this.ParserErrors.Add(grp.FileName, fileErrors.ToArray());
            }


            //create WinAppEventLogSet instance using event window
            DateTime startDate = this.Entries[0].Timestamp;
            DateTime endDate = this.Entries[this.Entries.Count - 1].Timestamp;

            if (this.IncludeWindowsAppLogEvents)
            {
                var eventlog = new WindowsEventLogSet(startDate, endDate);
                foreach (var ev in eventlog.Entries)
                {
                    this.Entries.Add(ev);
                }
            }

            this.Entries.Sort();
            var totalTime = Math.Round((DateTime.Now - startTime).TotalSeconds, 2);
            this.LoadTime = totalTime;
        }

        #endregion
    }
}
