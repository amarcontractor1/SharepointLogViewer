using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib.CommonLog
{
    public class CommonLogSet : LogSet, ILogFileDetails
    {
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

        public CommonLogSet()
        {

        }

        public CommonLogSet(string[] files, bool dumpErrors = false)
        {
            var startTime = DateTime.Now;
            this.FileNames = files;
            var runningTasks = new List<Task<CommonLogFile>>(files.Length + 1);
            var logs = new List<CommonLogFile>();

            foreach(var fi in files)
            {
                var t = Task<CommonLogFile>.Factory.StartNew(() => { return new CommonLogFile(fi); });
                runningTasks.Add(t);
            }

            Task.WaitAll(runningTasks.ToArray());
            runningTasks.ForEach(t => logs.Add(t.Result));

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

            this.Entries.Sort();
            var totalTime = Math.Round((DateTime.Now - startTime).TotalSeconds, 2);
            this.LoadTime = totalTime;
        }
    }
}
