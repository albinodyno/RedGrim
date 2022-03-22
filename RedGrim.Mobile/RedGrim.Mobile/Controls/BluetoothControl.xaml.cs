using Android.Bluetooth;
using Java.Util;
using RedGrim.Mobile.Models;
using RedGrim.Mobile.Tools;
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
        public ViewModels.SampleViewModel ViewModel
        {
            get => BindingContext as ViewModels.SampleViewModel;
            set => BindingContext = value;
        }


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


        //private void SAMPLETEST()
        //{
        //    string someValueFromBT = "TEST";
        //    //nameGauge1.ViewModel.gaugeValue = someValueFromBT;
        //}

        public BluetoothControl()
        {
            InitializeComponent();
            ViewModel = new ViewModels.SampleViewModel();
            LoadAdapter();
            UpdateTheme();
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
            ToggleMenu();
            ConnectDevice();
        }
        #endregion

        #region Bluetooth Device Setup and Gauges Setup
        public async void ConnectSavedDevice()
        {
            if (gaugeCommands != null) return;

            tbkBTStatus.Text = "Atempting...";
            tbkBTStatus.TextColor = ColorMaster.ColorSecondary;

            try
            {
                savedDeviceName = SettingsControl.loadedSettings.btDeviceName;
                savedDeviceAddress = SettingsControl.loadedSettings.btDeviceAddress;

                tbkBTStatus.Text = savedDeviceName + "...";

                device = (from bd in adapter.BondedDevices where bd.Name == savedDeviceName select bd).FirstOrDefault();
                if (device == null) throw new Exception("not in list of available devices");
                await ConnectionHandling();
            }
            catch (Exception ex)
            {
                FailedConnection($"Saved Device not found - {ex.Message}");
            }
        }
        public async void ConnectDevice()
        {
            tbkBTStatus.Text = "Atempting...";
            tbkBTStatus.TextColor = ColorMaster.ColorSecondary;

            try
            {
                if (pkrBluetoothPicker.SelectedIndex == -1)
                    throw new Exception("Device not found");
                else
                    device = (from bd in adapter.BondedDevices where bd.Name == pkrBluetoothPicker.Items[pkrBluetoothPicker.SelectedIndex] select bd).FirstOrDefault();

                await ConnectionHandling();
            }
            catch(Exception ex)
            {
                FailedConnection(ex.Message);
            }
        }

        //Main handler for running through connections and tests
        public async Task ConnectionHandling()
        {
            failCount = 0;
            connectionSetup = await ConnectSocket();
            if (connectionSetup) connectionSetup = await TestConnection(true);
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

        private async Task<bool> TestConnection(bool coldStart)
        {
            try
            {
                // Write data to the device
                byte[] writeBuffer;

                if(coldStart) writeBuffer = Encoding.ASCII.GetBytes("ATZ\r");
                else writeBuffer = Encoding.ASCII.GetBytes("010D\r");

                await gaugeCommands.socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                await gaugeCommands.socket.OutputStream.FlushAsync();

                await Task.Delay(1500);

                // Read data from the device
                byte[] readBuffer = new byte[512];
                int length = await socket.InputStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                string data = Encoding.ASCII.GetString(readBuffer);

                if (data.Contains("ERROR")) throw new Exception(data);

                SystemLogEntry($"Connection Test Successful - {data}", false);
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
                //gagRadialMain.Headers[0].Text = $"{gaugeCommands.MainGauge.Name} ({gaugeCommands.MainGauge.UOM})";
                //gagRadialMain.Scales[0].Interval = gaugeCommands.MainGauge.TickSpacing;
                //gagRadialMain.Scales[0].EndValue = gaugeCommands.MainGauge.Max;
                //gagRadialMain.Scales[0].StartValue = gaugeCommands.MainGauge.Min;
                //gagRadialMain.Scales[0].Ranges[0].StartValue = gaugeCommands.MainGauge.Min;

                gagMainLabel.Text = gaugeCommands.MainGauge.Name;
                gagMainValue.Text = "0";
                gagMainUoM.Text = gaugeCommands.MainGauge.UOM;

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

        public async void RunGauges()
        {
            loopPid = true;
            UpdateLog("Started Looping...");
            while (loopPid)
                try
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    //await gaugeCommands.WriteSinglePID(gaugeCommands.MainGauge);
                    //gagRadialMain.Scales[0].Pointers[0].Value = gaugeCommands.MainGauge.GaugeValue;
                    //gagRadialMain.Scales[0].Ranges[0].EndValue = gaugeCommands.MainGauge.GaugeValue;

                    //Main gauge
                    await gaugeCommands.WriteSinglePID(gaugeCommands.MainGauge);
                    gagMainValue.Text = Convert.ToString(gaugeCommands.MainGauge.GaugeValue);
                    await gagMainProgBar.ProgressTo((gaugeCommands.MainGauge.GaugeValue / gaugeCommands.MainGauge.Max), 200, Easing.Linear);
                    if (gaugeCommands.MainGauge.GaugeValue >= gaugeCommands.MainGauge.Warning) gagMainFrame.BorderColor = gagMainLabel.TextColor = gagMainUoM.TextColor = gagMainValue.TextColor = ColorMaster.ColorCritical;
                    else gagMainFrame.BorderColor = gagMainLabel.TextColor = gagMainUoM.TextColor = gagMainValue.TextColor = ColorMaster.ColorPrimary;

                    //Box Gauge 1
                    await gaugeCommands.WriteSinglePID(gaugeCommands.BoxGauge1);
                    gagBox1Value.Text = Convert.ToString(gaugeCommands.BoxGauge1.GaugeValue);
                    if (gaugeCommands.BoxGauge1.GaugeValue >= gaugeCommands.BoxGauge1.Warning) gagBox1Frame.BorderColor = gagBox1Label.TextColor = gagBox1UoM.TextColor = gagBox1Value.TextColor = ColorMaster.ColorCritical;
                    else gagBox1Frame.BorderColor = gagBox1Label.TextColor = gagBox1UoM.TextColor = gagBox1Value.TextColor = ColorMaster.ColorPrimary;

                    //Box Gauge 2
                    await gaugeCommands.WriteSinglePID(gaugeCommands.BoxGauge2);
                    gagBox2Value.Text = Convert.ToString(gaugeCommands.BoxGauge2.GaugeValue);
                    if (gaugeCommands.BoxGauge2.GaugeValue >= gaugeCommands.BoxGauge2.Warning) gagBox2Frame.BorderColor = gagBox2Label.TextColor = gagBox2UoM.TextColor = gagBox2Value.TextColor = ColorMaster.ColorCritical;
                    else gagBox2Frame.BorderColor = gagBox2Label.TextColor = gagBox2UoM.TextColor = gagBox2Value.TextColor = ColorMaster.ColorPrimary;

                    //Box Gauge 3
                    await gaugeCommands.WriteSinglePID(gaugeCommands.BoxGauge3);
                    gagBox3Value.Text = Convert.ToString(gaugeCommands.BoxGauge3.GaugeValue);
                    if (gaugeCommands.BoxGauge3.GaugeValue >= gaugeCommands.BoxGauge3.Warning) gagBox3Frame.BorderColor = gagBox3Label.TextColor = gagBox3UoM.TextColor = gagBox3Value.TextColor = ColorMaster.ColorCritical;
                    else gagBox3Frame.BorderColor = gagBox3Label.TextColor = gagBox3UoM.TextColor = gagBox3Value.TextColor = ColorMaster.ColorPrimary;

                    //Box Gauge 4
                    await gaugeCommands.WriteSinglePID(gaugeCommands.BoxGauge4);
                    gagBox4Value.Text = Convert.ToString(gaugeCommands.BoxGauge4.GaugeValue);
                    if (gaugeCommands.BoxGauge4.GaugeValue >= gaugeCommands.BoxGauge4.Warning) gagBox4Frame.BorderColor = gagBox4Label.TextColor = gagBox4UoM.TextColor = gagBox4Value.TextColor = ColorMaster.ColorCritical;
                    else gagBox4Frame.BorderColor = gagBox4Label.TextColor = gagBox4UoM.TextColor = gagBox4Value.TextColor = ColorMaster.ColorPrimary;

                    sw.Stop();
                    UpdateLog($"-----Time to Execute: {Convert.ToString(sw.ElapsedMilliseconds)}");

                    if (failCount > 10)
                        loopPid = await TestConnection(false);
                }
                catch (Exception ex)
                {
                    FailedConnection($"Error at PID Loop - {ex.Message}");
                    loopPid = false;
                }

            ResetGauges();
            UpdateLog("...Stopped Looping");
        }

        public async void RunGaugesTEST()
        {
            loopPid = true;
            UpdateLog("Started Looping...");
            while (loopPid)
                try
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    await gaugeCommands.WriteStackPID();

                    //Main gauge
                    gagMainValue.Text = Convert.ToString(gaugeCommands.gauges[0].GaugeValue);
                    await gagMainProgBar.ProgressTo((gaugeCommands.gauges[0].GaugeValue / gaugeCommands.gauges[0].Max), 200, Easing.Linear);
                    if (gaugeCommands.gauges[0].GaugeValue >= gaugeCommands.gauges[0].Warning) gagMainFrame.BorderColor = gagMainLabel.TextColor = gagMainUoM.TextColor = gagMainValue.TextColor = ColorMaster.ColorCritical;
                    else gagMainFrame.BorderColor = gagMainLabel.TextColor = gagMainUoM.TextColor = gagMainValue.TextColor = ColorMaster.ColorPrimary;

                    //Box Gauge 1
                    gagBox1Value.Text = Convert.ToString(gaugeCommands.gauges[1].GaugeValue);
                    if (gaugeCommands.gauges[1].GaugeValue >= gaugeCommands.gauges[1].Warning) gagBox1Frame.BorderColor = gagBox1Label.TextColor = gagBox1UoM.TextColor = gagBox1Value.TextColor = ColorMaster.ColorCritical;
                    else gagBox1Frame.BorderColor = gagBox1Label.TextColor = gagBox1UoM.TextColor = gagBox1Value.TextColor = ColorMaster.ColorPrimary;

                    //Box Gauge 2
                    gagBox2Value.Text = Convert.ToString(gaugeCommands.gauges[2].GaugeValue);
                    if (gaugeCommands.gauges[2].GaugeValue >= gaugeCommands.gauges[2].Warning) gagBox2Frame.BorderColor = gagBox2Label.TextColor = gagBox2UoM.TextColor = gagBox2Value.TextColor = ColorMaster.ColorCritical;
                    else gagBox2Frame.BorderColor = gagBox2Label.TextColor = gagBox2UoM.TextColor = gagBox2Value.TextColor = ColorMaster.ColorPrimary;

                    //Box Gauge 3
                    gagBox3Value.Text = Convert.ToString(gaugeCommands.gauges[3].GaugeValue);
                    if (gaugeCommands.gauges[3].GaugeValue >= gaugeCommands.gauges[3].Warning) gagBox3Frame.BorderColor = gagBox3Label.TextColor = gagBox3UoM.TextColor = gagBox3Value.TextColor = ColorMaster.ColorCritical;
                    else gagBox3Frame.BorderColor = gagBox3Label.TextColor = gagBox3UoM.TextColor = gagBox3Value.TextColor = ColorMaster.ColorPrimary;

                    //Box Gauge 4
                    gagBox4Value.Text = Convert.ToString(gaugeCommands.gauges[4].GaugeValue);
                    if (gaugeCommands.gauges[4].GaugeValue >= gaugeCommands.gauges[4].Warning) gagBox4Frame.BorderColor = gagBox4Label.TextColor = gagBox4UoM.TextColor = gagBox4Value.TextColor = ColorMaster.ColorCritical;
                    else gagBox4Frame.BorderColor = gagBox4Label.TextColor = gagBox4UoM.TextColor = gagBox4Value.TextColor = ColorMaster.ColorPrimary;

                    sw.Stop();
                    UpdateLog($"-----Time to Execute: {Convert.ToString(sw.ElapsedMilliseconds)}");

                    if (failCount > 10)
                        loopPid = await TestConnection(false);
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
            //gagRadialMain.Scales[0].Pointers[0].Value = 0;
            gagMainValue.Text = "---";
            gagMainProgBar.Progress = 0;

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
                tbkBTStatus.TextColor = ColorMaster.ColorPrimary;
                SaveDevice();

                //Clear Logs on startup
                if (!SettingsControl.debugMode)
                {
                    log = String.Empty;
                    errorLog = String.Empty;
                }
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
                tbkBTStatus.TextColor = ColorMaster.ColorCritical;
                SystemLogEntry($"{error}", false);
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region PID Error Code Reading

        private async void GetErrorCodes()
        {
            tbkErrorCode.Text = "Retrieving trouble codes...";
            List<string> errorCodes = new List<string>();
            if (gaugeCommands != null)
            {
                errorCodes = await gaugeCommands.WriteTroubleRequest();

                if (errorCodes.Count < 1)
                    tbkErrorCode.Text = "<<<---No error codes--->>>";

                foreach (string s in errorCodes)
                {
                    tbkErrorCode.Text = tbkErrorCode.Text + $"--{s}\n\n";
                }
            }
            else
                tbkErrorCode.Text = "<<<--No Connection-->>";
        }

        private async void ClearErrorCodes()
        {
            if (gaugeCommands != null)
            {
                await gaugeCommands.ClearTroubleCodes();
                tbkErrorCode.Text = "---Ready---";
            }

            else
                tbkErrorCode.Text = "<<<--No Connection-->>";
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
            //IDK if this is doable or worth it
        }

        private void btnOBDLog_Clicked(object sender, EventArgs e)
        {
            //IDK if this is doable or worth it
        }

        private void btnErrorCodes_Clicked(object sender, EventArgs e)
        {
            OBDErrorCodePage.IsVisible = true;
            tbkErrorCode.Text = "---Ready---";
        }
        private void btnGetErrorCodes_Clicked(object sender, EventArgs e)
        {
            GetErrorCodes();
        }
        private void btnClearErrorCodePage_Clicked(object sender, EventArgs e)
        {
            ClearErrorCodes();
        }
        private void btnCloseErrorCodePage_Clicked(object sender, EventArgs e)
        {
            OBDErrorCodePage.IsVisible = false;
        }

        private async void btnThemeChange_Clicked(object sender, EventArgs e)
        {
            if (SettingsControl.saveSettings.theme == 0) ColorMaster.ChangeTheme(1);
            else if (SettingsControl.saveSettings.theme == 1) ColorMaster.ChangeTheme(2);
            else ColorMaster.ChangeTheme(0);

            await UpdateTheme();

            SystemLogEntry($"Theme changed to {SettingsControl.saveSettings.theme}", false);
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

        public async Task UpdateTheme()
        {
            gagMainFrame.BorderColor = ColorMaster.ColorPrimary;
            gagBox1Frame.BorderColor = ColorMaster.ColorPrimary;
            gagBox2Frame.BorderColor = ColorMaster.ColorPrimary;
            gagBox3Frame.BorderColor = ColorMaster.ColorPrimary;
            gagBox4Frame.BorderColor = ColorMaster.ColorPrimary;

            gagMainLabel.TextColor = ColorMaster.ColorPrimary;
            gagBox1Label.TextColor = ColorMaster.ColorPrimary;
            gagBox2Label.TextColor = ColorMaster.ColorPrimary;
            gagBox3Label.TextColor = ColorMaster.ColorPrimary;
            gagBox4Label.TextColor = ColorMaster.ColorPrimary;

            gagMainUoM.TextColor = ColorMaster.ColorPrimary;
            gagBox1UoM.TextColor = ColorMaster.ColorPrimary;
            gagBox2UoM.TextColor = ColorMaster.ColorPrimary;
            gagBox3UoM.TextColor = ColorMaster.ColorPrimary;
            gagBox4UoM.TextColor = ColorMaster.ColorPrimary;

            gagMainValue.TextColor = ColorMaster.ColorPrimary;
            gagBox1Value.TextColor = ColorMaster.ColorPrimary;
            gagBox2Value.TextColor = ColorMaster.ColorPrimary;
            gagBox3Value.TextColor = ColorMaster.ColorPrimary;
            gagBox4Value.TextColor = ColorMaster.ColorPrimary;

            imgIcon.Source = ColorMaster.MainImage.Source;
        }

        #endregion

        #region Old
        //public async void RunGauges1()
        //{
        //    loopPid = true;
        //    UpdateLog("Started Looping...");
        //    while (loopPid)
        //        try
        //        {
        //            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //            sw.Start();

        //            //Get Data
        //            //loopPid = await gaugeCommands.ExecutePIDs(); //- For Grouped execution of commands
        //            await gaugeCommands.ExecuteSinglePIDs();   //- For Single execution of commands

        //            sw.Stop();
        //            UpdateLog($"-----Time to Execute: {Convert.ToString(sw.ElapsedMilliseconds)}");

        //            //Update Gauge UI
        //            gagRadialMain.Scales[0].Pointers[0].Value = gaugeCommands.MainGauge.GaugeValue;
        //            gagBox1Value.Text = Convert.ToString(gaugeCommands.BoxGauge1.GaugeValue);
        //            gagBox2Value.Text = Convert.ToString(gaugeCommands.BoxGauge2.GaugeValue);
        //            gagBox3Value.Text = Convert.ToString(gaugeCommands.BoxGauge3.GaugeValue);
        //            gagBox4Value.Text = Convert.ToString(gaugeCommands.BoxGauge4.GaugeValue);

        //            if (failCount > 10)
        //                loopPid = await TestConnection();
        //        }
        //        catch (Exception ex)
        //        {
        //            FailedConnection($"Error at PID Loop - {ex.Message}");
        //            loopPid = false;
        //        }
        //    ResetGauges();
        //    UpdateLog("...Stopped Looping");
        //}

        #endregion
    }
}