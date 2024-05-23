using Freshdesk;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Rooms;
using Microsoft.VisualBasic;
using Stripe;
using Stripe.FinancialConnections;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Chilkat.Http;
using static SQLite.SQLite3;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ICWebApp.Application.Services
{
    public class BookingService : IBookingService
    {
        private IRoomProvider _RoomProvider;
        private ITEXTProvider _TextProvider;
        private IMessageService _MessageService;
        private IAUTHProvider _AuthProvider;
        private IMailerService _MailerService;
        private ISMSService _SMSService;
        private ICONTProvider _ContProvider;

        public event Action? OnValuesChanged;

        public BookingService(IRoomProvider _RoomProvider, ITEXTProvider _TextProvider,
            IMessageService _MessageService, IAUTHProvider _AuthProvider,
            IMailerService _MailerService, ISMSService _SMSService,
            ICONTProvider _ContProvider)
        {
            this._RoomProvider = _RoomProvider;
            this._TextProvider = _TextProvider;
            this._MessageService = _MessageService;
            this._AuthProvider = _AuthProvider;
            this._MailerService = _MailerService;
            this._SMSService = _SMSService;
            this._ContProvider = _ContProvider;
        }

        public string GetDatesInString(TimeFilter TimeFilter)
        {
            if (TimeFilter.ShowSeries == false)
            {
                StringBuilder _result = new StringBuilder();
                foreach (TimeFilter_Meeting _timeFilterMeeting in TimeFilter.Meetings)
                {
                    if (_result.Length > 0)
                    {
                        _result.Append("<hr>");
                    }
                    if (_timeFilterMeeting.FromDate != null && _timeFilterMeeting.ToDate != null &&
                        _timeFilterMeeting.FromHour != null && _timeFilterMeeting.ToHour != null &&
                        _timeFilterMeeting.From_Combined != null && _timeFilterMeeting.To_Combined != null)
                    {
                        if (_timeFilterMeeting.FromDate.Value.Day == _timeFilterMeeting.ToDate.Value.Day &&
                            _timeFilterMeeting.FromDate.Value.Month == _timeFilterMeeting.ToDate.Value.Month &&
                            _timeFilterMeeting.FromDate.Value.Year == _timeFilterMeeting.ToDate.Value.Year)
                        {
                            _result.Append("<div class='string-date-item'>");
                                _result.Append("<div class='string-calendar-icon'>");
                                    _result.Append("<i class=\"fa-regular fa-calendar\">");
                                    _result.Append("</i>");
                                    _result.Append(_timeFilterMeeting.FromDate.Value.ToString("ddd dd.MM.yyyy"));
                                _result.Append("</div>");
                                _result.Append("<div class='string-time-icon'>");
                                    _result.Append("<i class=\"fa-regular fa-clock\">");
                                    _result.Append("</i>");
                                    _result.Append(_timeFilterMeeting.FromHour.Value.ToString(" HH:mm - "));
                                    _result.Append(_timeFilterMeeting.ToHour.Value.ToString(" HH:mm"));
                                _result.Append("</div>");
                            _result.Append("</div>");
                        }
                        else
                        {
                            _result.Append("<div class='string-date-item'>");
                                _result.Append("<div class='string-date-text'>");
                                    _result.Append(_TextProvider.Get("FRONTEND_BOOKING_FROM"));
                                _result.Append("</div>");
                                _result.Append("<div class='string-calendar-icon'>");
                                    _result.Append("<i class=\"fa-regular fa-calendar\">");
                                    _result.Append("</i>");
                                    _result.Append(_timeFilterMeeting.From_Combined.Value.ToString("ddd dd.MM.yyyy"));
                                _result.Append("</div>");
                                _result.Append("<div class='string-time-icon'>");
                                    _result.Append("<i class=\"fa-regular fa-clock\">");
                                    _result.Append("</i>");
                                    _result.Append(_timeFilterMeeting.From_Combined.Value.ToString("HH:mm"));
                                _result.Append("</div>");
                            _result.Append("</div>");
                            _result.Append("<div class='string-date-item'>");
                                _result.Append("<div class='string-date-text'>");
                                    _result.Append(_TextProvider.Get("FRONTEND_BOOKING_TO"));
                                _result.Append("</div>");
                                _result.Append("<div class='string-calendar-icon'>");
                                    _result.Append("<i class=\"fa-regular fa-calendar\">");
                                    _result.Append("</i>");
                                    _result.Append(_timeFilterMeeting.To_Combined.Value.ToString("ddd dd.MM.yyyy"));
                                _result.Append("</div>");
                                _result.Append("<div class='string-time-icon'>");
                                    _result.Append("<i class=\"fa-regular fa-clock\">");
                                    _result.Append("</i>");
                                    _result.Append(_timeFilterMeeting.To_Combined.Value.ToString("HH:mm"));
                                _result.Append("</div>");
                            _result.Append("</div>");
                        }
                    }
                }
                return _result.ToString();
            }
            else if (TimeFilter.ShowSeries == true &&
                     TimeFilter.Series_StartDate != null && TimeFilter.Series_EndDate != null &&
                     TimeFilter.Series_FromHour != null && TimeFilter.Series_ToHour != null)
            {
                if (TimeFilter.Series_Type == SeriesType.Daily)
                {
                    if (TimeFilter.Daily_EveryDay == false && TimeFilter.Daily_DayInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Date)
                    {
                        var dailyText = _TextProvider.Get("BOOKING_STRING_DAILY_INTERVAL_TEXT_ENDDATE");

                        dailyText = dailyText.Replace("{0}", TimeFilter.Daily_DayInterval.ToString());
                        dailyText = dailyText.Replace("{1}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                        dailyText = dailyText.Replace("{2}", TimeFilter.Series_EndDate.Value.ToString("dd.MM.yyyy"));
                        dailyText = dailyText.Replace("{3}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                        dailyText = dailyText.Replace("{4}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                        return dailyText;
                    }
                    else if (TimeFilter.Series_Duration_Type == SeriesDurationType.Date)
                    {
                        var dailyText = _TextProvider.Get("BOOKING_STRING_DAILY_TEXT_ENDDATE");

                        dailyText = dailyText.Replace("{0}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                        dailyText = dailyText.Replace("{1}", TimeFilter.Series_EndDate.Value.ToString("dd.MM.yyyy"));
                        dailyText = dailyText.Replace("{2}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                        dailyText = dailyText.Replace("{3}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                        return dailyText;
                    }
                    else if (TimeFilter.Daily_EveryDay == false && TimeFilter.Daily_DayInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions)
                    {
                        var dailyText = _TextProvider.Get("BOOKING_STRING_DAILY_INTERVAL_TEXT_REPITIION");

                        dailyText = dailyText.Replace("{0}", TimeFilter.Daily_DayInterval.ToString());
                        dailyText = dailyText.Replace("{1}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                        dailyText = dailyText.Replace("{2}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));
                        dailyText = dailyText.Replace("{3}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                        dailyText = dailyText.Replace("{4}", TimeFilter.Series_RepetitionCount.ToString());

                        return dailyText;
                    }
                    else if (TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions)
                    {
                        var dailyText = _TextProvider.Get("BOOKING_STRING_DAILY_TEXT_REPITION");

                        dailyText = dailyText.Replace("{0}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                        dailyText = dailyText.Replace("{1}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));
                        dailyText = dailyText.Replace("{2}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                        dailyText = dailyText.Replace("{3}", TimeFilter.Series_RepetitionCount.ToString());

                        return dailyText;
                    }
                }
                else if (TimeFilter.Series_Type == SeriesType.Weekly)
                {
                    if (TimeFilter.Weekly_Weekdays != null && TimeFilter.Weekly_Weekdays.Count() > 0)
                    {
                        string weekDayText = "";
                        string undText = _TextProvider.Get("AND");

                        foreach (WeekDay weekDay in TimeFilter.Weekly_Weekdays.OrderBy(p => p).ToList())
                        {
                            if (weekDay == TimeFilter.Weekly_Weekdays.OrderBy(p => p).ToList().Last())
                            {
                                weekDayText = weekDayText.TrimEnd().TrimEnd(',');

                                weekDayText += " " + undText + " " + _TextProvider.Get(weekDay.ToString().ToUpper());
                            }
                            else
                            {
                                weekDayText += _TextProvider.Get(weekDay.ToString().ToUpper()) + ", ";
                            }
                        }

                        weekDayText = weekDayText.TrimStart((undText + " ").ToCharArray());
                        weekDayText = weekDayText.TrimEnd(", ".ToCharArray());

                        if (TimeFilter.Weekly_DayInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Date)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_WEEKLY_INTERVAL_TEXT_ENDDATE");

                            dailyText = dailyText.Replace("{0}", TimeFilter.Weekly_DayInterval.ToString());
                            dailyText = dailyText.Replace("{1}", weekDayText);
                            dailyText = dailyText.Replace("{2}",
                                TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{3}",
                                TimeFilter.Series_EndDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{5}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Weekly_DayInterval == 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Date)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_WEEKLY_TEXT_ENDDATE");

                            dailyText = dailyText.Replace("{0}", weekDayText);
                            dailyText = dailyText.Replace("{1}",
                                TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{2}",
                                TimeFilter.Series_EndDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{3}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Weekly_DayInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_WEEKLY_INTERVAL_TEXT_REPITION");

                            dailyText = dailyText.Replace("{0}", TimeFilter.Weekly_DayInterval.ToString());
                            dailyText = dailyText.Replace("{1}", weekDayText);
                            dailyText = dailyText.Replace("{2}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{3}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{4}",
                                TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{5}",
                                TimeFilter.Series_RepetitionCount.ToString());

                            return dailyText;
                        }
                        else if (TimeFilter.Weekly_DayInterval == 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_WEEKLY_TEXT_REPITION");

                            dailyText = dailyText.Replace("{0}", weekDayText);
                            dailyText = dailyText.Replace("{1}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{2}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{3}",
                                TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{4}",
                                TimeFilter.Series_RepetitionCount.ToString());

                            return dailyText;
                        }
                    }
                }
                else if (TimeFilter.Series_Type == SeriesType.Monthly)
                {
                    if (TimeFilter.Monthly_Type == MonthlyType.FixedDate)
                    {
                        if (TimeFilter.Monthly_MonthInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Date && (TimeFilter.Monthly_DayOfMonth < 29 || !TimeFilter.Monthly_TakeLastOfMonth))
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_FIXED_INTERVAL_TEXT_ENDDATE");

                            dailyText = dailyText.Replace("{0}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{1}", TimeFilter.Series_EndDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{2}", TimeFilter.Monthly_MonthInterval.ToString());
                            dailyText = dailyText.Replace("{3}", _TextProvider.Get("BOOKING_STRING_MONTHLY_DAYOFMONTH_" + TimeFilter.Monthly_DayOfMonth.ToString()));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{5}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Monthly_MonthInterval == 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Date && (TimeFilter.Monthly_DayOfMonth < 29 || !TimeFilter.Monthly_TakeLastOfMonth))
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_FIXED_TEXT_ENDDATE");

                            dailyText = dailyText.Replace("{0}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{1}", TimeFilter.Series_EndDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{2}", _TextProvider.Get("BOOKING_STRING_MONTHLY_DAYOFMONTH_" + TimeFilter.Monthly_DayOfMonth.ToString()));
                            dailyText = dailyText.Replace("{3}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Monthly_MonthInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Date && TimeFilter.Monthly_DayOfMonth > 28 && TimeFilter.Monthly_TakeLastOfMonth)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_FIXED_INTERVAL_TEXT_ENDDATE_LASTDAY");

                            dailyText = dailyText.Replace("{0}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{1}", TimeFilter.Series_EndDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{2}", TimeFilter.Monthly_MonthInterval.ToString());
                            dailyText = dailyText.Replace("{3}", _TextProvider.Get("BOOKING_STRING_MONTHLY_DAYOFMONTH_" + TimeFilter.Monthly_DayOfMonth.ToString()));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{5}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Monthly_MonthInterval == 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Date && TimeFilter.Monthly_DayOfMonth > 28 && TimeFilter.Monthly_TakeLastOfMonth)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_FIXED_TEXT_ENDDATE_LASTDAY");

                            dailyText = dailyText.Replace("{0}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{1}", TimeFilter.Series_EndDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{2}", _TextProvider.Get("BOOKING_STRING_MONTHLY_DAYOFMONTH_" + TimeFilter.Monthly_DayOfMonth.ToString()));
                            dailyText = dailyText.Replace("{3}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Monthly_MonthInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions && (TimeFilter.Monthly_DayOfMonth < 29 || !TimeFilter.Monthly_TakeLastOfMonth))
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_FIXED_INTERVAL_TEXT_REPITION");

                            dailyText = dailyText.Replace("{0}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{1}", TimeFilter.Series_RepetitionCount.ToString());
                            dailyText = dailyText.Replace("{2}", TimeFilter.Monthly_MonthInterval.ToString());
                            dailyText = dailyText.Replace("{3}", _TextProvider.Get("BOOKING_STRING_MONTHLY_DAYOFMONTH_" + TimeFilter.Monthly_DayOfMonth.ToString()));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{5}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Monthly_MonthInterval == 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions && (TimeFilter.Monthly_DayOfMonth < 29 || !TimeFilter.Monthly_TakeLastOfMonth))
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_FIXED_TEXT_REPITION");

                            dailyText = dailyText.Replace("{0}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{1}", TimeFilter.Series_RepetitionCount.ToString());
                            dailyText = dailyText.Replace("{2}", _TextProvider.Get("BOOKING_STRING_MONTHLY_DAYOFMONTH_" + TimeFilter.Monthly_DayOfMonth.ToString()));
                            dailyText = dailyText.Replace("{3}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Monthly_MonthInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions && TimeFilter.Monthly_DayOfMonth > 28 && TimeFilter.Monthly_TakeLastOfMonth)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_FIXED_INTERVAL_TEXT_REPITION_LASTDAY");

                            dailyText = dailyText.Replace("{0}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{1}", TimeFilter.Series_RepetitionCount.ToString());
                            dailyText = dailyText.Replace("{2}", TimeFilter.Monthly_MonthInterval.ToString());
                            dailyText = dailyText.Replace("{3}", _TextProvider.Get("BOOKING_STRING_MONTHLY_DAYOFMONTH_" + TimeFilter.Monthly_DayOfMonth.ToString()));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{5}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Monthly_MonthInterval == 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions && TimeFilter.Monthly_DayOfMonth > 28 && TimeFilter.Monthly_TakeLastOfMonth)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_FIXED_TEXT_REPITION_LASTDAY");

                            dailyText = dailyText.Replace("{0}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{1}", TimeFilter.Series_RepetitionCount.ToString());
                            dailyText = dailyText.Replace("{2}", _TextProvider.Get("BOOKING_STRING_MONTHLY_DAYOFMONTH_" + TimeFilter.Monthly_DayOfMonth.ToString()));
                            dailyText = dailyText.Replace("{3}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                    }
                    else if (TimeFilter.Monthly_Type == MonthlyType.WeekRelevant)
                    {
                        if (TimeFilter.Monthly_MonthInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Date)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_WEEK_RELEVANT_INTERVAL_TEXT_ENDDATE");

                            dailyText = dailyText.Replace("{0}", _TextProvider.Get("BOOKING_WEEKLY_INTERVAL_" + TimeFilter.Monthly_WeekInterval.ToString()));
                            dailyText = dailyText.Replace("{1}", _TextProvider.Get(TimeFilter.Monthly_WeekDay.ToString()));
                            dailyText = dailyText.Replace("{2}", TimeFilter.Monthly_MonthInterval.ToString());
                            dailyText = dailyText.Replace("{3}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_EndDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{5}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{6}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Monthly_MonthInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Date)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_WEEK_RELEVANT_TEXT_ENDDATE");

                            dailyText = dailyText.Replace("{0}", _TextProvider.Get("BOOKING_WEEKLY_INTERVAL_" + TimeFilter.Monthly_WeekInterval.ToString()));
                            dailyText = dailyText.Replace("{1}", _TextProvider.Get(TimeFilter.Monthly_WeekDay.ToString()));
                            dailyText = dailyText.Replace("{2}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{3}", TimeFilter.Series_EndDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{5}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));

                            return dailyText;
                        }
                        else if (TimeFilter.Monthly_MonthInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_WEEK_RELEVANT_INTERVAL_TEXT_REPITIONS");

                            dailyText = dailyText.Replace("{0}", _TextProvider.Get("BOOKING_WEEKLY_INTERVAL_" + TimeFilter.Monthly_WeekInterval.ToString()));
                            dailyText = dailyText.Replace("{1}", _TextProvider.Get(TimeFilter.Monthly_WeekDay.ToString()));
                            dailyText = dailyText.Replace("{2}", TimeFilter.Monthly_MonthInterval.ToString());
                            dailyText = dailyText.Replace("{3}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{5}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{6}", TimeFilter.Series_RepetitionCount.ToString());

                            return dailyText;
                        }
                        else if (TimeFilter.Monthly_MonthInterval > 1 && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions)
                        {
                            var dailyText = _TextProvider.Get("BOOKING_STRING_MONTHLY_WEEK_RELEVANT_TEXT_REPITIONS");

                            dailyText = dailyText.Replace("{0}", _TextProvider.Get("BOOKING_WEEKLY_INTERVAL_" + TimeFilter.Monthly_WeekInterval.ToString()));
                            dailyText = dailyText.Replace("{1}", _TextProvider.Get(TimeFilter.Monthly_WeekDay.ToString()));
                            dailyText = dailyText.Replace("{2}", TimeFilter.Series_FromHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{3}", TimeFilter.Series_ToHour.Value.ToString("HH:mm"));
                            dailyText = dailyText.Replace("{4}", TimeFilter.Series_StartDate.Value.ToString("dd.MM.yyyy"));
                            dailyText = dailyText.Replace("{5}", TimeFilter.Series_RepetitionCount.ToString());

                            return dailyText;
                        }
                    }
                }
            }

            return "";
        }

        public string GetRoomsInString(List<V_ROOM_Rooms> RoomList, List<V_ROOM_RoomOptions>? Options = null)
        {
            if (RoomList.Where(p => p.IsSelected == true).Count() > 0)
            {
                string result = "";

                foreach (var room in RoomList.Where(p => p.RoomGroupFamilyID == null).OrderBy(p => p.Name).ToList())
                {
                    bool isSelected = false;

                    if (room.IsSelected == true)
                    {
                        isSelected = true;
                    }

                    if (RoomList.Where(p => p.RoomGroupFamilyID == room.ID && p.IsSelected == true).Any())
                    {
                        isSelected = true;
                    }

                    if (isSelected == true)
                    {
                        if (room.BookableErrors != null && room.BookableErrors.Count() > 0)
                        {
                            result +=
                                "<div class='room-string-building'><i class=\"fa-regular fa-calendar-exclamation room-error\"></i>" +
                                room.Name + "</div>";
                        }
                        else
                        {
                            result += "<div class='room-string-building'>" + room.Name + "</div>";
                        }
                    }

                    if (room.HasRooms == true)
                    {
                        foreach (var subR in RoomList.Where(p => p.RoomGroupFamilyID == room.ID && p.IsSelected == true)
                                     .OrderBy(p => p.Name).ToList())
                        {
                            if (subR.BookableErrors != null && subR.BookableErrors.Count() > 0)
                            {
                                result +=
                                    "<div class='room-string-subroom'><i class=\"fa-solid fa-arrow-turn-down-right\"></i><i class=\"fa-regular fa-calendar-exclamation room-error\"></i>" +
                                    subR.Name + "</div>";
                            }
                            else
                            {
                                result +=
                                    "<div class='room-string-subroom'><i class=\"fa-solid fa-arrow-turn-down-right\"></i>" +
                                    subR.Name + "</div>";
                            }
                            if (Options != null)
                            {
                                List<V_ROOM_RoomOptions> optionList = Options.Where(p => p.ROOM_Room_ID == subR.ID && p.Enabled != false && p.IsSelected == true).ToList();
                                foreach (V_ROOM_RoomOptions option in optionList.OrderBy(p => p.Pos).ToList())
                                {
                                    result +=
                                    "<div class='room-string-subroom-options'><i class=\"fa-solid fa-arrow-turn-down-right\"></i>" +
                                    option.Name + "</div>";
                                }
                            }
                        }
                    }
                    if (Options != null)
                    {
                        List<V_ROOM_RoomOptions> optionList = Options.Where(p => p.ROOM_Room_ID == room.ID && p.Enabled != false && p.IsSelected == true).ToList();
                        foreach (V_ROOM_RoomOptions option in optionList.OrderBy(p => p.Pos).ToList())
                        {
                            result +=
                            "<div class='room-string-room-options'><i class=\"fa-solid fa-arrow-turn-down-right\"></i>" +
                            option.Name + "</div>";
                        }
                    }
                }

                return result;
            }

            return _TextProvider.Get("FRONTEND_BOOKING_NO_ROOM_SELECTED");
        }

        public void NotifyValuesChanged()
        {
            OnValuesChanged?.Invoke();
        }

        public async Task<string?> CheckRoomAvailability(V_ROOM_Rooms Room, Booking _booking, bool IncludeRoomName = false)
        {
            List<V_ROOM_Booking> _roomBookings = new List<V_ROOM_Booking>();
            if (_booking.StartDate != null && _booking.EndDate != null)
            {
                _roomBookings = await _RoomProvider.VerfiyBookings(Room.ID, _booking.StartDate.Value, _booking.EndDate.Value);
                if (_roomBookings != null && _roomBookings.Any())
                {
                    bool _isOverlapping = false;

                    foreach (V_ROOM_Booking _roomBooking in _roomBookings)
                    {
                        if (_roomBooking.StartDate != null && _roomBooking.EndDate != null && _booking.StartDate.Value < _roomBooking.EndDate.Value && _roomBooking.StartDate.Value < _booking.EndDate.Value)
                        {
                            _isOverlapping = true;
                            break;
                        }
                    }
                    if (_isOverlapping)
                    {
                        StringBuilder errorMessage = new StringBuilder();

                        if (IncludeRoomName)
                        {
                            if (!string.IsNullOrEmpty(Room.BuildingName))
                            {
                                errorMessage.Append("(");
                                errorMessage.Append(Room.BuildingName);
                                errorMessage.Append(") ");
                            }

                            errorMessage.Append(Room.Name);
                            errorMessage.Append(" - ");
                        }
                        if (_booking.StartDate.Value.Date == _booking.EndDate.Value.Date)
                        {
                            errorMessage.Append(
                                            _TextProvider.Get("ROOM_NOT_AVAILABLE_IN_SINGLE_DAY_TIMES")
                                                         .Replace("{0}", "<b>" + _booking.StartDate.Value.ToString("dd.MM.yyy") + "</b>")
                                                         .Replace("{1}", "<b>" + _booking.StartDate.Value.ToString("HH:mm") + "</b>")
                                                         .Replace("{2}", "<b>" + _booking.EndDate.Value.ToString("HH:mm") + "</b>"));
                        }
                        else
                        {
                            errorMessage.Append(
                                            _TextProvider.Get("ROOM_NOT_AVAILABLE_TIMES")
                                                         .Replace("{0}", "<b>" + _booking.StartDate.Value.ToString("dd.MM.yyy") + "</b>")
                                                         .Replace("{1}", "<b>" + _booking.StartDate.Value.ToString("HH:mm") + "</b>")
                                                         .Replace("{2}", "<b>" + _booking.EndDate.Value.ToString("dd.MM.yyy") + "</b>")
                                                         .Replace("{3}", "<b>" + _booking.EndDate.Value.ToString("HH:mm") + "</b>"));
                        }
                        return errorMessage.Replace(Environment.NewLine, "<br/>").ToString();
                    }
                    else
                    {
                        StringBuilder errorMessage = new StringBuilder();

                        if (IncludeRoomName)
                        {
                            if (!string.IsNullOrEmpty(Room.BuildingName))
                            {
                                errorMessage.Append("(");
                                errorMessage.Append(Room.BuildingName);
                                errorMessage.Append(") ");
                            }

                            errorMessage.Append(Room.Name);
                            errorMessage.Append(" - ");
                        }

                        errorMessage.Append(
                                        _TextProvider.Get("ROOM_NOT_AVAILABLE_BUFFER_TIME")
                                                     .Replace("{0}", "<b>" + Room.BufferTime + "</b>"));

                        return errorMessage.Replace(Environment.NewLine, "<br/>").ToString();
                    }
                }
            }

            return null;
        }

        public List<Booking> GetDatesToBook(TimeFilter TimeFilter)
        {
            List<Booking> _result = new List<Booking>();

            if (TimeFilter.ShowSeries == false)
            {
                foreach (TimeFilter_Meeting _meeting in TimeFilter.Meetings)
                {
                    if (_meeting.From_Combined != null && _meeting.To_Combined != null &&
                        _meeting.From_Combined < _meeting.To_Combined)
                        {
                            _result.Add(new Booking()
                            {
                                StartDate = _meeting.From_Combined.Value,
                                EndDate = _meeting.To_Combined.Value
                            });
                        }
                }
            }
            else if (TimeFilter.ShowSeries == true && TimeFilter.Series_StartDate != null &&
                    ((TimeFilter.Series_EndDate != null && TimeFilter.Series_StartDate < TimeFilter.Series_EndDate && TimeFilter.Series_Duration_Type == SeriesDurationType.Date) || (TimeFilter.Series_RepetitionCount > 0 && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions)) &&
                    TimeFilter.Series_FromHour != null && TimeFilter.Series_ToHour != null && TimeFilter.Series_ToHour.Value.TimeOfDay > TimeFilter.Series_FromHour.Value.TimeOfDay)
            {
                DateTime _date = TimeFilter.Series_StartDate.Value.Date;
                int _count = 0;
                bool _firstMatch = false;

                if (TimeFilter.Series_Type == SeriesType.Daily)
                {
                    int _intervall = TimeFilter.Daily_DayInterval;
                    if (TimeFilter.Daily_EveryDay)
                    {
                        _intervall = 1;
                    }
                    while ((TimeFilter.Series_EndDate != null && _date.Date <= TimeFilter.Series_EndDate.Value.Date && TimeFilter.Series_Duration_Type == SeriesDurationType.Date) || (_count < TimeFilter.Series_RepetitionCount && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions))
                    {
                        // Erste Übereinstimmung für Alternierung setzen
                        if (!_firstMatch)
                        {
                            _firstMatch = true;
                        }
                        // Neue Buchung setzen
                        _result.Add(new Booking()
                        {
                            StartDate = _date.Date.AddHours(TimeFilter.Series_FromHour.Value.Hour).AddMinutes(TimeFilter.Series_FromHour.Value.Minute),
                            EndDate = _date.Date.AddHours(TimeFilter.Series_ToHour.Value.Hour).AddMinutes(TimeFilter.Series_ToHour.Value.Minute)
                        });
                        // Mit Datum auf nächsten Termin springen
                        _date = _date.AddDays(_intervall);
                        // Wenn der erste Termin gefunden wurde, für Alternierung die Tage zählen
                        if (_firstMatch)
                        {
                            _count++;
                        }
                    }
                }
                else if (TimeFilter.Series_Type == SeriesType.Weekly && TimeFilter.Weekly_Weekdays != null && TimeFilter.Weekly_Weekdays.Any())
                {
                    while ((TimeFilter.Series_EndDate != null && _date.Date <= TimeFilter.Series_EndDate.Value.Date && TimeFilter.Series_Duration_Type == SeriesDurationType.Date) || (_result.Count() < TimeFilter.Series_RepetitionCount && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions))
                    {
                        if (TimeFilter.Weekly_Weekdays.Contains((WeekDay)_date.DayOfWeek) && _count % TimeFilter.Weekly_DayInterval == 0)
                        {
                            // Erste Übereinstimmung für Alternierung setzen
                            if (!_firstMatch)
                            {
                                _firstMatch = true;
                            }
                            // Neue Buchung setzen
                            _result.Add(new Booking()
                            {
                                StartDate = _date.Date.AddHours(TimeFilter.Series_FromHour.Value.Hour).AddMinutes(TimeFilter.Series_FromHour.Value.Minute),
                                EndDate = _date.Date.AddHours(TimeFilter.Series_ToHour.Value.Hour).AddMinutes(TimeFilter.Series_ToHour.Value.Minute)
                            });
                        }
                        // Mit Datum auf nächsten Termin springen
                        _date = _date.AddDays(1);
                        // Wenn der erste Termin gefunden wurde, für Alternierung die Wochen zählen
                        if (_firstMatch && _date.DayOfWeek == DayOfWeek.Monday)
                        {
                            _count++;
                        }
                    }
                }
                else if (TimeFilter.Series_Type == SeriesType.Monthly && TimeFilter.Monthly_Type == MonthlyType.FixedDate)
                {
                    DateTime? _lastDate = null;
                    while ((TimeFilter.Series_EndDate != null && _date.Date <= TimeFilter.Series_EndDate.Value.Date && TimeFilter.Series_Duration_Type == SeriesDurationType.Date) || (_result.Count() < TimeFilter.Series_RepetitionCount && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions))
                    {
                        // Wenn eingestellt, soll der letzte vom Monat verwendet werden anstatt nach dem nächsten übereinstimmenden Datum zu suchen
                        DateTime _lastDayOfMonth = new DateTime(_date.Year, _date.Month, 1).AddMonths(1).AddDays(-1);
                        int _dayOfMonth = TimeFilter.Monthly_DayOfMonth;
                        if (TimeFilter.Monthly_TakeLastOfMonth == true && TimeFilter.Monthly_DayOfMonth > _lastDayOfMonth.Date.Day)
                        {
                            _dayOfMonth = _lastDayOfMonth.Date.Day;
                        }
                        // Berechnung alternierung
                        int _monthDifference = 0;
                        if (_lastDate != null)
                        {
                            _monthDifference = ((_date.Year - _lastDate.Value.Year) * 12) + _date.Month - _lastDate.Value.Month;
                        }
                        if (_date.Date.Day == _dayOfMonth && (_monthDifference >= TimeFilter.Monthly_MonthInterval || _lastDate == null))
                        {
                            _result.Add(new Booking()
                            {
                                StartDate = _date.Date.AddHours(TimeFilter.Series_FromHour.Value.Hour).AddMinutes(TimeFilter.Series_FromHour.Value.Minute),
                                EndDate = _date.Date.AddHours(TimeFilter.Series_ToHour.Value.Hour).AddMinutes(TimeFilter.Series_ToHour.Value.Minute)
                            });
                            _lastDate = _date;
                            // Mit Datum auf nächsten Termin springen
                            _date = _date.AddMonths(1);
                        }
                        else
                        {
                            // Mit Datum auf nächsten Termin springen
                            _date = _date.AddDays(1);
                        }
                    }
                }
                else if (TimeFilter.Monthly_Type == MonthlyType.WeekRelevant)
                {
                    DateTime? _lastDate = null;
                    int _countMatchingWeekDays = 0;
                    int _selectedCountWeekDays = 1;
                    switch (TimeFilter.Monthly_WeekInterval)
                    {
                        case WeekDayOfMonth.First:
                            _selectedCountWeekDays = 1;
                            break;
                        case WeekDayOfMonth.Second:
                            _selectedCountWeekDays = 2;
                            break;
                        case WeekDayOfMonth.Third:
                            _selectedCountWeekDays = 3;
                            break;
                        case WeekDayOfMonth.Fourth:
                            _selectedCountWeekDays = 4;
                            break;
                    }
                    // Initialisiere Datum im richtigen Monat
                    DateTime _firstMatchingDay = new DateTime(_date.Year, _date.Month, 1);
                    if (TimeFilter.Monthly_WeekInterval != WeekDayOfMonth.Last)
                    {
                        while ((WeekDay)_firstMatchingDay.DayOfWeek != TimeFilter.Monthly_WeekDay || _countMatchingWeekDays < _selectedCountWeekDays)
                        {
                            if ((WeekDay)_firstMatchingDay.DayOfWeek == TimeFilter.Monthly_WeekDay)
                            {
                                _countMatchingWeekDays++;
                                if (_countMatchingWeekDays >= _selectedCountWeekDays)
                                {
                                    _countMatchingWeekDays = 0;
                                    break;
                                }
                            }
                            _firstMatchingDay = _firstMatchingDay.AddDays(1);
                        }
                    }
                    else
                    {
                        _firstMatchingDay = _date;
                        while (_firstMatchingDay.Month == _date.Month && ((WeekDay)_firstMatchingDay.DayOfWeek != TimeFilter.Monthly_WeekDay || _firstMatchingDay.Month == _firstMatchingDay.AddDays(7).Month))
                        {
                            _firstMatchingDay = _firstMatchingDay.AddDays(1);
                        }
                    }
                    if (_firstMatchingDay.Date < _date)
                    {
                        _date = new DateTime(_date.Year, _date.Month, 1).AddMonths(1);
                    }
                    else
                    {
                        _date = new DateTime(_date.Year, _date.Month, 1);
                    }
                    // Berechne Tage
                    while ((TimeFilter.Series_EndDate != null && _date.Date <= TimeFilter.Series_EndDate.Value.Date && TimeFilter.Series_Duration_Type == SeriesDurationType.Date) || (_result.Count() < TimeFilter.Series_RepetitionCount && TimeFilter.Series_Duration_Type == SeriesDurationType.Repetitions))
                    {
                        // Berechnung alternierung
                        int _monthDifference = 0;
                        if (_lastDate != null)
                        {
                            _monthDifference = ((_date.Year - _lastDate.Value.Year) * 12) + _date.Month - _lastDate.Value.Month;
                        }
                        if (TimeFilter.Monthly_WeekDay == (WeekDay)_date.DayOfWeek && (_monthDifference >= TimeFilter.Monthly_MonthInterval || _lastDate == null))
                        {
                            _countMatchingWeekDays++;
                            if ((_countMatchingWeekDays >= _selectedCountWeekDays && TimeFilter.Monthly_WeekInterval != WeekDayOfMonth.Last) || (_date.Month != _date.AddDays(7).Month && TimeFilter.Monthly_WeekInterval == WeekDayOfMonth.Last))
                            {
                                _result.Add(new Booking()
                                {
                                    StartDate = _date.Date.AddHours(TimeFilter.Series_FromHour.Value.Hour).AddMinutes(TimeFilter.Series_FromHour.Value.Minute),
                                    EndDate = _date.Date.AddHours(TimeFilter.Series_ToHour.Value.Hour).AddMinutes(TimeFilter.Series_ToHour.Value.Minute)
                                });
                                _lastDate = _date;
                                _countMatchingWeekDays = 0;
                                _date = new DateTime(_date.Year, _date.Month, 1).AddMonths(1);
                                continue;
                            }
                        }
                        else if (_monthDifference < TimeFilter.Monthly_MonthInterval && _lastDate != null)
                        {
                            _date = new DateTime(_date.Year, _date.Month, 1).AddMonths(1);
                            continue;
                        }
                        // Mit Datum auf nächsten Termin springen
                        _date = _date.AddDays(1);
                    }
                }
            }

            return _result;
        }

        public async Task<decimal?> GetRoomCost(Guid ROOM_Room_ID, Guid? AUTH_Company_Type_ID, DateTime StartDate,
            DateTime EndDate)
        {
            var Room = await _RoomProvider.GetRoom(ROOM_Room_ID);

            if (Room == null)
                return 0;

            var pricing = await _RoomProvider.GetRoomPricing(ROOM_Room_ID);

            var relevantPricing = pricing.FirstOrDefault(p => p.Default == true);
            decimal? price = 0;

            if (AUTH_Company_Type_ID != null)
            {
                var CompanyPricing = pricing.FirstOrDefault(p => p.AUTH_Company_Type_ID == AUTH_Company_Type_ID);

                if (CompanyPricing != null)
                {
                    relevantPricing = CompanyPricing;
                }
            }

            if (relevantPricing != null)
            {
                DateTime calcDate = StartDate;

                while (calcDate < EndDate)
                {
                    var todayEnd = new DateTime(calcDate.Year, calcDate.Month, calcDate.Day).AddDays(1);
                    TimeSpan time = TimeSpan.Zero;

                    if (todayEnd < EndDate)
                    {
                        time = todayEnd - calcDate;
                    }
                    else
                    {
                        todayEnd = EndDate;
                        time = EndDate - calcDate;
                    }

                    if (time.TotalMinutes > 0)
                    {
                        if (Room.PricingType == RoomPricingType.Daily)
                        {
                            if (time.TotalMinutes > (relevantPricing.HalfDayHourLimit * 60))
                            {
                                price += relevantPricing.FullDayPrice;
                            }
                            else
                            {
                                price += relevantPricing.HalfDayPrice;
                            }
                        }
                        else if (Room.PricingType == RoomPricingType.Hourly)
                        {
                            var locprice = Math.Ceiling(decimal.Parse(time.TotalMinutes.ToString()) / 60) *
                                           relevantPricing.HourPrice;

                            if (locprice < relevantPricing.MinPrice)
                            {
                                price += relevantPricing.MinPrice;
                            }
                            else if (locprice > relevantPricing.MaxPrice)
                            {
                                price += relevantPricing.MaxPrice;
                            }
                            else
                            {
                                price += locprice;
                            }
                        }
                    }

                    calcDate = todayEnd;
                }
            }

            return price;
        }

        public async Task<bool> SendMessage(string BodyTextCode, string TitleTextCode, Guid ROOM_Booking_ID,
            string BaseUri, Guid MessageType_ID)
        {
            var dbBooking = await _RoomProvider.GetBooking(ROOM_Booking_ID);

            if (dbBooking != null)
            {
                Guid? Auth_User_ID = null;
                Guid? AUTH_Municiaplity_ID = null;
                string Title = "";
                string EMail = "";
                string Phone = "";

                if (dbBooking.ROOM_BookingGroup_ID != null)
                {
                    var bookingGroup = await _RoomProvider.GetBookingGroup(dbBooking.ROOM_BookingGroup_ID.Value);

                    if (bookingGroup != null)
                    {
                        Auth_User_ID = bookingGroup.AUTH_User_ID;
                        AUTH_Municiaplity_ID = bookingGroup.AUTH_MunicipalityID;
                        Title = bookingGroup.Title;
                        EMail = bookingGroup.Email;
                        Phone = bookingGroup.MobilePhone;
                    }
                }
                else if (dbBooking.ROOM_BookingGroupBackend_ID != null)
                {
                    var backendBookingGroup =
                        await _RoomProvider.GetBookingGroupBackend(dbBooking.ROOM_BookingGroupBackend_ID.Value);

                    if (backendBookingGroup != null)
                    {
                        Auth_User_ID = backendBookingGroup.AUTH_Users_ID;
                        AUTH_Municiaplity_ID = backendBookingGroup.AUTH_Municipality_ID;
                        Title = backendBookingGroup.Title;
                        EMail = backendBookingGroup.EMail;
                        Phone = backendBookingGroup.MobilePhone;
                    }
                }

                if (Auth_User_ID != null && AUTH_Municiaplity_ID != null)
                {
                    Guid userLangID = LanguageSettings.German;

                    var userLang = await _AuthProvider.GetSettings(Auth_User_ID.Value);

                    if (userLang != null && userLang.LANG_Languages_ID == LanguageSettings.Italian)
                    {
                        userLangID = LanguageSettings.Italian;
                    }

                    var text = _TextProvider.Get(BodyTextCode, userLangID);

                    text = text.Replace("{0}", Title);

                    if (dbBooking.StartDate != null && dbBooking.EndDate != null)
                    {
                        text = text.Replace("{1}", dbBooking.StartDate.Value.ToString("dd.MM.yyyy HH:mm"));
                        text = text.Replace("{2}", dbBooking.EndDate.Value.ToString("dd.MM.yyyy HH:mm"));
                    }

                    var msg = await _MessageService.GetMessage(Auth_User_ID.Value,
                        AUTH_Municiaplity_ID.Value,
                        text,
                        text,
                        _TextProvider.Get(TitleTextCode, userLangID),
                        MessageType_ID, false, null);

                    if (msg != null)
                    {
                        await _MessageService.SendMessage(msg, BaseUri + "/User/Services");
                    }
                }
                else if (AUTH_Municiaplity_ID != null && (!string.IsNullOrEmpty(EMail) || !string.IsNullOrEmpty(Phone)))
                {
                    string title = _TextProvider.Get(TitleTextCode, LanguageSettings.German) + " - " +
                                   _TextProvider.Get(TitleTextCode, LanguageSettings.Italian);

                    string text = _TextProvider.Get(BodyTextCode, LanguageSettings.German);
                    text += "<br><br>" + _TextProvider.Get(BodyTextCode, LanguageSettings.Italian);

                    text = text.Replace("{0}", Title);

                    if (dbBooking.StartDate != null && dbBooking.EndDate != null)
                    {
                        text = text.Replace("{1}", dbBooking.StartDate.Value.ToString("dd.MM.yyyy HH:mm"));
                        text = text.Replace("{2}", dbBooking.EndDate.Value.ToString("dd.MM.yyyy HH:mm"));
                    }

                    if (!string.IsNullOrEmpty(EMail))
                    {
                        var mail = new MSG_Mailer();

                        mail.ToAdress = EMail;

                        mail.Subject = title;

                        mail.Body = text;

                        await _MailerService.SendMail(mail, null, AUTH_Municiaplity_ID.Value);
                    }

                    if (!string.IsNullOrEmpty(Phone))
                    {
                        text = text.Replace("<br>", Environment.NewLine);

                        var sms = new MSG_SMS();

                        sms.ID = Guid.NewGuid();
                        sms.PhoneNumber = Phone;
                        sms.Message = text;
                        sms.AUTH_Municipality_ID = AUTH_Municiaplity_ID.Value;

                        await _SMSService.SendSMS(sms, AUTH_Municiaplity_ID.Value);
                    }
                }
            }

            return true;
        }

        public async void SendMessagesToContacts(Guid AUTH_Municipality_ID, Guid BookinGroup_ID, string Title,
            bool Cancel = false, ROOM_Booking? Booking = null)
        {
            var bookings = new List<ROOM_Booking>();

            if (Booking != null)
            {
                bookings = new List<ROOM_Booking>() { Booking };
            }
            else
            {
                bookings = await _RoomProvider.GetBookingsByGroupID(BookinGroup_ID);
            }

            foreach (var booking in bookings)
            {
                if (booking.ROOM_Room_ID != null)
                {
                    var room = await _RoomProvider.GetVRoom(booking.ROOM_Room_ID.Value);

                    if (room != null)
                    {
                        var cts = await _RoomProvider.GetBookingOrBookingGroupContactsToNotify(BookinGroup_ID, booking);
                        var contactsToEmail = cts.Item1;
                        var contactsToSms = cts.Item2;

                        foreach (var contact in contactsToEmail)
                        {
                            if (contact == null)
                                continue;
                            var dbCont = await _ContProvider.GetContact(contact.Value);

                            if (dbCont == null || string.IsNullOrEmpty(dbCont.EMail))
                                continue;


                            MSG_Mailer mail = new MSG_Mailer();

                            mail.ToAdress = dbCont.EMail;

                            if (!Cancel)
                            {
                                mail.Subject = _TextProvider.Get("ROOM_PERSONAL_NEW_SUBJECT");

                                mail.Body = _TextProvider.Get("ROOM_PERSONAL_NEW_BODY");

                                mail.Body = mail.Body.Replace("{Title}", Title);
                                mail.Body = mail.Body.Replace("{Room}", room.CombinedName);

                                if (booking.StartDate != null && booking.EndDate != null)
                                {
                                    mail.Body = mail.Body.Replace("{Time}",
                                        booking.StartDate.Value.ToString("dd.MM.yyyy HH:mm") + " - " +
                                        booking.EndDate.Value.ToString("dd.MM.yyyy HH:mm"));
                                }
                            }
                            else
                            {
                                mail.Subject = _TextProvider.Get("ROOM_PERSONAL_CANCELLED_SUBJECT");

                                mail.Body = _TextProvider.Get("ROOM_PERSONAL_CANCELLED_BODY");

                                mail.Body = mail.Body.Replace("{Title}", Title);
                                mail.Body = mail.Body.Replace("{Room}", room.CombinedName);

                                if (booking.StartDate != null && booking.EndDate != null)
                                {
                                    mail.Body = mail.Body.Replace("{Time}",
                                        booking.StartDate.Value.ToString("dd.MM.yyyy HH:mm") + " - " +
                                        booking.EndDate.Value.ToString("dd.MM.yyyy HH:mm"));
                                }
                            }

                            await _MailerService.SendMail(mail, null, AUTH_Municipality_ID);
                        }

                        foreach (var contact in contactsToSms)
                        {
                            if (contact == null)
                                continue;
                            var dbCont = await _ContProvider.GetContact(contact.Value);

                            if (dbCont == null || string.IsNullOrEmpty(dbCont.Phone))
                                continue;


                            MSG_SMS sms = new MSG_SMS();

                            sms.PhoneNumber = dbCont.Phone;

                            if (!Cancel)
                            {
                                sms.Message = _TextProvider.Get("ROOM_PERSONAL_NEW_SMS");

                                sms.Message = sms.Message.Replace("{Title}", Title);
                                sms.Message = sms.Message.Replace("{Room}", room.CombinedName);

                                if (booking.StartDate != null && booking.EndDate != null)
                                {
                                    sms.Message = sms.Message.Replace("{Time}",
                                        booking.StartDate.Value.ToString("dd.MM.yyyy HH:mm") + " - " +
                                        booking.EndDate.Value.ToString("dd.MM.yyyy HH:mm"));
                                }
                            }
                            else
                            {
                                sms.Message = _TextProvider.Get("ROOM_PERSONAL_CANCELLED_SMS");

                                sms.Message = sms.Message.Replace("{Title}", Title);
                                sms.Message = sms.Message.Replace("{Room}", room.CombinedName);

                                if (booking.StartDate != null && booking.EndDate != null)
                                {
                                    sms.Message = sms.Message.Replace("{Time}",
                                        booking.StartDate.Value.ToString("dd.MM.yyyy HH:mm") + " - " +
                                        booking.EndDate.Value.ToString("dd.MM.yyyy HH:mm"));
                                }
                            }
                            await _SMSService.SendSMS(sms, AUTH_Municipality_ID);
                        }
                    }
                }
            }
        }
    }
}