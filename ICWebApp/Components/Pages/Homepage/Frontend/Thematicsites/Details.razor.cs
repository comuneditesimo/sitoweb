using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Thematicsites
{
    public partial class Details
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager {  get; set; }
        [Parameter] public string ID { get; set; }
        private V_HOME_Thematic_Sites? Data;
        private List<HOME_Thematic_Sites_Document>? Documents;
        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowHelpSection = true;
            SessionWrapper.ShowTitleSmall = true;

            if(string.IsNullOrEmpty(ID))
            {
                NavManager.NavigateTo("/");
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
                return;
            }

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Sites", "HOMEPAGE_FOOTER_COOKIE_TITLE", null, null, true);

            Data = await HomeProvider.GetVThematicsite(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

            if(Data == null)
            {
                NavManager.NavigateTo("/");
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
                return;
            }

            Documents = await HomeProvider.GetThematicsite_Documents(Data.ID);

            SessionWrapper.PageTitle = Data.Title;
            SessionWrapper.PageDescription = Data.DescriptionShort;

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
    }
}
