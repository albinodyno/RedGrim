using RedGrim.App.Models;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace RedGrim.App.Controls
{
    public sealed partial class SettingsController : UserControl
    {
        //To load from json file
        public static CurrentSettings SavedSettings = new CurrentSettings();
        static string savedSettingsJson;
        bool settingsReady = false;

        public SettingsController()
        {
            this.InitializeComponent();
        }

        public async Task<bool> GetSavedSettings()
        {
            try
            {
                StorageFile savedJsonFile = await ApplicationData.Current.LocalFolder.GetFileAsync("RedGrimSettings.json");
                savedSettingsJson = await FileIO.ReadTextAsync(savedJsonFile);

                //load object with settings from json string
                SavedSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.CurrentSettings>(savedSettingsJson);
                if (SavedSettings == null || SavedSettings.aux1 == null)
                    throw new Exception();               
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry($"Saved json not found or blank, loading default settings - {ex.Message}");
                settingsReady = await LoadDefaults();
            }

            if(settingsReady)
                settingsReady = await LoadValues();

            return settingsReady;
        }

        public static async Task<bool> LoadDefaults()
        {
            try
            {
                SavedSettings = new CurrentSettings();
                //Load object with default values
                SavedSettings.aux1 = "AUX 1";
                SavedSettings.aux2 = "AUX 2";
                SavedSettings.aux3 = "AUX 3";
                SavedSettings.aux4 = "AUX 4";
                SavedSettings.backCam = true;
                SavedSettings.frontCam = true;
                SavedSettings.topCam = true;
                SavedSettings.theme = "Dark";
                SavedSettings.obdProtocol = "AT SP 0";
                SavedSettings.btDeviceID = "";
                SavedSettings.btDeviceName = "";
                SavedSettings.elmDelay = 2000;
                SavedSettings.pidDelay = 1000;

                //serialize object to json
                savedSettingsJson = Newtonsoft.Json.JsonConvert.SerializeObject(SavedSettings);

                //Create json file because it wasnt found
                StorageFile saveFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("RedGrimSettings.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(saveFile, savedSettingsJson);
                return true;
            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry("Error loading default settings");
                return false;
            }
        }

        private async Task<bool> LoadValues()
        {
            try
            {
                txbAux1.Text = SavedSettings.aux1;
                txbAux2.Text = SavedSettings.aux2;
                txbAux3.Text = SavedSettings.aux3;
                txbAux4.Text = SavedSettings.aux4;

                if (SavedSettings.theme == "Dark")
                {
                    rbtDark.IsChecked = true;
                }
                else if (SavedSettings.theme == "Light")
                {
                    rbtLight.IsChecked = true;
                }

                tgbFCam.IsChecked = SavedSettings.frontCam;
                tgbBCam.IsChecked = SavedSettings.backCam;
                tgbTCam.IsChecked = SavedSettings.topCam;

                tbkELMDelay.Text = Convert.ToString(SavedSettings.elmDelay);
                tbkPIDDelay.Text = Convert.ToString(SavedSettings.pidDelay);
                return true;
            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry("Error loading values");
                return false;
            }

        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void btnResetSettings_Click(object sender, RoutedEventArgs e)
        {
            ResetSettings();
        }

        public void SaveSettings()
        {
            try
            {
                SavedSettings.aux1 = txbAux1.Text;
                SavedSettings.aux2 = txbAux2.Text;
                SavedSettings.aux3 = txbAux3.Text;
                SavedSettings.aux4 = txbAux4.Text;

                SavedSettings.frontCam = (bool)tgbFCam.IsChecked;
                SavedSettings.backCam = (bool)tgbBCam.IsChecked;
                SavedSettings.topCam = (bool)tgbTCam.IsChecked;

                if ((bool)rbtDark.IsChecked)
                    SavedSettings.theme = "Dark";
                else if ((bool)rbtLight.IsChecked)
                    SavedSettings.theme = "Light";

                SavedSettings.pidDelay = Convert.ToInt32(tbkPIDDelay.Text);
                SavedSettings.elmDelay = Convert.ToInt32(tbkELMDelay.Text);

                SaveJson();
                tbkSettingsStatus.Foreground = new SolidColorBrush(Colors.Green);
                tbkSettingsStatus.Text = "Save Successful: reboot to apply updates";
            }
            catch (Exception ex)
            {
                tbkSettingsStatus.Foreground = new SolidColorBrush(Colors.OrangeRed);
                tbkSettingsStatus.Text = "Save Unsuccessful";
            }
        }

        public static async void SaveJson()
        {
            try
            {
                //Convert object to JSON
                savedSettingsJson = Newtonsoft.Json.JsonConvert.SerializeObject(SavedSettings);

                //Get storage location and write over with new settings
                StorageFile saveFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("RedGrimSettings.json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(saveFile, savedSettingsJson);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public async void ResetSettings()
        {
            StorageFile saveFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("RedGrimSettings.json", CreationCollisionOption.ReplaceExisting);
            tbkSettingsStatus.Foreground = new SolidColorBrush(Colors.OrangeRed);
            tbkSettingsStatus.Text = "Reset To Default: Reboot to Confirm or hit Save to Cancel";
        }


        # region Bluetooth Log Buttons
        private void btnShowLog_Click(object sender, RoutedEventArgs e)
        {
            pnlLog.Visibility = Visibility.Visible;
            UpdateLogUI();
        }

        private void btnCloseLog_Click(object sender, RoutedEventArgs e)
        {
            pnlLog.Visibility = Visibility.Collapsed;
        }

        private void btnUpdateLog_Click(object sender, RoutedEventArgs e)
        {
            UpdateLogUI();
        }

        private void UpdateLogUI()
        {
            tbkLog.Text = BluetoothControl.log;
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            BluetoothControl.log = string.Empty;
            UpdateLogUI();
        }
        #endregion

        # region Error Log Buttons
        private void btnShowErrorLog_Click(object sender, RoutedEventArgs e)
        {
            pnlErrorLog.Visibility = Visibility.Visible;
            UpdateErrorLogUI();
        }
        private void btnCloseErrorLog_Click(object sender, RoutedEventArgs e)
        {
            pnlErrorLog.Visibility = Visibility.Collapsed;
        }
        private void btnUpdateErrorLog_Click(object sender, RoutedEventArgs e)
        {
            UpdateErrorLogUI();
        }
        private void UpdateErrorLogUI()
        {
            tbkErrorLog.Text = MainPage.errorLog;
        }

        private void btnClearErrorLog_Click(object sender, RoutedEventArgs e)
        {
            MainPage.errorLog = string.Empty;
            UpdateErrorLogUI();
        }
        #endregion

        #region Gauge Buttons
        //Gauge Buttons
        private void btnGaugeSettings_Click(object sender, RoutedEventArgs e)
        {
            pnlGaugeSettings.Visibility = Visibility.Visible;
        }

        private void btnCloseGaugeSettings_Click(object sender, RoutedEventArgs e)
        {
            pnlGaugeSettings.Visibility = Visibility.Collapsed;
        }

        private void btnForceGaugeStartup_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnAddPID_Click(object sender, RoutedEventArgs e)
        {
            int value = Convert.ToInt32(tbkPIDDelay.Text) + 100;
            tbkPIDDelay.Text = Convert.ToString(value);
        }

        private void btnSubPID_Click(object sender, RoutedEventArgs e)
        {
            int value = Convert.ToInt32(tbkPIDDelay.Text) - 100;
            tbkPIDDelay.Text = Convert.ToString(value);
        }

        private void btnAddELM_Click(object sender, RoutedEventArgs e)
        {
            int value = Convert.ToInt32(tbkELMDelay.Text) + 100;
            tbkELMDelay.Text = Convert.ToString(value);
        }

        private void btnSubELM_Click(object sender, RoutedEventArgs e)
        {
            int value = Convert.ToInt32(tbkELMDelay.Text) - 100;
            tbkELMDelay.Text = Convert.ToString(value);
        }
        #endregion
    }
}
