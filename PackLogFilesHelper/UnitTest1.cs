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
        public void IsNotCurrentMonth_Test()
        {
            YearMonthInfo dt;

            var res = PackLogsHelper.IsCurrentMonth("2012-01-01.log", out dt);
            Assert.IsFalse(res);
            Assert.AreEqual(dt, new YearMonthInfo(2012, 01));

            res = PackLogsHelper.IsCurrentMonth(DateTime.Now.ToString(FormatHelper.DateFormat) + ".log", out dt);
            Assert.IsTrue(res);
            Assert.AreEqual(dt, new YearMonthInfo(DateTime.Now.Year, DateTime.Now.Month));
        }

        [TestMethod]
        public void PackLogFIles_Test()
        {
            var res = PackLogsHelper.Pack("..\\..\\logs");
            Assert.IsTrue(res);
        }
    }
}
