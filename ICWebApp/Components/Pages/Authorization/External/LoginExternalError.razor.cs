using Blazored.LocalStorage;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Authorization.External
{
    public partial class LoginExternalError
    {
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IAccountService AccountService {get;set;}
        [Inject] IAUTHProvider AuthProvider {get;set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] Blazored.SessionStorage.ISessionStorageService SessionStorage { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ILocalStorageService LocalStorage { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] private IUrlService UrlService {get; set; }
        [Parameter] public string ErrorCode { get; set; }

        private string RedirectURL { get; set; }

        protected override void OnInitialized()
        {
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                RedirectURL = await SessionStorage.GetItemAsync<string>("RedirectURL");

                var MunicipalID = await LocalStorage.GetItemAsync<string>("Comunix.Municipality");

                if (MunicipalID != null)
                {
                    SessionWrapper.AUTH_Municipality_ID = Guid.Parse(MunicipalID);
                    //URLUPDATE --TOTEST
                    var baseUrl =
                        await UrlService.GetDefaultUrlForMunicipality(SessionWrapper.AUTH_Municipality_ID.Value,
                            LanguageSettings.German);
                    
                    if (!string.IsNullOrEmpty(baseUrl))
                    {
                        NavManager.NavigateTo("https://" + baseUrl + "/LoginError/" + ErrorCode, true);
                    }
                }

                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }
        private async void BackToLogin()
        {
            if (!string.IsNullOrEmpty(RedirectURL))
            {
                await SessionStorage.SetItemAsync("RedirectURL", RedirectURL);
                NavManager.NavigateTo("/Login/" + RedirectURL.Replace("/", "^"));
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
                return;
            }

            NavManager.NavigateTo("/Login");
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
        }
    }
}
