using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Timer = System.Timers.Timer;

namespace LogViewer.Lib.SharePoint
{
    public class SharePointLogMonitor : INotifyPropertyChanged, IDisposable
    {
        #region Declarations

        private const string _propMonitorDirectory = "MonitorDirectory";
        private const string _propLogset = "LogSet";
        private FileSystemWatcher _fsw;
        private int _pos;
        private string _file;
        private object _lock = new object();
        private SharePointLogFileParser _parser = new SharePointLogFileParser();
        private Timer _timer = new Timer();
        public LogSet LogSet { get; private set; }
        public bool IsRunning { get; private set; }
        public string MonitorDirectory
        {
            get
            {
                return _fsw.Path;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    _fsw.Path = value;
                    NotifyPropertyChanged(_propMonitorDirectory);
                }
                else
                {
                    throw new DirectoryNotFoundException("Directory does not exist!");
                }
            }
        }
        /// <summary>
        /// value in seconds
        /// </summary>
        private int _interval;
        public int Interval
        {
            get
            {
                return _interval;
            }
            set
            {
                _interval = value;
                _timer.Interval = _interval * 1000;
            }
        }

        #endregion

        #region Constructors

        public SharePointLogMonitor()
        {
            _pos = -1;
            this.IsRunning = false;
            _fsw = new FileSystemWatcher()
            {
                IncludeSubdirectories = false,
                Filter = "*.log"
            };

            _fsw.Created += _fsw_Created;
            _fsw.Changed += _fsw_Changed;
            _timer.Elapsed += _timer_Elapsed;

            this.LogSet = new LogSet();
        }

        public SharePointLogMonitor(string Directory)
            : this()
        {
            this.MonitorDirectory = Directory;
        }

        #endregion

        #region Actions

        public void Start()
        {
            _timer.Start();
            _fsw.EnableRaisingEvents = true;
            this.IsRunning = true;
        }

        public void Stop()
        {
            _timer.Stop();
            _fsw.EnableRaisingEvents = false;
            this.IsRunning = false;
        }

        #endregion

        #region Events

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Enabled = false;

            if (!string.IsNullOrEmpty(_file))
            {
                var entries = new List<LogEntry>();
                var strm = File.OpenRead(_file);
                using (var contents = new StreamReader(strm))
                {
                    //move cursor to current location
                    var currentPos = -1;
                    while (contents.Peek() > -1)
                    {
                        currentPos++;
                        var line = contents.ReadLine();
                        if (currentPos < _pos)
                            continue;
                        else
                            break;
                    }

                    while (contents.Peek() > -1)
                    {
                        try
                        {
                            currentPos++;
                            var entry = _parser.ParseSingleLine(contents.ReadLine());
                            entries.Add(entry);
                        }
                        catch { /* do nothing */ }
                        finally
                        {
                            _pos = currentPos;
                        }
                    }
                }

                if (entries.Count > 0)
                {
                    try
                    {
                        this.LogSet.Merge(entries, true);
                        NotifyPropertyChanged(_propLogset);
                    }
                    catch { }
                }
            }

            _timer.Enabled = true;
        }

        void _fsw_Created(object sender, FileSystemEventArgs e)
        {
            InitLogFile(e.FullPath);
        }

        void _fsw_Changed(object sender, FileSystemEventArgs e)
        {
            if (string.IsNullOrEmpty(_file))
            {
                InitLogFile(e.FullPath);
                _fsw.Changed -= _fsw_Changed;
            }
        }

        #endregion

        #region Helpers

        private void InitLogFile(string path)
        {
            string error;
            if (!_parser.ValidateFileFormat(path, out error))
                return;

            _pos = -1;
            _file = path;
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

        #region IDisposable

        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }
            }

            disposed = true;
        }


        ~SharePointLogMonitor()
        {
            Dispose(true);
        }

        #endregion
    }
}
