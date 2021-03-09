using RedGrim.App.Models;
using System;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Devices.Geolocation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RedGrim.App.Controls
{
    public sealed partial class MapControl : UserControl
    {
        double latitude = 33.453663;
        double longitude = -111.943865;

        public MapControl()
        {
            this.InitializeComponent();
            SetupMap();
        }

        public void SetupMap()
        {
            mapMain.MapServiceToken = "vlFE3FEKdnHvzsgEyNdp~75E7UBeIr34OmETHmj4qJg~Att81oCZItAgQLYKpTC4sAgCiCKSEY-M8DmBmv4iLugT12t1N5LcbVIsXabcIDz6";

            BasicGeoposition basicGeoposition = new BasicGeoposition()
            {
                Latitude = latitude,
                Longitude = longitude,
                Altitude = 2
            };
            Geopoint center = new Geopoint(basicGeoposition);
            mapMain.Center = center;
            mapMain.ZoomLevel = 10;

            mapMain.CenterChanged += MapMain_CenterChanged;
        }

        private void MapMain_CenterChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {
            latitude = sender.Center.Position.Latitude;
            longitude = sender.Center.Position.Longitude;
            UpdateCoordinates();
        }

        private void UpdateCoordinates()
        {
            tbkLat.Text = Convert.ToString(latitude);
            tbkLong.Text = Convert.ToString(longitude);

        }

        private void btnPin_Click(object sender, RoutedEventArgs e)
        {
            if (btnMapPin.Visibility == Visibility.Visible)
                btnMapPin.Visibility = Visibility.Collapsed;
            else
                btnMapPin.Visibility = Visibility.Visible;

        }
    }
}
