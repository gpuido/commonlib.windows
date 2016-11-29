using Microsoft.VisualStudio.TestTools.UnitTesting;
using OasCommonLib.Helpers;

namespace Base64Exchange
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void EncodeTestMethod()
        {
            string urlParams = "---1234567890---";
            string encoded = CoderHelper.Encode(urlParams);

            Assert.IsTrue(encoded.StartsWith("_"));
        }

        [TestMethod]
        public void DecodeTest()
        {
            string encodedStr = "_3HGHNLNLS0tMTIzNDU2Nzg5MC0tLQ=="; // "_4736YWN0aW9uPWxvZ2luJmxvZ2luPTEyMyZwYXNzd2Q9MzIx";
            string decoded = CoderHelper.Decode(encodedStr);

            Assert.AreEqual("---1234567890---", decoded);
        }

        [TestMethod]
        public void DecodeTest2()
        {
            string encoded = "_17ZYWN0aW9uPXBpbmcmY2xpZW50PTEuMC40LjU=";
            string decoded = CoderHelper.Decode(encoded);

            Assert.IsTrue(decoded.Length > 0);
            Assert.IsTrue(decoded.Contains("ping"));
        }
    }
}
