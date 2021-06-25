﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using RedGrim.Mobile.Models;

namespace RedGrim.Mobile.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsControl : ContentView
    {
        public static Settings saveSettings = new Settings();
        public static Settings loadedSettings = new Settings();

        public SettingsControl()
        {
            InitializeComponent();
        }

        public void UpdateVersionNum()
        {
            lblVersionNumber.Text = MainPage.VersionNumber;
        }

        private void btnGaugeSettings_Clicked(object sender, EventArgs e)
        {

        }

        private void btnUpdateLog_Clicked(object sender, EventArgs e)
        {
            tbkLog.Text = BluetoothControl.log;
        }

        private void btnClearLog_Clicked(object sender, EventArgs e)
        {
            BluetoothControl.log = "";
            tbkLog.Text = BluetoothControl.log;
        }

        private void btnCloseLog_Clicked(object sender, EventArgs e)
        {
            SettingsOBDPage.IsVisible = false;
        }

        private void btnShowLog_Clicked(object sender, EventArgs e)
        {
            SettingsOBDPage.IsVisible = true;
            tbkLog.Text = BluetoothControl.log;
        }

        private void btnShowErrorLog_Clicked(object sender, EventArgs e)
        {
            SettingsErrorPage.IsVisible = true;
            tbkErrorLog.Text = MainPage.errorLog;
        }

        private void btnSaveSettings_Clicked(object sender, EventArgs e)
        {
            SaveData();
        }

        private void btnResetSettings_Clicked(object sender, EventArgs e)
        {

        }

        private void btnUpdateErrorLog_Clicked(object sender, EventArgs e)
        {
            tbkErrorLog.Text = MainPage.errorLog;
        }

        private void btnClearErrorLog_Clicked(object sender, EventArgs e)
        {
            MainPage.errorLog = "";
            tbkErrorLog.Text = MainPage.errorLog;
        }

        private void btnCloseErrorLog_Clicked(object sender, EventArgs e)
        {
            SettingsErrorPage.IsVisible = false;
        }

        private void btnAddELM_Clicked(object sender, EventArgs e)
        {

        }

        private void btnSubELM_Clicked(object sender, EventArgs e)
        {

        }

        private void btnAddPID_Clicked(object sender, EventArgs e)
        {

        }

        private void btnSubPID_Clicked(object sender, EventArgs e)
        {

        }

        private void Button_Clicked(object sender, EventArgs e)
        {

        }

        private void btnCloseGaugeSettings_Clicked(object sender, EventArgs e)
        {

        }

        #region Save/Load Methods

        public static void LoadData()
        {
            try
            {
                string loadPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RedGrimSettings.txt");

                if (loadPath == null || !File.Exists(loadPath))
                {
                    throw new Exception("File not found");
                }

                StreamReader reader = new StreamReader(loadPath, true);
                string jsonSettings = reader.ReadToEnd();

                loadedSettings = (Settings)JsonConvert.DeserializeObject(jsonSettings);
                saveSettings = loadedSettings;
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry($"---Load Settings Failed - {ex.Message}");
            }
        }


        public static void SaveData()
        {
            try
            {
                string jsonSettings = JsonConvert.SerializeObject(saveSettings);

                string savePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "RedGrimSettings.txt");

                StreamWriter writer = File.CreateText(savePath);
                writer.Write(jsonSettings);
                MainPage.SystemLogEntry("---Settings saved successfully");
            }
            catch(Exception ex)
            {
                MainPage.SystemLogEntry("---Failed to save settings");
            }
        }

        #endregion
    }
}