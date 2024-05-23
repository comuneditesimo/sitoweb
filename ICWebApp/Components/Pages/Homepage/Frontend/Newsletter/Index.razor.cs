using DocumentFormat.OpenXml.Spreadsheet;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Newsletter
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

        private List<V_HOME_Municipal_Newsletter> Newsletters = new List<V_HOME_Municipal_Newsletter>();
        private List<V_HOME_Municipal_Newsletter_Type>? Types;
        private string? SearchText;
        int MaxCounter = 6;
        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_FRONTEND_MUNICIPAL_NEWSLETTER");
            SessionWrapper.PageSubTitle = null;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.MunicipalNewsletter);

                if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                {
                    SessionWrapper.PageDescription = pagesubTitle.Description;
                }
                else
                {
                    SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FRONTEND_MUNICIPAL_NEWSLETTER_DESCRIPTION_SHORT");
                }
            }

            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Villagelife", "HP_MAINMENU_DORFLEBEN", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Newsletter", "HOMEPAGE_FRONTEND_MUNICIPAL_NEWSLETTER", null, null, true);

            if(SessionWrapper.AUTH_Municipality_ID != null)
            {
                var docs = await HomeProvider.GetMunicipalNewsletters(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                Newsletters = docs.Where(p => p.ReleaseDate == null || p.ReleaseDate <= DateTime.Now).ToList();
                Types = await HomeProvider.GetNewsletter_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void OnNewsTypeClicked(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Type/Newsletter/" + ID);
            StateHasChanged();            
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 6;

            if (Newsletters.Count() < MaxCounter)
            {
                MaxCounter = Newsletters.Count();
            }

            StateHasChanged();
        }
    }
}
