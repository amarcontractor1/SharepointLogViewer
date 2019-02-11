using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer
{
    public class SelectedColumnsEventArgs : EventArgs
    {
        public Dictionary<string, bool> Selected { get; set; }
    }
}
