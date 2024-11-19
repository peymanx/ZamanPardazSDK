using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZamanPardazSDK
{
  
    public class LogRecord
    {
        public int DeviceNumber { get; set; }
        public int EnrollNumber { get; set; }
        public int MachineNumber { get; set; }
        public int Event { get; set; }
        public int VerifyMode { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
