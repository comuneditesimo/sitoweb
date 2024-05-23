using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Domain.Models;
using ICWebApp.Domain.Models.Syncfusion;
using ICWebApp.Components.Pages.Form.Frontend;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Common;
using Microsoft.JSInterop;
using System.Data.SqlTypes;

namespace ICWebApp.Components.Components.Homepage.Frontend.Address
{
    public partial class Map
    {
        [Inject] IGeoService GeoService { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string? Address { get; set; }
        [Parameter] public string? Room { get; set; }
        [Parameter] public double? Latitude { get; set; }
        [Parameter] public double? Longitude { get; set; }

        private MarkerData? Data;
        string? SWMap = null;
        public string[] Subdomains { get; set; } = new string[] { "a", "b", "c" };
        public string UrlTemplate { get; set; } = "https://#= subdomain #.tile.openstreetmap.org/#= zoom #/#= x #/#= y #.png";
        public string Attribution { get; set; } = "&copy; <a href='https://osm.org/copyright'>OpenStreetMap contributors</a>";

        protected override async void OnInitialized()
        {
            if(Data == null)
            {
                Data = new MarkerData();
            }

            Data.Name = TextProvider.Get("BACKEND_HOMEPAGE_MAP_LOCATION");

            if (!string.IsNullOrEmpty(Address))
            {
                Data.Name = Address;
            }
            if(Latitude != null && Longitude != null)
            {
                Data.Latitude = Latitude.Value;
                Data.Longitude = Longitude.Value;
            }

            if (!string.IsNullOrEmpty(Address))
            {
                if (Data.Latitude == null && Data.Longitude == null)
                {
                    await SetGeoLocation();
                }
            }
            else if(Latitude != null && Longitude != null)
            {
                if (string.IsNullOrEmpty(Data.Name))
                {
                    Data.Latitude = Latitude.Value;
                    Data.Longitude = Longitude.Value;
                }
            }

            SWMap = @"https://www.google.com/maps/place/" + Data.Latitude.ToString().Replace(",", ".") + "," + Data.Longitude.ToString().Replace(",", ".");

            StateHasChanged();
            base.OnInitialized();
        }
        private async Task SetGeoLocation()
        {
            string addressToGeocode = "";

            if (Address != null)
            {
                addressToGeocode = Address;
            }

            addressToGeocode += ", Italien";

            if (!string.IsNullOrEmpty(addressToGeocode))
            {
                var result = await GeoService.GetCoordinates(addressToGeocode);

                if (result != null && Data != null && result.Longitude != null && result.Latitude != null)
                {
                    Data.Longitude = result.Longitude.Value;
                    Data.Latitude = result.Latitude.Value;
                }
            }

            StateHasChanged();
        }    
        private void OpenMaps()
        {
            if(SWMap != null)
            {
                JSRuntime.InvokeVoidAsync("openInNewTab", SWMap);
            }
        }
    }
}
