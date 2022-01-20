using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using RedGrim.Mobile.Controls;

namespace RedGrim.Mobile.Tools
{
    public static class ColorMaster
    {
        public static Color ColorPrimary { get; set; } = Color.White;
        public static Color ColorSecondary { get; set; } = Color.White;
        public static Color ColorWarning { get; set; } = Color.White;
        public static Color ColorCritical { get; set; } = Color.White;
        public static Color ColorLabel { get; set; } = Color.White;
        public static Color ColorText { get; set; } = Color.White;
        public static Color ColorOn { get; set; } = Color.OrangeRed;
        public static Color ColorOnSecondary { get; set; } = Color.DarkGoldenrod;
        public static Color ColorOff { get; set; } = Color.LightGray;
        public static Image MainImage { get; set; } = new Image() {Source = ImageSource.FromFile("splash.png") };

        public static void DefaultTheme()
        {
            DarkTheme();
        }

        public static void ChangeTheme(int input)
        {
            switch (input)
            {
                case 0:
                    DarkTheme();
                    break;
                case 1:
                    StealthTheme();
                    break;
                case 2:
                    MobTheme();
                    break;
            }

            MainImage.Source = ImageSource.FromFile("splash.png");

            SettingsControl.saveSettings.theme = input;
            SettingsControl.SaveData();   
        }

        public static void DarkTheme()
        {
            ColorPrimary = Color.Cyan;
            ColorSecondary = Color.Magenta;
            ColorWarning = Color.DarkGoldenrod;
            ColorCritical = Color.OrangeRed;
            ColorLabel = Color.White;
            ColorText = Color.DarkGoldenrod;
        }

        public static void StealthTheme()
        {
            ColorPrimary = Color.Gray;
            ColorSecondary = Color.OrangeRed;
            ColorWarning = Color.OrangeRed;
            ColorCritical = Color.OrangeRed;
            ColorLabel = Color.White;
            ColorText = Color.White;
        }

        public static void MobTheme()
        {
            ColorPrimary = Color.White;
            ColorSecondary = Color.OrangeRed;
            ColorWarning = Color.OrangeRed;
            ColorCritical = Color.OrangeRed;
            ColorLabel = Color.White;
            ColorText = Color.White;
        }
    }
}
