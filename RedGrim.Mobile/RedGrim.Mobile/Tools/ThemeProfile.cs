using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace RedGrim.Mobile.Tools
{
    public class ThemeProfile
    {
        public static Color ColorPrimary { get; set; }
        public static Color ColorSecondary { get; set; }
        public static Color ColorWarning { get; set; }
        public static Color ColorCritical { get; set; }
        public static Color ColorText { get; set; }

        public ThemeProfile()
        {
            ColorPrimary = Color.Cyan;
        }
    }
}
