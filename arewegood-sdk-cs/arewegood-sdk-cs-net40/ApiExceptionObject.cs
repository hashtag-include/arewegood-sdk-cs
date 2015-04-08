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
        public ApiExceptionObject(string type, params object[] data)
        {
            if (data.Length == 1)
                this["data"] = JToken.FromObject(data[0]);
            else
            {
                var wrapped = JsonConvert.SerializeObject(data);
                this["data"] = JsonConvert.DeserializeObject<JToken>(wrapped);
            }

            this["type"] = type;
        }
    }
}
