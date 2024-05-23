using DocumentFormat.OpenXml.Spreadsheet;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Association
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider {  get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        private List<V_HOME_Association> Organisations = new List<V_HOME_Association>();
        private List<V_HOME_Association_Type>? Types;
        private string? SearchText;
        int MaxCounter = 6;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_FRONTEND_ASSOCIATIONS");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FRONTEND_ASSOCIATIONS_DESCRIPTIONSHORT");
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Association", "HOMEPAGE_FRONTEND_ASSOCIATIONS", null, null, true);

            if(SessionWrapper.AUTH_Municipality_ID != null)
            {
                Organisations = await HomeProvider.GetAssociations(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                Types = await HomeProvider.GetAssociation_Type(LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void OnTypeClicked(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Type/Association/" + ID);
            StateHasChanged();
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 6;

            if (Organisations.Count() < MaxCounter)
            {
                MaxCounter = Organisations.Count();
            }

            StateHasChanged();
        }
    }
}
