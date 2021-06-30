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
            gagRadial1.PointerPositionChanged += Radial_PointerPositionChanged;
            gagRadial2.PointerPositionChanged += Radial_PointerPositionChanged;

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
        public async void ConnectSavedDevice()
        {
            tbkBTStatus.Text = "Atempting Connection...";
            tbkBTStatus.TextColor = Color.Purple;

            try
            {
                savedDeviceName = SettingsControl.loadedSettings.btDeviceName;
                savedDeviceAddress = SettingsControl.loadedSettings.btDeviceAddress;

                device = (from bd in adapter.BondedDevices where bd.Name == savedDeviceName select bd).FirstOrDefault();
                if (device == null) throw new Exception("not in list of available devices");
                ConnectionHandling();
            }

            catch (Exception ex)
            {
                FailedConnection($"Saved Device not found - {ex.Message}");
            }
        }

        public async void ConnectDevice()
        {
            tbkBTStatus.Text = "Atempting Connection...";
            tbkBTStatus.TextColor = Color.Purple;

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

                gaugeCommands = new GaugeCommands(socket, 1000, 250);

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
                byte[] writeBuffer = Encoding.ASCII.GetBytes("ATZ\r");
                await gaugeCommands.socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                await gaugeCommands.socket.OutputStream.FlushAsync();

                // Read data from the device
                byte[] readBuffer = new byte[50];
                int length = await socket.InputStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                string data = Encoding.ASCII.GetString(readBuffer);


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

                gagRadial1.Headers[0].Text = $"{gaugeCommands.RadialGauge1.Name} ({gaugeCommands.RadialGauge1.UOM})";
                gagRadial1.Scales[0].Interval = gaugeCommands.RadialGauge1.TickSpacing;
                gagRadial1.Scales[0].StartValue = gaugeCommands.RadialGauge1.Min;
                gagRadial1.Scales[0].EndValue = gaugeCommands.RadialGauge1.Max;

                gagRadial2.Headers[0].Text = $"{gaugeCommands.RadialGauge2.Name} ({gaugeCommands.RadialGauge2.UOM})";
                gagRadial2.Scales[0].Interval = gaugeCommands.RadialGauge2.TickSpacing;
                gagRadial2.Scales[0].StartValue = gaugeCommands.RadialGauge2.Min;
                gagRadial2.Scales[0].EndValue = gaugeCommands.RadialGauge2.Max;

                gagBox1Label.Text = gaugeCommands.BoxGauge1.Name;
                gagBox1Value.Text = "0";
                gagBox1UoM.Text = gaugeCommands.BoxGauge1.UOM;

                gagBox2Label.Text = gaugeCommands.BoxGauge2.Name;
                gagBox2Value.Text = "0";
                gagBox2UoM.Text = gaugeCommands.BoxGauge2.UOM;

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
        public async void RunGauges()
        {
            loopPid = true;
            while (loopPid)
                try
                {
                    UpdateLog("Started Looping...");
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    //Get Data
                    //loopPid = await gaugeCommands.ExecutePIDs(); //- For Grouped execution of commands
                    await gaugeCommands.ExecuteSinglePIDs();   //- For Single execution of commands

                    sw.Stop();
                    UpdateLog($"-----Time to Execute: {Convert.ToString(sw.ElapsedMilliseconds)}");

                    //Update Gauge UI
                    gagRadialMain.Scales[0].Pointers[0].Value = gaugeCommands.MainGauge.GaugeValue;
                    gagRadial1.Scales[0].Pointers[0].Value = gaugeCommands.RadialGauge1.GaugeValue;
                    gagRadial2.Scales[0].Pointers[0].Value = gaugeCommands.RadialGauge2.GaugeValue;
                    gagBox1Value.Text = Convert.ToString(gaugeCommands.BoxGauge1.GaugeValue);
                    gagBox2Value.Text = Convert.ToString(gaugeCommands.BoxGauge2.GaugeValue);

                    if (failCount > 10)
                        loopPid = await TestConnection();

                }
                catch (Exception ex)
                {
                    FailedConnection($"Error at PID Loop - {ex.Message}");
                }

            SystemLogEntry($"Stopped Looping", false);
            UpdateLog("Stopped Looping");
        }

        public async void StopGauges()
        {
            loopPid = false;
        }
        #endregion

        #region Successful/Failed Connection Handling
        private async void SuccessfulConnection()
        {
            try
            {
                tbkOBDDevice.Text = device.Name;
                tbkOBDStatus.Text = "Connected";
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

                tbkOBDDevice.Text = "None";
                tbkOBDStatus.Text = "No Connection";
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

        private void btnBTMenuOptions_Clicked(object sender, EventArgs e)
        {
            BTMenuOptions.IsVisible = true;
            btnBTMenuOptions.IsVisible = false;
        }

        private void btnCloseBTMenuOptions_Clicked(object sender, EventArgs e)
        {
            BTMenuOptions.IsVisible = false;
            btnBTMenuOptions.IsVisible = true;
        }
        private void btnStopGauges_Clicked(object sender, EventArgs e)
        {
            StopGauges();
        }

        private void btnStartGauges_Clicked(object sender, EventArgs e)
        {
            RunGauges();
        }

        private void btnAutoConnect_Clicked(object sender, EventArgs e)
        {
            ConnectSavedDevice();
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
                if (sender == gagRadial1)
                {
                    lblRadial1Value.Text = Convert.ToString(args.pointerValue);
                    return;
                }
                if (sender == gagRadial2)
                {
                    lblRadial2Value.Text = Convert.ToString(args.pointerValue);
                    return;
                }
                throw new Exception("unkown radial gauge");
                // the rest of them
            }
            catch(Exception ex)
            {
                SystemLogEntry(ex.Message, false);
            }
        }

        #endregion

    }
}