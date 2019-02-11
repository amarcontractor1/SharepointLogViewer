using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.CustomControls
{
    public class LogSetAlert
    {
        public object Controls { get; internal set; }
        public bool HasErrors { get; internal set; }
        public event EventHandler<EventArgs> OnViewErrors;

        public LogSetAlert() { }

        public LogSetAlert(object controls, bool hasErrors)
        {
            this.Controls = controls;
            this.HasErrors = hasErrors;
        }

        public void ViewErrors()
        {
            if(OnViewErrors != null)
            {
                OnViewErrors(this, new EventArgs());
            }
        }
    }
}
