using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arewegood
{
    public class ApiAuthenticationObject : JObject
    {
        public ApiAuthenticationObject(string apiKey)
        {
            this["type"] = "api_token";
            this["data"] = apiKey;
        }
    }
}
