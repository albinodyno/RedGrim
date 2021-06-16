using RedGrim.App.Controls;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Media.Capture;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace RedGrim.App
{
    public sealed partial class MainPage : Page
    {
        public static string errorLog = string.Empty;

        bool settingsPaneActive = false;
        bool auxPaneActive = false;
        bool mapPaneActive = false;

        public MainPage()
        {
            this.InitializeComponent();
            LoadSavedSettings();
        }

        public async void LoadSavedSettings()
        {
            bool settingsReady;
            if (settingsReady = await pnlSettings.GetSavedSettings())
            {
                LoadThemeSettings();
                pnlAux.LoadAuxSettings();
                pnlBT.LoadBTSettings();
            }
            else
                SystemLogEntry("CRITICAL ERROR LOADING SETTINGS");
        }

        public void LoadThemeSettings()
        {
            if (SettingsController.SavedSettings.theme == "Dark")
            {
                grdMainGrid.Background = new SolidColorBrush(Colors.Black);
                pnlSideMenu.Background = new SolidColorBrush(Colors.Black);
            }
            else if(SettingsController.SavedSettings.theme == "Light")
            {
                grdMainGrid.Background = new SolidColorBrush(Colors.White);
            }
        }

        public static void SystemLogEntry(string entry)
        {
            //Add date time, but need interenet connection....
            errorLog = errorLog + $"{entry}\r\r";
        }

        #region Side Menu Buttons
        //Side Menu Buttons
        private void btnBTSetup_Click(object sender, RoutedEventArgs e)
        {
            pnlBT.ToggleBTMenu();
            if (auxPaneActive)
                pnlAux.Visibility = Visibility.Visible;
            pnlBT.Visibility = Visibility.Visible;
            pnlSideMenu.Visibility = Visibility.Visible;
            pnlSettings.Visibility = Visibility.Collapsed;
            settingsPaneActive = false;
            mapPaneActive = false;
        }

        private void btnAuxLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (!auxPaneActive)
            {
                pnlAux.Visibility = Visibility.Visible;
                pnlBT.Visibility = Visibility.Visible;
                pnlSettings.Visibility = Visibility.Collapsed;
                settingsPaneActive = false;
                mapPaneActive = false;
                auxPaneActive = true;
            }
            else
            {
                pnlAux.Visibility = Visibility.Collapsed;
                pnlBT.Visibility = Visibility.Visible;
                pnlSettings.Visibility = Visibility.Collapsed;
                settingsPaneActive = false;
                auxPaneActive = false;
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!settingsPaneActive)
            {
                pnlAux.Visibility = Visibility.Collapsed;
                pnlBT.Visibility = Visibility.Collapsed;
                mapPaneActive = false;
                pnlSettings.Visibility = Visibility.Visible;
                settingsPaneActive = true;
            }
            else
            {
                if (auxPaneActive)
                    pnlAux.Visibility = Visibility.Visible;
                pnlSettings.Visibility = Visibility.Collapsed;
                pnlBT.Visibility = Visibility.Visible;
                settingsPaneActive = false;
            }
        }

        private void btnMapLaunch_Click(object sender, RoutedEventArgs e)
        {
            if(!mapPaneActive)
            {
                pnlAux.Visibility = Visibility.Collapsed;
                pnlBT.Visibility = Visibility.Collapsed;
                pnlSettings.Visibility = Visibility.Collapsed;
                mapPaneActive = true;
            }
            else
            {
                if (auxPaneActive)
                    pnlAux.Visibility = Visibility.Visible;
                pnlBT.Visibility = Visibility.Visible;
                mapPaneActive = false;
            }
        }
        #endregion
    }
}
