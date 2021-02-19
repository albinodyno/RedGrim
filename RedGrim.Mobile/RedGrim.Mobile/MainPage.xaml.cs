using System;
using System.ComponentModel;
using System.Threading;
using Xamarin.Forms;

namespace RedGrim.Mobile
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public static string errorLog = string.Empty;
        public int HourOffset { get; private set; }

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var clockRefresh = new Timer(dueTime: 0, period: 1000, callback: UpdateTimeLabel, state: null);
        }

        private void UpdateTimeLabel(object state = null)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                time.Text = DateTime.Now.AddHours(HourOffset).ToLongTimeString();
            }
            );
        }

        //private void OnUpButton_Clicked(object sender, EventArgs e)
        //{
        //    HourOffset++;
        //    UpdateTimeLabel();
        //}

        //private void OnDownButton_Clicked(object sender, EventArgs e)
        //{
        //    HourOffset--;
        //    UpdateTimeLabel();
        //}

        public static void SystemLogEntry(string entry)
        {
            //Add date time, but need interenet connection....
            errorLog = errorLog + $"{entry}\r";
        }

        private void btnBTSetup_Clicked(object sender, EventArgs e)
        {

        }
    }
}