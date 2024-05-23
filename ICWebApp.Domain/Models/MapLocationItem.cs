using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class MapLocationItem
    {
        public double Lat { get; set; }
        public double Lan { get; set; }
        public double[] LatLan
        {
            get
            {
                return new double[] { Lat, Lan };
            }
        }
    }
}
