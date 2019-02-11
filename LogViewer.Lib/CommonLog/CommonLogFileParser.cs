using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogViewer.Lib.CommonLog
{
    class CommonLogFileParser : ILogFileParser<CommonLogEntry>
    {
        private const string _csvDelim = ",";

        public CommonLogFileParser() { }

        /// <summary>
        /// Parses the log file and returns a list of newly created CommonLogEntry items.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public List<CommonLogEntry> ParseFile(string filename, out List<LogEntryError> errors)
        {
            if (!File.Exists(filename))
                throw new ArgumentException("File not found!");

            var retval = new List<CommonLogEntry>();
            errors = new List<LogEntryError>();

            using (var file = new StreamReader(File.OpenRead(filename)))
            {
                file.ReadLine();//move past header
                while (file.Peek() >= 0)
                {
                    try
                    {
                        var logTokens = file.ReadLine().Split(new string[] { _csvDelim }, StringSplitOptions.None);
                        var timestamp = logTokens[0].Trim();
                        timestamp = timestamp.Substring(1, timestamp.Length - 2);
                        var process = logTokens[1].Trim();
                        process = process.Substring(1, process.Length - 2);
                        var tid = logTokens[2].Trim();
                        tid = tid.Substring(1, tid.Length - 2);
                        var area = logTokens[3].Trim();
                        area = area.Substring(1, area.Length - 2);
                        var category = logTokens[4].Trim();
                        category = category.Substring(1, category.Length - 2);
                        var eventId = logTokens[5].Trim();
                        eventId = eventId.Substring(1, eventId.Length - 2);
                        var lvl = logTokens[6].Trim();
                        lvl = lvl.Substring(1, lvl.Length - 2);
                        var level = LogEntry.GetLogEntryLevel(lvl);
                        var msg = logTokens[7].Trim();
                        msg = msg.Substring(1, msg.Length - 2);
                        var message = new StringBuilder(msg);

                        retval.Add(new CommonLogEntry()
                        {
                            Area = area,
                            Category = category,
                            EventID = eventId,
                            FileName = filename,
                            Level = level,
                            Message = message.ToString(),
                            Process = process,
                            TID = tid,
                            Timestamp = DateTime.Parse(timestamp)
                        });
                    }
                    catch (Exception ex)
                    {
                        var err = new LogEntryError
                        {
                            Exception = ex,
                            Source = filename,
                            Type = LogEntryErrorType.Parser
                        };
                        errors.Add(err);
                    }
                }
            }

            return retval;
        }

        /// <summary>
        /// Creates a CommonLogEntry from a single line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public CommonLogEntry ParseSingleLine(string line)
        {
            var logTokens = line.Split(new string[] { _csvDelim }, StringSplitOptions.None);
            var timestamp = logTokens[0].Trim();
            var process = logTokens[1].Trim();
            var tid = logTokens[2].Trim();
            var area = logTokens[3].Trim();
            var category = logTokens[4].Trim();
            var eventId = logTokens[5].Trim();
            var level = LogEntry.GetLogEntryLevel(logTokens[6].Trim());
            var message = new StringBuilder(logTokens[7].Trim());

            return new CommonLogEntry()
            {
                Area = area,
                Category = category,
                EventID = eventId,
                FileName = "-single entry-",
                Level = level,
                Message = message.ToString(),
                Process = process,
                TID = tid,
                Timestamp = DateTime.Parse(timestamp)
            };
        }

        /// <summary>
        /// Validates common file format
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="detailedErrorMessage"></param>
        /// <returns></returns>
        public bool ValidateFileFormat(string filename, out string detailedErrorMessage)
        {
            detailedErrorMessage = string.Empty;

            try
            {
                //check file exists
                if (!File.Exists(filename))
                {
                    detailedErrorMessage = "File not found!";
                    return false;
                }

                using (var file = new StreamReader(File.OpenRead(filename)))
                {
                    //check header matches print format
                    var header = file.ReadLine().Split(new string[] { _csvDelim }, StringSplitOptions.RemoveEmptyEntries);

                    //check if tab delimiting is correct
                    int count = header.Count();
                    if (count != 8)
                    {
                        detailedErrorMessage = "Column header count not correct.";
                        return false;
                    }

                    //check columns names are correct
                    if (header[0].Trim().ToUpper() != "TIMESTAMP" ||
                        header[1].Trim().ToUpper() != "PROCESS" ||
                        header[2].Trim().ToUpper() != "TID" ||
                        header[3].Trim().ToUpper() != "AREA" ||
                        header[4].Trim().ToUpper() != "CATEGORY" ||
                        header[5].Trim().ToUpper() != "EVENTID" ||
                        header[6].Trim().ToUpper() != "LEVEL" ||
                        header[7].Trim().ToUpper() != "MESSAGE")
                    {
                        detailedErrorMessage = "Column header names not correct.";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                detailedErrorMessage = string.Format("General Exception, Message={0}", ex.Message);
                return false;
            }

            return true;
        }
    }
}
