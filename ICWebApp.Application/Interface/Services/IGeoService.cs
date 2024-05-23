using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using ICWebApp.Domain.Models.Geocoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IGeoService
    {
        public Task<bool> FetchGeoLocation();
        public GeoItem? GetGeoData();
        public event Action OnResponse;
        public Task<Address?> GetCoordinates(string Address, string? Culture = null);
        public Task<Address?> GetAddress(Location Location, string? Culture = null);
    }
}