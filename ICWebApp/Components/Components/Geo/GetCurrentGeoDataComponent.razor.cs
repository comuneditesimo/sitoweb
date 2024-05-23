using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.Models;
using ICWebApp.Domain.Models.Syncfusion;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace ICWebApp.Components.Components.Geo
{
    public partial class GetCurrentGeoDataComponent
    {
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ITEXTProvider TextProvider { get; set; }
        [Inject] public IDialogService? DialogService { get; set; }
        [Parameter] public EventCallback<MarkerData?> SetCurrentLocation { get; set; }
        [Parameter] public string? CssClass { get; set; }

        public async Task RequestGeoLocationAsync()
        {
            await RequestGeoLocationAsync(enableHighAccuracy: true, maximumAgeInMilliseconds: 0);
        }

        public async Task RequestGeoLocationAsync(bool enableHighAccuracy, int maximumAgeInMilliseconds)
        {
            DotNetObjectReference<GetCurrentGeoDataComponent> dotNetObjectReference = DotNetObjectReference.Create(this);

            await JSRuntime.InvokeVoidAsync(identifier: "getCurrentPosition",
                                            dotNetObjectReference,
                                            enableHighAccuracy,
                                            maximumAgeInMilliseconds);
        }

        [JSInvokable]
        public async Task OnSuccessAsync(MarkerData coordinates)
        {
            await SetCurrentLocation.InvokeAsync(coordinates);
        }

        [JSInvokable]
        public async Task OnErrorAsync(string error)
        {
            string item = error;
            if (DialogService != null)
            {
                await DialogService.OpenDialogWindow(TextProvider.Get("GEO_CANNOT_READ_ACTUAL_POSITION"), TextProvider.Get("GEO_DIALOG_ERROR_TITLE"), TextProvider.Get("GEO_DIALOG_CONFIRM_BUTTON"), TextProvider.Get("GEO_DIALOG_CANCEL_BUTTON"));
            }
            await SetCurrentLocation.InvokeAsync(null);
        }

    }
}
