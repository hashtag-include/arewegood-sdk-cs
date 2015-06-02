using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arewegood_sdk_cs
{
    public class Arewegood
    {
        public Arewegood(string apiKey, ArewegoodOptions opts = null)
        {
            if (apiKey == null || apiKey.Length == 0) throw new ArgumentException("apiKey must contain a value");
            if (opts == null) opts = new ArewegoodOptions();

            conn = new Connections.PostConnection(opts);
        }

        private Connections.PostConnection conn;
    }
}
