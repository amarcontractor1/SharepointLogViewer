using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogViewer.Lib;
using System.IO;
using System;

namespace LogViewer.Tests
{
    [TestClass]
    public class LoggingTests
    {
        [TestMethod]
        [TestCategory("Application")]
        public void AddLogEntryTest()
        {
            var filename = "errors.log";
            var attempts = 5;
            var count = 0;

            if (File.Exists(filename))
                File.Delete(filename);
      
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    throw new Exception(string.Concat("Test ", i));
                }
                catch(Exception ex)
                {
                    ex.LogException();
                }
            }
   
            using (var file = File.OpenText(filename))
            {
                while (file.Peek() >= 0)
                {
                    var entry = file.ReadLine();
                    if (entry.StartsWith("["))
                        count++;
                }
            }

            Assert.IsTrue(attempts == count);
        }
    }
}
