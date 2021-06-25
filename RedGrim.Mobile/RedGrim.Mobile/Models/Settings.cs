using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedGrim.Mobile.Models
{
    public class Settings
    {
        //public static string aux1 { get; set; }
        //public static string aux2 { get; set; }
        //public static string aux3 { get; set; }
        //public static string aux4 { get; set; }

        public string btDeviceAddress { get; set; }
        public string btDeviceName { get; set; }
        public string obdProtocol { get; set; }
        public int elmDelay { get; set; }
        public int pidDelay { get; set; }
    }
}
