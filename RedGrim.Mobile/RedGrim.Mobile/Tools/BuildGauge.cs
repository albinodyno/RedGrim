using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGrim.Mobile.Models;

namespace RedGrim.Mobile.Tools
{
    public static class BuildGauge
    {
        public static Gauge VoltageGauge()
        {
            Gauge tmpGauge = new Gauge("Voltage", "V", 30, 0, 5, "0142\r", ParseGauge.Voltage, 4, 15, 18);
            return tmpGauge;
        }

        public static Gauge CoolantTempGauge()
        {
            Gauge tmpGauge = new Gauge("Coolant", "°F", 250, -25, 25, "0105\r", ParseGauge.CoolantTemp, 2, 200, 205);
            return tmpGauge;
        }

        public static Gauge IntakeTempGauge()
        {
            Gauge tmpGauge = new Gauge("Intake", "°F", 250, -25, 25, "010F\r", ParseGauge.Intaketemp, 2, 115, 145);
            return tmpGauge;
        }

        public static Gauge RPMGauge()
        {
            Gauge tmpGauge = new Gauge("RPM", "", 10000, 0, 1000, "010C\r", ParseGauge.RPM, 4, 4000, 6000);
            return tmpGauge;
        }

        public static Gauge MPHGauge()
        {
            Gauge tmpGauge = new Gauge("MPH", "", 160, 0, 20, "010D\r", ParseGauge.MPH, 2, 80, 95);
            return tmpGauge;
        }

        public static Gauge EngineLoadGauge()
        {
            Gauge tmpGauge = new Gauge("Load", "%", 100, 0, 20, "0104\r", ParseGauge.EngineLoad, 2, 95, 100);
            return tmpGauge;
        }

        public static Gauge ThrottlePosGauge()
        {
            Gauge tmpGauge = new Gauge("Throttle", "%", 100, 0, 20, "0111\r", ParseGauge.ThrottlePosition, 2, 95, 85);
            return tmpGauge;
        }

        public static Gauge FuelRateGauge()
        {
            Gauge tmpGauge = new Gauge("Fuel Rate", "G/h", 5, 0, 1, "015E\r", ParseGauge.FuelRate, 4, 10, 15);
            return tmpGauge;
        }

        public static Gauge TorqueGauge()
        {
            Gauge tmpGauge = new Gauge("Torque", "%", 100, 0, 20, "0162\r", ParseGauge.Torque, 2, 75, 85);
            return tmpGauge;
        }

        public static Gauge OilTempGauge()
        {
            Gauge tmpGauge = new Gauge("Oil Temp", "°F", 200, 0, 20, "015C\r", ParseGauge.OilTemp, 2, 175, 185);
            return tmpGauge;
        }
    }
}
