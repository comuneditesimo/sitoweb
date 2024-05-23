using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Person
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
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IContactHelper ContactHelper { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string? ID {  get; set; }

        private V_HOME_Person? Item;
        private List<V_HOME_Document>? Documents;
        private List<V_HOME_Organisation>? Organisations;
        private List<V_HOME_Venue>? Venues;
        private List<V_HOME_Authority>? Authorities;
        private List<V_HOME_Association>? Associations;
        private AUTH_Municipality? Municipality;
        private List<HOME_Person_Office_Hours>? PersonOfficeHours;

        protected override async void OnParametersSet()
        {
            if (ID == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Administration");
                StateHasChanged();
                return;
            }

            Item = await HomeProvider.GetVPerson(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

            if (Item == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Administration");
                StateHasChanged();
                return;
            }

            SessionWrapper.PageTitle = Item.Fullname;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = Item.DescriptionShort;
            SessionWrapper.ShowTitleSepparation = false;
            SessionWrapper.ShowQuestioneer = true;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Administration", "HP_MAINMENU_VERWALTUNG", null, null, false);

            if (Item.HOME_Person_Type_ID == PersonType.Politician)
            {
                CrumbService.AddBreadCrumb("/Hp/Type/Person/" + PersonType.Politician, Item.Type, null, null, false);
            }
            else
            {
                CrumbService.AddBreadCrumb("/Hp/Type/Person/" + PersonType.Employee, Item.Type, null, null, false);
            }

            CrumbService.AddBreadCrumb("/Hp/Person/" + ID, Item.Fullname, null, null, true);

            ActionBarService.ClearActionBar();
            ActionBarService.ShowDefaultButtons = true;
            ActionBarService.ShowShareButton = true;

            if(Item != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Documents = await HomeProvider.GetDocumentsByPerson(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                Organisations = await HomeProvider.GetOrganisationsByPerson(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                Venues = await HomeProvider.GetVenuesByPerson(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                Authorities = await HomeProvider.GetAuthoritiesByPerson(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                Associations = await HomeProvider.GetAssociationsByPerson(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.ID);
                Municipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);
                PersonOfficeHours = await HomeProvider.GetPersonOfficeHours(Item.ID);
            }

            if(Item != null && Item.AllowTimeslots == true)
            {
                SessionWrapper.PageButtonSecondaryAction = RequestAppointment;
                SessionWrapper.PageButtonSecondaryActionTitle = TextProvider.Get("HOMEPAGE_PERSON_REQUEST_APPOINTMENT");
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
        private void RequestAppointment()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Hp/Person/Request/" + ID);
            StateHasChanged();
        }
    }
}
