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
        GaugeCommands commands;

        string savedDeviceID;
        string savedDeviceName;

        public static string log = "";
        
        public BluetoothControl()
        {
            InitializeComponent();
        }

        private async void BluetoothSetup(bool newDevice)
        {
            try
            {
                adapter = BluetoothAdapter.DefaultAdapter;
                if (adapter == null)
                    throw new Exception("No Bluetooth adapter found.");

                if (!adapter.IsEnabled)
                    throw new Exception("Bluetooth adapter is not enabled.");

                Picker devicePicker = new Picker();
                devicePicker.Title = "Bluetooth Devices";
                foreach(BluetoothDevice d in adapter.BondedDevices)
                    devicePicker.Items.Add(d.Name);

                devicePicker.SelectedIndexChanged += (sender, args) =>
                {
                    
                    if (devicePicker.SelectedIndex == -1)
                        return;
                    else
                        device = (from bd in adapter.BondedDevices where bd.Name == devicePicker.Items[devicePicker.SelectedIndex] select bd).FirstOrDefault();
                };
            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
            }

            try
            {
                if (device == null)
                    throw new Exception("Named device not found.");

                var socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
                await socket.ConnectAsync();

            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
            }






            //// Read data from the device
            //await _socket.InputStream.ReadAsync(buffer, 0, buffer.Length);

            //// Write data to the device
            //await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

    }
}