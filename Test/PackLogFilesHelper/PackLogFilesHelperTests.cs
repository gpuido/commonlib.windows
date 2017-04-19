using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OasCommonLib.Helpers;
using static OasCommonLib.Helpers.PackLogsHelper;

namespace PackLogFilesHelper
{
    [TestClass]
    public class PackLogFilesHelperTests
    {
        [TestMethod]
        public void YearMonthInfo_Test()
        {
            YearMonthInfo ymi = new YearMonthInfo(2011, 3);
            Assert.AreEqual("2011-03", ymi.ToString());
        }

        [TestMethod]
        public void HowManyDaysSaveInArchive_Test()
        {
            var res = PackLogsHelper.HowManyDaysFromNow("2012-01-01.log", out YearMonthInfo dt);
            Assert.IsTrue(res > 100);
            Assert.AreEqual(dt.Year, 2012);
            Assert.AreEqual(dt.Month, 1);

            res = PackLogsHelper.HowManyDaysFromNow(DateTime.Now.ToString(FormatHelper.DateFormat) + ".log", out dt);
            Assert.IsTrue(res == 0);
            Assert.AreEqual(dt.Year, DateTime.Now.Year);
            Assert.AreEqual(dt.Month, DateTime.Now.Month);
        }

        [TestMethod]
        public void PackLogFIles_Test()
        {
            var res = PackLogsHelper.Pack("..\\..\\logs", 7);
            Assert.IsTrue(res);
        }
    }
}
