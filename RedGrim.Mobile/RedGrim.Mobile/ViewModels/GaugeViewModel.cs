using System;
using System.Collections.Generic;
using System.Text;
using RedGrim.Mobile.ViewModels;

namespace RedGrim.Mobile.ViewModels
{
    class GaugeViewModel : BaseViewModel
    {
        private string label;
        private string gaugeValue;


        public string Label
        {
            get => label;
            set => SetProperty(ref label, value);
        }

        public string GaugeValue
        {
            get => gaugeValue;
            set => SetProperty(ref gaugeValue, value);
        }
    }
}
