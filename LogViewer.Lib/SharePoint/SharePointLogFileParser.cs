using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib.SharePoint
{
    public class SharePointLogFileParser : ILogFileParser<SharePointLogEntry>
    {
        private const string _tab_symbol = "\t";

        public SharePointLogFileParser() { }

        public List<SharePointLogEntry> ParseFile(string filename, out List<LogEntryError> errors)
        {
            if (!File.Exists(filename))
                throw new ArgumentException("File not found!");

            var retval = new List<SharePointLogEntry>();
            errors = new List<LogEntryError>();

            using (var file = new StreamReader(File.OpenRead(filename)))
            {
                file.ReadLine();//move past header
                while (file.Peek() >= 0)
                {
                    try
                    {
                        var logTokens = file.ReadLine().Split(new string[] { _tab_symbol }, StringSplitOptions.None);
                        var timestamp = logTokens[0].Trim();
                        var process = logTokens[1].Trim();
                        var tid = logTokens[2].Trim();
                        var area = logTokens[3].Trim();
                        var category = logTokens[4].Trim();
                        var eventId = logTokens[5].Trim();
                        var level = LogEntry.GetLogEntryLevel(logTokens[6].Trim());
                        var message = new StringBuilder(logTokens[7].Trim());
                        var correlation = (logTokens[8] != null) ? logTokens[8].Trim() : string.Empty;

                        while (message.ToString().EndsWith("..."))
                        {
                            var nextLine = file.ReadLine();
                            string[] nextLine_Tokens = nextLine.Split(new string[] { _tab_symbol }, StringSplitOptions.None);
                            var nextLine_message = nextLine_Tokens[7].Substring(3);
                            message.Insert(message.Length - 1, nextLine_message);
                        }

                        retval.Add(new SharePointLogEntry()
                        {
                            Area = area,
                            Category = category,
                            Correlation = correlation,
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
        /// Parses a single line from a SharePoint log file (no exception handling)
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public  SharePointLogEntry ParseSingleLine(string line)
        {
            var logTokens = line.Split(new string[] { _tab_symbol }, StringSplitOptions.None);
            var timestamp = logTokens[0].Trim();
            var process = logTokens[1].Trim();
            var tid = logTokens[2].Trim();
            var area = logTokens[3].Trim();
            var category = logTokens[4].Trim();
            var eventId = logTokens[5].Trim();
            var level = LogEntry.GetLogEntryLevel(logTokens[6].Trim());
            var message = new StringBuilder(logTokens[7].Trim());
            var correlation = (logTokens[8] != null) ? logTokens[8].Trim() : string.Empty;

            return new SharePointLogEntry()
            {
                Area = area,
                Category = category,
                Correlation = correlation,
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
        /// Checks if the log file can be parsed correctly
        /// </summary>
        /// <param name="filename"></param>
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
                    //check header matches SharePoint format
                    var header = file.ReadLine().Split(new string[] { _tab_symbol }, StringSplitOptions.None);

                    //check if tab delimiting is correct
                    int count = header.Count();
                    if (count != 9)
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
                        header[7].Trim().ToUpper() != "MESSAGE" ||
                        header[8].Trim().ToUpper() != "CORRELATION")
                    {
                        detailedErrorMessage = "Column header names not correct.";
                        return false;
                    }

                    //check file has more than 1 file & parse a line of text
                    var tokens = file.ReadLine().Split(new string[] { _tab_symbol }, StringSplitOptions.None);

                    //check if tab delimiting is correct
                    int count2 = tokens.Count();
                    if (count2 < 8 || count2 > 9)
                    {
                        detailedErrorMessage = "Log entry tabbing not correct.";
                        return false;
                    }

                    try
                    {
                        //check if LEVEL is in the right place
                        var level = LogEntry.GetLogEntryLevel(tokens[6].Trim());
                    }
                    catch (ApplicationException)
                    {
                        detailedErrorMessage = "LEVEL position not correct.";
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
