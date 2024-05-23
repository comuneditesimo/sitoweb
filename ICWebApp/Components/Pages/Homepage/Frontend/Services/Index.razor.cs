using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.Models.Homepage.Services;
using ICWebApp.Components.Pages.Form.Admin;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Services
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] IHPServiceHelper HPServiceHelper { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        private List<ServiceItem>? Items;
        private List<ServiceKategorieItems>? Kategories;
        private string? SearchText;
        int MaxCounter = 5;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HP_MAINMENU_DIENSTE");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.PageDescription = TextProvider.Get("HP_MAINMENU_DIENSTE_DESCRIPTION_SHORT");
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;
            SessionWrapper.DataElement = "service-title";

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Services", "HP_MAINMENU_DIENSTE", null, null, true);

            GetData();

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private bool GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Items = HPServiceHelper.GetServices(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                Kategories = HPServiceHelper.GetKategories(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            return true;
        }
        private void OnItemClicked(string? Url)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo(Url);
                StateHasChanged();
            }
        }
        private void OnKategorieClicked(string? Url)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo(Url);
                StateHasChanged();
            }
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 5;

            if (Items != null && Items.Count() < MaxCounter)
            {
                MaxCounter = Items.Count();
            }

            StateHasChanged();
        }
    }
}
