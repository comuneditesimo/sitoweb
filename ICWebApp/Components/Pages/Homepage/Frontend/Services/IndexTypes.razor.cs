using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.Models.Homepage.Services;
using ICWebApp.Components.Pages.Form.Admin;
using Microsoft.AspNetCore.Components;
using System.Security.Cryptography;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Services
{
    public partial class IndexTypes
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
        [Inject] ILANGProvider LangProvider {get;set;}
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string KategorieID { get; set; }

        private ServiceKategorieItems? Kategorie;
        private List<ServiceItem>? Items;
        private List<ServiceKategorieItems>? Kategories;
        private List<ServiceAuthorityItems>? Authorities;
        private string? SearchText;
        int MaxCounter = 5;

        protected override async void OnParametersSet()
        {
            if (string.IsNullOrEmpty(KategorieID))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Services");
                StateHasChanged();
                return;
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Kategories = HPServiceHelper.GetKategories(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                if (Kategories != null)
                {
                    Kategorie = Kategories.FirstOrDefault(p => p.ID == Guid.Parse(KategorieID));
                }
            }

            if (Kategorie == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Services");
                StateHasChanged();
                return;
            }

            SessionWrapper.PageTitle = Kategorie.Title;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.PageDescription = Kategorie.ShortText;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;
            SessionWrapper.DataElement = "service-title";

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Services", "HP_MAINMENU_DIENSTE", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Type/Services/" + Kategorie.ID, null, null, Kategorie.Title, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Items = HPServiceHelper.GetServices(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            if (Kategorie != null)
            {
                Authorities = await HPServiceHelper.GetAuthorities(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Kategorie.ID);
            }

            try
            {
                EnviromentService.ScrollToTop();
            }
            catch { }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
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
        private void OnAuthorityClicked(string? Url)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo(Url);
                StateHasChanged();
            }
        }
        private void GoToAmministration()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Hp/Administration");
            StateHasChanged();            
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
