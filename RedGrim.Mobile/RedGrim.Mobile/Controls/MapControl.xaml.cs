using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;

namespace RedGrim.Mobile.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapControl : ContentView
    {
        public MapControl()
        {
            InitializeComponent();
        }

        public void CreateMap()
        {
            Position startingPoint = new Position(33.4484, -112.0740);
            MapSpan span = new MapSpan(startingPoint, 33.4484, -112.0740);

            Map mainMap = new Map(span);

            mapLayout.Children.Add(mainMap);
        }
    }
}