using DocumentFormat.OpenXml.Drawing.Charts;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Newtonsoft.Json.Bson;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor.Components.Editor;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Telerik.Blazor.ThemeConstants;

namespace ICWebApp.Components.Pages.Homepage.Backend.Authority
{
    public partial class Timeslots
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ID { get; set; }

        private AUTH_Authority? Authority;
        private List<LANG_Languages>? Languages;
        private Guid? CurrentLanguage;
        private List<V_AUTH_Authority_Reason> Reasons = new List<V_AUTH_Authority_Reason>();
        private List<AUTH_Authority_Dates_Timeslot> Times = new List<AUTH_Authority_Dates_Timeslot>();
        private List<AUTH_Authority_Dates_Closed> ClosedDates = new List<AUTH_Authority_Dates_Closed>();
        private bool ReasonEditWindowVisible = false;
        private AUTH_Authority_Reason? Data;
        private bool IsDataBusy = false;
        private DateTime actualDate = DateTime.Today;
        private DateTime startDate1 = DateTime.Today;
        private DateTime startDate2 = DateTime.Today.AddMonths(1);
        private DateTime startDate3 = DateTime.Today.AddMonths(2);
        private List<DateTime> DisabledDates = new List<DateTime>();

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Authorities", "MANMENU_BACKEND_HOMEPAGE_AUTHORITIES", null, null, false);
            CrumbService.AddBreadCrumb("/Backend/Homepage/Authorities/Timeslots", "MANMENU_BACKEND_HOMEPAGE_AUTHORITIES_TIMESLOTS", null, null, true);

            SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_AUTHORITIES_TIMESLOTS");
            SessionWrapper.PageSubTitle = "";

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Authority = await AuthProvider.GetAuthority(Guid.Parse(ID));

                Reasons = await AuthProvider.GetAuthorityReasons(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());
                Times = await AuthProvider.GetAuthorityTimes(Guid.Parse(ID));
                ClosedDates = await AuthProvider.GetAuthorityClosedDates(Guid.Parse(ID));

                CalcDisabledDates();
            }

            Languages = await LangProvider.GetAll();

            if (CurrentLanguage == null)
                CurrentLanguage = LanguageSettings.German;

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Authorities");
            StateHasChanged();
        }
        private void AddReason()
        {
            Data = new AUTH_Authority_Reason();
            Data.ID = Guid.NewGuid();

            Data.AUTH_Authority_ID = Guid.Parse(ID);

            if (Languages != null)
            {
                foreach (var l in Languages)
                {
                    if (Data.AUTH_Authority_Reason_Extended == null)
                    {
                        Data.AUTH_Authority_Reason_Extended = new List<AUTH_Authority_Reason_Extended>();
                    }

                    if (Data.AUTH_Authority_Reason_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                    {
                        var dataE = new AUTH_Authority_Reason_Extended()
                        {
                            ID = Guid.NewGuid(),
                            AUTH_Authority_Reason_ID = Data.ID,
                            LANG_Language_ID = l.ID
                        };

                        Data.AUTH_Authority_Reason_Extended.Add(dataE);
                    }
                }
            }

            ReasonEditWindowVisible = true;
            StateHasChanged();
        }
        private async void EditReason(V_AUTH_Authority_Reason Item)
        {
            Data = await AuthProvider.GetReason(Item.ID);

            if (Data != null)
            {
                Data.AUTH_Authority_Reason_Extended = await AuthProvider.GetReasonExtended(Data.ID);

                if (Languages != null)
                {
                    if (Data.AUTH_Authority_Reason_Extended != null && Data.AUTH_Authority_Reason_Extended.Count < Languages.Count)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.AUTH_Authority_Reason_Extended == null)
                            {
                                Data.AUTH_Authority_Reason_Extended = new List<AUTH_Authority_Reason_Extended>();
                            }

                            if (Data.AUTH_Authority_Reason_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new AUTH_Authority_Reason_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    AUTH_Authority_Reason_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                await AuthProvider.SetReasonExtended(dataE);
                                Data.AUTH_Authority_Reason_Extended.Add(dataE);
                            }
                        }
                    }
                }
            }

            ReasonEditWindowVisible = true;
            StateHasChanged();
        }
        private async void DeleteReason(V_AUTH_Authority_Reason Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE_REASON"), TextProvider.Get("WARNING")))
                    return;

                await AuthProvider.RemoveReason(Item.ID);

                Reasons = await AuthProvider.GetAuthorityReasons(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());
                StateHasChanged();
            }
        }
        private async void SaveReason()
        {
            if(Data !=  null)
            {
                await AuthProvider.SetReason(Data);

                if (Data.AUTH_Authority_Reason_Extended != null)
                {
                    foreach (var ext in Data.AUTH_Authority_Reason_Extended)
                    {
                        await AuthProvider.SetReasonExtended(ext);
                    }
                }

                Reasons = await AuthProvider.GetAuthorityReasons(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

                Data = null;
                ReasonEditWindowVisible = false;
                StateHasChanged();
            }
        }
        private void CloseReasonEditWindow()
        {
            Data = null;
            ReasonEditWindowVisible = false;
            StateHasChanged();
        }
        private void AddTime(int Weekday)
        {
            Times.Add(new AUTH_Authority_Dates_Timeslot()
            {
                ID = Guid.NewGuid(),
                AUTH_Authority_ID = Guid.Parse(ID),
                Weekday = Weekday
            });

            CalcDisabledDates();

            StateHasChanged();
        }
        private void RemoveTime(AUTH_Authority_Dates_Timeslot item)
        {
            Times.Remove(item);
            StateHasChanged();
        }
        private async void Save()
        {
            var existing = await AuthProvider.GetAuthorityTimes(Guid.Parse(ID));

            if (existing != null && Times != null)
            {
                var toDelete = existing.Where(p => !Times.Select(x => x.ID).Contains(p.ID)).ToList();
                var toAdd = Times.Where(p => !existing.Select(p => p.ID).Contains(p.ID)).ToList();


                foreach (var p in toDelete)
                {
                    await AuthProvider.RemoveAuthorityTimes(p.ID);
                }

                foreach (var p in toAdd)
                {
                    if (p.TimeFrom != null)
                    {
                        await AuthProvider.SetAuthorityTimes(p);
                    }
                }
            }

            var existingDates = await AuthProvider.GetAuthorityClosedDates(Guid.Parse(ID));

            if (existingDates != null && ClosedDates != null)
            {
                var toDelete = existingDates.Where(p => !ClosedDates.Select(x => x.ID).Contains(p.ID)).ToList();
                var toAdd = ClosedDates.Where(p => !existingDates.Select(p => p.ID).Contains(p.ID)).ToList();


                foreach (var p in toDelete)
                {
                    await AuthProvider.RemoveAuthorityClosedDates(p.ID);
                }

                foreach (var p in toAdd)
                {
                    await AuthProvider.SetAuthorityClosedDates(p);                    
                }
            }

            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Authorities");
            StateHasChanged();
        }
        private string? GetDayStatusClass(DateTime date)
        {
            if (ClosedDates != null)
            {
                var closed = ClosedDates.Where(p => p.Date != null).FirstOrDefault(p => p.Date.Value.Year == date.Year && p.Date.Value.Month == date.Month && p.Date.Value.Day == date.Day);

                if(closed != null)
                {
                    return "closed";
                }
            }

            return "";
        }
        private void SingleSelectionChangeHandler(DateTime date)
        {
            if (ClosedDates != null)
            {
                IsDataBusy = true;
                StateHasChanged();

                var existing = ClosedDates.Where(p => p.Date != null).FirstOrDefault(p => p.Date.Value.Year == date.Year && p.Date.Value.Month == date.Month && p.Date.Value.Day == date.Day);

                if (existing != null)
                {
                    ClosedDates.Remove(existing);
                }
                else
                {
                    ClosedDates.Add(new AUTH_Authority_Dates_Closed()
                    {
                        ID = Guid.NewGuid(),
                        AUTH_Authority_ID = Guid.Parse(ID),
                        Date = date
                    });
                }

                IsDataBusy = false;
                StateHasChanged();
            }
        }
        private void SetYearCalendar(DateTime newactualDate)
        {
            actualDate = newactualDate;
            startDate1 = actualDate;
            startDate2 = actualDate.AddMonths(1);
            startDate3 = actualDate.AddMonths(2);

        }
        public void Next()
        {
            SetYearCalendar(actualDate.AddMonths(1));
            StateHasChanged();

        }
        public void Prior()
        {
            SetYearCalendar(actualDate.AddMonths(-1));
            StateHasChanged();

        }
        public void CalcDisabledDates()
        {
            var allDates = new List<DateTime>();
            var aktiveDates = new List<DateTime>();
            DateTime StartDate = DateTime.Today;
            DateTime EndDate = DateTime.Today.AddDays(90);

            if (Authority != null)
            {
                StartDate = DateTime.Today.AddDays(Authority.MinDaysAheadOfBooking);
                EndDate = DateTime.Today.AddDays(Authority.MaxDaysBookable);
            }

            for (var dt = StartDate; dt <= EndDate; dt = dt.AddDays(1))
            {
                allDates.Add(dt);
            }

            DisabledDates = allDates;

            foreach (var t in Times) 
            {
                if (t.Weekday == 0)
                {
                    aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Monday).ToList());
                }
                else if (t.Weekday == 1)
                {
                    aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Tuesday).ToList());
                }
                else if (t.Weekday == 2)
                {
                    aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Wednesday).ToList());
                }
                else if (t.Weekday == 3)
                {
                    aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Thursday).ToList());
                }
                else if (t.Weekday == 4)
                {
                    aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Friday).ToList());
                }
                else if (t.Weekday == 5)
                {
                    aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Saturday).ToList());
                }
            }

            DisabledDates = DisabledDates.Where(p => !aktiveDates.Contains(p)).ToList();

            StateHasChanged();
        }
    }
}
