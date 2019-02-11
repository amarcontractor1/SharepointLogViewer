using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Lib.SharePoint
{
    public class SharePointLogFile : LogFile<SharePointLogEntry>
    {
        private static SharePointLogFileParser _parser = new SharePointLogFileParser();

        public SharePointLogFile(string filename) : base(filename, _parser)
        {

        }
    }
}
