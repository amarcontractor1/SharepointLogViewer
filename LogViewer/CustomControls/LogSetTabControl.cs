using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LogViewer.CustomControls
{
    class LogSetTabControl : TabControl
    {
        #region Declarations

        private Random _rng = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public LogSetTabControl() : base() { }

        #endregion

        #region Actions

        /// <summary>
        /// Adds tab item and sets it to current item
        /// </summary>
        /// <param name="item"></param>
        public void AddLogSetTab(LogSetTabItem item)
        {
            item.InteralID = GetUniqueName();
            this.Items.Add(item);
            this.SelectedIndex = this.Items.Count - 1;
        }

        /// <summary>
        /// Removed a tab using InternalID
        /// </summary>
        /// <param name="internalID"></param>
        public void RemoveLogSetTab(object tab)
        {
            this.Items.Remove(tab);
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Gets a random string
        /// </summary>
        /// <returns></returns>
        private string GetUniqueName()
        {
            char[] buffer = new char[20];

            for (int i = 0; i < 20; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }

            var temp = new string(buffer);
            return string.Concat("tc_", temp);
        }

        #endregion

    }
}
