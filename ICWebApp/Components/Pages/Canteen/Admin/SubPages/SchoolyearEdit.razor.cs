using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using System.Text;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Common.Dropdowns;
using Telerik.Blazor.Components.Editor;
using Syncfusion.Blazor.Popups;

namespace ICWebApp.Components.Pages.Canteen.Admin.SubPages
{
    public partial class SchoolyearEdit
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ID { get; set; }

        private TelerikGrid<CANTEEN_Schoolyear_RegistrationPeriods>? Grid { get; set; }
        private CANTEEN_Schoolyear? Data { get; set; }
        private List<StringDropDownItem> YearList = new List<StringDropDownItem>();
        private List<CANTEEN_Schoolyear_Base_Informations> SchoolYear_BaseInformations = new List<CANTEEN_Schoolyear_Base_Informations>();
        private List<CANTEEN_Schoolyear_RegistrationPeriods> Periods = new List<CANTEEN_Schoolyear_RegistrationPeriods>();
        private bool ShowPeriodWindow = false;
        private CANTEEN_Schoolyear_RegistrationPeriods? PeriodToEdit;
        private string _validationPeriodError = "";
        protected override async Task OnInitializedAsync()
        {
            if (ID == "New")
            {
                SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_CANTEEN_SCHOOLSYEAR_ADD");

                Data = new CANTEEN_Schoolyear();
                int _actualYear = DateTime.Today.Year;

                Data.ID = Guid.NewGuid();
                Data.DisplayText = _actualYear.ToString() + "-" + (_actualYear + 1).ToString();
                Data.BeginDate = DateTime.Now;
                Data.EndDate = DateTime.Now.AddYears(1);


                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                }
            }
            else
            {
                SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_CANTEEN_SCHOOLSYEAR_EDIT");

                Data = await CanteenProvider.GetSchoolyear(Guid.Parse(ID));

                if (Data == null)
                {
                    ReturnToPreviousPage();
                    return;
                }
            }

            SchoolYear_BaseInformations = await CanteenProvider.GetSchoolyear_BaseInformations();

            List<V_CANTEEN_Schoolyear> _alreadyCreatedSchoolYear = new List<V_CANTEEN_Schoolyear>();
            var startdate = DateTime.Now;
            if (Data.AUTH_Municipality_ID != null)
            {
                _alreadyCreatedSchoolYear = await CanteenProvider.GetSchoolsyearAll(Data.AUTH_Municipality_ID.Value);
            }

            while (startdate < DateTime.Now.AddYears(2))
            {
                StringDropDownItem _schoolYear = new StringDropDownItem()
                {
                    Value = startdate.Year.ToString() + "-" + (startdate.Year + 1).ToString(),
                    Text = startdate.Year.ToString() + "-" + (startdate.Year + 1).ToString()
                };
                if (_alreadyCreatedSchoolYear == null || !_alreadyCreatedSchoolYear.Any() || !_alreadyCreatedSchoolYear.Where(p => p.id != Data.ID).Select(p => p.DisplayText).Contains(_schoolYear.Text))
                {
                    YearList.Add(_schoolYear);
                }

                startdate = startdate.AddYears(1);
            }
            if (!YearList.Select(p => p.Text).Contains(Data.DisplayText))
            {
                StringDropDownItem? _firstYear = YearList.FirstOrDefault();
                if (_firstYear != null)
                {
                    Data.DisplayText = _firstYear.Value;
                }
            }

            if (ID == "New")
            {
                SetStartEndDate(Data);
            }
            Periods = await CanteenProvider.GetRegistrationPeriodList(Data.ID);

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private void SetStartEndDate(CANTEEN_Schoolyear? _schoolYear)
        {
            if (_schoolYear != null)
            {
                CANTEEN_Schoolyear_Base_Informations? _matchingItem = SchoolYear_BaseInformations.FirstOrDefault(p => p.SchoolYears == _schoolYear.DisplayText);
                if (_matchingItem != null)
                {
                    if (_matchingItem.BeginYear != null)
                    {
                        _schoolYear.BeginYear = _matchingItem.BeginYear.Value;
                    }
                    if (_matchingItem.Startdate != null)
                    {
                        _schoolYear.BeginDate = _matchingItem.Startdate;
                    }
                    if (_matchingItem.EndYear != null)
                    {
                        _schoolYear.EndYear = _matchingItem.EndYear.Value;
                    }
                    if (_matchingItem.Enddate != null)
                    {
                        _schoolYear.EndDate = _matchingItem.Enddate;
                    }
                }
            }
        }
        private void ReturnToPreviousPage()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            NavManager.NavigateTo("/Admin/Canteen/SchoolyearManagement");
        }
        private async void SaveForm()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            if (Data != null)
            {
                if (!string.IsNullOrEmpty(Data.DisplayText))
                {
                    var beginyear = Data.DisplayText.Split("-")[0];
                    var endyear = Data.DisplayText.Split("-")[1];

                    Data.BeginYear = int.Parse(beginyear);
                    Data.EndYear = int.Parse(endyear);
                }

                await CanteenProvider.SetSchoolyear(Data, Periods);
            }

            NavManager.NavigateTo("/Admin/Canteen/SchoolyearManagement");
        }
        private void AddPeriod()
        {
            if (Data != null)
            {
                PeriodToEdit = new CANTEEN_Schoolyear_RegistrationPeriods();
                // Initialisiere die Start/Endzeiten der Periode
                DateTime _startDate = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 1);
                DateTime _endDate = new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(2).Month, 1).AddDays(-1);
                // Setze Start/Endzeiten der Periode in Abhängigkeit vom ausgewählten Schuljahr
                if (Data.BeginDate != null)
                {
                    _startDate = new DateTime(Data.BeginDate.Value.Year, Data.BeginDate.Value.AddMonths(-4).Month, 1);
                    _endDate = new DateTime(Data.BeginDate.Value.Year, Data.BeginDate.Value.AddMonths(-1).Month, 1).AddDays(-1);
                }
                // Überprüfe ob es bereits eine entsprechende Anmeldeperiode gibt, setze ansonsten eine andere
                if (CheckIfPeriodOverlap(_startDate, _endDate))
                {
                    DateTime? _lastPeriod = Periods.Where(p => p.BeginDate != null && p.EndDate != null).Select(p => p.EndDate).Max();
                    if (_lastPeriod != null)
                    {
                        _startDate = new DateTime(_lastPeriod.Value.AddMonths(1).Year, _lastPeriod.Value.AddMonths(1).Month, 1);
                        _endDate = new DateTime(_lastPeriod.Value.AddMonths(1).Year, _lastPeriod.Value.AddMonths(2).Month, 1).AddDays(-1);
                    }
                }
                PeriodToEdit.ID = Guid.NewGuid();
                PeriodToEdit.BeginDate = _startDate;
                PeriodToEdit.EndDate = _endDate;

                PeriodToEdit.CANTEEN_Schoolyear_ID = Data.ID;

                _validationPeriodError = "";
                ShowPeriodWindow = true;
                StateHasChanged();
            }
        }
        private void EditPeriod(CANTEEN_Schoolyear_RegistrationPeriods Item)
        {
            PeriodToEdit = new CANTEEN_Schoolyear_RegistrationPeriods()
            {
                ID = Item.ID,
                CANTEEN_Schoolyear_ID = Item.CANTEEN_Schoolyear_ID,
                CANTEEN_Schoolyear = Item.CANTEEN_Schoolyear,
                BeginDate = Item.BeginDate,
                EndDate = Item.EndDate
            };

            _validationPeriodError = "";
            ShowPeriodWindow = true;
            StateHasChanged();
        }
        private void SavePeriod()
        {
            if(PeriodToEdit != null && PeriodToEdit.BeginDate != null && PeriodToEdit.EndDate != null && !CheckIfPeriodOverlap(PeriodToEdit.BeginDate.Value, PeriodToEdit.EndDate.Value, PeriodToEdit.ID))
            {
                CANTEEN_Schoolyear_RegistrationPeriods? _period = Periods.FirstOrDefault(p => p.ID == PeriodToEdit.ID);
                if (_period != null)
                {
                    _period.BeginDate = PeriodToEdit.BeginDate;
                    _period.EndDate = PeriodToEdit.EndDate;
                }
                else
                {
                    Periods.Add(PeriodToEdit);
                }
                if (Grid != null)
                {
                    Grid.Rebind();
                    StateHasChanged();
                }
                HidePeriodWindow();
            }
            else if (PeriodToEdit != null)
            {
                StringBuilder _errorMessage = new StringBuilder();
                if (PeriodToEdit.BeginDate == null)
                {
                    _errorMessage.AppendLine(TextProvider.Get("CANTEEN_SCHOOLYEAR_REGISTRATIONPERIOD_SELECTBEGINDATE"));
                }
                if (PeriodToEdit.EndDate == null)
                {
                    _errorMessage.AppendLine(TextProvider.Get("CANTEEN_SCHOOLYEAR_REGISTRATIONPERIOD_SELECTENDDATE"));
                }
                if (PeriodToEdit.BeginDate != null && PeriodToEdit.EndDate != null && CheckIfPeriodOverlap(PeriodToEdit.BeginDate.Value, PeriodToEdit.EndDate.Value, PeriodToEdit.ID))
                {
                    _errorMessage.AppendLine(TextProvider.Get("CANTEEN_SCHOOLYEAR_REGISTRATIONPERIOD_OVERLAP"));
                }
                _validationPeriodError = _errorMessage.ToString();
            }
        }
        private async void RemovePeriod(CANTEEN_Schoolyear_RegistrationPeriods Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE_PERIOD"), TextProvider.Get("WARNING")))
                    return;

                BusyIndicatorService.IsBusy = true;
                StateHasChanged();

                Periods.Remove(Item);

                if (Grid != null)
                {
                    Grid.Rebind();
                    StateHasChanged();
                }

                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }
        }
        private void HidePeriodWindow()
        {
            PeriodToEdit = null;

            ShowPeriodWindow = false;
            StateHasChanged();
        }
        private bool CheckIfPeriodOverlap(DateTime StartDate, DateTime EndDate, Guid? EditID = null)
        {
            if (Periods != null && Periods.Any())
            {
                foreach (CANTEEN_Schoolyear_RegistrationPeriods _period in Periods)
                {
                    if (EditID != null && _period.ID == EditID)
                    {
                        continue;
                    }
                    if (_period.BeginDate != null && _period.EndDate != null && _period.BeginDate.Value.Date < EndDate.Date && StartDate.Date < _period.EndDate.Value.Date)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
