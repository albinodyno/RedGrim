using System;
using System.ComponentModel;
using System.Threading;
using Xamarin.Forms;

namespace RedGrim.Mobile
{
    //[DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public static string errorLog = string.Empty;

        public MainPage()
        {
            InitializeComponent();
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
            SettingsPage.IsVisible = true;
            MapPage.IsVisible = false;
        }
    }
}