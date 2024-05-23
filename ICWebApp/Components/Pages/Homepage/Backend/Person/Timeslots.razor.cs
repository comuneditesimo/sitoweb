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

namespace ICWebApp.Components.Pages.Homepage.Backend.Person
{
    public partial class Timeslots
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ID { get; set; }

        private HOME_Person? Person;
        private List<HOME_Person_Dates_Timeslots> Times = new List<HOME_Person_Dates_Timeslots>();
        private List<HOME_Person_Dates_Closed> ClosedDates = new List<HOME_Person_Dates_Closed>();
        private bool IsDataBusy = false;
        private DateTime actualDate = DateTime.Today;
        private DateTime startDate1 = DateTime.Today;
        private DateTime startDate2 = DateTime.Today.AddMonths(1);
        private DateTime startDate3 = DateTime.Today.AddMonths(2);
        private List<DateTime> DisabledDates = new List<DateTime>();

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Person", "MAINMENU_BACKEND_HOMEPAGE_PERSON", null, null, false);
            CrumbService.AddBreadCrumb("/Backend/Homepage/Person/Timeslots", "MAINMENU_BACKEND_HOMEPAGE_PERSON_TIMESLOTS", null, null, true);

            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_PERSON_TIMESLOTS");
            SessionWrapper.PageSubTitle = "";

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Person = await HomeProvider.GetPerson(Guid.Parse(ID));

                Times = await HomeProvider.GetPersonTimes(Guid.Parse(ID));
                ClosedDates = await HomeProvider.GetPersonClosedDates(Guid.Parse(ID));

                CalcDisabledDates();
            }

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Person");
            StateHasChanged();
        }
        private void AddTime(int Weekday)
        {
            Times.Add(new HOME_Person_Dates_Timeslots()
            {
                ID = Guid.NewGuid(),
                HOME_Person_ID = Guid.Parse(ID),
                Weekday = Weekday
            });

            CalcDisabledDates();

            StateHasChanged();
        }
        private void RemoveTime(HOME_Person_Dates_Timeslots item)
        {
            Times.Remove(item);
            StateHasChanged();
        }
        private async void Save()
        {
            var existing = await HomeProvider.GetPersonTimes(Guid.Parse(ID));

            if (existing != null && Times != null)
            {
                var toDelete = existing.Where(p => !Times.Select(x => x.ID).Contains(p.ID)).ToList();
                var toAdd = Times.Where(p => !existing.Select(p => p.ID).Contains(p.ID)).ToList();


                foreach (var p in toDelete)
                {
                    await HomeProvider.RemovePersonTimes(p.ID);
                }

                foreach (var p in toAdd)
                {
                    if (p.TimeFrom != null)
                    {
                        await HomeProvider.SetPersonTimes(p);
                    }
                }
            }

            var existingDates = await HomeProvider.GetPersonClosedDates(Guid.Parse(ID));

            if (existingDates != null && ClosedDates != null)
            {
                var toDelete = existingDates.Where(p => !ClosedDates.Select(x => x.ID).Contains(p.ID)).ToList();
                var toAdd = ClosedDates.Where(p => !existingDates.Select(p => p.ID).Contains(p.ID)).ToList();


                foreach (var p in toDelete)
                {
                    await HomeProvider.RemovePersonClosedDates(p.ID);
                }

                foreach (var p in toAdd)
                {
                    await HomeProvider.SetPersonClosedDates(p);                    
                }
            }

            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Person");
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
                    ClosedDates.Add(new HOME_Person_Dates_Closed()
                    {
                        ID = Guid.NewGuid(),
                        HOME_Person_ID = Guid.Parse(ID),
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

            if (Person != null)
            {
                StartDate = DateTime.Today.AddDays(Person.MinDaysAheadOfBooking);
                EndDate = DateTime.Today.AddDays(Person.MaxDaysBookable);
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
