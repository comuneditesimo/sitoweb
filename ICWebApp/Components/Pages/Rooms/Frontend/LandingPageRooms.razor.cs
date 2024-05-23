using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Rooms.Frontend
{
    public partial class LandingPageRooms
    {
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        protected override async void OnInitialized()
        {
            var IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);

            if (!IsHomepage)
            {
                NavManager.NavigateTo("/Rooms/Service");
                return;
            }

            StateHasChanged();
            base.OnInitialized();
        }
    }
}
