using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.News
{
    public partial class Index
    {
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        private List<V_HOME_Article> News = new List<V_HOME_Article>();
        private List<V_HOME_Article_Type>? Types;
        private string? SearchText;
        int MaxCounter = 6;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("HP_MAINMENU_NEWS");
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = TextProvider.Get("FRONTEND_HOMEPAGE_NEWS_DESCRIPTION_SHORT");
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowQuestioneer = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/News", "HP_MAINMENU_NEWS", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var news = await HomeProvider.GetArticles(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                News = news.Where(p => p.ReleaseDate == null || p.ReleaseDate <= DateTime.Now).ToList();
                Types = await HomeProvider.GetArticle_Types(LangProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void OnNewsItemClicked(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/News/" + ID);
            StateHasChanged();
        }
        private void OnNewsTypeClicked(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/Type/News/" + ID);
            StateHasChanged();
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 6;

            if (News.Count() < MaxCounter)
            {
                MaxCounter = News.Count();
            }

            StateHasChanged();
        }
    }
}
