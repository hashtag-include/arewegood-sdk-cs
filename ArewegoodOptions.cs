using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arewegood_sdk_cs
{
    public class ArewegoodOptions
    {
        public ArewegoodOptions()
        {
            Name = "myapp";
            ServiceUrl = "https://arewegood.io/";
            PushRate = 0;
            Silent = false;
            Hostname = Windows.Networking.Connectivity.NetworkInformation.GetHostNames()[0];
            Platform = "Windows";
            Arch = "N/A";
            MonitorMemory = true;
            MonitorNetwork = true;
            Logger = new ArewegoodLogger(Name);
        }

        public string Name { get; set; }
        public string ServiceUrl { get; set; }
        public int PushRate { get; set; }
        public bool Silent { get; set; }
        public Windows.Networking.HostName Hostname { get; set; }
        public string Platform { get; set; }
        public string Arch { get; set; }
        public bool MonitorMemory { get; set; }
        public bool MonitorNetwork { get; set; }
        public IArewegoodLogger Logger { get; set; }
    }
}
