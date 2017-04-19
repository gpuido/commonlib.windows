using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OasCommonLib.UpdateHelper;

namespace UnZip
{
    [TestClass]
    public class UnZipTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var res = NewVersionHelper.UnpackNewVersion(@"..\..\zip", "ev.zip");

            Assert.IsTrue(res);
            Assert.AreEqual(NewVersionHelper.LastError, String.Empty);
        }
    }
}
