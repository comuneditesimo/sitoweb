using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class GeoItem
    {
        public double? Lan { get; set; }
        public double? Lat { get; set; }

        public double?[] LanLat
        {
            get
            {
                double? lat = null;
                double? lan = null;

                if (Lat != null)
                {
                    lat = Lat.Value;
                }
                if (Lan != null)
                {
                    lan = Lan.Value;
                }

                return new double?[] { lat, lan };
            }
        }
        public string? Title { get; set; }
    }
}
