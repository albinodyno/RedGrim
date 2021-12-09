using RedGrim.Mobile.Controls;
using RedGrim.Mobile.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Android.Bluetooth;

namespace RedGrim.Mobile.Models
{
    class GaugeCommands
    {
        public BluetoothSocket socket;

        public static int elmDelay = 1000;
        public static int pidDelay = 300;
        bool success;

        public List<Gauge> gauges;

        public Gauge MainGauge;
        public Gauge BoxGauge1;
        public Gauge BoxGauge2;
        public Gauge BoxGauge3;
        public Gauge BoxGauge4;

        //    {"RPM", "010C\r"},
        //    {"MPH","010D\r"},
        //    {"EngineLoad", "0104\r"},
        //    {"ThrottlePos", "0111\r"},
        //    {"CoolantTemp", "0105\r"},
        //    {"IntakeTemp", "010F\r"},
        //    {"Voltage", "0144\r"}
        //    {"FuelRate", "015E\r"},
        //    {"OilTemp", "015C\r"}
        //    {"Torque", "0162\r"}

        public GaugeCommands(BluetoothSocket btSocket)
        {
            socket = btSocket;

            try
            {
                elmDelay = SettingsControl.saveSettings.elmDelay;
                pidDelay = SettingsControl.saveSettings.pidDelay;
            }
            catch (Exception)
            {

            }

            //choose gauge content here
            MainGauge = BuildGauge.RPMGauge();
            BoxGauge1 = BuildGauge.CoolantTempGauge();
            BoxGauge2 = BuildGauge.IntakeTempGauge();
            BoxGauge3 = BuildGauge.EngineLoadGauge();
            BoxGauge4 = BuildGauge.ThrottlePosGauge();

            gauges = new List<Gauge>();

            gauges.Add(MainGauge);
            gauges.Add(BoxGauge1);
            gauges.Add(BoxGauge2);
            gauges.Add(BoxGauge3);
            gauges.Add(BoxGauge4);
        }

        #region Write/Read ELM
        public async Task<bool> SetupCommands()
        {
            try
            {
                await WriteELMCommand("ATZ\r");
                await WriteELMCommand("ATL0\r");
                await WriteELMCommand("ATH0\r");
                await WriteELMCommand("ATS0\r");
                await WriteELMCommand("ATSP6\r");
                await ReadELMCommands();

                return true;
            }
            catch(Exception ex)
            {
                BluetoothControl.SystemLogEntry($"ELM setup failed - {ex.Message}", false);
                return false;
            }
        }

        public async Task WriteELMCommand(string input)
        {
            try
            {
                byte[] writeBuffer = Encoding.ASCII.GetBytes(input);
                await socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                await socket.OutputStream.FlushAsync();

                await Task.Delay(elmDelay);
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry($"ELM WRITE setup failed - {ex.Message}", false);
            }
        }

        public async Task ReadELMCommands()
        {
            try
            {
                byte[] readBuffer = new byte[512];
                int length = await socket.InputStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                string data = Encoding.ASCII.GetString(readBuffer);

                BluetoothControl.UpdateLog(data);
            }
            catch(Exception ex)
            {
                BluetoothControl.SystemLogEntry($"ELM READ setup failed - {ex.Message}", false);
            }
        }
        #endregion

        #region Write/Read PID

        //SINGLE WRITE/READ
        public async Task WriteSinglePID(Gauge gauge)
        {
            //await Task.Delay(pidDelay);
            try
            {
                byte[] writeBuffer = Encoding.ASCII.GetBytes(gauge.OBDCommand);

                // Write data to the device
                await socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                await socket.OutputStream.FlushAsync();
                await Task.Delay(pidDelay);

                gauge.GaugeValue = await ReadSinglePID(gauge);
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
            }
        }

        public async Task<double> ReadSinglePID(Gauge gauge)
        {
            try
            {
                // Read data from the device
                byte[] readBuffer = new byte[512];
                int length = await socket.InputStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                string data = Encoding.ASCII.GetString(readBuffer);

                if (SettingsControl.debugMode) BluetoothControl.UpdateLog(data);

                double output = gauge.ParseGauge(data.Substring(9, gauge.HexNum));
                return output;
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
                return 0;
            }
        }

        //STACK WRITE/READ
        public async Task WriteStackPID()
        {
            try
            {
                foreach(Gauge g in gauges)
                {
                    byte[] writeBuffer = Encoding.ASCII.GetBytes(g.OBDCommand);

                    // Write data to the device
                    await socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                    await socket.OutputStream.FlushAsync();
                }

                await Task.Delay(pidDelay);
                await ReadStackPID();
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
            }
        }

        public async Task ReadStackPID()
        {
            try
            {
                // Read data from the device
                byte[] readBuffer = new byte[512];
                int length = await socket.InputStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                string data = Encoding.ASCII.GetString(readBuffer);

                await ParseStackPID(data);
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
            }
        }

        public async Task ParseStackPID(string data)
        {
            BluetoothControl.UpdateLog(data);

            string[] dt = data.Split('<');
            int index = 9;
            foreach (Gauge g in gauges)
            {
                g.GaugeValue = g.ParseGauge(dt[0].Substring(dt[0].Length - MainGauge.HexNum, MainGauge.HexNum));
                index += g.HexNum + 11;
            }

            ////double output = g.ParseGauge(data.Substring(9, gauge.HexNum));
            //gauges[0].GaugeValue = ParseGauge.Voltage(dt[0].Substring(dt[0].Length - MainGauge.HexNum, MainGauge.HexNum));


            //MainGauge.GaugeValue = MainGauge.ParseGauge(data.Substring(index, MainGauge.HexNum));
            //index += MainGauge.HexNum + 11;

            //BoxGauge1.GaugeValue = BoxGauge1.ParseGauge(data.Substring(index, BoxGauge1.HexNum));
            //index += BoxGauge1.HexNum + 11;

            //BoxGauge2.GaugeValue = BoxGauge2.ParseGauge(data.Substring(index, BoxGauge2.HexNum));
            //index += BoxGauge2.HexNum + 11;

            //BoxGauge3.GaugeValue = BoxGauge3.ParseGauge(data.Substring(index, BoxGauge3.HexNum));
            //index += BoxGauge3.HexNum + 11;

            //BoxGauge4.GaugeValue = BoxGauge4.ParseGauge(data.Substring(index, BoxGauge4.HexNum));
        }


        #endregion

        #region PID Error Code Reading

        public async Task<List<string>> WriteTroubleRequest()
        {
            List<string> troubleCodes = new List<string>();
            try
            {
                byte[] writeBuffer = Encoding.ASCII.GetBytes("03\r");
                await socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                await socket.OutputStream.FlushAsync();

                await Task.Delay(elmDelay);
                return await ReadTroubleResponse();
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry($"Trouble Code Request Failed - {ex.Message}", false);
                return new List<string>();
            }
        }

        public async Task<List<string>> ReadTroubleResponse()
        {
            try
            {
                byte[] readBuffer = new byte[512];
                int length = await socket.InputStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                string data = Encoding.ASCII.GetString(readBuffer);

                return await ParseTroubleResponse(data);
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry($"Trouble Code Response Failed - {ex.Message}", false);
                return new List<string>();
            }
        }

        public async Task<List<string>> ParseTroubleResponse(string input)
        {
            List<string> troubleCodes = new List<string>();
            try
            {
                BluetoothControl.UpdateLog(input);

                int loop = input.Length / 8;
                int index = 0;

                for(int i =0; i < loop; i ++)
                {
                    string hex = input.Substring(index, 8);
                    troubleCodes.Add(hex);

                    index = index + 8;
                }

                foreach(string t in troubleCodes)
                {
                    //Parse the hex into string
                }
            }
            catch(Exception ex)
            {
                BluetoothControl.SystemLogEntry($"Trouble Code Parsing Failed - {ex.Message}", false);
                troubleCodes.Add("Error Parsing Trouble Code - check system log");
            }
            return troubleCodes;
        }


        public async Task ClearTroubleCodes()
        {
            try
            {
                byte[] writeBuffer = Encoding.ASCII.GetBytes("04\r");
                await socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                await socket.OutputStream.FlushAsync();

                await Task.Delay(elmDelay);
                BluetoothControl.SystemLogEntry($"<<<--Trouble Codes Cleared, Engine Light Reset-->>>", false);
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry($"Trouble Code Clear Failed - {ex.Message}", false);
            }
        }
        #endregion
    }
}
