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

        string savedDeviceID = "";
        string savedDeviceName = "";

        public static string log = "";
        bool deviceConnected = false;
        bool loopPid = true;

        public BluetoothControl()
        {
            InitializeComponent();
            LoadAdapter();
        }

        #region Bluetooth Device Setup
        public void LoadAdapter()
        {
            adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null)
                throw new Exception("No Bluetooth adapter found.");

            if (!adapter.IsEnabled)
                throw new Exception("Bluetooth adapter is not enabled.");
        }

        public void LoadDevices()
        {
            foreach (BluetoothDevice d in adapter.BondedDevices)
                pkrBluetoothPicker.Items.Add(d.Name);
        }

        public async void ConnectionHandling()
        {
            deviceConnected = await ConnectDevice();

            if (deviceConnected)
                deviceConnected = await ConnectSocket();
            if (deviceConnected)
                deviceConnected = await TestConnection();
            if (deviceConnected)
            {
                SuccessfulConnection();
                InitGauges();
            }


        }

        public async void ConnectSavedDevice()
        {
            try
            {
                device = (from bd in adapter.BondedDevices where bd.Name == savedDeviceName select bd).FirstOrDefault();
            }

            catch (Exception ex)
            {
                FailedConnection($"Saved Device not found - {ex.Message}");
            }
        }


        public async Task<bool> ConnectDevice()
        {
            try
            {
                if (pkrBluetoothPicker.SelectedIndex == -1)
                    throw new Exception("Device not found");
                else
                {
                    device = (from bd in adapter.BondedDevices where bd.Name == pkrBluetoothPicker.Items[pkrBluetoothPicker.SelectedIndex] select bd).FirstOrDefault();
                    return true;
                }
            }
            catch(Exception ex)
            {
                FailedConnection(ex.Message);
                return false;
            }
        }

        private async Task<bool> ConnectSocket()
        {
            try
            {
                if (device == null)
                    throw new Exception("Device not found");

                socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                await socket.ConnectAsync();

                gaugeCommands = new GaugeCommands(socket, 300, 300);

                return true;
            }
            catch (Exception ex)
            {
                FailedConnection(ex.Message);
                return false;
            }


            //// Read data from the device
            //await socket.InputStream.ReadAsync(buffer, 0, buffer.Length);

            //// Write data to the device
            //await socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        private void pkrBluetoothPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConnectionHandling();
        }

        private async Task<bool> TestConnection()
        {
            try
            {
                byte[] writeBuffer = Encoding.ASCII.GetBytes("ATZ\r");
                byte[] readBuffer = new byte[20];

                // Write data to the device
                await socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);

                // Read data from the device
                await socket.InputStream.ReadAsync(readBuffer, 0, readBuffer.Length);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
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
                tbkOBDDevice.Text = "None";
                tbkOBDStatus.Text = "No Connection";
                tbkBTStatus.Text = "No Connection";
                tbkBTStatus.TextColor = Color.OrangeRed;
                MainPage.SystemLogEntry(error);
            }
            catch (Exception)
            {

            }
        }

        #endregion

        #region Gauges

        public async void InitGauges()
        {
            SetupGauges();

            if (await TestGauges())
            {
                //RunGauges();
            }
            else
                FailedConnection("Gauges failed testing");
        }

        public void SetupGauges()
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
        }

        public async Task<bool> TestGauges()
        {
            bool gaugeTest = true;

            if (gaugeCommands == null)
                gaugeTest = false;

            if (gaugeTest)
                gaugeTest = await gaugeCommands.SetupCommands();

            return gaugeTest;
        }

        public async void RunGauges()
        {
            while (loopPid)
                try
                {
                    //Get Data
                    bool success = await gaugeCommands.ExecutePIDs();

                    //Update Gauge UI
                    gagRadialMain.Scales[0].Pointers[0].Value = gaugeCommands.MainGauge.GaugeValue;
                    gagRadial1.Scales[0].Pointers[0].Value = gaugeCommands.RadialGauge1.GaugeValue;
                    gagBox1Value.Text = Convert.ToString(gaugeCommands.BoxGauge1.GaugeValue);

                }
                catch (Exception ex)
                {
                    FailedConnection($"Error at PID Loop - {ex.Message}");
                }

            MainPage.SystemLogEntry($"Stopped Looping");
        }

        public async void StopGauges()
        {

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
        }

        private void btnCloseBTMenuOptions_Clicked(object sender, EventArgs e)
        {
            BTMenuOptions.IsVisible = false;
        }

        #endregion
    }
}