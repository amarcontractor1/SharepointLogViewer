using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib
{
    public interface ILogFileParser<T> where T : LogEntry
    {
         List<T> ParseFile(string filename, out List<LogEntryError> errors);
         T ParseSingleLine(string line);
         bool ValidateFileFormat(string filename, out string detailedErrorMessage);
    }
}
