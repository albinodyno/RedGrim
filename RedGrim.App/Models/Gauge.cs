using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedGrim.App.Models
{
    class Gauge
    {
        private string name = "";
        private string uom = "";
        private double gaugeValue = 0;
        private int max = 0;
        private int tickSpacing = 10;
        private string obdCommand = "";

        //public string Name { get => name; set => SetProperty(ref name, value); }
        //public string UOM { get => uom; set => SetProperty(ref uom, value); }
        //public double GaugeValue { get => gaugeValue; set => SetProperty(ref value, value); }
        //public int Max { get => max; set => SetProperty(ref max, value); }
        //public int TickSpacing { get => tickSpacing; set => SetProperty(ref tickSpacing, value); }
        //public string OBDCommand { get => obdCommand; set => SetProperty(ref obdCommand, value); }

        public string Name { get { return name; } set { name = value; } }
        public string UOM { get { return uom; } set { uom = value; } }
        public double GaugeValue { get { return gaugeValue; } set { gaugeValue = value; } }
        public int Max { get { return max; } set { max = value; } }
        public int TickSpacing { get { return tickSpacing; } set { tickSpacing = value; } }
        public string OBDCommand { get { return obdCommand; } set { obdCommand = value; } } 
        

        public Gauge(string gName, string gUOM, int gMax, int gTickSpacing, string gOBDCommand)
        {
            name = gName;
            uom = gUOM;
            max = gMax;
            tickSpacing = gTickSpacing;
            obdCommand = gOBDCommand;
        }
    }
}
