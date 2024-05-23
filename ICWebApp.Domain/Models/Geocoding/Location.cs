using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Geocoding
{
    public class Location
    {
        public Location(double? Latitude, double? Longitude) 
        { 
            this.Latitude = Latitude;
            this.Longitude = Longitude;
        }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
    }
}
