using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.JsonPatch.Internal;
using System.Security.Cryptography;

namespace ICWebApp.Components.Components.Layout.Frontend
{
    public partial class Footer
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IAccountService AccountService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IHPServiceHelper HPServiceHelper {  get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        private AUTH_Municipality? Municipality;
        private AUTH_Municipality_Footer? MunicipalityFooter;
        private List<AUTH_MunicipalityApps>? AktiveApps = new List<AUTH_MunicipalityApps>();
        private List<ServiceKategorieItems> Categories = new List<ServiceKategorieItems>();
        private List<V_HOME_Article_Type>? Newstypes;
        private bool IsHomepage = false;
        private V_HOME_Accessibility? Accessibility;
        private List<V_HOME_Thematic_Sites>? ThematicSites;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var municipality = await SessionWrapper.GetMunicipalityID();

                if (SessionWrapper != null && municipality != null)
                {
                    Municipality = await AuthProvider.GetMunicipality(municipality.Value);
                    MunicipalityFooter = AuthProvider.GetMunicipalityFooter(municipality.Value, LANGProvider.GetCurrentLanguageID());
                    IsHomepage = await AppProvider.HasApplicationAsync(municipality.Value, Applications.Homepage);

                    Accessibility = await HomeProvider.GetVAccessibility(municipality.Value, LANGProvider.GetCurrentLanguageID());

                    if (IsHomepage) 
                    {
                        Categories = HPServiceHelper.GetKategories(municipality.Value, LANGProvider.GetCurrentLanguageID());
                        //NEEDS TO BE SYNCHRONOUS FOR PERFORMANCE
                        Newstypes = HomeProvider.GetArticle_TypesSync(LANGProvider.GetCurrentLanguageID());

                        if (SessionWrapper.AUTH_Municipality_ID != null)
                        {
                            ThematicSites = HomeProvider.GetThematicsitesByTypeSync(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID(), HOMEThematicSiteTypes.Footer);
                        }
                    } 
                    else
                    {
                        AktiveApps = await AuthProvider.GetMunicipalityApps();
                    }

                }

                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }
        private void GoToRooms()
        {
            if (!NavManager.Uri.Contains("/Rooms"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Rooms");
                StateHasChanged();
            }
        }
        private void GoToMensa()
        {
            if (!NavManager.Uri.Contains("/Canteen"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Canteen");
                StateHasChanged();
            }
        }
        private void GoToApplications()
        {
            if (!NavManager.Uri.Contains("/Form"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Form");
                StateHasChanged();
            }
        }
        private void GoToNotifications()
        {
            if (!NavManager.Uri.Contains("/Mantainance"))
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Mantainance");

                StateHasChanged();
            }
        }
        private void GoToUrl(string Url)
        {
            if (!NavManager.Uri.Contains(Url))
            {
                if (!string.IsNullOrEmpty(Url))
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo(Url);

                    StateHasChanged();
                }
            }
        }
    }
}
