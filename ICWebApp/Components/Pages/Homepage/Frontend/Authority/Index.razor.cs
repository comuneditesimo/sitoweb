using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Authority
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

        private List<V_HOME_Authority> Authorities = new List<V_HOME_Authority>();
        private string? SearchText;
        int MaxCounter = 6;
        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HOMEPAGE_FRONTEND_AUTHORITIES");
            SessionWrapper.PageSubTitle = null;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Authorities);

                if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                {
                    SessionWrapper.PageDescription = pagesubTitle.Description;
                }
                else
                {
                    SessionWrapper.PageDescription = TextProvider.Get("HOMEPAGE_FRONTEND_AUTHORITIES_DESCRIPTION_SHORT");
                }
            }

            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;
            SessionWrapper.ShowQuestioneer = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Authority", "HOMEPAGE_FRONTEND_AUTHORITIES", null, null, true);

            if(SessionWrapper.AUTH_Municipality_ID != null)
            {
                Authorities = await HomeProvider.GetAuthorities(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 6;

            if (Authorities.Count() < MaxCounter)
            {
                MaxCounter = Authorities.Count();
            }

            StateHasChanged();
        }
    }
}
