using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using arewegood;
using Newtonsoft.Json.Linq;

namespace test_arewegood_sdk_cs_net40
{
    [TestClass]
    public class ApiModels
    {
        [TestMethod]
        public void ApiAuthenticationObjectSerializes()
        {
            var a = new ApiAuthenticationObject("nonsense");
            Assert.AreEqual(JObject.Parse("{\"type\":\"api_token\",\"data\":\"nonsense\"}").ToString(), a.ToString(), "mismatch: "+a.ToString());
        }

        [TestMethod]
        public void ApiExceptionObjectSerializes()
        {
            var a = new ApiExceptionObject("nonsense","cats");
            Assert.AreEqual(JObject.Parse("{\"type\":\"nonsense\",\"data\":\"cats\"}").ToString(), a.ToString(), "mismatch: " + a.ToString());
        }

        [TestMethod]
        public void ApiExceptionObjectSerializesNumber()
        {
            var a = new ApiExceptionObject("nonsense", 2);
            Assert.AreEqual(JObject.Parse("{\"type\":\"nonsense\",\"data\":2}").ToString(), a.ToString(), "mismatch: " + a.ToString());
        }

        [TestMethod]
        public void ApiExceptionObjectSerializesBool()
        {
            var a = new ApiExceptionObject("nonsense", true);
            Assert.AreEqual(JObject.Parse("{\"type\":\"nonsense\",\"data\":true}").ToString(), a.ToString(), "mismatch: " + a.ToString());
        }

        [TestMethod]
        public void ApiExceptionObjectSerializesArray()
        {
            string[] arr = new string[2];
            arr[0] = "hi";
            arr[1] = "there";
            var a = new ApiExceptionObject("nonsense", arr);
            Assert.AreEqual(JObject.Parse("{\"type\":\"nonsense\",\"data\":[\"hi\",\"there\"]}").ToString(), a.ToString(), "mismatch: " + a.ToString());
        }
    }
}
