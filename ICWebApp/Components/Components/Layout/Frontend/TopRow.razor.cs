using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Application.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ICWebApp.Application.Provider;
using Microsoft.Identity.Client;

namespace ICWebApp.Components.Components.Layout.Frontend
{
    public partial class TopRow
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] private IUrlService UrlService { get; set; }
        [Parameter] public bool ShowMenu { get; set; } = false;
        [Parameter] public bool ShowUserData { get; set; } = false;
        [Parameter] public bool ShowSearchButton { get; set; } = true;
        private AUTH_Municipality? Municipality { get; set; }
        private bool IsHomepage = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                SessionWrapper.PageIsPublic = true;
                SessionWrapper.OnAUTHMunicipalityIDChanged += SessionWrapper_OnAUTHMunicipalityIDChanged;

                if (SessionWrapper.AUTH_Municipality_ID == null)
                {
                    var baseUri = NavManager.BaseUri;
                    var matchedMun = await UrlService.GetMunicipalityByUrl(baseUri);

                    if (matchedMun != null)
                    {
                        SessionWrapper.AUTH_Municipality_ID = matchedMun.AUTH_Municipality_ID;
                    }
                    else if (!baseUri.ToLower().Contains("spid"))
                    {
                        NavManager.NavigateTo("https://innovation-consulting.it/", true);
                    }
                }

                //Gets mun ID from local storage if municipality ID still null, should never happen!
                var municipality = await SessionWrapper.GetMunicipalityID();

                if (SessionWrapper != null && municipality != null)
                {
                    Municipality = await AuthProvider.GetMunicipality(municipality.Value);
                    IsHomepage = await AppProvider.HasApplicationAsync(municipality.Value, Applications.Homepage);
                }  

                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }
        private void SessionWrapper_OnAUTHMunicipalityIDChanged()
        {
            StateHasChanged();
        }
        private void ReturnToStart()
        {
            if (NavManager.Uri != NavManager.BaseUri)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/");
                StateHasChanged();
            }
        }
    }
}
