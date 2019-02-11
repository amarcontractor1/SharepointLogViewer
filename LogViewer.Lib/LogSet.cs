using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LogViewer.Lib
{
    /// <summary>
    /// Collection of LogEntry items
    /// </summary>
    public class LogSet : INotifyPropertyChanged
    {

        #region Declarations

        public ObservableCollection<LogEntry> Entries { get; private set; }
        public List<LogEntryError> Errors { get; set; }
        public bool HasErrors { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged(_propName);
            }
        }

        private const string _propEntries = "Entries";
        private const string _propName = "Name";
        private object _lock = new object();
        
        #endregion

        #region Constructors

        public LogSet()
        {
            this.Entries = new ObservableCollection<LogEntry>();
            BindingOperations.EnableCollectionSynchronization(this.Entries, _lock);
            this.Errors = new List<LogEntryError>();
        }

        #endregion

        #region Actions

        /// <summary>
        /// Merges current log set with another
        /// </summary>
        /// <param name="entries"></param>
        public virtual void Merge(LogSet logset, bool sort = false)
        {
            foreach (var l in logset.Entries)
            {
                this.Entries.Add(l);
            }

            if (sort)
            {
                this.Entries.Sort();
            }


            NotifyPropertyChanged(_propEntries);
        }

        /// <summary>
        /// Merges entries into current log
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="sort"></param>
        public virtual void Merge(IList<LogEntry> entries, bool sort = false)
        {
            foreach (var e in entries)
            {
                this.Entries.Add(e);
            }

            if (sort)
            {
                this.Entries.Sort();
            }

            NotifyPropertyChanged(_propEntries);
        }

        /// <summary>
        /// Clears current log
        /// </summary>
        public virtual void Clear()
        {
            this.Entries.Clear();
            NotifyPropertyChanged(_propEntries);
        }

        /// <summary>
        /// Saves entries to flat file
        /// </summary>
        /// <param name="stream"></param>
        public virtual void Save(StreamWriter stream)
        {
            var names = LogEntry.GetColumnNames();
            var heading = string.Join(",", names[0], names[1],
                names[2], names[3], names[4], names[5], names[6], names[7]);
            stream.WriteLine(heading);

            foreach (var entry in this.Entries)
            {
                var ln = string.Join(",",
                     string.Concat("'", entry.Timestamp, "'"),
                     string.Concat("'", entry.Process, "'"),
                     string.Concat("'", entry.TID, "'"),
                     string.Concat("'", entry.Area, "'"),
                     string.Concat("'", entry.Category, "'"),
                     string.Concat("'", entry.EventID, "'"),
                     string.Concat("'", entry.Level, "'"),
                     string.Concat("'", entry.Message, "'")
                    );
                stream.WriteLine(ln);
            }
        }


        /// <summary>
        /// Calculates metrics
        /// </summary>
        /// <returns></returns>
        public LogSetMetrics GetLogSetMetrics()
        {
            var metrics = new LogSetMetrics();

            if (this.Entries.Count > 0)
            {
                this.Entries.Sort();
                System.Threading.Thread.Sleep(100);//fixes small file metric load (blank otherwise) (why??)
                Task.WaitAll(
                    Task.Factory.StartNew(() =>
                        {
                            var tids = from en in this.Entries
                                       group en by en.TID into tid
                                       select new { Name = tid.Key, Count = tid.Count() };
                            tids.ToList().ForEach(x => metrics.TIDs.Add(x.Name, x.Count));
                        }),
                    Task.Factory.StartNew(() =>
                       {

                           var processes = from en in this.Entries
                                           group en by en.Process into proc
                                           select new { Name = proc.Key, Count = proc.Count() };
                           processes.ToList().ForEach(x => metrics.Processes.Add(x.Name, x.Count));
                       }),

                    Task.Factory.StartNew(() =>
                      {
                          var areas = from en in this.Entries
                                      group en by en.Area into area
                                      select new { Name = area.Key, Count = area.Count() };
                          areas.ToList().ForEach(x => metrics.Areas.Add(x.Name, x.Count));
                      }),

                    Task.Factory.StartNew(() =>
                      {

                          var categories = from en in this.Entries
                                           group en by en.Category into cat
                                           select new { Name = cat.Key, Count = cat.Count() };
                          categories.ToList().ForEach(x => metrics.Categories.Add(x.Name, x.Count));
                      }),

                    Task.Factory.StartNew(() =>
                        {
                            var eventids = from en in this.Entries
                                           group en by en.EventID into events
                                           select new { Name = events.Key, Count = events.Count() };
                            eventids.ToList().ForEach(x => metrics.EventIDs.Add(x.Name, x.Count));
                        }),

                    Task.Factory.StartNew(() =>
                      {
                          var levels = from en in this.Entries
                                       group en by en.Level.ToString() into level
                                       select new { Name = level.Key.ToString(), Count = level.Count() };
                          levels.ToList().ForEach(x => metrics.Levels.Add(x.Name, x.Count));
                      })
                );
            }

            return metrics;
        }


        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
