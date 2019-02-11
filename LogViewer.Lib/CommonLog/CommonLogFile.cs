using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogViewer.Lib;

namespace LogViewer.Lib.CommonLog
{
    public class CommonLogFile : LogFile<CommonLogEntry>
    {
        private static CommonLogFileParser _parser = new CommonLogFileParser();

        public CommonLogFile(string filename)
            : base(filename, _parser)
        {

        }
    }
}
