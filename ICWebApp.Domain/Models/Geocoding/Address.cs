using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Geocoding
{
    public class Address
    {
        public double? Longitude { get; set; }
        public double? Latitude { get; set;}
        public string? FormattedAddress { get; set; }
    }
}
