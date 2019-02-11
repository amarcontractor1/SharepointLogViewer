using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogViewer.Lib.SharePoint;
using LogViewer.Lib.Visualization;

namespace LogViewer.Tests
{
    [TestClass]
    public class VisualizationTests
    {
        [TestMethod]
        [TestCategory("Visualization")]
        public void GoogleChartTest()
        {
            var logset = new SharePointLogSet(new string[] { SharePointLogFileTests.good_SPTestFile });
            var chart = new GoogleChart(logset.GetLogSetMetrics());
            Assert.IsTrue(chart.HTML.Length > 0);
        }

    }
}
