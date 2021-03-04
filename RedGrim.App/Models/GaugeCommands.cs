using RedGrim.App.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RedGrim.App.Models
{
    class GaugeCommands
    {
        StreamSocket streamSocket;
        DataWriter obdWriter;
        DataReader obdReader;

        public static int SuccessLoops = 0;

        public int elmDelay;
        public int pidDelay;
        bool success;

        public Gauge botLeft;
        public Gauge botRight;
        public Gauge mainGauge;

        public Dictionary<string, string> obdCommands = new Dictionary<string, string>()
        {
            {"RPM", "010C\r"},
            {"MPH","010D\r"},
            {"EngineLoad", "0104\r"},
            {"ThrottlePos", "0111\r"},
            {"CoolantTemp", "0105\r"},
            {"IntakeTemp", "010F\r"},
            {"Voltage", "0144\r"}
        };

        public GaugeCommands(StreamSocket stream, int eDelay, int pDelay)
        {
            streamSocket = stream;
            //stream.Control.KeepAlive = true;

            obdWriter = new DataWriter(streamSocket.OutputStream);
            obdReader = new DataReader(streamSocket.InputStream);
            obdReader.InputStreamOptions = InputStreamOptions.Partial;

            elmDelay = eDelay;
            pidDelay = pDelay;

            botLeft = new Gauge("CoolantTemp", "°F", 300, 15, obdCommands["CoolantTemp"]);
            botRight = new Gauge("IntakeTemp", "°F", 300, 15, obdCommands["IntakeTemp"]);
            mainGauge = new Gauge("Voltage", "V", 50, 5, obdCommands["Voltage"]);
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
                MainPage.SystemLogEntry($"ELM Setup failed - {ex.Message}");
                return false;
            }
        }

        public async Task WriteELMCommand(string input)
        {
            try
            {
                obdWriter.WriteString(input);
                await obdWriter.StoreAsync();
                await obdWriter.FlushAsync();

                await Task.Delay(elmDelay);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry($"ELM Write failed - {ex.Message}");
            }
        }

        public async Task ReadELMCommands()
        {
            try
            {
                uint buffer = await obdReader.LoadAsync(512);
                string value = obdReader.ReadString(buffer);
                BluetoothControl.log = BluetoothControl.log + value;
            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry($"ELM Read failed - {ex.Message}");
            }
        }
        #endregion


        #region Execute PID Commands
        public async Task<bool> ExecutePIDs()
        {
            try
            {
               success = await WritePID(botLeft.OBDCommand);   //CoolantTemp
                if (success)
                    success = await WritePID(botRight.OBDCommand);  //IntakeTemp
                if (success)
                    success = await WritePID(mainGauge.OBDCommand); //Voltage
                else
                {
                    MainPage.SystemLogEntry($"Writing PIDS was unsuccessful");
                    ClearBuffer();
                    return true;
                }
                await Task.Delay(pidDelay);

                success = await ReadPID();
                SuccessLoops++;
                return success;
            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry($"PID Loop failed - {ex.Message}");
                return false;
            }
        }
        #endregion


        #region Write/Read PID
        public async Task<bool> WritePID(string command)        //Write in PID code
        {
            try
            {
                obdWriter.WriteString(command);
                await obdWriter.StoreAsync();
                await obdWriter.FlushAsync();
                await Task.Delay(pidDelay);  //Can i remove this if we wait for pid delay to read?
                return true;
            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry($"Error at Writing PID - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ReadPID()        //Read in PID response and parse
        {
            try
            {
                uint buffer = await obdReader.LoadAsync(512);
                string value = obdReader.ReadString(buffer);
                BluetoothControl.log = BluetoothControl.log + value;

                botLeft.GaugeValue = GaugeParse.CoolantTemp(value.Substring(9, 2));
                botRight.GaugeValue = GaugeParse.Intaketemp(value.Substring(23, 2));
                mainGauge.GaugeValue = GaugeParse.Voltage(value.Substring(37, 4));

                return true;
            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry($"Error at Reading PID - {ex.Message}");
                return false;
            }
        }

        public async void ClearBuffer()
        {
            try
            {
                uint buffer = await obdReader.LoadAsync(512);
                string value = obdReader.ReadString(buffer);
                BluetoothControl.log = BluetoothControl.log + value + "\r\rBUFFER CLEARED";
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry($"Error Clearing Buffer");
            }
        }
        #endregion
    }
}
