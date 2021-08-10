using RedGrim.Mobile.Controls;
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


        #region Execute PID Commands
        public async Task<bool> ExecutePIDs()
        {
            await WritePID(MainGauge.OBDCommand);
            //await WritePID(RadialGauge1.OBDCommand);
            //await WritePID(RadialGauge2.OBDCommand);
            await WritePID(BoxGauge1.OBDCommand);
            await WritePID(BoxGauge2.OBDCommand);
            await WritePID(BoxGauge3.OBDCommand);
            await WritePID(BoxGauge4.OBDCommand);

            await Task.Delay(pidDelay);

            success = await ReadPID();
            return success;
        }

        public async Task ExecuteSinglePIDs()
        {
            await WriteSinglePID(MainGauge);
            //await WriteSinglePID(RadialGauge1);
            //await WriteSinglePID(RadialGauge2);
            await WriteSinglePID(BoxGauge1);
            await WriteSinglePID(BoxGauge2);
            await WriteSinglePID(BoxGauge3);
            await WriteSinglePID(BoxGauge4);
        }
        #endregion


        #region Write/Read PID
        public async Task WritePID(string command) //For Single Write/Read: Add parameter for Gauge
        {
            try
            {
                byte[] writeBuffer = Encoding.ASCII.GetBytes(command);

                // Write data to the device
                await socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                await socket.OutputStream.FlushAsync();
                await Task.Delay(pidDelay);
            }
            catch (Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
            }
        }

        public async Task<bool> ReadPID()
        {
            try
            {
                // Read data from the device
                byte[] readBuffer = new byte[512];
                int length = await socket.InputStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                string data = Encoding.ASCII.GetString(readBuffer);

                if( SettingsControl.debugMode ) BluetoothControl.UpdateLog(data);

                string[] dt = data.Split('<');


                MainGauge.GaugeValue = GaugeParse.Voltage(dt[0].Substring(dt[0].Length-MainGauge.HexNum, MainGauge.HexNum));
                //RadialGauge1.GaugeValue = GaugeParse.CoolantTemp(data.Substring(24, 2));
                //RadialGauge2.GaugeValue = GaugeParse.Intaketemp(data.Substring(37, 2));
                //BoxGauge1.GaugeValue = GaugeParse.CoolantTemp(data.Substring(50, 2));
                //BoxGauge2.GaugeValue = GaugeParse.Intaketemp(data.Substring(63, 2));

                int index = 9;

                MainGauge.GaugeValue = MainGauge.ParseGauge(data.Substring(index, MainGauge.HexNum));
                index += MainGauge.HexNum + 11;

                BoxGauge1.GaugeValue = BoxGauge1.ParseGauge(data.Substring(index, BoxGauge1.HexNum));
                index += BoxGauge1.HexNum + 11;

                BoxGauge2.GaugeValue = BoxGauge2.ParseGauge(data.Substring(index, BoxGauge2.HexNum));
                index += BoxGauge2.HexNum + 11;

                BoxGauge3.GaugeValue = BoxGauge3.ParseGauge(data.Substring(index, BoxGauge3.HexNum));
                index += BoxGauge3.HexNum + 11;

                BoxGauge4.GaugeValue = BoxGauge4.ParseGauge(data.Substring(index, BoxGauge4.HexNum));

                return true;
            }
            catch(Exception ex)
            {
                BluetoothControl.SystemLogEntry(ex.Message, true);
                return false;
            }
        }

        public async Task WriteSinglePID(Gauge gauge)
        {
            await Task.Delay(pidDelay);
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

        #endregion
    }
}
