using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Newsletter
{
    public partial class IndexTypes
    {
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string? TypeID {  get; set; }

        private V_HOME_Municipal_Newsletter_Type? Type;
        private List<V_HOME_Municipal_Newsletter> News = new List<V_HOME_Municipal_Newsletter>();
        private List<V_HOME_Municipal_Newsletter_Type>? Types;
        private string? SearchText;
        int MaxCounter = 6;

        protected override async void OnParametersSet()
        {
            if (TypeID == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Newsletter");
                StateHasChanged();
                return;
            }

            Type = await HomeProvider.GetNewsletter_Type(Guid.Parse(TypeID), LangProvider.GetCurrentLanguageID());

            if (Type == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Newsletter");
                StateHasChanged();
                return;
            }

            SessionWrapper.PageTitle = Type.Type;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = Type.Description;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowQuestioneer = true;
            SessionWrapper.ShowTitleSepparation = false;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Villagelife", "HP_MAINMENU_DORFLEBEN", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Newsletter", "HOMEPAGE_FRONTEND_MUNICIPAL_NEWSLETTER", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Type/Newsletter/" + Type.ID.ToString(), Type.Type, null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null && Type != null)
            {
                var news = await HomeProvider.GetMunicipalNewslettersByType(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Type.ID);
                News = news.Where(p => p.ReleaseDate == null || p.ReleaseDate <= DateTime.Now).ToList();
                Types = await HomeProvider.GetNewsletter_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            try
            {
                EnviromentService.ScrollToTop();
            }
            catch { }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
        private void OnNewsTypeClicked(Guid ID)
        {
            if (Type != null && ID != Type.ID)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Type/Newsletter/" + ID);
                StateHasChanged();
            }
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
