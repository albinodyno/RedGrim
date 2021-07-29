using Android.Bluetooth;
using Java.Util;
using RedGrim.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RedGrim.Mobile.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothControl : ContentView
    {
        BluetoothAdapter adapter;
        BluetoothSocket socket;
        BluetoothDevice device;
        GaugeCommands gaugeCommands;

        public static string savedDeviceAddress = "";
        public static string savedDeviceName = "";

        public static string log = "";
        public static string errorLog = "";
        public static int failCount = 0;

        bool connectionSetup = false;
        bool loopPid = true;


        public BluetoothControl()
        {
            InitializeComponent();

            gagRadialMain.PointerPositionChanged += Radial_PointerPositionChanged;
            //gagRadial1.PointerPositionChanged += Radial_PointerPositionChanged;
            //gagRadial2.PointerPositionChanged += Radial_PointerPositionChanged;

            LoadAdapter();
        }

        #region Initial Bluetooth Setttings
        public void LoadAdapter()
        {
            try
            {
                adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null)
                    throw new Exception("No Bluetooth adapter found.");

                if (!adapter.IsEnabled)
                    throw new Exception("Bluetooth adapter is not enabled.");
            }
            catch(Exception ex)
            {
                FailedConnection(ex.Message);
            }
        }

        public void LoadDevices()
        {
            if (adapter.IsEnabled)
                foreach (BluetoothDevice d in adapter.BondedDevices)
                pkrBluetoothPicker.Items.Add(d.Name);
        }

        private void pkrBluetoothPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConnectDevice();
        }
        #endregion

        #region Bluetooth Device Setup and Gauges Setup
        public void ConnectSavedDevice()
        {
            if (gaugeCommands != null) return;

            tbkBTStatus.Text = "Atempting...";
            tbkBTStatus.TextColor = Color.Magenta;

            try
            {
                savedDeviceName = SettingsControl.loadedSettings.btDeviceName;
                savedDeviceAddress = SettingsControl.loadedSettings.btDeviceAddress;

                tbkBTStatus.Text = savedDeviceName + "...";

                device = (from bd in adapter.BondedDevices where bd.Name == savedDeviceName select bd).FirstOrDefault();
                if (device == null) throw new Exception("not in list of available devices");
                ConnectionHandling();
            }

            catch (Exception ex)
            {
                FailedConnection($"Saved Device not found - {ex.Message}");
            }
        }
        public void ConnectDevice()
        {
            tbkBTStatus.Text = "Atempting...";
            tbkBTStatus.TextColor = Color.Magenta;

            try
            {
                if (pkrBluetoothPicker.SelectedIndex == -1)
                    throw new Exception("Device not found");
                else
                    device = (from bd in adapter.BondedDevices where bd.Name == pkrBluetoothPicker.Items[pkrBluetoothPicker.SelectedIndex] select bd).FirstOrDefault();

                ConnectionHandling();
            }
            catch(Exception ex)
            {
                FailedConnection(ex.Message);
            }
        }

        //Main handler for running through connections and tests
        public async void ConnectionHandling()
        {
            failCount = 0;
            connectionSetup = await ConnectSocket();
            if (connectionSetup) connectionSetup = await TestConnection();
            if (connectionSetup) connectionSetup = await SetupGauges();
            if (connectionSetup) connectionSetup = await TestGauges();
            if (connectionSetup) SuccessfulConnection();

            if (connectionSetup) RunGauges();
        }

        private async Task<bool> ConnectSocket()
        {
            try
            {
                if (device == null)
                    throw new Exception("Device not found");

                socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                await socket.ConnectAsync();

                gaugeCommands = new GaugeCommands(socket);

                return true;
            }
            catch (Exception ex)
            {
                FailedConnection(ex.Message);
                return false;
            }
        }

        private async Task<bool> TestConnection()
        {
            try
            {
                // Write data to the device
                byte[] writeBuffer = Encoding.ASCII.GetBytes("010D\r");
                await gaugeCommands.socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                await gaugeCommands.socket.OutputStream.FlushAsync();

                // Read data from the device
                byte[] readBuffer = new byte[512];
                int length = await socket.InputStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                string data = Encoding.ASCII.GetString(readBuffer);

                if (data.Contains("ERROR")) throw new Exception($"Testing Connection Failed: {data}");

                UpdateLog($"Connection Test Successful - {data}");
                failCount = 0;
                return true;
            }
            catch (Exception ex)
            {
                FailedConnection($"Connection Test Failed - {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetupGauges()
        {
            try
            {
                gagRadialMain.Headers[0].Text = $"{gaugeCommands.MainGauge.Name} ({gaugeCommands.MainGauge.UOM})";
                gagRadialMain.Scales[0].Interval = gaugeCommands.MainGauge.TickSpacing;
                gagRadialMain.Scales[0].StartValue = gaugeCommands.MainGauge.Min;
                gagRadialMain.Scales[0].EndValue = gaugeCommands.MainGauge.Max;

                //gagRadial1.Headers[0].Text = $"{gaugeCommands.RadialGauge1.Name} ({gaugeCommands.RadialGauge1.UOM})";
                //gagRadial1.Scales[0].Interval = gaugeCommands.RadialGauge1.TickSpacing;
                //gagRadial1.Scales[0].StartValue = gaugeCommands.RadialGauge1.Min;
                //gagRadial1.Scales[0].EndValue = gaugeCommands.RadialGauge1.Max;

                //gagRadial2.Headers[0].Text = $"{gaugeCommands.RadialGauge2.Name} ({gaugeCommands.RadialGauge2.UOM})";
                //gagRadial2.Scales[0].Interval = gaugeCommands.RadialGauge2.TickSpacing;
                //gagRadial2.Scales[0].StartValue = gaugeCommands.RadialGauge2.Min;
                //gagRadial2.Scales[0].EndValue = gaugeCommands.RadialGauge2.Max;

                gagBox1Label.Text = gaugeCommands.BoxGauge1.Name;
                gagBox1Value.Text = "0";
                gagBox1UoM.Text = gaugeCommands.BoxGauge1.UOM;

                gagBox2Label.Text = gaugeCommands.BoxGauge2.Name;
                gagBox2Value.Text = "0";
                gagBox2UoM.Text = gaugeCommands.BoxGauge2.UOM;

                gagBox3Label.Text = gaugeCommands.BoxGauge3.Name;
                gagBox3Value.Text = "0";
                gagBox3UoM.Text = gaugeCommands.BoxGauge3.UOM;

                gagBox4Label.Text = gaugeCommands.BoxGauge4.Name;
                gagBox4Value.Text = "0";
                gagBox4UoM.Text = gaugeCommands.BoxGauge4.UOM;

                return true;
            }
            catch(Exception ex)
            {
                FailedConnection(ex.Message);
                return false;
            }
        }

        public async Task<bool> TestGauges()
        {
            try
            {
                if (gaugeCommands == null) 
                    throw new Exception("Gauge Test Failed, Gauge Commands Null");
                else
                    await gaugeCommands.SetupCommands();

                return true;
            }
            catch(Exception ex)
            {
                FailedConnection(ex.Message);
                return false;
            }
        }
        #endregion

        #region  Run Gauges
        public async void RunGauges1()
        {
            loopPid = true;
            UpdateLog("Started Looping...");
            while (loopPid)
                try
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    //Get Data
                    //loopPid = await gaugeCommands.ExecutePIDs(); //- For Grouped execution of commands
                    await gaugeCommands.ExecuteSinglePIDs();   //- For Single execution of commands

                    sw.Stop();
                    UpdateLog($"-----Time to Execute: {Convert.ToString(sw.ElapsedMilliseconds)}");

                    //Update Gauge UI
                    gagRadialMain.Scales[0].Pointers[0].Value = gaugeCommands.MainGauge.GaugeValue;
                    //gagRadial1.Scales[0].Pointers[0].Value = gaugeCommands.RadialGauge1.GaugeValue;
                    //gagRadial2.Scales[0].Pointers[0].Value = gaugeCommands.RadialGauge2.GaugeValue;
                    gagBox1Value.Text = Convert.ToString(gaugeCommands.BoxGauge1.GaugeValue);
                    gagBox2Value.Text = Convert.ToString(gaugeCommands.BoxGauge2.GaugeValue);
                    gagBox3Value.Text = Convert.ToString(gaugeCommands.BoxGauge3.GaugeValue);
                    gagBox4Value.Text = Convert.ToString(gaugeCommands.BoxGauge4.GaugeValue);

                    if (failCount > 10)
                        loopPid = await TestConnection();
                }
                catch (Exception ex)
                {
                    FailedConnection($"Error at PID Loop - {ex.Message}");
                    loopPid = false;
                }
            ResetGauges();
            UpdateLog("...Stopped Looping");
        }

        public async void RunGauges()
        {
            loopPid = true;
            UpdateLog("Started Looping...");
            while (loopPid)
                try
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    await gaugeCommands.WriteSinglePID(gaugeCommands.MainGauge);
                    gagRadialMain.Scales[0].Pointers[0].Value = gaugeCommands.MainGauge.GaugeValue;

                    //await gaugeCommands.WriteSinglePID(gaugeCommands.RadialGauge1);
                    //gagRadial1.Scales[0].Pointers[0].Value = gaugeCommands.RadialGauge1.GaugeValue;

                    //await gaugeCommands.WriteSinglePID(gaugeCommands.RadialGauge2);
                    //gagRadial2.Scales[0].Pointers[0].Value = gaugeCommands.RadialGauge2.GaugeValue;

                    await gaugeCommands.WriteSinglePID(gaugeCommands.BoxGauge1);
                    gagBox1Value.Text = Convert.ToString(gaugeCommands.BoxGauge1.GaugeValue);

                    await gaugeCommands.WriteSinglePID(gaugeCommands.BoxGauge2);
                    gagBox2Value.Text = Convert.ToString(gaugeCommands.BoxGauge2.GaugeValue);

                    await gaugeCommands.WriteSinglePID(gaugeCommands.BoxGauge3);
                    gagBox3Value.Text = Convert.ToString(gaugeCommands.BoxGauge3.GaugeValue);

                    await gaugeCommands.WriteSinglePID(gaugeCommands.BoxGauge4);
                    gagBox4Value.Text = Convert.ToString(gaugeCommands.BoxGauge4.GaugeValue);


                    sw.Stop();
                    UpdateLog($"-----Time to Execute: {Convert.ToString(sw.ElapsedMilliseconds)}");

                    if (failCount > 10)
                        loopPid = await TestConnection();
                }
                catch (Exception ex)
                {
                    FailedConnection($"Error at PID Loop - {ex.Message}");
                    loopPid = false;
                }
            ResetGauges();
            UpdateLog("...Stopped Looping");
        }

        public async void StopGauges()
        {
            loopPid = false;
        }

        public void ResetGauges()
        {
            gagRadialMain.Scales[0].Pointers[0].Value = 0;
            //gagRadial1.Scales[0].Pointers[0].Value = 0;
            //gagRadial2.Scales[0].Pointers[0].Value = 0;
            gagBox1Value.Text = "---";
            gagBox2Value.Text = "---";
            gagBox3Value.Text = "---";
            gagBox4Value.Text = "---";
        }
        #endregion

        #region Successful/Failed Connection Handling
        private async void SuccessfulConnection()
        {
            try
            {
                tbkBTStatus.Text = "Connected";
                tbkBTStatus.TextColor = Color.Cyan;
                SaveDevice();
            }
            catch (Exception ex)
            {
                FailedConnection(ex.Message);
            }
        }

        private void FailedConnection(string error)
        {
            try
            {
                if (device != null) device.Dispose();
                if (socket != null) socket.Close();
                if (gaugeCommands != null) gaugeCommands = null;

                tbkBTStatus.Text = "No Connection";
                tbkBTStatus.TextColor = Color.OrangeRed;
                SystemLogEntry($"{error}", false);
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region Button Events

        public void ToggleMenu()
        {
            if (!pnlBTSettings.IsVisible)
                pnlBTSettings.IsVisible = true;
            else
                pnlBTSettings.IsVisible = false;

            LoadDevices();
        }

        private void btnStopGauges_Clicked(object sender, EventArgs e)
        {
            StopGauges();
        }

        private void btnStartGauges_Clicked(object sender, EventArgs e)
        {
            tbkBTStatus.Text = "Atempting...";
            tbkBTStatus.TextColor = Color.Magenta;
            RunGauges();
        }

        private void btnAutoConnect_Clicked(object sender, EventArgs e)
        {
            ConnectSavedDevice();
        }

        private void btnErrorLog_Clicked(object sender, EventArgs e)
        {
           
        }

        private void btnOBDLog_Clicked(object sender, EventArgs e)
        {

        }

        #endregion

        #region Misc
        private async void SaveDevice()
        {
            SettingsControl.saveSettings.btDeviceName = device.Name;
            SettingsControl.saveSettings.btDeviceAddress = device.Address;

            SettingsControl.SaveData();
        }

        public static void SystemLogEntry(string entry, bool criticalFail)
        {
            if (criticalFail)
            {
                BluetoothControl.failCount++;
                errorLog = errorLog + $"--{entry} - Critical Fail Count: {BluetoothControl.failCount}\n\n";
            }
            else
                errorLog = errorLog + $"--{entry}\n\n";
        }

        public static void UpdateLog(string input)
        {
            log = log + "\r\n" + input;
        }

        private void Radial_PointerPositionChanged(object sender, Syncfusion.SfGauge.XForms.PointerPositionChangedArgs args)
        {
            try
            {
                if (sender == gagRadialMain)
                {
                    lblMainValue.Text = Convert.ToString(args.pointerValue);
                    return;
                }
                //if (sender == gagRadial1)
                //{
                //    lblRadial1Value.Text = Convert.ToString(args.pointerValue);
                //    return;
                //}
                //if (sender == gagRadial2)
                //{
                //    lblRadial2Value.Text = Convert.ToString(args.pointerValue);
                //    return;
                //}
                //throw new Exception("unkown radial gauge");
                //// the rest of them
            }
            catch(Exception ex)
            {
                SystemLogEntry(ex.Message, false);
            }
        }

        #endregion

    }
}