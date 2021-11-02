using System;
using System.Collections.Generic;
using System.Text;

namespace RedGrim.Mobile.ViewModels
{
    public class SampleViewModel : BaseViewModel
    {
        private string _SomeLabel;
        //private SquareGaugeViewModel SquareGage1;


        public string SomeLabel
        {
            get => _SomeLabel;
            set => SetProperty(ref _SomeLabel, value);
        }
    }
}
