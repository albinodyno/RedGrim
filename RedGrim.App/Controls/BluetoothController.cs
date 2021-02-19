using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RedGrim.App.Controls
{
    class BluetoothController
    {
        static TextBlock tbkBTStatus = new TextBlock();
        static TextBlock tbkOBDDevice = new TextBlock();

        static BluetoothDevice btDevice;
        static StreamSocket streamSocket;
        static DataWriter obdWriter;

        static bool obdServiceOn = false;
        static bool obdSocketOn = false;

        public static async void BluetoothOBDSetup()
        {
            TimeSpan avgTime;
            var bounds = Window.Current.Bounds;
            double height = bounds.Height;
            double width = bounds.Width;
   
            try
            {
                DevicePicker devicePicker = new DevicePicker();
                devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothDevice.GetDeviceSelectorFromPairingState(false));
                devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothDevice.GetDeviceSelectorFromPairingState(true));

                DeviceInformation device = await devicePicker.PickSingleDeviceAsync(new Rect(width * .5, height * .5, 0, 0));

                if (device != null)
                {
                    tbkOBDDevice.Text = $"{device.Name} : {device.Id}";
                }

                btDevice = await BluetoothDevice.FromIdAsync(device.Id);
                tbkBTStatus.Text = "Pairing...";
                DevicePairingResult pair = await btDevice.DeviceInformation.Pairing.PairAsync();

                if (btDevice.DeviceInformation.Pairing.IsPaired)
                {
                    tbkBTStatus.Text = "Paired Successfully";
                    RfcommDeviceServicesResult rfServices = await btDevice.GetRfcommServicesAsync();
                    RfcommDeviceService service = rfServices.Services[0];
                    if (service != null)
                    {
                        tbkBTStatus.Text = "Retrieved OBD Services";
                        streamSocket = new StreamSocket();
                        await streamSocket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication).AsTask();
                        //service.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                obdServiceOn = false;
                obdSocketOn = false;
            }
        }

        private async void SendOBD(string input)
        {
            try
            {
                if (obdServiceOn && obdSocketOn)
                {
                    obdWriter = new DataWriter(streamSocket.OutputStream);
                    obdWriter.WriteString(input + "\r");
                    await obdWriter.StoreAsync().AsTask();
                    await obdWriter.FlushAsync().AsTask();
                    obdWriter.DetachStream();
                    obdWriter.Dispose();

                    tbkOBDDevice.Text = await ReadOBD();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task<string> ReadOBD()
        {
            #region Debugging/Time Stuff
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            #endregion

            bool looping = true;
            string input = "";
            int timeout = 5000;
            DataReader obdReader = new DataReader(streamSocket.InputStream);

            try
            {
                while (looping)
                {
                    var task = obdReader.LoadAsync(1).AsTask();
                    if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
                    {
                        byte[] bufferRead = new byte[obdReader.UnconsumedBufferLength];
                        obdReader.ReadBytes(bufferRead);
                        obdReader.DetachStream();
                        input += Encoding.UTF8.GetString(bufferRead);
                        obdReader.Dispose();

                        if (input.Substring(input.Length - 1) == ">" || input.Substring(input.Length - 3) == "..." && input.Length > 10)
                        {
                            looping = false;
                        }
                    }
                    else
                    {
                        obdReader.Dispose();
                        looping = false;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            #region Debugging/Time Stuff
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            tbkBTStatus.Text = ts.TotalMilliseconds.ToString();
            #endregion

            return input;
        }

    }
}
