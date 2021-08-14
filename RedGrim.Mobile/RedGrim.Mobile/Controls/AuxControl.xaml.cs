using Android.Bluetooth;
using Java.Util;
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
    public partial class AuxControl : ContentView
    {
        BluetoothAdapter adapter;
        BluetoothSocket socket;
        BluetoothDevice device;

        string auxDeviceName = "HC05";
        bool connected = false;

        public AuxControl()
        {
            InitializeComponent();
        }

        #region Aux Bluetooth Connection
        private void btnAuxConnect_Clicked(object sender, EventArgs e)
        {
            tbkAuxStatus.Text = "Connecting...";

            if (!connected) ConnectSavedDevice();
            else TestConnection();
        }

        public async void ConnectSavedDevice()
        {
            try
            {
                //Adapter
                adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null)
                    throw new Exception("Bluetooth adapter not found.");

                if (!adapter.IsEnabled)
                    throw new Exception("Bluetooth adapter is not enabled.");

                //Device
                device = (from bd in adapter.BondedDevices where bd.Name == auxDeviceName select bd).FirstOrDefault();
                if (device == null) throw new Exception($"Aux device ({auxDeviceName}) not found");

                //Socket
                socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                await socket.ConnectAsync();

                SuccessfulConnection();
            }

            catch (Exception ex)
            {
                FailedAuxConnection(ex.Message);
            }
        }

        public async void TestConnection()
        {
            try
            {
                // Write data to the device
                byte[] writeBuffer = Encoding.ASCII.GetBytes("");
                await socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                await socket.OutputStream.FlushAsync();
            }
            catch (Exception ex)
            {
                FailedAuxConnection(ex.Message);
            }
        }

        private async void SuccessfulConnection()
        {
            try
            {
                connected = true;
                tbkAuxStatus.Text = "Connected";
                tbkAuxStatus.TextColor = Color.Magenta;
                BluetoothControl.SystemLogEntry($"AUX - Connected Successfully", false);
            }
            catch (Exception ex)
            {
                FailedAuxConnection(ex.Message);
            }
        }

        private void FailedAuxConnection(string error)
        {
            connected = false;
            if (device != null) device.Dispose();
            if (socket != null) socket.Close();

            tbkAuxStatus.Text = "No Connection";
            tbkAuxStatus.TextColor = Color.OrangeRed;
            BluetoothControl.SystemLogEntry($"AUX - {error}", false);
        }

        private async void SendCommand(string input)
        {
            try
            {
                // Write data to the device
                byte[] writeBuffer = Encoding.ASCII.GetBytes(input);
                await socket.OutputStream.WriteAsync(writeBuffer, 0, writeBuffer.Length);
                await socket.OutputStream.FlushAsync();
            }
            catch(Exception ex)
            {
                TestConnection();
            }
        }

        #endregion


        #region ON/OFF Buttons
        //ON
        private void btnAux1On_Clicked(object sender, EventArgs e)
        {
            btnAux1On.IsVisible = false;
            btnAux1Off.IsVisible = true;
            bdr1.BorderColor = Color.OrangeRed;
            if (connected) SendCommand("101");
        }

        private void btnAux2On_Clicked(object sender, EventArgs e)
        {
            btnAux2On.IsVisible = false;
            btnAux2Off.IsVisible = true;
            bdr2.BorderColor = Color.OrangeRed;
            if (connected) SendCommand("201");
        }

        private void btnAux3On_Clicked(object sender, EventArgs e)
        {
            btnAux3On.IsVisible = false;
            btnAux3Off.IsVisible = true;
            bdr3.BorderColor = Color.OrangeRed;
            if (connected) SendCommand("301");
        }

        private void btnAux4On_Clicked(object sender, EventArgs e)
        {
            btnAux4On.IsVisible = false;
            btnAux4Off.IsVisible = true;
            bdr4.BorderColor = Color.OrangeRed;
            if (connected) SendCommand("401");
        }

        //OFF
        private void btnAux1Off_Clicked(object sender, EventArgs e)
        {
            btnAux1Off.IsVisible = false;
            btnAux1On.IsVisible = true;
            bdr1.BorderColor = Color.LightGray;
            if (connected) SendCommand("100");
        }

        private void btnAux2Off_Clicked(object sender, EventArgs e)
        {
            btnAux2Off.IsVisible = false;
            btnAux2On.IsVisible = true;
            bdr2.BorderColor = Color.LightGray;
            if (connected) SendCommand("200");
        }

        private void btnAux3Off_Clicked(object sender, EventArgs e)
        {
            btnAux3Off.IsVisible = false;
            btnAux3On.IsVisible = true;
            bdr3.BorderColor = Color.LightGray;
            if (connected) SendCommand("300");
        }

        private void btnAux4Off_Clicked(object sender, EventArgs e)
        {
            btnAux4Off.IsVisible = false;
            btnAux4On.IsVisible = true;
            bdr4.BorderColor = Color.LightGray;
            if (connected) SendCommand("400");
        }

        #endregion



    }
}