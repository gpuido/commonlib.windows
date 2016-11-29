using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ReadJsonConfigFile
{
    [TestClass]
    public class ReadJsonTests

    {
        [TestMethod]
        public void ParseJson()
        {
            string json = File.ReadAllText("../../evcp.json");
            var ocd = OasCommonLib.Config.OasConfig.ParseJson(json, "1.1.1.1");

            Assert.IsNotNull(ocd.CaseConfig);
            Assert.AreEqual(ocd.CaseConfig.EMSCasePath.Length, 3);
            Assert.IsTrue(ocd.CaseConfig.MCFPath.Length > 0);
            Assert.IsTrue(ocd.CaseConfig.DBFCasePath.Length > 0);
            Assert.IsTrue(ocd.CaseConfig.RemovedCasePath.Length > 0);
            Assert.IsTrue(ocd.CaseConfig.CaseExt.Length > 0);
            Assert.IsTrue(ocd.CaseConfig.ImagePath.Length > 0);
            Assert.IsTrue(ocd.CaseConfig.ImageExts.Length > 0);
            Assert.IsTrue(ocd.CaseConfig.CaseImagePath.Length > 0);
            Assert.IsTrue(ocd.CaseConfig.CaseAudioPath.Length > 0);

            Assert.IsNotNull(ocd.WebConfig);
            Assert.IsTrue(ocd.WebConfig.Url.Length > 0);
            Assert.IsTrue(ocd.WebConfig.LoginInfo.Length > 0);

            Assert.IsNotNull(ocd.WebServer);
            if (ocd.WebServer.RunServer)
            {
                Assert.IsTrue(ocd.WebServer.Port > 0);
                Assert.IsTrue(ocd.WebServer.Ips.Count > 0);
            }


            Assert.IsTrue(ocd.LogPath.Length > 0);
            Assert.IsTrue(ocd.DataPath.Length > 0);
            Assert.IsTrue(ocd.ImageExportPath.Length > 0);
            Assert.IsTrue(ocd.LocalStoragePath.Length > 0);

            Assert.IsTrue(ocd.StandardDescription.Length > 0);


            Assert.IsTrue(ocd.ServerPollTimeout > 0);
            Assert.IsTrue(ocd.WatcherDelay > 0);
            Assert.IsTrue(ocd.SqLiteDbPath.Length > 0);
            Assert.IsTrue(ocd.HowManyDaysToShow > 0);
            Assert.IsTrue(ocd.SortHeader > 0);
            Assert.IsTrue(ocd.CurrentVersion.Length > 0);

        }
    }
}
