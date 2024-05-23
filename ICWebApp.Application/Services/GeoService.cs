using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.Models;
using ICWebApp.Domain.Models.Geocoding;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace ICWebApp.Application.Services
{
    public class GeoService : IGeoService
    {
        private readonly IJSRuntime _jsRuntime;
        private GeoItem _geoItem;
        private bool _response = false;
        public event Action OnResponse;

        public GeoService(IJSRuntime _jsRuntime)
        {
            this._jsRuntime = _jsRuntime;
        }

        public GeoItem? GetGeoData()
        {
            return _geoItem;
        }
        [JSInvokable]
        public async Task GeoLoaded(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    var items = System.Text.Json.JsonSerializer.Deserialize<string>(data).Split(";");

                    if (items != null && items.Count() > 1)
                    {
                        _geoItem = new GeoItem();
                        _geoItem.Lan = double.Parse(items[0].Replace(".",","));
                        _geoItem.Lat = double.Parse(items[1].Replace(".", ","));
                    }
                }
                catch { }
            }

            NotifyOnResponse();
        }
        private void NotifyOnResponse() => OnResponse?.Invoke();

        public async Task<bool> FetchGeoLocation()
        {
            _geoItem = new GeoItem();
            var Reference = DotNetObjectReference.Create(new JSRuntimeEventHelper(GeoLoaded));
            await _jsRuntime.InvokeVoidAsync("Geo_OpenUrl", Reference, "GetGeoData");

            return true;
        }
        public async Task<Address?> GetCoordinates(string Address, string? Culture = null)
        {
            if (Culture == null)
            {
                Culture = CultureInfo.CurrentCulture.Name;
            }

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.mapbox.com/");

            var requestUri = "search/geocode/v6/forward?q=" + HttpUtility.UrlEncode(Address, Encoding.UTF8);
            requestUri += "&access_token=pk.eyJ1IjoiaW5ub3ZhdGlvbmNvbnN1bHRpbmciLCJhIjoiY2x3OTZ0M3I2MDBnYTJpb2VxeTdqYW13ZiJ9.qyRAK_N6rrgiaRQLPeXIew";
            requestUri += "&country=IT";
            requestUri += "&language=" + Culture;

            var result = await client.GetAsync(requestUri);

            var data = await result.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(data))
            {
                dynamic d = JsonConvert.DeserializeObject(data);

                var a = new Address();

                try
                {
                    if (d.features[0].properties.context.address != null)
                    {
                        a.FormattedAddress = d.features[0].properties.context.address.name + ", " + d.features[0].properties.context.postcode.name + " " + d.features[0].properties.context.place.name;
                    }
                    else
                    {
                        var adress = "";

                        if(d.features[0].properties.context.name != null)
                        {
                            adress = d.features[0].properties.context.name;
                        }

                        if(d.features[0].properties.context.place_formatted != null)
                        {
                            adress += " " + d.features[0].properties.context.place_formatted;
                        }

                        a.FormattedAddress = adress;
                    }
                    a.Latitude = d.features[0].properties.coordinates.latitude;
                    a.Longitude = d.features[0].properties.coordinates.longitude;

                    return a;
                }
                catch(Exception ex) 
                {
                }
            }

            return new Address();
        }
        public async Task<Address?> GetAddress(Location Location, string? Culture = null)
        {
            if(Culture == null)
            {
                Culture = CultureInfo.CurrentCulture.Name;
            }

            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.mapbox.com/");

            var requestUri = "search/geocode/v6/reverse?longitude=" + Location.Longitude.ToString().Replace(",", ".");
            requestUri += "&latitude=" + Location.Latitude.ToString().Replace(",",".");
            requestUri += "&access_token=pk.eyJ1IjoiaW5ub3ZhdGlvbmNvbnN1bHRpbmciLCJhIjoiY2x3OTZ0M3I2MDBnYTJpb2VxeTdqYW13ZiJ9.qyRAK_N6rrgiaRQLPeXIew";
            requestUri += "&country=IT";
            requestUri += "&language=" + Culture;

            var result = await client.GetAsync(requestUri);

            var data = await result.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(data))
            {
                dynamic d = JsonConvert.DeserializeObject(data);

                var a = new Address();

                try
                {
                    a.FormattedAddress = d.features[0].properties.context.address.name + ", " + d.features[0].properties.context.postcode.name + " " + d.features[0].properties.context.place.name;
                    a.Latitude = d.features[0].properties.coordinates.latitude;
                    a.Longitude = d.features[0].properties.coordinates.longitude;

                    return a;
                }
                catch { }
            }

            return new Address();
        }
    }
}