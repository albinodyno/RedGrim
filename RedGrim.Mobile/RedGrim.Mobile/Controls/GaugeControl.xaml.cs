using RedGrim.Mobile.ViewModels;
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
    public partial class GaugeControl : ContentView
    {
        private GaugeViewModel ViewModel
        {
            get => BindingContext as GaugeViewModel;
            //set => BindingContext = value;
            set
            {
                BindingContext = value;
                UpdateColors();
            }
        }

        //CAlor1
        //Color2
        //Color3

        //LowRange
        //MidRange
        //HighRange


        public GaugeControl()
        {
            InitializeComponent();
            ViewModel = new GaugeViewModel();
        }

        private void UpdateColors()
        {

        }
    }
}