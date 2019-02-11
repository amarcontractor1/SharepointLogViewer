using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace LogViewer.Lib
{
    /// <summary>
    /// Parses a log file
    /// </summary>
    /// <typeparam name="T">LogEntry type (Common, SharePoint, etc)</typeparam>
    public class LogFile<T> where T : LogEntry
    {
        public string Name { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasFileError { get; set; }
        public bool HasParsingError { get; set; }
        public List<T> LogEntries { get; protected set; }
        public List<LogEntryError> LogParseErrors { get; protected set; }
        public ILogFileParser<T> Parser { get; protected set; }

        private LogFile() { }

        /// <summary>
        /// Class requires both a filename and a parser
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="parser">The ILogFileParser to use</param>
        public LogFile(string filename, ILogFileParser<T> parser)
        {
            this.Name = filename;
            this.Parser = parser;
            this.LogParseErrors = new List<LogEntryError>();//init default

            string error;

            if (!this.Parser.ValidateFileFormat(filename, out error))
            {
                this.HasFileError = true;
                this.ErrorMessage = error;
            }
            else
            {
                var temp = new List<LogEntryError>();

                this.LogEntries = this.Parser.ParseFile(filename, out temp);
                this.LogParseErrors = temp;
                if (LogParseErrors.Count > 0)
                {
                    this.HasParsingError = true;
                    this.ErrorMessage = string.Format("{0} file entries could not be parsed.", LogParseErrors.Count);
                }
            }
        }
    }
}
