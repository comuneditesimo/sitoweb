using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Cookies
{
    public partial class ConsentCookie
    {
        [Inject] IHttpContextAccessor Http { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        bool showBanner;
        string cookieString;

        protected override void OnInitialized()
        {
            var consentFeature = Http.HttpContext.Features.Get<ITrackingConsentFeature>();
            showBanner = !consentFeature?.CanTrack ?? false;
            cookieString = consentFeature?.CreateConsentCookie();
        }

        private void AcceptMessage()
        {
            JSRuntime.InvokeVoidAsync("CookieFunction.acceptMessage", cookieString);
            NavManager.NavigateTo(NavManager.Uri, true);
        }
        private void ShowCookiePage()
        {
            JSRuntime.InvokeVoidAsync("CookieFunction.acceptMessage", cookieString);
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Hp/Cookies", true);
            StateHasChanged();
        }
    }
}
