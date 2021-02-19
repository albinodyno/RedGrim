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
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Navigation;
using System.Reflection;
using Windows.UI;
using RedGrim.App.Models;
using RedGrim.App.Controls;
using Windows.Storage;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RedGrim.App.Controls
{
    public sealed partial class AuxControl : UserControl
    {
        //GPIO Variables
        GpioController gpioController;
        GpioPin pinAux1;
        GpioPin pinAux2;
        GpioPin pinAux3;
        GpioPin pinAux4;
        bool gpioCompatible;

        public AuxControl()
        {
            this.InitializeComponent();
            SetupGPIO();
        }

        public void LoadAuxSettings()
        {
            btnAux1Off.Content = SettingsController.SavedSettings.aux1;
            btnAux1On.Content = SettingsController.SavedSettings.aux1;
            btnAux2Off.Content = SettingsController.SavedSettings.aux2;
            btnAux2On.Content = SettingsController.SavedSettings.aux2;
            btnAux3Off.Content = SettingsController.SavedSettings.aux3;
            btnAux3On.Content = SettingsController.SavedSettings.aux3;
            btnAux4Off.Content = SettingsController.SavedSettings.aux4;
            btnAux4On.Content = SettingsController.SavedSettings.aux4;
        }

        private void SetupGPIO()
        {
            try
            {
                //Initialize all gpio pins as OFF... I think that means HIGH
                gpioController = GpioController.GetDefault();

                if (gpioController == null)
                {
                    gpioCompatible = false;
                    tbkStatus.Foreground = new SolidColorBrush(Colors.OrangeRed);
                    tbkStatus.Text = "No GPIO Control Found";
                    return;
                }

                pinAux1 = gpioController.OpenPin(5);
                pinAux1.SetDriveMode(GpioPinDriveMode.Output);
                pinAux1.Write(GpioPinValue.High);

                pinAux2 = gpioController.OpenPin(6);
                pinAux2.SetDriveMode(GpioPinDriveMode.Output);
                pinAux2.Write(GpioPinValue.High);

                pinAux3 = gpioController.OpenPin(13);
                pinAux3.SetDriveMode(GpioPinDriveMode.Output);
                pinAux3.Write(GpioPinValue.High);

                pinAux4 = gpioController.OpenPin(19);
                pinAux4.SetDriveMode(GpioPinDriveMode.Output);
                pinAux4.Write(GpioPinValue.High);

                gpioCompatible = true;
            }
            catch (NullReferenceException ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                gpioCompatible = false;
            }
        }

        //AUX 1
        private void btnAux1On_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(gpioCompatible)
                    pinAux1.Write(GpioPinValue.Low);
                btnAux1On.Visibility = Visibility.Collapsed;
                btnAux1Off.Visibility = Visibility.Visible;
                bdr1.BorderBrush = new SolidColorBrush(Colors.OrangeRed);
                bdr1.BorderThickness = new Thickness(7);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                gpioCompatible = false;
            }     
        }

        private void btnAux1Off_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(gpioCompatible)
                    pinAux1.Write(GpioPinValue.High);
                btnAux1On.Visibility = Visibility.Visible;
                btnAux1Off.Visibility = Visibility.Collapsed;
                bdr1.BorderBrush = new SolidColorBrush(Colors.Purple);
                bdr1.BorderThickness = new Thickness(5);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                gpioCompatible = false;
            }
        }

        //AUX 2
        private void btnAux2On_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(gpioCompatible)
                    pinAux2.Write(GpioPinValue.Low);
                btnAux2On.Visibility = Visibility.Collapsed;
                btnAux2Off.Visibility = Visibility.Visible;
                bdr2.BorderBrush = new SolidColorBrush(Colors.OrangeRed);
                bdr2.BorderThickness = new Thickness(7);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                gpioCompatible = false;
            }
        }

        private void btnAux2Off_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gpioCompatible)
                    pinAux2.Write(GpioPinValue.High);
                btnAux2On.Visibility = Visibility.Visible;
                btnAux2Off.Visibility = Visibility.Collapsed;
                bdr2.BorderBrush = new SolidColorBrush(Colors.Purple);
                bdr2.BorderThickness = new Thickness(5);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                gpioCompatible = false;
            }
        }

        //AUX 3
        private void btnAux3On_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gpioCompatible)
                    pinAux3.Write(GpioPinValue.Low);
                btnAux3On.Visibility = Visibility.Collapsed;
                btnAux3Off.Visibility = Visibility.Visible;
                bdr3.BorderBrush = new SolidColorBrush(Colors.OrangeRed);
                bdr3.BorderThickness = new Thickness(7);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                gpioCompatible = false;
            }
        }

        private void btnAux3Off_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gpioCompatible)
                    pinAux3.Write(GpioPinValue.High);
                btnAux3On.Visibility = Visibility.Visible;
                btnAux3Off.Visibility = Visibility.Collapsed;
                bdr3.BorderBrush = new SolidColorBrush(Colors.Purple);
                bdr3.BorderThickness = new Thickness(5);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                gpioCompatible = false;
            }
        }

        // AUX 4
        private void btnAux4On_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gpioCompatible)
                    pinAux4.Write(GpioPinValue.Low);
                btnAux4On.Visibility = Visibility.Collapsed;
                btnAux4Off.Visibility = Visibility.Visible;
                bdr4.BorderBrush = new SolidColorBrush(Colors.OrangeRed);
                bdr4.BorderThickness = new Thickness(7);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                gpioCompatible = false;
            }
        }

        private void btnAux4Off_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gpioCompatible)
                    pinAux4.Write(GpioPinValue.High);
                btnAux4On.Visibility = Visibility.Visible;
                btnAux4Off.Visibility = Visibility.Collapsed;
                bdr4.BorderBrush = new SolidColorBrush(Colors.Purple);
                bdr4.BorderThickness = new Thickness(5);
            }
            catch (Exception ex)
            {
                MainPage.SystemLogEntry(ex.Message);
                gpioCompatible = false;
            }
        }
    }
}
