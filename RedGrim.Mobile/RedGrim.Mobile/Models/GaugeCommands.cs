using RedGrim.Mobile.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Android.Bluetooth;
//using Windows.Devices.Bluetooth;
//using Windows.Devices.Bluetooth.Rfcomm;
//using Windows.Networking.Sockets;
//using Windows.Storage.Streams;

namespace RedGrim.Mobile.Models
{
    class GaugeCommands
    {
        public BluetoothSocket socket;
        //DataWriter obdWriter;
        //DataReader obdReader;

        public int elmDelay = 1000;
        public int pidDelay = 300;
        bool success;

        public Gauge MainGauge;
        public Gauge RadialGauge1;
        public Gauge RadialGauge2;
        public Gauge BoxGauge1;
        public Gauge BoxGauge2;


        //public Dictionary<string, string> obdCommands = new Dictionary<string, string>()
        //{
        //    {"RPM", "010C\r"},
        //    {"MPH","010D\r"},
        //    {"EngineLoad", "0104\r"},
        //    {"ThrottlePos", "0111\r"},
        //    {"CoolantTemp", "0105\r"},
        //    {"IntakeTemp", "010F\r"},
        //    {"Voltage", "0144\r"}
        //};

        public GaugeCommands(BluetoothSocket btSocket, int eDelay, int pDelay)
        {
            socket = btSocket;

            elmDelay = eDelay;
            pidDelay = pDelay;

            //choose gauge content here
            MainGauge = BuildGauge.RPMGauge();
            RadialGauge1 = BuildGauge.CoolantGauge();
            RadialGauge2 = BuildGauge.VoltageGauge();
            BoxGauge1 = BuildGauge.MPHGauge();
            BoxGauge2 = BuildGauge.IntakeGauge();
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
                MainPage.SystemLogEntry($"ELM setup failed - {ex.Message}");
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
                MainPage.SystemLogEntry($"ELM WRITE setup failed - {ex.Message}");
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
                MainPage.SystemLogEntry($"ELM READ setup failed - {ex.Message}");
            }
        }
        #endregion


        #region Execute PID Commands
        public async Task<bool> ExecutePIDs()
        {
            await WritePID(MainGauge.OBDCommand);
            await WritePID(RadialGauge1.OBDCommand);
            await WritePID(RadialGauge2.OBDCommand);
            await WritePID(BoxGauge1.OBDCommand);
            await WritePID(BoxGauge2.OBDCommand);

            await Task.Delay(pidDelay);

            success = await ReadPID();
            return success;
        }

        public async Task ExecuteSinglePIDs()
        {
            await WriteSinglePID(MainGauge);
            await WriteSinglePID(RadialGauge1);
            await WriteSinglePID(RadialGauge2);
            await WriteSinglePID(BoxGauge1);
            await WriteSinglePID(BoxGauge2);
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
                MainPage.SystemLogEntry(ex.Message);
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

                BluetoothControl.UpdateLog(data);

                string[] dt = data.Split('<');



                MainGauge.GaugeValue = GaugeParse.Voltage(dt[0].Substring(dt[0].Length-MainGauge.HexNum, MainGauge.HexNum));
                //RadialGauge1.GaugeValue = GaugeParse.CoolantTemp(data.Substring(24, 2));
                //RadialGauge2.GaugeValue = GaugeParse.Intaketemp(data.Substring(37, 2));
                //BoxGauge1.GaugeValue = GaugeParse.CoolantTemp(data.Substring(50, 2));
                //BoxGauge2.GaugeValue = GaugeParse.Intaketemp(data.Substring(63, 2));

                int index = 9;

                MainGauge.GaugeValue = MainGauge.ParseGauge(data.Substring(index, MainGauge.HexNum));
                index += MainGauge.HexNum + 11;

                RadialGauge1.GaugeValue = RadialGauge1.ParseGauge(data.Substring(index, RadialGauge1.HexNum));
                index += RadialGauge1.HexNum + 11;

                RadialGauge2.GaugeValue = RadialGauge2.ParseGauge(data.Substring(index, RadialGauge2.HexNum));
                index += RadialGauge2.HexNum + 11;

                BoxGauge1.GaugeValue = BoxGauge1.ParseGauge(data.Substring(index, BoxGauge1.HexNum));
                index += BoxGauge1.HexNum + 11;

                BoxGauge2.GaugeValue = BoxGauge2.ParseGauge(data.Substring(index, BoxGauge2.HexNum));

                return true;
            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
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
                MainPage.SystemLogEntry(ex.Message);
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

                BluetoothControl.UpdateLog(data);

                double output = gauge.ParseGauge(data.Substring(9, gauge.HexNum));
                return output;
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                return 0;
            }
        }

        #endregion
    }
}
