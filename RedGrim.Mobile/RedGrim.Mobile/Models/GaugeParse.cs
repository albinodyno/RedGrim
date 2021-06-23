using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedGrim.Mobile.Models
{
    public static class GaugeParse
    {
        public static double CoolantTemp(string hex)
        {
            double value;
            try
            {
                value = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                value = ((value - 40) * 1.8) + 32;
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                value = 0;
            }
            return value;
        }

        public static double Intaketemp(string hex)
        {
            double value;
            try
            {
                value = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                value = ((value - 40) * 1.8) + 32;
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                value = 0;
            }
            return value;
        }

        public static double RPM(string hex)
        {
            double value;
            try
            {
                value = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                value = Math.Round((value / 2.55), 1);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                value = 0;
            }
            return value;
        }

        public static double MPH(string hex)
        {
            double value;

            try
            {
                value = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                value = Math.Round(value / 1.609, 1);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                value = 0;
            }
            return value;
        }

        public static double Voltage(string hex)
        {
            double value;
            try
            {
                double input1 = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                double input2 = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);

                value = Math.Round((((256 * input1) + input2) / 1000), 1);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                value =  0;
            }
                return value;
        }

        public static Gauge BuildGauge(string name)
        {
            

            Gauge tmpGauge = new Gauge();

            switch (name)
            {
                case "Voltage":
                    tmpGauge.Name = name;
                    tmpGauge.UOM = "V";
                    tmpGauge.Max = 50;
                    tmpGauge.Min = 0;
                    tmpGauge.TickSpacing = 5;
                    tmpGauge.OBDCommand = "0144\r";
                    tmpGauge.ParseGauge = Voltage;
                    tmpGauge.HexNum = 4;
                    break;

                case "CoolantTemp":
                    tmpGauge.Name = name;
                    tmpGauge.UOM = "°F";
                    tmpGauge.Max = 250;
                    tmpGauge.Min = -25;
                    tmpGauge.TickSpacing = 25;
                    tmpGauge.OBDCommand = "0105\r";
                    tmpGauge.ParseGauge = CoolantTemp;
                    tmpGauge.HexNum = 2;
                    break;

                case "IntakeTemp":
                    tmpGauge.Name = name;
                    tmpGauge.UOM = "°F";
                    tmpGauge.Max = 250;
                    tmpGauge.Min = -25;
                    tmpGauge.TickSpacing = 25;
                    tmpGauge.OBDCommand = "010F\r";
                    tmpGauge.ParseGauge = Intaketemp;
                    tmpGauge.HexNum = 2;
                    break;

                case "RPM":
                    tmpGauge.Name = name;
                    tmpGauge.UOM = "x1000";
                    tmpGauge.Max = 250;
                    tmpGauge.Min = -25;
                    tmpGauge.TickSpacing = 25;
                    tmpGauge.OBDCommand = "010F\r";
                    tmpGauge.ParseGauge = RPM;
                    tmpGauge.HexNum = 2;
                    break;

                case "MPH":
                    tmpGauge.Name = name;
                    tmpGauge.UOM = "MPH";
                    tmpGauge.Max = 250;
                    tmpGauge.Min = -25;
                    tmpGauge.TickSpacing = 25;
                    tmpGauge.OBDCommand = "010F\r";
                    tmpGauge.ParseGauge = MPH;
                    tmpGauge.HexNum = 2;
                    break;

                case "EngineLoad":
                    tmpGauge.Name = name;
                    tmpGauge.UOM = "%";
                    tmpGauge.Max = 250;
                    tmpGauge.Min = -25;
                    tmpGauge.TickSpacing = 25;
                    tmpGauge.OBDCommand = "010F\r";
                    tmpGauge.ParseGauge = RPM;
                    tmpGauge.HexNum = 2;
                    break;

                case "ThrottlePos":
                    tmpGauge.Name = name;
                    tmpGauge.UOM = "%";
                    tmpGauge.Max = 250;
                    tmpGauge.Min = -25;
                    tmpGauge.TickSpacing = 25;
                    tmpGauge.OBDCommand = "010F\r";
                    tmpGauge.ParseGauge = RPM;
                    tmpGauge.HexNum = 2;
                    break;

                default:
                    tmpGauge.Name = name;
                    tmpGauge.UOM = "";
                    tmpGauge.Max = 100;
                    tmpGauge.Min = 0;
                    tmpGauge.TickSpacing = 20;
                    tmpGauge.OBDCommand = "";
                    tmpGauge.ParseGauge = RPM;
                    tmpGauge.HexNum = 2;
                    break;
            }


            return tmpGauge;
        }
    }
}
