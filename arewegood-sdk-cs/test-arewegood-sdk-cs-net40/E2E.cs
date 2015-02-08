using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using arewegood;

namespace test_arewegood_sdk_cs_net40
{
    [TestClass]
    public class E2E
    {
        [TestMethod]
        [Ignore] //until we add mdns support to proxy
        public void TraceMessageToARunningProxy()
        {
            Arewegood instance = new Arewegood("nonsense");
            instance.Trace("all is well");
        }
    }
}
