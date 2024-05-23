using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Amministration;
using ICWebApp.Domain.Models.Homepage.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Assistance
{
    public partial class Success
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IPRIVProvider PrivProvider { get; set; }
        [Inject] IHPServiceHelper HPServiceHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string? ID { get; set; }
        private HOME_Assistance? Data;

        protected override async Task OnInitializedAsync()
        {
            if(ID == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/");
                StateHasChanged();
                return;
            }

            Data = await HomeProvider.GetAssistance(Guid.Parse(ID));

            if (Data == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/");
                StateHasChanged();
                return;
            }

            SessionWrapper.PageTitle = TextProvider.Get("HOME_ASSISTANCE_SUCCESS_PAGE_TITLE");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HOME_ASSISTANCE_SUCCESS_PAGE_DESCRIPTION");
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowHelpSection = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Services", "HP_MAINMENU_DIENSTE", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Assistance", "HOME_ASSISTANCE_PAGE_TITLE", null, null, true);

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
    }
}
