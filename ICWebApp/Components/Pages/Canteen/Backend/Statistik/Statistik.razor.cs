using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor;
using ICWebApp.Components.Pages.Canteen.Frontend;
using Telerik.Reporting;
using ClosedXML.Excel;
using System;
using ICWebApp.Application.Settings;

namespace ICWebApp.Components.Pages.Canteen.Backend.Statistik
{

    public partial class Statistik
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }

        private bool IsDataBusy = true;
        private DateTime? _fromDate = null;

        private DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                if (value != _fromDate)
                {
                    _fromDate = value;
                    ReloadData();
                }
            }
        }

        private DateTime? _toDate = null;

        private DateTime? ToDate
        {
            get => _toDate;
            set
            {
                if (value != _toDate)
                {
                    _toDate = value;
                    ReloadData();
                }
            }
        }

        private bool ShowCompleted = false;
        private List<V_CANTEEN_Schoolyear> SchoolyearList = new List<V_CANTEEN_Schoolyear>();
        private V_CANTEEN_Schoolyear? CurrentSchoolyear;
        private List<V_CANTEEN_Statistik_Meals> RawData = new List<V_CANTEEN_Statistik_Meals>();
        private List<StatistikData> DomicileData = new List<StatistikData>();
        private List<StatistikData> MealData = new List<StatistikData>();
        private List<StatistikData> CanteenData = new List<StatistikData>();
        private List<StatistikData> SchoolData = new List<StatistikData>();
        private List<StatistikData> ClassData = new List<StatistikData>();
        private List<V_CANTEEN_Statistik_Subscribers> SubscriberRawData = new List<V_CANTEEN_Statistik_Subscribers>();
        private List<StatistikData> SubscriberDomicileData = new List<StatistikData>();
        private List<StatistikData> SubscriberMealData = new List<StatistikData>();
        private List<StatistikData> SubscriberCanteenData = new List<StatistikData>();
        private List<StatistikData> SubscriberSchoolData = new List<StatistikData>();
        private List<StatistikData> SubscriberClassData = new List<StatistikData>();
        private List<StatistikHeatmapData> SubscriberWeekSchoolData = new List<StatistikHeatmapData>();
        private List<StatistikHeatmapData> SubscriberWeekCanteenData = new List<StatistikHeatmapData>();
        private ElementReference MealStatistikContainer;
        private long AdditionalMeals = 0;
        private CANTEEN_Configuration? _config;
        protected override async Task OnInitializedAsync()
        {   
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_CANTEEN_STATISTIK_MEALS");
            SessionWrapper.PageSubTitle = TextProvider.Get("");

            CrumbService.ClearBreadCrumb();

            FromDate = DateTime.Now.AddMonths(-1);
            ToDate = DateTime.Now.AddMonths(1);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                _config = await CanteenProvider.GetConfiguration(SessionWrapper.AUTH_Municipality_ID.Value);
                SchoolyearList = await CanteenProvider.GetSchoolsyearAll(SessionWrapper.AUTH_Municipality_ID.Value);

                var current = await CanteenProvider.GetSchoolsyearsCurrent(SessionWrapper.AUTH_Municipality_ID.Value);

                CurrentSchoolyear = SchoolyearList.OrderByDescending(p => p.BeginYear).FirstOrDefault();                
            }
            
            await GetSubscriberData();
            await GetData();

            BusyIndicatorService.IsBusy = false;
            IsDataBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async Task<bool> GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && FromDate != null && ToDate != null)
            {
                DomicileData = new List<StatistikData>();
                MealData = new List<StatistikData>();
                CanteenData = new List<StatistikData>();
                SchoolData = new List<StatistikData>();
                ClassData = new List<StatistikData>();

                var Data = await CanteenProvider.GetStatistikMeals(SessionWrapper.AUTH_Municipality_ID.Value, FromDate.Value, ToDate.Value, LangProvider.GetCurrentLanguageID());

                Data = Data.Where(p => p.Completed == 1).ToList();

                foreach (var group in Data.Where(p => p.CancelDate == null).GroupBy(p => p.Child_DomicileMunicipality))
                {
                    var newPos = new StatistikData();

                    newPos.Category = group.Key;
                    newPos.Value = group.Count();

                    DomicileData.Add(newPos);
                }

                foreach (var group in Data.Where(p => p.CancelDate == null).GroupBy(p => p.Meal))
                {
                    var newPos = new StatistikData();

                    newPos.Category = group.Key;
                    newPos.Value = group.Count();

                    MealData.Add(newPos);
                }

                foreach (var group in Data.Where(p => p.CancelDate == null).GroupBy(p => p.Canteen))
                {
                    var newPos = new StatistikData();

                    newPos.Category = group.Key;
                    newPos.Value = group.Count();

                    CanteenData.Add(newPos);
                }

                foreach (var group in Data.Where(p => p.CancelDate == null).GroupBy(p => p.School))
                {
                    var newPos = new StatistikData();

                    newPos.Category = group.Key;
                    newPos.Value = group.Count();

                    SchoolData.Add(newPos);
                }

                foreach (var group in Data.Where(p => p.CancelDate == null).GroupBy(p => p.SchoolClass))
                {
                    var newPos = new StatistikData();

                    newPos.Category = group.Key;
                    newPos.Value = group.Count();

                    ClassData.Add(newPos);
                }

                RawData = Data;

                var data = SubscriberRawData;

                AdditionalMeals = 0;

                foreach (var day in RawData.Select(p => p.Date).Distinct().ToList())
                {
                    foreach (var school in RawData.Select(p => p.CANTEEN_School_ID).Distinct().ToList())
                    {
                        if (school != null)
                        {
                            var add = await CanteenProvider.GetAdditionalPersonalList(school.Value);
                            CANTEEN_School_AdditionalPersonal? _additionalPersonalList = add.FirstOrDefault();
                            List<V_CANTEEN_Statistik_Subscribers> _subscribersOfSchool = data.Where(p => p.SchoolID == school).ToList();

                            if (_additionalPersonalList != null)
                            {
                                if (day != null && day.Value.DayOfWeek == DayOfWeek.Monday)
                                {
                                    if (_additionalPersonalList.MO != null)
                                    {
                                        AdditionalMeals += _additionalPersonalList.MO.Value;
                                    }
                                    if (_additionalPersonalList.MOPP != null && _additionalPersonalList.MOPP > 0)
                                    {
                                        AdditionalMeals += long.Parse(Math.Ceiling(decimal.Parse(_subscribersOfSchool.Where(p => p.DayMo == true).Count().ToString()) / decimal.Parse(_additionalPersonalList.MOPP.Value.ToString())).ToString());
                                    }
                                }
                                else if (day != null && day.Value.DayOfWeek == DayOfWeek.Tuesday)
                                {
                                    if (_additionalPersonalList.DI != null)
                                    {
                                        AdditionalMeals += _additionalPersonalList.DI.Value;
                                    }
                                    if (_additionalPersonalList.DIPP != null && _additionalPersonalList.DIPP > 0)
                                    {
                                        AdditionalMeals += long.Parse(Math.Ceiling(decimal.Parse(_subscribersOfSchool.Where(p => p.DayTue == true).Count().ToString()) / decimal.Parse(_additionalPersonalList.DIPP.Value.ToString())).ToString());
                                    }
                                }
                                else if (day != null && day.Value.DayOfWeek == DayOfWeek.Wednesday)
                                {
                                    if (_additionalPersonalList.MI != null)
                                    {
                                        AdditionalMeals += _additionalPersonalList.MI.Value;
                                    }
                                    if (_additionalPersonalList.MIPP != null && _additionalPersonalList.MIPP > 0)
                                    {
                                        AdditionalMeals += long.Parse(Math.Ceiling(decimal.Parse(_subscribersOfSchool.Where(p => p.DayWed == true).Count().ToString()) / decimal.Parse(_additionalPersonalList.MIPP.Value.ToString())).ToString());
                                    }
                                }
                                else if (day != null && day.Value.DayOfWeek == DayOfWeek.Thursday)
                                {
                                    if (_additionalPersonalList.DO != null)
                                    {
                                        AdditionalMeals += _additionalPersonalList.DO.Value;
                                    }
                                    if (_additionalPersonalList.DOPP != null && _additionalPersonalList.DOPP > 0)
                                    {
                                        AdditionalMeals += long.Parse(Math.Ceiling(decimal.Parse(_subscribersOfSchool.Where(p => p.DayThu == true).Count().ToString()) / decimal.Parse(_additionalPersonalList.DOPP.Value.ToString())).ToString());
                                    }
                                }
                                else if (day != null && day.Value.DayOfWeek == DayOfWeek.Friday)
                                {
                                    if (_additionalPersonalList.FR != null)
                                    {
                                        AdditionalMeals += _additionalPersonalList.FR.Value;
                                    }
                                    if (_additionalPersonalList.FRPP != null && _additionalPersonalList.FRPP > 0)
                                    {
                                        AdditionalMeals += long.Parse(Math.Ceiling(decimal.Parse(_subscribersOfSchool.Where(p => p.DayFri == true).Count().ToString()) / decimal.Parse(_additionalPersonalList.FRPP.Value.ToString())).ToString());
                                    }
                                }
                            }
                        }
                    }
                }
                StateHasChanged();
            }

            return true;
        }
        private async Task<bool> GetSubscriberData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && CurrentSchoolyear != null)
            {
                SubscriberDomicileData = new List<StatistikData>();
                SubscriberMealData = new List<StatistikData>();
                SubscriberCanteenData = new List<StatistikData>();
                SubscriberSchoolData = new List<StatistikData>();
                SubscriberClassData = new List<StatistikData>();
                SubscriberWeekSchoolData = new List<StatistikHeatmapData>();
                SubscriberWeekCanteenData = new List<StatistikHeatmapData>();

                var data = await CanteenProvider.GetStatistikSubscribers(SessionWrapper.AUTH_Municipality_ID.Value, CurrentSchoolyear.id, LangProvider.GetCurrentLanguageID());

                foreach (var group in data.Where(p => p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted).GroupBy(p => p.Child_DomicileMunicipality))
                {
                    var newPos = new StatistikData();

                    newPos.Category = group.Key;
                    newPos.Value = group.Count();

                    SubscriberDomicileData.Add(newPos);
                }

                foreach (var group in data.Where(p => p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted).GroupBy(p => p.Meal))
                {
                    var newPos = new StatistikData();

                    newPos.Category = group.Key;
                    newPos.Value = group.Count();

                    SubscriberMealData.Add(newPos);
                }

                foreach (var group in data.Where(p => p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted).GroupBy(p => p.Canteen))
                {
                    var newPos = new StatistikData();

                    newPos.Category = group.Key;
                    newPos.Value = group.Count();

                    SubscriberCanteenData.Add(newPos);
                }

                foreach (var group in data.Where(p => p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted).GroupBy(p => p.School))
                {
                    var newPos = new StatistikData();

                    newPos.Category = group.Key;
                    newPos.Value = group.Count();

                    SubscriberSchoolData.Add(newPos);
                }

                foreach (var group in data.Where(p => p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted).GroupBy(p => p.SchoolClass))
                {
                    var newPos = new StatistikData();

                    newPos.Category = group.Key;
                    newPos.Value = group.Count();

                    SubscriberClassData.Add(newPos);
                }

                foreach (var group in data.Where(p => p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted).GroupBy(p => p.School))
                {                    
                    var newPosMo = new StatistikHeatmapData();

                    newPosMo.CategoryY = group.Key;
                    newPosMo.CategoryX = TextProvider.Get("MONDAY");
                    newPosMo.Value = group.Where(p => p.DayMo == true).Count();
                    newPosMo.PriorityOrder = 0;

                    SubscriberWeekSchoolData.Add(newPosMo);

                    var newPosTue = new StatistikHeatmapData();

                    newPosTue.CategoryY = group.Key;
                    newPosTue.CategoryX = TextProvider.Get("TUESDAY");
                    newPosTue.Value = group.Where(p => p.DayTue == true).Count();
                    newPosTue.PriorityOrder = 1;

                    SubscriberWeekSchoolData.Add(newPosTue);

                    var newPosWed = new StatistikHeatmapData();

                    newPosWed.CategoryY = group.Key;
                    newPosWed.CategoryX = TextProvider.Get("WEDNESDAY");
                    newPosWed.Value = group.Where(p => p.DayWed == true).Count();
                    newPosWed.PriorityOrder = 2;

                    SubscriberWeekSchoolData.Add(newPosWed);

                    var newPosThu = new StatistikHeatmapData();

                    newPosThu.CategoryY = group.Key;
                    newPosThu.CategoryX = TextProvider.Get("THURSDAY");
                    newPosThu.Value = group.Where(p => p.DayThu == true).Count();
                    newPosThu.PriorityOrder = 3;

                    SubscriberWeekSchoolData.Add(newPosThu);

                    var newPosFri = new StatistikHeatmapData();

                    newPosFri.CategoryY = group.Key;
                    newPosFri.CategoryX = TextProvider.Get("FRIDAY");
                    newPosFri.Value = group.Where(p => p.DayFri == true).Count();
                    newPosFri.PriorityOrder = 4;

                    SubscriberWeekSchoolData.Add(newPosFri);
                }

                foreach (var group in data.Where(p => p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted).GroupBy(p => p.Canteen))
                {
                    var newPosMo = new StatistikHeatmapData();

                    newPosMo.CategoryY = group.Key;
                    newPosMo.CategoryX = TextProvider.Get("MONDAY");
                    newPosMo.Value = group.Where(p => p.DayMo == true).Count();
                    newPosMo.PriorityOrder = 0;

                    SubscriberWeekCanteenData.Add(newPosMo);

                    var newPosTue = new StatistikHeatmapData();

                    newPosTue.CategoryY = group.Key;
                    newPosTue.CategoryX = TextProvider.Get("TUESDAY");
                    newPosTue.Value = group.Where(p => p.DayTue == true).Count();
                    newPosTue.PriorityOrder = 1;

                    SubscriberWeekCanteenData.Add(newPosTue);

                    var newPosWed = new StatistikHeatmapData();

                    newPosWed.CategoryY = group.Key;
                    newPosWed.CategoryX = TextProvider.Get("WEDNESDAY");
                    newPosWed.Value = group.Where(p => p.DayWed == true).Count();
                    newPosWed.PriorityOrder = 2;

                    SubscriberWeekCanteenData.Add(newPosWed);

                    var newPosThu = new StatistikHeatmapData();

                    newPosThu.CategoryY = group.Key;
                    newPosThu.CategoryX = TextProvider.Get("THURSDAY");
                    newPosThu.Value = group.Where(p => p.DayThu == true).Count();
                    newPosThu.PriorityOrder = 3;

                    SubscriberWeekCanteenData.Add(newPosThu);

                    var newPosFri = new StatistikHeatmapData();

                    newPosFri.CategoryY = group.Key;
                    newPosFri.CategoryX = TextProvider.Get("FRIDAY");
                    newPosFri.Value = group.Where(p => p.DayFri == true).Count();
                    newPosFri.PriorityOrder = 4;

                    SubscriberWeekCanteenData.Add(newPosFri);
                }

                SubscriberRawData = data;
            }

            return true;
        }
        private async Task<bool> ReloadData()
        {
            IsDataBusy = true;
            StateHasChanged();

            await GetData();

            IsDataBusy = false;
            StateHasChanged();

            return true;
        }
        private async Task<bool> DownloadData(List<StatistikData> Data, string Title)
        {
            var wb = new XLWorkbook();

            var ws = wb.Worksheets.Add("Data");
            ws.Cell(1, 1).Value = _fromDate!.Value.ToShortDateString() + " - " + _toDate!.Value.ToShortDateString();

            for(int row = 1; row < Data.Count; row++)
            {
                ws.Cell(row + 1, 1).Value = Data[row].Category;
                ws.Cell(row + 1, 2).Value = Data[row].Value;
            }

            var ms = new MemoryStream();

            wb.SaveAs(ms);

            await EnviromentService.DownloadFile(ms.ToArray(), Title + ".xlsx");

            return true;
        }
        private async Task<bool> DownloadHeatMapData(List<StatistikHeatmapData> Data, string Title)
        {
            var wb = new XLWorkbook();

            var ws = wb.Worksheets.Add("Data");

            int columnCount = 1;

            foreach (var column in Data.GroupBy(p => p.CategoryX))
            {
                if (column.Key != null)
                {
                    ws.Cell(1, columnCount + 1).Value = column.Key;

                    int rowCount = 1;

                    var rowdata = Data.Where(p => p.CategoryX != null && p.CategoryX.ToString() == column.Key.ToString()).OrderBy(p => p.CategoryY).ToList();

                    foreach (var row in rowdata)
                    {
                        if (ws.Cell(rowCount + 1, 1).Value == null || ws.Cell(rowCount + 1, 1).Value == "")
                        {
                            ws.Cell(rowCount + 1, 1).Value = row.CategoryY;
                        }

                        ws.Cell(rowCount + 1, columnCount + 1).Value = row.Value;

                        rowCount++;
                    }

                    columnCount++;
                }
            }

            var ms = new MemoryStream();

            wb.SaveAs(ms);

            await EnviromentService.DownloadFile(ms.ToArray(), Title + ".xlsx");

            return true;
        }
        private async Task<bool> DownloadMealRawData()
        {
            var wb = new XLWorkbook();

            var ws = wb.Worksheets.Add("Data");
            
            ws.Cell(1, 1).Value = _fromDate!.Value.ToShortDateString() + " - " + _toDate!.Value.ToShortDateString();
            ws.Cell(2, 1).Value = TextProvider.Get("CANTEEN_STATISTIK_DATA");
            ws.Cell(2, 2).Value = TextProvider.Get("CANTEEN_DASHBOARD_FIRSTNAME");
            ws.Cell(2, 3).Value = TextProvider.Get("CANTEEN_DASHBOARD_LASTNAME");
            ws.Cell(2, 4).Value = TextProvider.Get("CANTEEN_STATISTIK_PROVINCE");
            ws.Cell(2, 5).Value = TextProvider.Get("CANTEEN_STATISTIK_POSTALCODE");
            ws.Cell(2, 6).Value = TextProvider.Get("CANTEEN_STATISTIK_MUNICIPALITY");
            ws.Cell(2, 7).Value = TextProvider.Get("CANTEEN_STATISTIK_MEAL"); 
            ws.Cell(2, 8).Value = TextProvider.Get("CANTEEN_STATISTIK_CANCELDATE"); 
            ws.Cell(2, 9).Value = TextProvider.Get("CANTEEN_STATISTIK_CANTEEN"); 
            ws.Cell(2, 10).Value = TextProvider.Get("CANTEEN_STATISTIK_SCHOOL"); 
            ws.Cell(2, 11).Value = TextProvider.Get("CANTEEN_STATISTIK_SCHOOLCLASS");
            if (_config?.PosMode == true)
            {
                ws.Cell(2, 12).Value = TextProvider.Get("CANTEEN_APPEARED");
                ws.Cell(2, 13).Value = TextProvider.Get("CANTEEN_APPEARED_CANCELLED");
                ws.Cell(2, 14).Value = TextProvider.Get("CANTEEN_NOT_APPEARED");
            }
            int rowCount = 2;

            foreach (var column in RawData.OrderBy(p => p.Date))
            {
                ws.Cell(rowCount + 1, 1).Value = column.Date!.Value.ToShortDateString();
                ws.Cell(rowCount + 1, 2).Value = column.FirstName;
                ws.Cell(rowCount + 1, 2).Value = column.LastName;
                ws.Cell(rowCount + 1, 4).Value = column.Child_DomicileProvince;
                ws.Cell(rowCount + 1, 5).Value = column.Child_DomicilePostalCode;
                ws.Cell(rowCount + 1, 6).Value = column.Child_DomicileMunicipality;
                ws.Cell(rowCount + 1, 7).Value = column.Meal;
                ws.Cell(rowCount + 1, 8).Value = column.CancelDate;
                ws.Cell(rowCount + 1, 9).Value = column.Canteen;
                ws.Cell(rowCount + 1, 10).Value = column.School;
                ws.Cell(rowCount + 1, 11).Value = column.SchoolClass;
                if (_config?.PosMode == true)
                {
                    ws.Cell(rowCount + 1, 12).Value = (column.Used && !column.UsedWhileCanceled && !column.NotAttended) ? "x" : "";
                    ws.Cell(rowCount + 1, 13).Value = column.UsedWhileCanceled ? "x" : "";
                    ws.Cell(rowCount + 1, 14).Value = (!column.Used || column.NotAttended) ? "x" : "";
                }

                rowCount++;                
            }

            var ms = new MemoryStream();

            wb.SaveAs(ms);

            await EnviromentService.DownloadFile(ms.ToArray(), TextProvider.Get("CANTEEN_STATISTIK_TITLE_MEALS") + ".xlsx");

            return true;
        }
        private async Task<bool> DownloadSubscriptionRawData()
        {
            var wb = new XLWorkbook();

            var ws = wb.Worksheets.Add("Data");

            ws.Cell(1, 1).Value = TextProvider.Get("CANTEEN_STATISTIK_PROVINCE");
            ws.Cell(1, 2).Value = TextProvider.Get("CANTEEN_STATISTIK_POSTALCODE");
            ws.Cell(1, 3).Value = TextProvider.Get("CANTEEN_STATISTIK_MUNICIPALITY");
            ws.Cell(1, 4).Value = TextProvider.Get("CANTEEN_STATISTIK_MEAL");
            ws.Cell(1, 5).Value = TextProvider.Get("CANTEEN_STATISTIK_CANTEEN");
            ws.Cell(1, 6).Value = TextProvider.Get("CANTEEN_STATISTIK_SCHOOL");
            ws.Cell(1, 7).Value = TextProvider.Get("CANTEEN_STATISTIK_SCHOOLCLASS");
            ws.Cell(1, 8).Value = TextProvider.Get("MONDAY");
            ws.Cell(1, 9).Value = TextProvider.Get("TUESDAY");
            ws.Cell(1, 10).Value = TextProvider.Get("WEDNESDAY");
            ws.Cell(1, 11).Value = TextProvider.Get("THURSDAY");
            ws.Cell(1, 12).Value = TextProvider.Get("FRIDAY");

            int rowCount = 1;

            foreach (var column in SubscriberRawData.OrderBy(p => p.Child_DomicileProvince))
            {
                ws.Cell(rowCount + 1, 1).Value = column.Child_DomicileProvince;
                ws.Cell(rowCount + 1, 2).Value = column.Child_DomicilePostalCode;
                ws.Cell(rowCount + 1, 3).Value = column.Child_DomicileMunicipality;
                ws.Cell(rowCount + 1, 4).Value = column.Meal;
                ws.Cell(rowCount + 1, 5).Value = column.Canteen;
                ws.Cell(rowCount + 1, 6).Value = column.School;
                ws.Cell(rowCount + 1, 7).Value = column.SchoolClass;
                if(column.DayMo == true)
                {
                    ws.Cell(rowCount + 1, 8).Value = "x";
                }
                if (column.DayTue == true)
                {
                    ws.Cell(rowCount + 1, 9).Value = "x";
                }
                if (column.DayWed == true)
                {
                    ws.Cell(rowCount + 1, 10).Value = "x";
                }
                if (column.DayThu == true)
                {
                    ws.Cell(rowCount + 1, 11).Value = "x";
                }
                if (column.DayFri == true)
                {
                    ws.Cell(rowCount + 1, 12).Value = "x";
                }

                rowCount++;
            }

            var ms = new MemoryStream();

            wb.SaveAs(ms);

            await EnviromentService.DownloadFile(ms.ToArray(), TextProvider.Get("CANTEEN_STATISTIK_TITLE_SUBSCRIPTIONS") + ".xlsx");

            return true;
        }
        private async void SetCurrentSchoolYear(V_CANTEEN_Schoolyear Data)
        {
            CurrentSchoolyear = Data;

            IsDataBusy = true;
            StateHasChanged();

            await GetSubscriberData();
            await GetData();

            IsDataBusy = false;
            StateHasChanged();
        }
    }
}