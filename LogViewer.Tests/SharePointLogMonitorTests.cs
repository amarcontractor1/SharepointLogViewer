using LogViewer.Lib.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogViewer.Tests
{
    [TestClass]
    public class SharePointLogMonitorTests
    {
        public const string dir = "Test Samples\\";
        public const string file = "monitor.log";
        private const int _intervalMulti = 2000;

        [TestMethod]
        [TestCategory("SharePoint Log Monitor")]
        public void LogMonitorTest()
        {
            var monitor = new SharePointLogMonitor();
            var path = Path.Combine(dir, file);

            try
            {
                //create monitor                   
                var settings = new SharePointLogMonitorSettings()
                {
                    Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir),
                    Interval = 30
                };
                
                monitor.MonitorDirectory = settings.Directory;
                monitor.Interval = settings.Interval.Value;
                monitor.Start();
                Assert.IsTrue(monitor.MonitorDirectory == settings.Directory);
                Assert.IsTrue(monitor.Interval == settings.Interval);

                //create log file and let monitor take initial count
                File.Copy(SharePointLogFileTests.good_SPTestFile, path);
                Thread.Sleep(settings.Interval.Value * _intervalMulti);
                var count1 = monitor.LogSet.Entries.Count;
                monitor.Stop();
                monitor.Start();

                using(var append = new StreamReader(File.Open(SharePointLogFileTests.good_SPTestFile,FileMode.Open)))
                {
                    append.ReadLine();
                    var entry = append.ReadLine();
                    File.AppendAllText(path, entry, Encoding.Unicode);//-add 1 entry- 
                }

                Thread.Sleep(settings.Interval.Value * _intervalMulti);
                var count2 = monitor.LogSet.Entries.Count;
                Assert.IsTrue((count1 + 1) == count2);//-check that 1 entry was added-
            }
            catch(Exception ex) 
            {
                if (ex is AssertFailedException)
                    throw;
            }
            finally
            {
                monitor.Stop();
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
                monitor.Dispose();
            }
        }
    }
}
