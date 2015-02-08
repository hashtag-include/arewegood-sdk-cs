using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace arewegood
{
    public class ApiExceptionObject : JObject
    {
        public ApiExceptionObject(string type, object data)
        {
            var wrapped = JsonConvert.SerializeObject(data);
            this["type"] = type;
            this["data"] = JsonConvert.DeserializeObject<JToken>(wrapped);
        }
    }
}
