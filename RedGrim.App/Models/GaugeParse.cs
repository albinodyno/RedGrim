using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedGrim.App.Models
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
            }
                return value;
        }
    }
}
