using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Documents
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
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string? ID {  get; set; }

        private V_HOME_Document? Item;
        private HOME_Document_Data? ItemData;
        private List<V_HOME_Document_Type>? Types;
        private List<V_HOME_Person>? People;
        private List<V_HOME_Organisation>? Organisation;
        private List<V_HOME_Authority>? Authorities;

        protected override async void OnParametersSet()
        {
            if (ID == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Document");
                StateHasChanged();
                return;
            }

            Item = await HomeProvider.GetVDocument(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

            if (Item == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Document");
                StateHasChanged();
                return;
            }

            SessionWrapper.PageTitle = Item.Title;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = Item.DescriptionShort;
            SessionWrapper.ShowTitleSepparation = false;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Document", "HOMEPAGE_FRONTEND_DOCUMENTS", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Document/" + ID, Item.Title, null, null, true);

            ActionBarService.ClearActionBar();
            ActionBarService.ShowDefaultButtons = true;
            ActionBarService.ShowShareButton = true;

            if(Item != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                ActionBarService.ThemeList = await HomeProvider.GetThemesByDocument(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                
                ItemData = await HomeProvider.GetDocumentDataByLanguage(Item.ID, LangProvider.GetCurrentLanguageID());

                if(ItemData == null || ItemData.Data == null)
                {
                    ItemData = await HomeProvider.GetDocumentDataByLanguage(Item.ID, LanguageSettings.German);
                }

                Types = await HomeProvider.GetDocument_TypeByDocument(Item.ID, LangProvider.GetCurrentLanguageID());

                People = await HomeProvider.GetPeopleByDocument(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                Organisation = await HomeProvider.GetOrganisationsByDocument(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                Authorities = await HomeProvider.GetDocument_Authorities(Item.ID, LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
        private async void Download()
        {
            if (ItemData != null && Item != null) 
            {
                await EnviromentService.DownloadFile(ItemData.Data, Item.Title + ItemData.Fileextension, true);
                StateHasChanged();
            }
        }
        private void OnTypeClicked(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Type/Document/" + ID);
            StateHasChanged();
        }
    }
}
