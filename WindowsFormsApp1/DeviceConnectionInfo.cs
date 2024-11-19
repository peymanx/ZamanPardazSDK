using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZamanPardazSDK
{
    public class DeviceConnection
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public int MachineNumber { get; set; }
        public int Password { get; set; }
    }

    public class DeviceConnectionInfo
    {
        public DeviceConnection DeviceConnection { get; set; }
    }


}
