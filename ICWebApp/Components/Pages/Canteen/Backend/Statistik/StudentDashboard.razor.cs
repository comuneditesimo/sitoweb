using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.Security.Cryptography;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor;

namespace ICWebApp.Components.Pages.Canteen.Backend.Statistik
{
    public partial class StudentDashboard
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] private IBreadCrumbService CrumbService { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }

        private List<V_CANTEEN_Daily_Movements> DailyMovements = new List<V_CANTEEN_Daily_Movements>();
        private List<CANTEEN_School> Schools = new List<CANTEEN_School>();
        private List<CANTEEN_Canteen> Canteens = new List<CANTEEN_Canteen>();
        private List<CANTEEN_MealMenu?> MenuTypes = new List<CANTEEN_MealMenu?>();
        private CANTEEN_Configuration? _config;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_CANTEEN_STUDENTS_DASHBOARD");
            SessionWrapper.PageSubTitle = TextProvider.Get("");
            CrumbService.ClearBreadCrumb();

            CrumbService.AddBreadCrumb("/Backend/Canteen/Subscriptionlist", "MAINMENU_CANTEEN", null, null, true);
            CrumbService.AddBreadCrumb("/Admin/Students/Dashboard", "MAINMENU_BACKEND_CANTEEN_STUDENTS_DASHBOARD", null, null);

            await GetData();

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async Task<bool> GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                MenuTypes = await CanteenProvider.GetCANTEEN_MealMenuList(SessionWrapper.AUTH_Municipality_ID.Value);

                DailyMovements = await CanteenProvider.GetSubscriberMovementsDaily(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                var schools = await CanteenProvider.GetSchools(SessionWrapper.AUTH_Municipality_ID.Value);

                Schools = schools.OrderBy(p => p.Name).ToList();

                var canteens = await CanteenProvider.GetCanteens(SessionWrapper.AUTH_Municipality_ID.Value);

                Canteens = canteens.OrderBy(p => p.Name).ToList();
                _config = await CanteenProvider.GetConfiguration(SessionWrapper.AUTH_Municipality_ID.Value);
            }

            return true;
        }
        private void GoToDailyStudentList()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Admin/Students/DailyStudentlist");
            StateHasChanged();
        }
        private void GoToStudentList()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Admin/Students/Studentlist");
            StateHasChanged();
        }
        private void GoToCanteenList(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Admin/Students/Studentlist/Canteen/" + ID);
            StateHasChanged();
        }
        private void GoToDailyCanteenList(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Admin/Students/DailyStudentlist/Canteen/" + ID );
            StateHasChanged();
        }
        private void GoToSchoolList(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Admin/Students/Studentlist/School/" + ID);
            StateHasChanged();
        }
        private void GoToDailySchoolList(Guid ID)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Admin/Students/DailyStudentlist/School/" + ID);
            StateHasChanged();
        }
    }
}