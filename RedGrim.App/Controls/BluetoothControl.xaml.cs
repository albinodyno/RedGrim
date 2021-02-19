using RedGrim.App.Models;
using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace RedGrim.App.Controls
{
    public sealed partial class BluetoothControl : UserControl
    {
        StreamSocket streamSocket;
        BluetoothDevice btDevice;
        GaugeCommands commands;

        string savedDeviceID;
        string savedDeviceName;
        
        public static string log = "";
        bool loopPid = true;

        public BluetoothControl()
        {
            this.InitializeComponent();
        }

        public void LoadBTSettings()
        {
            savedDeviceID = SettingsController.SavedSettings.btDeviceID;
            savedDeviceName = SettingsController.SavedSettings.btDeviceName;
            if (SettingsController.SavedSettings.btDeviceID != "")
                savedDeviceID = SettingsController.SavedSettings.btDeviceID;
        }

        public async void ConnectBTOBD(bool newDevice)
        {
            var bounds = Window.Current.Bounds;
            double height = bounds.Height;
            double width = bounds.Width;

            try
            {
                DevicePicker devicePicker = new DevicePicker();
                devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothDevice.GetDeviceSelectorFromPairingState(!newDevice));
                DeviceInformation device = await devicePicker.PickSingleDeviceAsync(new Rect(width * .5, height * .5, 0, 0));

                if (device == null)
                    return;

                tbkStatus.Text = $"Status : Connecting to {device.Name}...";
                btDevice = await BluetoothDevice.FromIdAsync(device.Id);
                DevicePairingResult pair = await btDevice.DeviceInformation.Pairing.PairAsync();

                BTSetup();
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                UnsuccessfulConnection(ex.Message);
                DisconnectBluetooth();
            }
        }

        public async void ConnectSavedBTOBD()
        {
            if (savedDeviceID == null || savedDeviceID == "")
                return;

            try
            {
                tbkStatus.Text = "Status : Connecting to last device...";
                btDevice = await BluetoothDevice.FromIdAsync(savedDeviceID);
                //btDevice.ConnectionStatusChanged += BtDevice_ConnectionStatusChanged;

                if (btDevice.DeviceInformation.Pairing.IsPaired)
                    BTSetup();
                else
                    UnsuccessfulConnection($"Bluetooth device not found - {btDevice.DeviceInformation.Name}");
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                UnsuccessfulConnection(ex.Message);
                DisconnectBluetooth();
            }
        }

        private async void BTSetup()
        {
            try
            {
                RfcommDeviceServicesResult rfServices = await btDevice.GetRfcommServicesAsync();
                RfcommDeviceService service = rfServices.Services[0];

                if (service == null)
                    throw new Exception();

                streamSocket = new StreamSocket();
                await streamSocket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);

                commands = new GaugeCommands(streamSocket, SettingsController.SavedSettings.elmDelay, SettingsController.SavedSettings.pidDelay);

                SaveBTDevice();
                SuccessfulConnection();
                service.Dispose();
            }
            catch (UnauthorizedAccessException ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                UnsuccessfulConnection(ex.Message);
                DisconnectBluetooth();
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                UnsuccessfulConnection(ex.Message);
            }
        }

        private void SuccessfulConnection()
        {
            try
            {
                tbkOBDDevice.Text = btDevice.Name;
                tbkOBDStatus.Text = "Connected";
                tbkStatus.Text = "Connected";
                tbkStatus.Foreground = new SolidColorBrush(Colors.DarkCyan);
                StartGauges();
            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                UnsuccessfulConnection(ex.Message);
            }
        }

        private async void UnsuccessfulConnection(string status)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => 
                {
                    if(btDevice != null)
                        tbkOBDStatus.Text = $"{btDevice.Name} - {status}";
                    else
                        tbkOBDStatus.Text = $"{status}";

                    tbkOBDDevice.Text = "None";
                    tbkStatus.Text = "Disconnected";
                    tbkStatus.Foreground = new SolidColorBrush(Colors.OrangeRed);

                    MainPage.SystemLogEntry(status);
                    //pnlErrorCodes.Visibility = Visibility.Collapsed;
                    //ResetGauges();
                });
        }

        private void DisconnectBluetooth()
        {
            if (btDevice != null)
                btDevice.Dispose();

            btDevice = null;
            streamSocket = null;
            //write code to clear and save settings for saved bt device
        }

        #region Gauges
        public async void StartGauges()
        {
            if (commands == null)
                loopPid = false;

            if (loopPid)
                loopPid = await commands.SetupCommands();

            if (loopPid)
                SetupGauges();    

            while(loopPid)
                try
                {
                    bool success = await commands.ExecutePIDs();

                    //await UpdateGauges();
                    gagTbk1.Text = Convert.ToString(commands.botLeft.GaugeValue);
                    gagTbk2.Text = Convert.ToString(commands.botRight.GaugeValue);
                    gagMainGauge.Value = commands.mainGauge.GaugeValue;
                }
                catch (Exception ex)
                {
                    UnsuccessfulConnection($"Error at PID Loop - {ex.Message}");
                }

            MainPage.SystemLogEntry($"Stopped Looping");      
        }

        //public async Task<bool> UpdateGauges()
        //{
        //    try
        //    {
        //        gagTbk1.Text = Convert.ToString(commands.botLeft.GaugeValue);
        //        gagTbk2.Text = Convert.ToString(commands.botRight.GaugeValue);
        //        gagMainGauge.Value = commands.mainGauge.GaugeValue;

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MainPage.SystemLogEntry(ex.Message);
        //        return true;
        //    }
        //}

        public void SetupGauges()
        {
            gagTbk1Name.Text = commands.botLeft.Name;
            gagTbk1Uom.Text = commands.botLeft.UOM;

            gagTbk2Name.Text = commands.botRight.Name;
            gagTbk2Uom.Text = commands.botRight.UOM;

            gagMainGauge.Unit = $"{commands.mainGauge.Name} ({commands.mainGauge.UOM})";
            gagMainGauge.TickSpacing = commands.mainGauge.TickSpacing;
            gagMainGauge.Maximum = commands.mainGauge.Max;
        }
        #endregion

        #region Misc
        public void ToggleBTMenu()
        {
            if (pnlBTSettings.Visibility == Visibility.Collapsed)
                pnlBTSettings.Visibility = Visibility.Visible;
            else
                pnlBTSettings.Visibility = Visibility.Collapsed;
        }

        public void SaveBTDevice()
        {
            SettingsController.SavedSettings.btDeviceID = btDevice.DeviceId;
            SettingsController.SavedSettings.btDeviceName = btDevice.Name;
            SettingsController.SaveJson();
        }
        #endregion

        #region Buttons
        private void btnPairNewDevice_Click(object sender, RoutedEventArgs e)
        {
            ConnectBTOBD(true);
        }

        private void btnConnectDevice_Click(object sender, RoutedEventArgs e)
        {
            ConnectBTOBD(false);
        }

        private void btnDisconnectDevice_Click(object sender, RoutedEventArgs e)
        {
            DisconnectBluetooth();
            UnsuccessfulConnection("Disconnected");
        }

        private void btnStartGauges_Click(object sender, RoutedEventArgs e)
        {
            StartGauges();
        }
        private void btnStopGauges_Click(object sender, RoutedEventArgs e)
        {
            loopPid = false;
        }
        #endregion

        #region Engine Codes (Not Ready)
        //READ AND CLEAR ERROR CODES

        private void btnErrorCodes_Click(object sender, RoutedEventArgs e)
        {
            loopPid = false;
            pnlErrorCodes.Visibility = Visibility.Visible;
        }

        private void btnCloseErrorLog_Click(object sender, RoutedEventArgs e)
        {

            loopPid = true;
            StartGauges();
            pnlErrorCodes.Visibility = Visibility.Collapsed;
        }

        private void btnReadCodes_Click(object sender, RoutedEventArgs e)
        {
            tbkErrorCodes.Text = tbkErrorCodes.Text + "Working on this dingus\n";
        }

        private void btnClearCodes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnClearErrLog_Click(object sender, RoutedEventArgs e)
        {
            tbkErrorCodes.Text = string.Empty;
        }
        #endregion

        #region Old Stuff
        //OLLLLDDDD STUFFFFF

        //OLD VARIABLES
        //string result = "";
        //string s = "";
        //uint readBuffer;
        //int pidDelay = 50;
        //DataWriter obdWriter;
        //DataReader obdReader;

        //public async void ConnectBTOBD(bool newDevice)
        //{
        //    var bounds = Window.Current.Bounds;
        //    double height = bounds.Height;
        //    double width = bounds.Width;

        //    try
        //    {
        //        DevicePicker devicePicker = new DevicePicker();
        //        devicePicker.Filter.SupportedDeviceSelectors.Add(BluetoothDevice.GetDeviceSelectorFromPairingState(!newDevice));
        //        DeviceInformation device = await devicePicker.PickSingleDeviceAsync(new Rect(width * .5, height * .5, 0, 0));

        //        if (device == null)
        //            return;

        //        tbkStatus.Text = $"Status : Connecting to {device.Name}...";
        //        btDevice = await BluetoothDevice.FromIdAsync(device.Id);
        //        DevicePairingResult pair = await btDevice.DeviceInformation.Pairing.PairAsync();

        //        BTSetup();

        //        //RfcommDeviceServicesResult rfServices = await btDevice.GetRfcommServicesAsync();

        //        //if (rfServices.Services.Count > 0)
        //        //{
        //        //    //RfcommDeviceService service = rfServices.Services[0];
        //        //    //streamSocket = new StreamSocket();
        //        //    //await streamSocket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
        //        //    //commands = new OBDCommands(streamSocket);
        //        //    //SuccessfulConnection();
        //        //    //SaveBTDevice();
        //        //    //service.Dispose();
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        UnsuccessfulConnection(ex.Message);
        //        DisconnectBluetooth();
        //    }
        //}



        //public async void ConnectSavedBTOBD()
        //{
        //    if (savedDeviceID == null || savedDeviceID == "")
        //        return;

        //    try
        //    {
        //        tbkStatus.Text = "Status : Connecting to last device...";
        //        btDevice = await BluetoothDevice.FromIdAsync(savedDeviceID);
        //        //btDevice.ConnectionStatusChanged += BtDevice_ConnectionStatusChanged;

        //        if (btDevice.DeviceInformation.Pairing.IsPaired)
        //        {
        //            BTSetup();

        //            //RfcommDeviceServicesResult rfServices = await btDevice.GetRfcommServicesAsync();
        //            //RfcommDeviceService service = rfServices.Services[0];

        //            //if (service == null)
        //            //    throw new Exception();

        //            //streamSocket = new StreamSocket();
        //            //await streamSocket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);
        //            ////await streamSocket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName, SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication).AsTask();

        //            //commands = new OBDCommands(streamSocket);

        //            //SuccessfulConnection();
        //            //service.Dispose();
        //        }
        //        else
        //            UnsuccessfulConnection($"Bluetooth device not found - {btDevice.DeviceInformation.Name}");
        //    }
        //    catch (Exception ex)
        //    {
        //        UnsuccessfulConnection(ex.Message);
        //        DisconnectBluetooth();
        //    }
        //}

        //public async Task<bool> SetupCommands()
        //{
        //    try
        //    {
        //        //SETUP

        //        obdWriter.WriteString("ATZ\r");
        //        await obdWriter.StoreAsync();
        //        await obdWriter.FlushAsync();
        //        Thread.Sleep(1000);

        //        obdWriter.WriteString("ATL0\r");
        //        await obdWriter.StoreAsync();
        //        await obdWriter.FlushAsync();
        //        Thread.Sleep(500);

        //        obdWriter.WriteString("ATH0\r");
        //        await obdWriter.StoreAsync();
        //        await obdWriter.FlushAsync();
        //        Thread.Sleep(500);

        //        obdWriter.WriteString("ATS0\r");
        //        await obdWriter.StoreAsync();
        //        await obdWriter.FlushAsync();
        //        Thread.Sleep(500);

        //        obdWriter.WriteString("ATSP6\r");
        //        await obdWriter.StoreAsync();
        //        await obdWriter.FlushAsync();
        //        Thread.Sleep(500);

        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //public async Task<bool> ReadSetup()
        //{
        //    try
        //    {
        //        readBuffer = await obdReader.LoadAsync(512);
        //        //Task.Run(() => obdReader.LoadAsync(512)).Wait();
        //        s = obdReader.ReadString(readBuffer);
        //        log = log + s;
        //        s = String.Empty;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //public async Task PIDLoop()
        //{
        //    try
        //    {
        //        bool looping = true;
        //        bool completed;
        //        //LOOP Commands
        //        while (looping)
        //        {
        //            await WritePID("010C\r");
        //            completed = await ReadRPM();
        //            await WritePID("0104\r");
        //            completed = await ReadEngineLoad();
        //            //await WritePID("0111\r");
        //            //completed = await ReadThrottle();
        //            await WritePID("0105\r");
        //            completed = await ReadCoolantTemp();
        //            //await WritePID("010F\r");
        //            //completed = await ReadIntakeTemp();
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        tbkOBDStatus.Text = $"Error in PID Loop: {ex.Message}";
        //        return;
        //    }
        //}

        //public async Task WritePID(string input)
        //{
        //    obdWriter.WriteString(input);
        //    await obdWriter.StoreAsync();
        //    await obdWriter.FlushAsync();
        //    await Task.Delay(pidDelay);
        //}

        //async Task<bool> ReadRPM()
        //{
        //    try
        //    {
        //        readBuffer = await obdReader.LoadAsync(512);
        //        result = obdReader.ReadString(readBuffer);
        //        log = log + result;
        //        string result2 = result.Substring(9, 4);
        //        int value = int.Parse(result2, System.Globalization.NumberStyles.HexNumber);
        //        value = value / 4;

        //        if (value < 200 || value > 5000)
        //        {

        //        }
        //        else
        //        {
        //            gagSm1.Value = value;
        //        }

        //        await Task.Delay(pidDelay);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //async Task<bool> ReadEngineLoad()
        //{
        //    try
        //    {
        //        readBuffer = await obdReader.LoadAsync(512);
        //        result = obdReader.ReadString(readBuffer);
        //        log = log + result;
        //        string result2 = result.Substring(9, 4);
        //        double value = int.Parse(result2, System.Globalization.NumberStyles.HexNumber);
        //        value = Math.Round((value / 2.55), 1);
        //        gagMainGauge.Value = value;
        //        await Task.Delay(pidDelay);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //async Task<bool> ReadThrottle()
        //{
        //    try
        //    {
        //        readBuffer = await obdReader.LoadAsync(512);
        //        result = obdReader.ReadString(readBuffer);
        //        log = log + result;
        //        string result2 = result.Substring(9, 4);
        //        double value = int.Parse(result2, System.Globalization.NumberStyles.HexNumber);
        //        value = value / 2.55;
        //        gagSm1.Value = value;
        //        await Task.Delay(pidDelay);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //async Task<bool> ReadCoolantTemp()
        //{
        //    try
        //    {
        //        readBuffer = await obdReader.LoadAsync(512);
        //        result = obdReader.ReadString(readBuffer);
        //        log = log + result;
        //        string result2 = result.Substring(9, 4);
        //        double value = int.Parse(result2, System.Globalization.NumberStyles.HexNumber);
        //        value = ((value - 40) * 1.8) + 32;
        //        gagTbk1.Text = Convert.ToString(value);
        //        await Task.Delay(pidDelay);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        //async Task<bool> ReadIntakeTemp()
        //{
        //    try
        //    {
        //        readBuffer = await obdReader.LoadAsync(512);
        //        result = obdReader.ReadString(readBuffer);
        //        log = log + result;
        //        string result2 = result.Substring(9, 4);
        //        double value = int.Parse(result2, System.Globalization.NumberStyles.HexNumber);
        //        value = ((value - 40) * 1.8) + 32;
        //        gagTbk2.Text = Convert.ToString(value);
        //        await Task.Delay(pidDelay);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}


        //async Task<bool> ReadRPM()
        //{
        //    DataReader rpmReader = new DataReader(streamSocket.InputStream);   
        //    rpmReader.InputStreamOptions = InputStreamOptions.Partial;

        //    try
        //    {
        //        readBuffer = await rpmReader.LoadAsync(512);
        //        result = rpmReader.ReadString(readBuffer);
        //        log = log + result;
        //        string result2 = result.Substring(9,4);
        //        int value = int.Parse(result2, System.Globalization.NumberStyles.HexNumber);
        //        value = value / 4;
        //        gagSm1.Value = value;
        //    }
        //    catch(Exception ex)
        //    {
        //        return false;
        //    }
        //    return true;

        //async Task ReadRPMold()
        //{
        //    try
        //    {
        //        uint buffer = await obdReader.LoadAsync(4);
        //        result = obdReader.ReadString(4);
        //        log = log + result;

        //        int value = int.Parse(result, System.Globalization.NumberStyles.HexNumber);
        //        value = value / 4;

        //        if (value != 0 && value < 300)
        //            return;

        //        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        //            () =>
        //            {
        //                gagSm1.Value = value;
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        return;
        //    }
        //}
        //}
        //async Task ReadOBD8()
        //{
        //    bool looping = true;
        //    while (looping)
        //    {
        //        try
        //        {
        //            readBuffer = await obdReader.LoadAsync(1);
        //            s = obdReader.ReadString(1);
        //            log = log + s;

        //            if (s == "\\r")
        //            {
        //                readBuffer = await obdReader.LoadAsync(4);
        //                s = obdReader.ReadString(4);
        //                log = log + s;

        //                switch (s)
        //                {
        //                    //case "4104":
        //                    //    Task REL = ReadEngineLoad();
        //                    //    await REL;
        //                    //    looping = false;
        //                    //    break;
        //                    case "410C":
        //                        await ReadRPM();
        //                        looping = false;
        //                        break;
        //                    //case "4111":
        //                    //    Task RTH = ReadThrottle();
        //                    //    await RTH;
        //                    //    break;
        //                    //case "4105":
        //                    //    Task RCT = ReadCoolantTemp();
        //                    //    await RCT;
        //                    //    looping = false;
        //                    //    break;
        //                    //case "410F":
        //                    //    Task RIT = ReadIntakeTemp();
        //                    //    await RIT;
        //                    //    looping = false;
        //                    //    break;
        //                    default:
        //                        //obdReader.DetachBuffer();
        //                        break;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            obdReader.DetachBuffer();
        //        }
        //    }
        //}

        //async void ReadOBD7()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            uint buffer;
        //            buffer = await obdReader.LoadAsync(1);
        //            s = obdReader.ReadString(1);
        //            log = log + s;

        //            if (s == "\n")
        //            {
        //                buffer = await obdReader.LoadAsync(4);
        //                s = obdReader.ReadString(4);
        //                log = log + s;

        //                switch (s)
        //                {
        //                    //case "4104":
        //                    //    Task REL = ReadEngineLoad();
        //                    //    await REL;
        //                    //    break;
        //                    case "410C":
        //                        Task RPM = ReadRPM();
        //                        await ReadRPM();
        //                        break;
        //                    //case "4111":
        //                    //    ReadThrottle();
        //                    //    break;
        //                    //case "4105":
        //                    //    ReadCoolantTemp();
        //                    //    break;
        //                    //case "410F":
        //                    //    ReadIntakeTemp();
        //                    //    break;
        //                    default:
        //                        //obdReader.DetachBuffer();
        //                        break;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            obdReader.DetachBuffer();
        //        }
        //        await Task.Delay(50);
        //    }
        //}

        //async void ReadOBD6()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            uint buffer;
        //            buffer = await obdReader.LoadAsync(1);
        //            s = obdReader.ReadString(1);
        //            log = log + s;

        //            if (s == ">")
        //            {
        //                buffer = await obdReader.LoadAsync(5);
        //                s = obdReader.ReadString(5);
        //                log = log + s;

        //                switch (s)
        //                {
        //                    case "0104\r":
        //                        ReadEngineLoad();
        //                        break;
        //                    case "010C\r":
        //                        ReadRPM();
        //                        break;
        //                    case "0111\r":
        //                        ReadThrottle();
        //                        break;
        //                    case "0105\r":
        //                        ReadCoolantTemp();
        //                        break;
        //                    case "010F\r":
        //                        ReadIntakeTemp();
        //                        break;
        //                    default:
        //                        //obdReader.DetachBuffer();
        //                        break;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            //obdReader.DetachBuffer();
        //        }
        //    }
        //}

        //async void ReadOBD5()
        //{
        //    while (true)
        //    {
        //    uint buffer;
        //        try
        //        {
        //            buffer = await obdReader.LoadAsync(1);
        //                s = obdReader.ReadString(1);

        //                if (s != ">")
        //                {
        //                    if (s.Equals("\n") || s.Equals("\r"))
        //                    {
        //                        if (result.Length > 3 && result.Substring(0, 4) == "41 0")
        //                            Parse(result);

        //                        result = "";
        //                    }
        //                    else
        //                        result = result + s;
        //                }   
        //        }
        //        catch (Exception ex)
        //        {
        //            obdReader.DetachBuffer();
        //        }
        //    }
        //}


        //private async Task<string> ReadOBD()
        //{
        //    bool looping = true;
        //    string input = "";
        //    int timeout = 5000;

        //    try
        //    {
        //        while (looping)
        //        {
        //            var task = obdReader.LoadAsync(1).AsTask();
        //            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
        //            {
        //                byte[] bufferRead = new byte[obdReader.UnconsumedBufferLength];
        //                obdReader.ReadBytes(bufferRead);
        //                obdReader.DetachStream();
        //                input += Encoding.UTF8.GetString(bufferRead);
        //                obdReader.Dispose();

        //                if (input.Substring(input.Length - 1) == ">" || input.Substring(input.Length - 3) == "..." && input.Length > 10)
        //                {
        //                    looping = false;
        //                }
        //            }
        //            else
        //            {
        //                obdReader.Dispose();
        //                looping = false;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return input;
        //}
        //private async void LoopRPM()
        //{
        //    Task t = Task.Run(async () =>
        //    {
        //        DataWriter RPMWriter = new DataWriter(streamSocket.OutputStream);
        //        DataReader RPMReader = new DataReader(streamSocket.InputStream);
        //        string output;
        //        while (obdReady)
        //        {
        //            //Send Command
        //            RPMWriter.WriteString("010C\r");
        //            await RPMWriter.StoreAsync().AsTask();
        //            //await RPMWriter.FlushAsync().AsTask();

        //            //Read Response
        //            output = "";
        //            bool looping = true;
        //            while(looping)
        //            {
        //                var task = RPMReader.LoadAsync(1).AsTask();
        //                if (await Task.WhenAny(task, Task.Delay(5000)) == task)
        //                {
        //                    byte[] bufferRead = new byte[obdReader.UnconsumedBufferLength];
        //                    RPMReader.ReadBytes(bufferRead);
        //                    RPMReader.DetachStream();
        //                    output += Encoding.UTF8.GetString(bufferRead);
        //                    RPMReader.Dispose();

        //                    //if (output.Substring(output.Length - 1) == ">" || output.Substring(output.Length - 3) == "..." && output.Length > 10)
        //                    if(output.Length > 11)
        //                    {
        //                        looping = false;
        //                    }
        //                }
        //                else
        //                {
        //                    RPMReader.Dispose();
        //                }
        //            }

        //            //Convert Value
        //            int value = int.Parse(output.Substring(6), System.Globalization.NumberStyles.HexNumber);
        //            value = value / 4;

        //            gagRPM.Value = Convert.ToInt32(value);
        //        }

        //        RPMWriter.DetachStream();
        //        RPMWriter.Dispose();
        //    });
        //}

        //private void Parse(string input)
        //{
        //    try
        //    {
        //        string value = input.Replace(" ", "");
        //        string gaugeId = value.Substring(0,4);

        //        switch(gaugeId)
        //        {
        //            case "410C":
        //                UpdateRPM(value.Substring(4));
        //                break;
        //            case "4105":
        //                UpdateCoolant(value.Substring(4));
        //                break;
        //            case "410F":
        //                UpdateIntake(value.Substring(4));
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        return;
        //    }
        //}

        //private async void UpdateRPM(string input)
        //{
        //    try
        //    {
        //        int value = int.Parse(input, System.Globalization.NumberStyles.HexNumber);
        //        value = value / 4;
        //        //if (value < 200)
        //        //    return;

        //        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        //            () =>
        //            {
        //                gagSm1.Value = value;
        //            });
        //    }
        //    catch(Exception ex)
        //    {
        //        return;
        //    }
        //}

        //private async void UpdateCoolant(string input)
        //{
        //    try
        //    {
        //        int value = int.Parse(input, System.Globalization.NumberStyles.HexNumber);
        //        value = value - 40;
        //        value = (value * (9 / 5)) + 32;

        //        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        //            () =>
        //            {
        //                gagTbk1.Text = Convert.ToString(value) + " °F";
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        return;
        //    }
        //}

        //private async void UpdateIntake(string input)
        //{
        //    try
        //    {
        //        int value = int.Parse(input, System.Globalization.NumberStyles.HexNumber);
        //        value = value - 40;
        //        value = (value * (9 / 5)) + 32;

        //        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        //            () =>
        //            {
        //                gagTbk2.Text = Convert.ToString(value) + " °F";
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        return;
        //    }
        //}


        //void ReadOBD2()
        //{
        //    Task t = Task.Run(async () => 
        //    {
        //        while (true)
        //        {
        //            uint buf;
        //            buf = await obdReader.LoadAsync(1);
        //            if (obdReader.UnconsumedBufferLength > 0)
        //            {
        //                string s = obdReader.ReadString(1);
        //                result = result + s;
        //                if (s.Equals("\n") || s.Equals("\r"))
        //                {
        //                    try
        //                    {
        //                        result = "";
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                await Task.Delay(TimeSpan.FromSeconds(0));
        //            }
        //        }
        //    });
        //}

        private void BtDevice_ConnectionStatusChanged(BluetoothDevice sender, object args)
        {
            if (btDevice == null)
                return;
            try
            {
                if (btDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                    return;
                else
                {
                    DisconnectBluetooth();
                    UnsuccessfulConnection("Manually Disconnected or Device Lost");
                }
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                return;
            }
        }

        #endregion
    }
}
