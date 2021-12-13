using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RedGrim.Mobile.Tools
{
    public static class ColorMaster
    {
        public static Color ColorPrimary { get; set; } = Color.Cyan;
        public static Color ColorSecondary { get; set; } = Color.Magenta;
        public static Color ColorWarning { get; set; } = Color.DarkGoldenrod;
        public static Color ColorCritical { get; set; } = Color.OrangeRed;
        public static Color ColorLabel { get; set; } = Color.White;
        public static Color ColorText { get; set; } = Color.DarkGoldenrod;

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
        }

        public static void DarkTheme()
        {
            Color ColorPrimary = Color.Cyan;
            Color ColorSecondary = Color.Magenta;
            Color ColorWarning = Color.DarkGoldenrod;
            Color ColorCritical = Color.OrangeRed;
            Color ColorLabel = Color.White;
            Color ColorText = Color.DarkGoldenrod;
        }

        public static void StealthTheme()
        {
            Color ColorPrimary = Color.Gray;
            Color ColorSecondary = Color.OrangeRed;
            Color ColorWarning = Color.OrangeRed;
            Color ColorCritical = Color.OrangeRed;
            Color ColorLabel = Color.White;
            Color ColorText = Color.White;
        }

        public static void MobTheme()
        {
            Color ColorPrimary = Color.Gray;
            Color ColorSecondary = Color.OrangeRed;
            Color ColorWarning = Color.OrangeRed;
            Color ColorCritical = Color.OrangeRed;
            Color ColorLabel = Color.White;
            Color ColorText = Color.White;
        }


    }
}
