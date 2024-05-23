using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.PagoPA.Domain;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Telerik.Reporting;

namespace ICWebApp.Components.Pages.Landing.Frontend
{
    public partial class Landing
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] INEWSProvider NEWSProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string ID { get; set; }

        private List<AUTH_MunicipalityApps>? AktiveApps = new List<AUTH_MunicipalityApps>();
        private NEWS_Article? Article;
        private V_HOME_Article? News;
        private List<V_HOME_Article_Theme>? Themes;
        private List<V_HOME_Theme>? HighlightThemes;
        private List<V_HOME_Appointment>? Events;
        private List<V_HOME_Thematic_Sites>? ThematicSites;
        private AUTH_Municipality? Municipality;
        private bool IsHomepage = false;
        private string? SearchText;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = null;
            SessionWrapper.PageSubTitle = null; 
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowQuestioneer = true;
            SessionWrapper.ShowBreadcrumb = false;

            CrumbService.ClearBreadCrumb();
            CrumbService.ShowBreadCrumb = false;

            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Municipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);
            }

            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);

                if (IsHomepage == false)
                {
                    var articles = await NEWSProvider.GetArticleList(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID(), 30);

                    if (articles != null)
                    {
                        if (!string.IsNullOrEmpty(ID))
                        {
                            Article = articles.Where(p => p.ID == Guid.Parse(ID)).FirstOrDefault();

                            if (Article == null)
                            {
                                Article = articles.OrderByDescending(p => p.PublishingDate).FirstOrDefault();
                            }
                        }
                        else
                        {
                            Article = articles.OrderByDescending(p => p.PublishingDate).FirstOrDefault();
                        }
                    }

                    AktiveApps = await AuthProvider.GetMunicipalityApps();
                }
                else
                {
                    //NEEDS TO BE SYNCHRONOUS FOR PERFORMANCE
                    News = HomeProvider.GetLatestArticle(LANGProvider.GetCurrentLanguageID());

                    if (News != null)
                    {
                        Themes = HomeProvider.GetVThemesByArticle(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID(), News.ID);
                    }

                    Events = HomeProvider.GetCurrentMonthEvents(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
                    HighlightThemes = HomeProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
                    ThematicSites = await HomeProvider.GetThematicsitesByType(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID(), HOMEThematicSiteTypes.Landing);
                    //*******/
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                if (IsHomepage == false)
                {
                    var articles = await NEWSProvider.GetArticleList(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID(), 30);

                    if (articles != null)
                    {
                        if (!string.IsNullOrEmpty(ID))
                        {
                            Article = articles.Where(p => p.ID == Guid.Parse(ID)).FirstOrDefault();

                            if (Article == null)
                            {
                                Article = articles.OrderByDescending(p => p.PublishingDate).FirstOrDefault();
                            }
                        }
                        else
                        {
                            Article = articles.OrderByDescending(p => p.PublishingDate).FirstOrDefault();
                        }
                    }
                }
                else
                {
                    News = HomeProvider.GetLatestArticle(LANGProvider.GetCurrentLanguageID());

                    if (News != null)
                    {
                        Themes = HomeProvider.GetVThemesByArticle(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID(), News.ID);
                    }
                }
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnParametersSetAsync();
        }   
        private void GoToNewsType()
        {
            if (News != null && News.HOME_Article_Type_ID != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Type/News/" + News.HOME_Article_Type_ID);
                StateHasChanged();
            }
        }
        private void GoToNews()
        {
            if (News != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/News/" + News.ID);
                StateHasChanged();
            }
        }
        private void GoToTheme(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Hp/Theme/" + ID);
            StateHasChanged();
        }
        private void GoToEvent(Guid ID)
        {
            if (News != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Event/" + ID);
                StateHasChanged();
            }
        }
        private void GoToAllThemes()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Hp/Theme");
            StateHasChanged();
        }
        private void GoToSearchPage()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/hp/search/" + SearchText);
            StateHasChanged();
        }
        private void GoToUrl(string Url)
        {
            if (!NavManager.Uri.Contains(Url))
            {
                if (!string.IsNullOrEmpty(Url))
                {
                    BusyIndicatorService.IsBusy = true;
                    NavManager.NavigateTo(Url);

                    StateHasChanged();
                }
            }
        }
    }
}
