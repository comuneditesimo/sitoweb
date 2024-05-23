using Blazored.SessionStorage;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Redirect
{
    public partial class RedirectToLogin
    {
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] ISessionStorageService SessionStorage { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set;}
        [Parameter] public string? RedirectURL { get; set; }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!string.IsNullOrEmpty(RedirectURL))
            {
                RedirectURL = RedirectURL.Replace(NavManager.BaseUri, "");

                await SessionStorage.SetItemAsync("RedirectURL", RedirectURL);
                
                NavManager.NavigateTo("/Login/" + RedirectURL.Replace("/", "^"));
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
                return;
            }

            NavManager.NavigateTo("/Login");
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
