using DocumentFormat.OpenXml.Drawing.Diagrams;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Form.Frontend.Mantainance
{
    public partial class MantainanceList
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }

        private List<FORM_Definition> DefinitionList { get; set; }
        private List<AUTH_Authority> AuthorityList { get; set; }
        private string? _keyword;
        private string? Keyword
        {
            get
            {
                return _keyword;
            }
            set
            {
                _keyword = value;
                ShowCount = 6;
                StateHasChanged();
            }
        }
        private int ShowCount { get; set; } = 6;
        private bool IsHomepage = false;

        protected override async Task OnInitializedAsync()
        {
            StateHasChanged();

            await GetData();

            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);
            }

            if (IsHomepage)
            {
                SessionWrapper.PageTitle = TextProvider.Get("FRONTEND_MANTAINANCE_TITLE");
                SessionWrapper.PageSubTitle = null;
                
                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Maintenance);

                    if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                    {
                        SessionWrapper.PageDescription = pagesubTitle.Description;
                    }
                    else
                    {
                        SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FRONTEND_MAINTENANCE_DESCRIPTION");
                    }
                }

                SessionWrapper.ShowHelpSection = true;

                CrumbService.ClearBreadCrumb();
                CrumbService.AddBreadCrumb("/Hp/Services", "HP_MAINMENU_DIENSTE", null);
                CrumbService.AddBreadCrumb("/Hp/Type/Services/" + HomepageServices.Maintenance, null, null, TextProvider.Get("FRONTEND_MANTAINANCE_TITLE"));                
                CrumbService.AddBreadCrumb("/Mantainance", "FRONTEND_MANTAINANCE_TITLE", null);
            }
            else 
            { 
                SessionWrapper.PageTitle = TextProvider.Get("FRONTEND_MANTAINANCE_LIST");

                CrumbService.ClearBreadCrumb();
                CrumbService.AddBreadCrumb("/Mantainance", "FRONTEND_MANTAINANCE_LIST", null);
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async Task<bool> GetData()
        {
            if(SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                var def = await FormDefinitionProvider.GetDefinitionListByCategory(SessionWrapper.AUTH_Municipality_ID.Value, FORMCategories.Maintenance);

                DefinitionList = def.Where(p => p.Enabled == true).ToList();
            }

            return true;
        }
        private void NavigateTo(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (!NavManager.Uri.EndsWith(url))
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo(url);
                    StateHasChanged();
                }
            }
        }
        private void IncreaseShowCount()
        {
            ShowCount += 6;
            StateHasChanged();
        }
        private async void ScrollToTop()
        {
            await EnviromentService.ScrollToTop();
        }
    }
}
