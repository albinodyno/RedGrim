using System;
using System.ComponentModel;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace RedGrim.Mobile
{
    //Signing android APK: https://docs.microsoft.com/en-us/xamarin/android/deploy-test/signing/?tabs=windows
    public partial class MainPage : ContentPage
    {
        public static string errorLog = string.Empty;
        public static string VersionNumber;

        public MainPage()
        {
            InitializeComponent();
            VersionTracking.Track();
            VersionNumber = VersionTracking.CurrentVersion;
        }

        public static void SystemLogEntry(string entry)
        {
            errorLog = errorLog + $"--{entry}\n\n";
        }

        private void btnBTSetup_Clicked(object sender, EventArgs e)
        {
            pnlBT.ToggleMenu();
            AuxPage.IsVisible = false;
            pnlBT.IsVisible = true;
            pnlSideMenu.IsVisible = true;
            SettingsPage.IsVisible = false;
            MapPage.IsVisible = false;
        }

        private void btnSettingsLaunch_Clicked(object sender, EventArgs e)
        {

            AuxPage.IsVisible = false;
            pnlBT.IsVisible = false;
            pnlSideMenu.IsVisible = true;
            SettingsPage.UpdateVersionNum();
            SettingsPage.IsVisible = true;
            MapPage.IsVisible = false;
        }

        private void btnMapLaunch_Clicked(object sender, EventArgs e)
        {
            if(!MapPage.IsVisible)
            {
                MapPage.IsVisible = true;
                pnlBT.IsVisible = false;
                pnlSideMenu.IsVisible = true;
                SettingsPage.IsVisible = false;
            }
            else
            {
                MapPage.IsVisible = false;
                pnlBT.IsVisible = true;
                pnlSideMenu.IsVisible = true;
                SettingsPage.IsVisible = false;
            }
        }
    }
}