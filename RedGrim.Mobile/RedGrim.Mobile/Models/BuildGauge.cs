using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedGrim.Mobile.Models
{
    public static class BuildGauge
    {
        public static Gauge VoltageGauge()
        {
            Gauge tmpGauge = new Gauge("Voltage", "V", 50, 0, 5, "0144\r", GaugeParse.Voltage, 4);
            return tmpGauge;
        }

        public static Gauge CoolantGauge()
        {
            Gauge tmpGauge = new Gauge("CoolantTemp", "°F", 250, -25, 25, "0105\r", GaugeParse.CoolantTemp, 2);
            return tmpGauge;
        }

        public static Gauge IntakeGauge()
        {
            Gauge tmpGauge = new Gauge("CoolantTemp", "°F", 250, -25, 25, "0105\r", GaugeParse.Intaketemp, 2);
            return tmpGauge;
        }

        public static Gauge RPMGauge()
        {
            Gauge tmpGauge = new Gauge("CoolantTemp", "°F", 250, -25, 25, "0105\r", GaugeParse.RPM, 2);
            return tmpGauge;
        }

        public static Gauge MPHGauge()
        {
            Gauge tmpGauge = new Gauge("CoolantTemp", "°F", 250, -25, 25, "0105\r", GaugeParse.MPH, 2);
            return tmpGauge;
        }

        //public static Gauge EngineLoadGauge()
        //{
        //    Gauge tmpGauge = new Gauge("CoolantTemp", "°F", 250, -25, 25, "0105\r", GaugeParse.RPM, 2);
        //    return tmpGauge;
        //}

        //public static Gauge ThrottlePosGauge()
        //{
        //    Gauge tmpGauge = new Gauge("CoolantTemp", "°F", 250, -25, 25, "0105\r", GaugeParse.CoolantTemp, 2);
        //    return tmpGauge;
        //}
    }
}
