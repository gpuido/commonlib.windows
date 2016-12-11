using Microsoft.VisualStudio.TestTools.UnitTesting;
using OasCommonLib.VinParsers;
using System;

namespace VehicleBuildParseTest
{
    [TestClass]
    public class VehicleBuildParse
    {
        [TestMethod]
        public void VehicleBuildParse_Test()
        {
            int build = BuildYearRecoverHelper.Recover("1");
            Assert.AreEqual(build, 2001);

            build = BuildYearRecoverHelper.Recover("19");
            Assert.AreEqual(build, 1919);

            int prevYear = DateTime.Now.Year - 1;
            build = BuildYearRecoverHelper.Recover(prevYear.ToString());
            Assert.AreEqual(build, prevYear);

            build = BuildYearRecoverHelper.Recover("");
            Assert.AreEqual(build, 2000);

            build = BuildYearRecoverHelper.Recover("51");
            Assert.AreEqual(build, 1951);
        }
    }
}
