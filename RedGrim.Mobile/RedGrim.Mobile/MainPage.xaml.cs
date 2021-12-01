using System;
using System.Timers;
using System.ComponentModel;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Essentials;
using RedGrim.Mobile.Controls;
using System.Collections.Generic;

namespace RedGrim.Mobile
{
    //Signing android APK: https://docs.microsoft.com/en-us/xamarin/android/deploy-test/signing/?tabs=windows
    public partial class MainPage : ContentPage
    {
        public static string VersionNumber;
        private static System.Timers.Timer clockTimer;
        public static string currentTime;

        public MainPage()
        {
            InitializeComponent();
            VersionTracking.Track();
            VersionNumber = $"v{VersionTracking.CurrentVersion}";
            DeviceDisplay.KeepScreenOn = !DeviceDisplay.KeepScreenOn;
            Battery.BatteryInfoChanged += Battery_BatteryInfoChanged;
            //SetTimer();
            StartUp();
        }

        public void SetTimer()
        {
            // Create a timer with a two second interval.
            clockTimer = new System.Timers.Timer(60000);
            // Hook up the Elapsed event for the timer. 
            clockTimer.Elapsed += OnTimedEvent;
            clockTimer.AutoReset = true;
            clockTimer.Enabled = true;

            currentTime = DateTime.Now.ToShortTimeString();
            //lblClock.Text = currentTime;
        }

        public async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            currentTime = DateTime.Now.ToShortTimeString();
            //lblClock.Text = currentTime;
        }

        private void UpdateClock()
        {

        }

        void Battery_BatteryInfoChanged(object sender, BatteryInfoChangedEventArgs e)
        {
            if (e.PowerSource != BatteryPowerSource.Battery) StartUp();
            BluetoothControl.SystemLogEntry($"Power State changed to: {e.PowerSource}", false);
        }

        private void btnBTSetup_Clicked(object sender, EventArgs e)
        {
            if(!BluetoothPage.IsVisible)
            {
                AuxPage.IsVisible = false;
                BluetoothPage.IsVisible = true;
                pnlSideMenu.IsVisible = true;
                SettingsPage.IsVisible = false;
                MapPage.IsVisible = false;
            }
            else
            {
                BluetoothPage.ToggleMenu();
            }
        }

        private void btnSettingsLaunch_Clicked(object sender, EventArgs e)
        {
            if(!SettingsPage.IsVisible)
            {
                BluetoothPage.IsVisible = false;
                AuxPage.IsVisible = false;
                pnlSideMenu.IsVisible = true;
                SettingsPage.UpdateVersionNum();
                SettingsPage.IsVisible = true;
                MapPage.IsVisible = false;
                AuxPage.IsVisible = false;
            }
            else
            {
                BluetoothPage.IsVisible = true;
                AuxPage.IsVisible = false;
                pnlSideMenu.IsVisible = true;
                SettingsPage.IsVisible = false;
                MapPage.IsVisible = false;
                AuxPage.IsVisible = false;
            }
        }

        private void btnMapLaunch_Clicked(object sender, EventArgs e)
        {
            if(!MapPage.IsVisible)
            {
                MapPage.IsVisible = true;
                BluetoothPage.IsVisible = false;
                pnlSideMenu.IsVisible = true;
                SettingsPage.IsVisible = false;
                AuxPage.IsVisible = false;
            }
            else
            {
                MapPage.IsVisible = false;
                BluetoothPage.IsVisible = true;
                pnlSideMenu.IsVisible = true;
                SettingsPage.IsVisible = false;
                AuxPage.IsVisible = false;
            }
        }

        private void btnAuxLaunch_Clicked(object sender, EventArgs e)
        {
            if (!AuxPage.IsVisible)
            {
                MapPage.IsVisible = false;
                SettingsPage.IsVisible = false;
                BluetoothPage.IsVisible = true;
                AuxPage.IsVisible = true;
                pnlSideMenu.IsVisible = true;

            }
            else
            {
                MapPage.IsVisible = false;
                SettingsPage.IsVisible = false;
                AuxPage.IsVisible = false;
                BluetoothPage.IsVisible = true;
                pnlSideMenu.IsVisible = true;
            }
        }

        private void btnConnectAll_Clicked(object sender, EventArgs e)
        {
            BluetoothPage.ConnectSavedDevice();
            AuxPage.ConnectSavedDevice();
        }


        private void StartUp()
        {
            if (Battery.PowerSource != BatteryPowerSource.Battery)
            {
                BluetoothPage.ConnectSavedDevice();
                AuxPage.ConnectSavedDevice();
            }
        }

    }
}