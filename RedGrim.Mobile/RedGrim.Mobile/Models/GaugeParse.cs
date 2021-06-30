using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedGrim.Mobile.Controls;

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
                BluetoothControl.SystemLogEntry(ex.Message, true);
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
                BluetoothControl.SystemLogEntry(ex.Message, true);
                value = 0;
            }
            return value;
        }

        public static double RPM(string hex)
        {
            double value;
            try
            {
                double input1 = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                double input2 = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);

                value = Math.Round(((input1 * 256 + input2) / 4), 1);
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
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
                BluetoothControl.SystemLogEntry(ex.Message, true);
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
                BluetoothControl.SystemLogEntry(ex.Message, true);
                value = 0;
            }
            return value;
        }

        public static double ThrottlePosition(string hex)
        {
            double value;
            try
            {
                value = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                value = (value / 2.55) * 100;
                value = Math.Round(value, 1);
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
                value = 0;
            }
            return value;
        }

        public static double FuelRate(string hex)
        {
            double value;
            try
            {
                double input1 = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                double input2 = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);

                value = (((256 * input1) + input2) / 20)/3.785;
                value = Math.Round(value, 1);
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
                value = 0;
            }
            return value;
        }

        public static double Torque(string hex)
        {
            double value;
            try
            {
                value = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                value = value / 2.55;
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
                value = 0;
            }
            return value;
        }

        public static double EngineLoad(string hex)
        {
            double value;
            try
            {
                value = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                value = (value / 2.55)*100;
                value = Math.Round(value, 1);
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
                value = 0;
            }
            return value;
        }

        public static double OilTemp(string hex)
        {
            double value;
            try
            {
                value = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                value = ((value - 40) * 1.8) + 32;
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
                value = 0;
            }
            return value;
        }
    }
}
