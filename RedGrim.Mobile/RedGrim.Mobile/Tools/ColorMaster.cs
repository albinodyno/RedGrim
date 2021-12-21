using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using RedGrim.Mobile.Controls;

namespace RedGrim.Mobile.Tools
{
    public static class ColorMaster
    {
        //private static Color colorPrimary = Color.Blue;
        //private static Color colorSecondary = Color.Magenta;
        //private static Color colorWarning = Color.DarkGoldenrod;
        //private static Color colorCritical = Color.OrangeRed;
        //private static Color colorLabel = Color.White;
        //private static Color colorText = Color.DarkGoldenrod;

        //public static Color ColorPrimary { get { return colorPrimary; } set { colorPrimary = value; } } 
        //public static Color ColorSecondary { get { return colorSecondary; } set { colorSecondary = value; } }
        //public static Color ColorWarning { get { return colorWarning; } set { colorWarning = value; } }
        //public static Color ColorCritical { get { return colorCritical; } set { colorCritical = value; } }
        //public static Color ColorLabel { get { return colorLabel; } set { colorLabel = value; } }
        //public static Color ColorText { get { return colorText; } set { colorText = value; } }


        public static Color ColorPrimary { get; set; } = Color.White;
        public static Color ColorSecondary { get; set; } = Color.White;
        public static Color ColorWarning { get; set; } = Color.White;
        public static Color ColorCritical { get; set; } = Color.White;
        public static Color ColorLabel { get; set; } = Color.White;
        public static Color ColorText { get; set; } = Color.White;

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
