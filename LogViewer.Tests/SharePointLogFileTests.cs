using Microsoft.VisualStudio.TestTools.UnitTesting;
using LogViewer.Lib.SharePoint;
using System.Linq;
using LogViewer.Lib;

namespace LogViewer.Tests
{
    [TestClass]
    public class SharePointLogFileTests
    {
        public const string good_SPTestFile = "Test Samples/Sample_Good_SPLogFile.log";
        const string bad_SPTestFile_1 = "Test Samples/Sample_Bad_SP_single.log";
        const string bad_SPTestFile_2 = "Test Samples/Sample_Bad_SP_multi.log";
        const string bad_SPTestFile_3 = "Test Samples/Sample_Bad_SP_incorrectFormat.log";
        const string bad_FilePath = "fakedirectory/fakefile.log";

        [TestMethod]
        [TestCategory("SharePoint Log File")]
        public void FileNotFoundTest()
        {
            var logfile = new SharePointLogFile(bad_FilePath);
            Assert.IsTrue(logfile.HasFileError);
        }

        [TestMethod]
        [TestCategory("SharePoint Log File")]
        public void BadFileFormatTest_single_line()
        {
            var logfile = new SharePointLogFile(bad_SPTestFile_1);
            Assert.IsTrue(logfile.ErrorMessage == "Column header count not correct.");
        }

        [TestMethod]
        [TestCategory("SharePoint Log File")]
        public void BadFileFormatTest_multi_line()
        {
            var logfile = new SharePointLogFile(bad_SPTestFile_2);
            Assert.IsTrue(logfile.HasFileError);
            Assert.IsTrue(logfile.ErrorMessage == "LEVEL position not correct.");
        }

        [TestMethod]
        [TestCategory("SharePoint Log File")]
        public void BadFileFormatTest_incorrectFormat()
        {
            var logfile = new SharePointLogFile(bad_SPTestFile_3);
            Assert.IsTrue(logfile.HasFileError);
            Assert.IsTrue(logfile.ErrorMessage == "Column header count not correct.");
        }

        [TestMethod]
        [TestCategory("SharePoint Log File")]
        public void GoodFileFormatTest()
        {
            var logfile = new SharePointLogFile(good_SPTestFile);
            Assert.IsFalse(logfile.HasFileError);//no errors
            Assert.AreEqual(logfile.LogEntries.Count(), 3160);//log entries count
        }

        [TestMethod]
        [TestCategory("SharePoint LogSet")]
        public void SharePointLogSetTest()
        {
            var logset = new SharePointLogSet(new string[] { good_SPTestFile }, false, false);
            Assert.AreEqual(logset.Entries.Count, 3160);
        }

        [TestMethod]
        [TestCategory("SharePoint LogSet")]
        public void SPLogILogFileDetailsTest()
        {
            var logset = new SharePointLogSet(new string[] { good_SPTestFile }, false, false);
            var fileDetails = logset as ILogFileDetails;

            Assert.IsNotNull(fileDetails);
            Assert.IsTrue(fileDetails.FileNames.Length > 0);
            Assert.IsTrue(fileDetails.LoadTime > 0);
            Assert.IsTrue(fileDetails.Count > 0);
            Assert.IsTrue(fileDetails.ParserErrors.Count == 0);
        }
    }
}
