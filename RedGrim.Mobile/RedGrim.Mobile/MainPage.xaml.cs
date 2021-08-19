using System;
using System.ComponentModel;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Essentials;
using RedGrim.Mobile.Controls;

namespace RedGrim.Mobile
{
    //Signing android APK: https://docs.microsoft.com/en-us/xamarin/android/deploy-test/signing/?tabs=windows
    public partial class MainPage : ContentPage
    {
        public static string VersionNumber;

        public MainPage()
        {
            InitializeComponent();
            VersionTracking.Track();
            VersionNumber = VersionTracking.CurrentVersion;
            DeviceDisplay.KeepScreenOn = !DeviceDisplay.KeepScreenOn;
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
    }
}