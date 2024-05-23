using DocumentFormat.OpenXml.Spreadsheet;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Organisation
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider {  get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        private List<V_HOME_Organisation> Organisations = new List<V_HOME_Organisation>();
        private List<V_HOME_Organisation_Type>? Types;
        private string? SearchText;
        int MaxCounter = 6;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_FRONTEND_ORGANISATIONS");
            SessionWrapper.PageSubTitle = null;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Organisations);

                if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                {
                    SessionWrapper.PageDescription = pagesubTitle.Description;
                }
                else
                {
                    SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FRONTEND_ORGANISATIONS_DESCRIPTIONSHORT");
                }
            }

            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Organisation", "HOMEPAGE_FRONTEND_ORGANISATIONS", null, null, true);

            if(SessionWrapper.AUTH_Municipality_ID != null)
            {
                Organisations = await HomeProvider.GetOrganisations(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                Types = await HomeProvider.GetOrganisation_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void OnTypeClicked(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Type/Organisation/" + ID);
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
