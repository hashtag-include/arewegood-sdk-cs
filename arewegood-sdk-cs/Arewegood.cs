using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using Newtonsoft.Json.Linq;
using Zeroconf;
using System.Threading.Tasks;

namespace arewegood
{
    public class Arewegood
    {
        private string _apiKey;
        private string _serviceName;


        public Arewegood(string apiKey, string serviceName)
        {
            _apiKey = apiKey;
            _serviceName = serviceName;
            ZeroconfResolver.ResolveAsync(serviceName).ContinueWith((continuation) =>
            {
                var list = continuation.Result;
            });
        }
    }
}
