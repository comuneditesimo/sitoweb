using DocumentFormat.OpenXml.Spreadsheet;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Documents
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

        private List<V_HOME_Document> Documents = new List<V_HOME_Document>();
        private List<V_HOME_Document_Type>? Types;
        private string? SearchText;
        int MaxCounter = 6;
        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_FRONTEND_DOCUMENTS");
            SessionWrapper.PageSubTitle = null;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Documents);

                if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                {
                    SessionWrapper.PageDescription = pagesubTitle.Description;
                }
                else
                {
                    SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FRONTEND_DOCUMENTS_DESCRIPTIONSHORT");
                }
            }

            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Document", "HOMEPAGE_FRONTEND_DOCUMENTS", null, null, true);

            if(SessionWrapper.AUTH_Municipality_ID != null)
            {
                var docs = await HomeProvider.GetDocuments(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                Documents = docs.Where(p => p.ReleaseDate == null || p.ReleaseDate <= DateTime.Now).ToList();
                Types = await HomeProvider.GetDocument_Types(LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void OnTypeClicked(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Type/Document/" + ID);
            StateHasChanged();
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 6;

            if (Documents.Count() < MaxCounter)
            {
                MaxCounter = Documents.Count();
            }

            StateHasChanged();
        }
    }
}
