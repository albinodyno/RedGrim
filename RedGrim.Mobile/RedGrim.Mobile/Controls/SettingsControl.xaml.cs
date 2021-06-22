using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RedGrim.Mobile.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsControl : ContentView
    {
        public SettingsControl()
        {
            InitializeComponent();
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
            tbkLog.Text = "";
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
    }
}