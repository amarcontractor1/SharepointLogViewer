using System;

namespace LogViewer.Lib.SharePoint
{
    public class SharePointLogEntry : LogEntry
    {
        public string FileName { get; set; }   
        public string Correlation { get; set; }

    }
}
