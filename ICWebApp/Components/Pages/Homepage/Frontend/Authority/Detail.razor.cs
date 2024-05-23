using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Authority
{
    public partial class Detail
    {
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IActionBarService ActionBarService { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string? ID {  get; set; }

        private V_HOME_Authority? Item;
        private List<AUTH_Authority_Office_Hours>? OfficeHours;
        private List<V_HOME_Person> People { get; set; }
        private List<V_HOME_Document>? Documents { get; set; }

        protected override async void OnParametersSet()
        {
            if (ID == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Authority");
                StateHasChanged();
                return;
            }

            Item = await HomeProvider.GetAuthority(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

            if (Item == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Authority");
                StateHasChanged();
                return;
            }

            SessionWrapper.PageTitle = Item.Title;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = Item.DescriptionShort;
            SessionWrapper.ShowTitleSepparation = false;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Authority", "HOMEPAGE_FRONTEND_AUTHORITIES", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Authority/" + ID, Item.Title, null, null, true);

            ActionBarService.ClearActionBar();
            ActionBarService.ShowDefaultButtons = true;
            ActionBarService.ShowShareButton = true;

            if(Item != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                ActionBarService.ThemeList = await HomeProvider.GetThemesByAuthority(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                People = await HomeProvider.GetPeopleByAuthority(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                Documents = await HomeProvider.GetDocumentsByAuthority(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                OfficeHours = await AuthProvider.GetOfficeHours(Item.ID);
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
    }
}
