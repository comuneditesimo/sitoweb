using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Venue
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

        private V_HOME_Venue_Type? Type;
        private List<V_HOME_Venue> Items = new List<V_HOME_Venue>();
        private List<V_HOME_Venue_Type>? Types;
        private string? SearchText;
        int MaxCounter = 6;

        protected override async void OnParametersSet()
        {
            if (TypeID == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Venue");
                StateHasChanged();
                return;
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Types = await HomeProvider.GetVenue_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            if (Types != null)
            {
                Type = Types.FirstOrDefault(p => p.ID == Guid.Parse(TypeID));
            }

            if (Type == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Venue");
                StateHasChanged();
                return;
            }

            SessionWrapper.PageTitle = Type.Type;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = Type.Description;
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Venue", "HOMEPAGE_FRONTEND_VENUES", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Venue/" + Type.ID.ToString(), Type.Type, null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null && Type != null)
            {
                Items = await HomeProvider.GetVenuesByType(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Type.ID);
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
        private void OnTypeClicked(Guid ID)
        {
            if (Type != null && ID != Type.ID)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Type/Venue/" + ID);
                StateHasChanged();
            }
        }
        private void ShowMore()
        {
            MaxCounter = MaxCounter + 6;

            if (Items.Count() < MaxCounter)
            {
                MaxCounter = Items.Count();
            }

            StateHasChanged();
        }
    }
}
