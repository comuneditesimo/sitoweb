using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Domain.Models;
using ICWebApp.Domain.Models.Syncfusion;
using ICWebApp.Components.Pages.Form.Frontend;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Maps;
using Telerik.Blazor.Components;
using ICWebApp.Domain.Models.Geocoding;

namespace ICWebApp.Components.Components.Homepage.Backend.Address
{
    public partial class Map
    {
        [Inject] IGeoService GeoService { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Parameter] public string? Address { get; set; }
        [Parameter] public string? Room { get; set; }
        [Parameter] public EventCallback<string?> AddressChanged { get; set; }
        [Parameter] public EventCallback<string?> RoomChanged { get; set; }
        [Parameter] public double? Latitude { get; set; }
        [Parameter] public double? Longitude { get; set; }
        [Parameter] public EventCallback<double?> LatitudeChanged { get; set; }
        [Parameter] public EventCallback<double?> LongitudeChanged { get; set; }
        [Parameter] public bool EnableRoom { get; set; } = false;
        [Parameter] public bool AllowDefault { get; set; } = true;
        [Parameter] public bool ShowAddressBar { get; set; } = true;
        [Parameter] public Guid? Language { get; set; }

        private MarkerData? Data;
        private SfMaps? SfMap;
        private string? _address
        {
            get
            {
                return Address;
            }
            set
            {
                Address = value;
                AddressChanged.InvokeAsync(Address);
            }
        }
        private string? _room
        {
            get
            {
                return Room;
            }
            set
            {
                Room = value;
                RoomChanged.InvokeAsync(Room);
            }
        }
        private bool MapInitialized = false;

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

            if (string.IsNullOrEmpty(Address) && Latitude == null && Longitude == null && AllowDefault)
            {
                var Municipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                if (Municipality != null)
                {
                    if (Municipality.Lat != null && Municipality.Lng != null)
                    {
                        Data.Longitude = Municipality.Lng.Value;
                        Data.Latitude = Municipality.Lat.Value;
                    }

                    Latitude = Data.Latitude;
                    Longitude = Data.Longitude;

                    await LatitudeChanged.InvokeAsync(Data.Latitude);
                    await LongitudeChanged.InvokeAsync(Data.Longitude);
                }

                await SetAdress();
            }
            else
            {
                if (!string.IsNullOrEmpty(Address))
                {
                    if (Latitude == null && Longitude == null)
                    {
                        await SetGeoLocation();
                    }
                }
                else if (Latitude != null && Longitude != null)
                {
                    if (string.IsNullOrEmpty(Address))
                    {
                        await SetAdress();
                    }
                }
            }

            StateHasChanged();
            MapInitialized = true;
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
                string? culture = null;

                if(Language != null)
                {
                    culture = LangProvider.GetCodeByLanguage(Language.Value);
                }

                var result = await GeoService.GetCoordinates(addressToGeocode, culture);

                if (result != null && Data != null && result.Longitude != null && result.Latitude != null)
                {
                    Data.Longitude = result.Longitude.Value;
                    Data.Latitude = result.Latitude.Value;

                    Latitude = Data.Latitude;
                    Longitude = Data.Longitude;

                    await LatitudeChanged.InvokeAsync(Data.Latitude);
                    await LongitudeChanged.InvokeAsync(Data.Longitude);
                }

                if (SfMap != null && MapInitialized)
                {
                    if (Data != null)
                    {
                        await SfMap.ZoomByPosition(new MapsCenterPosition() { Latitude = Data.Latitude, Longitude = Data.Longitude }, 18);
                    }
                }
            }

            StateHasChanged();
        }
        private async void SetCurrentLocation(MarkerData? _coordinates)
        {
            if (Data != null && _coordinates != null)
            {
                Data.Longitude = _coordinates.Longitude;
                Data.Latitude = _coordinates.Latitude;

                Latitude = Data.Latitude;
                Longitude = Data.Longitude;

                await LatitudeChanged.InvokeAsync(Data.Latitude);
                await LongitudeChanged.InvokeAsync(Data.Longitude);

                await SetAdress();

                if (SfMap != null && MapInitialized)
                {
                    if (Data != null)
                    {
                        await SfMap.ZoomByPosition(new MapsCenterPosition() { Latitude = Data.Latitude, Longitude = Data.Longitude }, 18);
                    }
                }

                StateHasChanged();
            }
        }
        private async Task<string?> SetAdress()
        {
            if (Data != null)
            {
                try
                {
                    string? culture = null;

                    if (Language != null)
                    {
                        culture = LangProvider.GetCodeByLanguage(Language.Value);
                    }

                    var result = await GeoService.GetAddress(new Location(Data.Latitude, Data.Longitude), culture);

                    if (result != null)
                    {
                        Address = result.FormattedAddress;

                        await AddressChanged.InvokeAsync(Address);
                    }

                    StateHasChanged();
                }
                catch { }
            }

            return Address;
        }
        private async void MapClicked(MouseEventArgs args)
        {
            if (Data != null && args != null)
            {
                Data.Longitude = args.Longitude;
                Data.Latitude = args.Latitude;

                Latitude = Data.Latitude;
                Longitude = Data.Longitude;

                await LatitudeChanged.InvokeAsync(Data.Latitude);
                await LongitudeChanged.InvokeAsync(Data.Longitude);

                await SetAdress();

                if (SfMap != null && MapInitialized)
                {
                    if (Data != null)
                    {
                        await SfMap.ZoomByPosition(new MapsCenterPosition() { Latitude = Data.Latitude, Longitude = Data.Longitude }, 18);
                    }
                }

                StateHasChanged();
            }
        }
    }
}
