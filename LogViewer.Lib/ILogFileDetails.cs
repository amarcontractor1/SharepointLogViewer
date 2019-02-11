using System;
using System.Collections.Generic;

namespace LogViewer.Lib
{
    public interface ILogFileDetails
    {
        string[] FileNames { get; set; }
        int Count { get; }
        Dictionary<string, string[]> ParserErrors { get; set; }
        double LoadTime { get; set; }
    }
}
