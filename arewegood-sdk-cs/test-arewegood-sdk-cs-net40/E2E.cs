using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using arewegood;

namespace test_arewegood_sdk_cs_net40
{
    [TestClass]
    public class E2E
    {
        [TestMethod]
        [Timeout(40 * 1000)] //40ms, since WS timeouts @ 30s, this gives us 10s to http
        //[Ignore] //until we add mdns support to proxy
        public void TraceMessageToARunningProxy()
        {
            Arewegood instance = new Arewegood("4d42ba712ce146b6b2400b38c70df07b");
            instance.Trace("all is well");
        }
    }
}
