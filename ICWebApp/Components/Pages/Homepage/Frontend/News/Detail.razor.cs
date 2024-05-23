using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.News
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
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string? ID {  get; set; }

        private V_HOME_Article? News;
        private List<V_HOME_Document>? Documents;

        protected override async void OnParametersSet()
        {
            if (ID == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/News");
                StateHasChanged();
                return;
            }

            News = await HomeProvider.GetVArticle(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

            if (News == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/News");
                StateHasChanged();
                return;
            }

            SessionWrapper.PageTitle = News.Subject;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = News.ShortText;
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowQuestioneer = true;

            if (News.ReleaseDate != null)
            {
                SessionWrapper.ReleaseDate = News.ReleaseDate;
            }
            else
            {
                SessionWrapper.ReleaseDate = News.CreationDate;
            }

            if (!string.IsNullOrEmpty(News.Content))
            {
                var words = News.Content.Split(' ');

                var duration = (int)(words.Count() / 200);

                if(duration < 2)
                {
                    SessionWrapper.ReadDuration = 2;
                }
                else
                {
                    SessionWrapper.ReadDuration = duration;
                }
            }


            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/News", "HP_MAINMENU_NEWS", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/News/" + ID, News.Subject, null, null, true);

            ActionBarService.ClearActionBar();
            ActionBarService.ShowDefaultButtons = true;
            ActionBarService.ShowShareButton = true;

            if(News != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                ActionBarService.ThemeList = await HomeProvider.GetThemesByArticle(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), News.ID);
                Documents = await HomeProvider.GetDocumentsByArticle(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), News.ID);
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
    }
}
