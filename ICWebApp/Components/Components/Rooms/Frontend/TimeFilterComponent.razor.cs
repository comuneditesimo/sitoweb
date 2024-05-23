using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Rooms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


namespace ICWebApp.Components.Components.Rooms.Frontend
{
    public partial class TimeFilterComponent
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IBookingService BookingService { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] IRoomProvider RoomProvider { get; set; }

        [Parameter] public TimeFilter? Filter { get; set; }
        [Parameter] public EventCallback OnSearch { get; set; }

        IJSObjectReference? _module;
        private List<WeekDayOfMonth> WeekDayOfMonthList = new List<WeekDayOfMonth>();
        private List<WeekDay> WeekDayList = new List<WeekDay>();
        private DateTime MinDate = DateTime.Today;
        private DateTime MaxDate = DateTime.Today.AddYears(2);

        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var config = await RoomProvider.GetSettings(SessionWrapper.AUTH_Municipality_ID.Value);

                if (config != null)
                {
                    if (config.MinRoomBookingDays != null)
                    {
                        MinDate = MinDate.AddDays(config.MinRoomBookingDays.Value);
                    }

                    if (config.MaxRoomBookingDays != null)
                    {
                        MaxDate = DateTime.Now.AddDays(config.MaxRoomBookingDays.Value);
                    }
                }
            }

            WeekDayOfMonthList.Add(WeekDayOfMonth.First);
            WeekDayOfMonthList.Add(WeekDayOfMonth.Second);
            WeekDayOfMonthList.Add(WeekDayOfMonth.Third);
            WeekDayOfMonthList.Add(WeekDayOfMonth.Fourth);
            WeekDayOfMonthList.Add(WeekDayOfMonth.Last);

            WeekDayList.Add(WeekDay.Monday);
            WeekDayList.Add(WeekDay.Tuesday);
            WeekDayList.Add(WeekDay.Wednesday);
            WeekDayList.Add(WeekDay.Thursday);
            WeekDayList.Add(WeekDay.Friday);
            WeekDayList.Add(WeekDay.Saturday);
            WeekDayList.Add(WeekDay.Sunday);

            BookingService.OnValuesChanged += BookingService_OnValuesChanged;

            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            if (Filter != null)
            {
                if (SessionWrapper.AUTH_Municipality_ID != null && (Filter.Meetings == null || !Filter.Meetings.Any()))
                {
                    Filter.AddNewMeeting();
                    await CheckBookingExpiredDate();
                }
            }

            StateHasChanged();
            await base.OnParametersSetAsync();
        }
        private async void BookingService_OnValuesChanged()
        {
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_module == null)
            {
                _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/Rooms/RoomHelper.js");
            }

            if (_module != null && firstRender == true && Filter != null && Filter.ShowSeries == true)
            {
                await _module.InvokeVoidAsync("HideElement", "room-meeting-window");
                await _module.InvokeVoidAsync("ShowElement", "room-series-window");
            }

            await base.OnAfterRenderAsync(firstRender);
        }
        private async void CreateSeries()
        {
            if(Filter != null && _module != null)
            {
                Filter.ShowSeries = !Filter.ShowSeries;

                if (Filter.ShowSeries == true)
                {
                    await _module.InvokeVoidAsync("HideElement", "room-meeting-window");
                    await _module.InvokeVoidAsync("ShowElement", "room-series-window");
                }
                else
                {
                    await _module.InvokeVoidAsync("HideElement", "room-series-window");
                    await _module.InvokeVoidAsync("ShowElement", "room-meeting-window");
                }

                TimeFilter_Meeting? _firstMeeting = Filter.Meetings.FirstOrDefault();
                if (_firstMeeting != null)
                {
                    if (_firstMeeting.FromDate != null)
                    {
                        Filter.Series_StartDate = _firstMeeting.FromDate.Value.Date;
                        Filter.Series_EndDate = _firstMeeting.FromDate.Value.Date.AddMonths(1);
                    }
                    if (_firstMeeting.FromHour != null)
                    {
                        Filter.Series_FromHour = _firstMeeting.FromHour.Value;
                    }
                    if (_firstMeeting.ToHour != null)
                    {
                        Filter.Series_ToHour = _firstMeeting.ToHour.Value;
                    }
                }

                BookingService.NotifyValuesChanged();

                StateHasChanged();
            }
        }
        private void OnWeekDayChanged(WeekDay WeekDay)
        {
            if(Filter != null)
            {
                Filter.Weekly_ToggleWeekDay(WeekDay);
            }
        }
        private bool GetWeekDayValue(WeekDay WeekDay)
        {
            if (Filter != null)
            {
                if (Filter.Weekly_Weekdays.Contains(WeekDay))
                {
                    return true;
                }
            }

            return false;
        }
        private void Search()
        {
            OnSearch.InvokeAsync();
        }
        private async Task CheckBookingExpiredDate()
        {
            if (Filter != null)
            {
                foreach (TimeFilter_Meeting _meeting in Filter.Meetings)
                {
                    if (SessionWrapper.AUTH_Municipality_ID != null)
                    {
                        ROOM_Settings? config = await RoomProvider.GetSettings(SessionWrapper.AUTH_Municipality_ID.Value);

                        if (config != null && config.MinRoomBookingDays != null && _meeting.FromDate != null && _meeting.FromDate.Value.Date < DateTime.Today.AddDays(config.MinRoomBookingDays.Value))
                        {
                            if (_meeting.ToDate != null)
                            {
                                _meeting.ToDate = DateTime.Today.AddDays(config.MinRoomBookingDays.Value);
                            }
                            _meeting.FromDate = DateTime.Today.AddDays(config.MinRoomBookingDays.Value);
                        }
                    }
                }
            }
        }
        private async Task AddMeeting()
        {
            if (Filter != null)
            {
                Filter.AddNewMeeting();
                await CheckBookingExpiredDate();
            }
        }
        private void RemoveMeeting(TimeFilter_Meeting _meeting)
        {
            if (Filter != null)
            {
                TimeFilter_Meeting? _matchingMeeting = Filter.Meetings.FirstOrDefault( p => p == _meeting);
                if (_matchingMeeting != null)
                {
                    Filter.RemoveMeeting(_matchingMeeting);
                }
            }
        }
    }
}
