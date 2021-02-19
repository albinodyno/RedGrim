using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using Windows.Devices.Gpio;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.Media.Capture;
using Windows.UI.Core;
using Newtonsoft.Json.Linq;
using RedGrim.App.Models;
using RedGrim.App.Controls;
using Windows.Storage;
using Windows.UI;
using System.Reflection.PortableExecutable;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RedGrim.App
{
    public sealed partial class MainPage : Page
    {
        public static string errorLog = string.Empty;

        //Camera variables
        DisplayRequest displayRequest = new DisplayRequest();
        MediaCapture mediaCapture;
        bool isPreviewing;
        bool camActive = false;
        bool settingsActive = false;
        bool auxActive = false;

        public MainPage()
        {
            this.InitializeComponent();
            LoadSavedSettings();
        }

        public async void LoadSavedSettings()
        {
            Task FindSettingsTask = SettingsController.GetSavedSettings();
            await FindSettingsTask;
            LoadCamSettings();
            LoadThemeSettings();
            pnlAux.LoadAuxSettings();
            pnlBT.LoadBTSettings();
            pnlBT.ConnectSavedBTOBD();
        }

        public void LoadCamSettings()
        {
            btnFrontCam.IsEnabled = SettingsController.SavedSettings.frontCam;
            btnBackCam.IsEnabled = SettingsController.SavedSettings.backCam;
            btnTopCam.IsEnabled = SettingsController.SavedSettings.topCam;
        }

        public void LoadThemeSettings()
        {
            if (SettingsController.SavedSettings.theme == "Dark")
            {
                grdMainGrid.Background = new SolidColorBrush(Colors.Black);
                pnlSideMenu.Background = new SolidColorBrush(Colors.Black);
                pnlCamMenu.Background = new SolidColorBrush(Colors.Black);
            }
            else if(SettingsController.SavedSettings.theme == "Light")
            {
                grdMainGrid.Background = new SolidColorBrush(Colors.White);
            }
        }

        public static void SystemLogEntry(string entry)
        {
            //Add date time, but need interenet connection....
            errorLog = errorLog + $"{entry}\r";
        }

        #region Side Menu Buttons
        //Side Menu Buttons
        private void btnBTSetup_Click(object sender, RoutedEventArgs e)
        {
            pnlBT.ToggleBTMenu();
            if (auxActive)
                pnlAux.Visibility = Visibility.Visible;
            pnlBT.Visibility = Visibility.Visible;
            pnlSideMenu.Visibility = Visibility.Visible;
            grdCamera.Visibility = Visibility.Collapsed;
            pnlSettings.Visibility = Visibility.Collapsed;
            settingsActive = false;
            camActive = false;
        }

        private void btnCamLaunch_Click(object sender, RoutedEventArgs e)
        {
            if(!camActive)
            {
                StartBackVidStream();
                pnlAux.Visibility = Visibility.Collapsed;
                pnlBT.Visibility = Visibility.Collapsed;
                settingsActive = false;
                pnlSettings.Visibility = Visibility.Collapsed;
                pnlSideMenu.Visibility = Visibility.Collapsed;
                grdCamera.Visibility = Visibility.Visible;
                camActive = true;
            }
            else 
            {
                CloseCameraAsync();
                pnlBT.Visibility = Visibility.Visible;
                if (auxActive)
                    pnlAux.Visibility = Visibility.Visible;
                pnlSideMenu.Visibility = Visibility.Visible;
                grdCamera.Visibility = Visibility.Collapsed;
                camActive = false;
            }
        }

        private void btnAuxLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (!auxActive)
            {
                pnlAux.Visibility = Visibility.Visible;
                pnlBT.Visibility = Visibility.Visible;
                pnlSettings.Visibility = Visibility.Collapsed;
                settingsActive = false;
                auxActive = true;
            }
            else
            {
                pnlAux.Visibility = Visibility.Collapsed;
                pnlBT.Visibility = Visibility.Visible;
                pnlSettings.Visibility = Visibility.Collapsed;
                settingsActive = false;
                auxActive = false;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!settingsActive)
            {
                pnlAux.Visibility = Visibility.Collapsed;
                pnlBT.Visibility = Visibility.Collapsed;
                pnlSettings.Visibility = Visibility.Visible;
                settingsActive = true;
            }
            else
            {
                if (auxActive)
                    pnlAux.Visibility = Visibility.Visible;
                pnlSettings.Visibility = Visibility.Collapsed;
                pnlBT.Visibility = Visibility.Visible;
                settingsActive = false;
            }
        }
        #endregion

        #region Camera Methods
        //Camera Methods
        private async Task StartBackVidStream()
        {
            if(btnBackCam.IsEnabled)
            {
                try
                {
                    mediaCapture = new MediaCapture();
                    await mediaCapture.InitializeAsync();

                    displayRequest.RequestActive();
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                }
                catch (UnauthorizedAccessException)
                {
                    // This will be thrown if the user denied access to the camera in privacy settings
                    //ShowMessageToUser("The app was denied access to the camera");
                    return;
                }

                try
                {
                    camBackCamera.Source = mediaCapture;
                    await mediaCapture.StartPreviewAsync();
                    isPreviewing = true;
                }
                catch (System.IO.FileLoadException)
                {
                    //mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
                }
            }
        }

        private async Task CloseCameraAsync()
        {
            if (mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    camBackCamera.Source = null;
                    //camFrontCamera.Source = null;
                    //camTopCamera.Source = null;
                    if (displayRequest != null)
                    {
                        try
                        {
                            displayRequest.RequestRelease();
                        }
                        catch(Exception ex)
                        {

                        }
                    }
                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }
        }

        private void btnFrontCam_Click(object sender, RoutedEventArgs e)
        {
            CloseCameraAsync();
            //StartFrontCamStream();
        }

        private void btnBackCam_Click(object sender, RoutedEventArgs e)
        {
            CloseCameraAsync();
            StartBackVidStream();
        }

        private void btnTopCam_Click(object sender, RoutedEventArgs e)
        {
            CloseCameraAsync();
            //StartTopVidStream();
        }
        #endregion
    }
}
