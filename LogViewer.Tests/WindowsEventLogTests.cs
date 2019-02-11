using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogViewer.Lib;
using LogViewer.Lib.WindowsEvent;
using System;
using System.Diagnostics;

namespace LogViewer.Tests
{
    [TestClass]
    public class WindowsEventLogTests
    {
        DateTime startTime = DateTime.Now.Subtract(TimeSpan.FromDays(1));
        DateTime endTime = DateTime.Now;

        [TestMethod]
        [TestCategory("Windows Event Log")]
        [ExpectedException(typeof(ArgumentException))]
        public void EventLogWindowTest()
        {
            var startTime = DateTime.Now;
            var badEndTime = DateTime.Now.Subtract(TimeSpan.FromDays(1));
            var winAppEventLogSet = new WindowsEventLogSet(startTime, badEndTime);
        }

        [TestMethod]
        [TestCategory("Windows Event Log")]
        public void EventLogConnectionTest()
        {
            var winAppEventLogSet = new WindowsEventLogSet(startTime, endTime);
            Assert.IsNotNull(winAppEventLogSet);
        }

        [TestMethod]
        [TestCategory("Windows Event Log")]
        public void WinAppEventLogItemsCountTest()
        {
            var test_count = Get_WinAppEventLogItemsCountTest();
            var winAppEventLogSet = new WindowsEventLogSet(startTime, endTime);
            Assert.AreEqual(test_count, winAppEventLogSet.Entries.Count);
        }

        #region test helpers

        public int Get_WinAppEventLogItemsCountTest()
        {
            var appName = "Application";
            var appEvents = new EventLog() { Source = appName };
            var count = 0;

            foreach (EventLogEntry entry in appEvents.Entries)
            {
                if (entry.TimeGenerated >= startTime && entry.TimeGenerated <= endTime)
                    count++;
            }

            return count;
        }

        #endregion
        

    }
}
