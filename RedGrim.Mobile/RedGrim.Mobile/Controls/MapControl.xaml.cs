using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;

namespace RedGrim.Mobile.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapControl : ContentView
    {
        public Xamarin.Forms.Maps.Map mainMap;
        public double currentLat;
        public double currentLong;
        public MapControl()
        {
            InitializeComponent();
            CreateMap();
        }

        public void CreateMap()
        {
            GetCurrentCoordinates();
            Position startingPoint = new Position(currentLat, currentLong);
            MapSpan span = new MapSpan(startingPoint, .01, .01);

            mainMap = new Xamarin.Forms.Maps.Map(span);
            mainMap.MapType = MapType.Satellite;
            

            mapLayout.Children.Add(mainMap);
            
        }

        private async void GetCurrentCoordinates()
        {
            Location currentLocation = await Geolocation.GetLastKnownLocationAsync();
            currentLat = currentLocation.Latitude;
            currentLong = currentLocation.Longitude;
        }
        

        private async void PanToCurrentLocation()
        {
            try
            {
                GetCurrentCoordinates();
                if (currentLat != null && currentLong != null)
                {
                    Position pos = new Position(currentLat, currentLong);
                    MapSpan mspan = new MapSpan(pos, .01, .01);
                    mainMap.MoveToRegion(mspan);

                    Pin currentPin = new Pin();
                    currentPin.Position = pos;
                    currentPin.Label = "^";
                    currentPin.StyleId = "current";
                    currentPin.Type = PinType.Generic;

                    mainMap.Pins.Add(currentPin);
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }
        }

        #region Map Button Handlers

        private void btnPanToCurrentLocation_Clicked(object sender, EventArgs e)
        {
            PanToCurrentLocation();
        }

        #endregion
    }
}