using System.Linq;

namespace ICWebApp.Domain.Models.Rooms
{
    public class TimeFilter : IDisposable
    {
        // Variablen
        private List<TimeFilter_Meeting> _meetings = new List<TimeFilter_Meeting>();
        private bool _showSeries = false;
        private SeriesType _seriesType = SeriesType.Daily;
        private bool _daily_EveryDay = false;
        private int _daily_DayInterval = 1;
        private int _weekly_DayInterval = 1;
        private List<WeekDay> _weekly_Weekdays = new List<WeekDay>();
        private int _monthly_MonthInterval = 1;
        private int _monthly_DayOfMonth = DateTime.Now.Day;
        private bool _monthly_TakeLastOfMonth = true;
        private MonthlyType _monthly_Type = MonthlyType.FixedDate;
        private WeekDayOfMonth _monthly_WeekInterval = WeekDayOfMonth.First;
        private WeekDay _monthly_WeekDay = WeekDay.Monday;
        private DateTime? _series_StartDate = DateTime.Today;
        private DateTime? _series_EndDate = DateTime.Today.AddDays(7);
        private SeriesDurationType _series_Duration_Type = SeriesDurationType.Date;
        private DateTime? _series_FromHour;
        private DateTime? _series_ToHour;
        private int _series_RepetitionCount = 1;
        private System.Timers.Timer? _eventTriggerTimer;
        public event Action? OnDataChanged;
        // Properties
        public List<TimeFilter_Meeting> Meetings
        {
            get
            {
                return _meetings;
            }
            set
            {
                _meetings = value;
                NotifyDataChanged();
            }
        }
        public bool ShowSeries
        {
            get
            {
                return _showSeries;
            }
            set
            {
                _showSeries = value;
                NotifyDataChanged();
            }
        }
        public SeriesType Series_Type
        {
            get
            {
                return _seriesType;
            }
            set
            {
                _seriesType = value;
                NotifyDataChanged();
            }
        }
        public bool Daily_EveryDay
        {
            get
            {
                return _daily_EveryDay;
            }
            set
            {
                _daily_EveryDay = value;
                NotifyDataChanged();
            }
        }
        public int Daily_DayInterval 
        {
            get
            {
                return _daily_DayInterval;
            }
            set
            {
                if (value >= 1)
                {
                    _daily_DayInterval = value;
                }
                else
                {
                    _daily_DayInterval = 1;
                }
                NotifyDataChanged();
            }
        }
        public int Weekly_DayInterval
        {
            get
            {
                return _weekly_DayInterval;
            }
            set
            {
                if (value >= 1)
                {
                    _weekly_DayInterval = value;
                }
                else
                {
                    _weekly_DayInterval = 1;
                }
                NotifyDataChanged();
            }
        }
        public List<WeekDay> Weekly_Weekdays
        {
            get
            {
                return _weekly_Weekdays;
            }
            set
            {
                _weekly_Weekdays = value;
                NotifyDataChanged();
            }
        }
        public MonthlyType Monthly_Type
        {
            get
            {
                return _monthly_Type;
            }
            set
            {
                _monthly_Type = value;
                NotifyDataChanged();
            }
        }
        public int Monthly_DayOfMonth
        {
            get
            {
                return _monthly_DayOfMonth;
            }
            set
            {
                if (value >= 1 && value <= 31)
                {
                    _monthly_DayOfMonth = value;
                }
                else if (value < 1)
                {
                    _monthly_DayOfMonth = 1;
                }
                else if (value > 31)
                {
                    _monthly_DayOfMonth = 31;
                }

                NotifyDataChanged();
            }
        }
        public bool Monthly_TakeLastOfMonth
        {
            get
            {
                return _monthly_TakeLastOfMonth;
            }
            set
            {
                _monthly_TakeLastOfMonth = value;
                NotifyDataChanged();
            }
        }
        public int Monthly_MonthInterval
        {
            get
            {
                return _monthly_MonthInterval;
            }
            set
            {
                if (value >= 1)
                {
                    _monthly_MonthInterval = value;
                }
                else
                {
                    _monthly_MonthInterval = 1;
                }
                NotifyDataChanged();
            }
        }
        public WeekDayOfMonth Monthly_WeekInterval
        {
            get
            {
                return _monthly_WeekInterval;
            }
            set
            {
                _monthly_WeekInterval = value;
                NotifyDataChanged();
            }
        }
        public WeekDay Monthly_WeekDay
        {
            get
            {
                return _monthly_WeekDay;
            }
            set
            {
                _monthly_WeekDay = value;
                NotifyDataChanged();
            }
        }
        public DateTime? Series_StartDate
        {
            get
            {
                return _series_StartDate;
            }
            set
            {
                _series_StartDate = value;

                if (_series_EndDate < _series_StartDate)
                {
                    _series_EndDate = value;
                }
                NotifyDataChanged();
            }
        }
        public DateTime? Series_EndDate
        {
            get
            {
                return _series_EndDate;
            }
            set
            {
                _series_EndDate = value;

                if (value < _series_StartDate)
                {
                    _series_StartDate = value;
                }
                NotifyDataChanged();
            }
        }
        public SeriesDurationType Series_Duration_Type
        {
            get
            {
                return _series_Duration_Type;
            }
            set
            {
                _series_Duration_Type = value;
                NotifyDataChanged();
            }
        }
        public DateTime? Series_FromHour
        {
            get
            {
                return _series_FromHour;
            }
            set
            {
                _series_FromHour = CheckMinutes(value);
                NotifyDataChanged();
            }
        }
        public DateTime? Series_ToHour
        {
            get
            {
                return _series_ToHour;
            }
            set
            {
                _series_ToHour = CheckMinutes(value);
                NotifyDataChanged();
            }
        }
        public int Series_RepetitionCount 
        {
            get
            {
                return _series_RepetitionCount;
            }
            set 
            {
                if (value >= 1)
                {
                    _series_RepetitionCount = value;
                }
                else
                {
                    _series_RepetitionCount = 1;
                }
                NotifyDataChanged();
            } 
        }
        // Methoden
        public void AddNewMeeting(DateTime? fromDate = null, DateTime? fromHour = null, DateTime? toDate = null, DateTime? toHour = null)
        {
            if (this.Meetings == null)
            {
                this.Meetings = new List<TimeFilter_Meeting>();
            }
            this.Meetings.Add(CreateNewTimeSpan(fromDate, fromHour, toDate, toHour));
        }
        public void RemoveMeeting(TimeFilter_Meeting _meeting)
        {
            _meeting.OnDataChanged -= this.OnDataChange;
            this.Meetings.Remove(_meeting);
        }
        public void Weekly_ToggleWeekDay(WeekDay WeekDay)
        {
            if (this._weekly_Weekdays.Contains(WeekDay))
            {
                this._weekly_Weekdays.Remove(WeekDay);
            }
            else
            {
                this._weekly_Weekdays.Add(WeekDay);
            }
            NotifyDataChanged();
        }
        private TimeFilter_Meeting CreateNewTimeSpan(DateTime? fromDate = null, DateTime? fromHour = null, DateTime? toDate = null, DateTime? toHour = null)
        {
            DateTime _now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0).AddDays(1).AddHours(1);
            TimeFilter_Meeting _netTimeSpan = new TimeFilter_Meeting();

            if (fromDate != null)
            {
                _netTimeSpan.FromDate = fromDate;
            }
            else
            {
                _netTimeSpan.FromDate = _now;
            }
            if (fromHour != null)
            {
                _netTimeSpan.FromHour = fromHour;
            }
            else
            {
                _netTimeSpan.FromHour = _now;
            }
            if (toDate != null && toHour >= _netTimeSpan.FromDate)
            {
                _netTimeSpan.ToDate = toDate;
            }
            else if (_now.AddHours(1) >= _netTimeSpan.FromDate)
            {
                _netTimeSpan.ToDate = _now.AddHours(1);
            }
            else if (_netTimeSpan.FromDate != null)
            {
                _netTimeSpan.ToDate = _netTimeSpan.FromDate.Value.AddHours(1);
            }
            if (toHour != null)
            {
                _netTimeSpan.ToHour = toHour;
            }
            else
            {
                _netTimeSpan.ToHour = _now.AddHours(1);
            }

            _netTimeSpan.OnDataChanged += this.OnDataChange;

            return _netTimeSpan;
        }
        private void NotifyDataChanged()
        {
            if (_eventTriggerTimer == null)
            {
                _eventTriggerTimer = new System.Timers.Timer(500);
                _eventTriggerTimer.Elapsed += _eventTriggerTimer_Elapsed;
            }

            _eventTriggerTimer.Stop();
            _eventTriggerTimer.Start();
        }
        private void _eventTriggerTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (OnDataChanged != null)
            {
                OnDataChanged.Invoke();
            }

            if (_eventTriggerTimer != null)
                _eventTriggerTimer.Stop();
        }
        private void OnDataChange()
        {
            if (OnDataChanged != null)
            {
                OnDataChanged.Invoke();
            }
        }
        public void UpdateSeriesRepitionOrEndTime(List<Booking> _bookings)
        {
            if (this.Series_Duration_Type == SeriesDurationType.Repetitions && this.ShowSeries == true && _bookings != null && _bookings.Any())
            {
                DateTime? _endDate = _bookings.Select(p => p.EndDate).Max();
                if (_endDate != null)
                {
                    this._series_EndDate = _endDate.Value.Date;
                }
            }
            else if (this.Series_Duration_Type == SeriesDurationType.Date && this.ShowSeries == true && _bookings != null && _bookings.Any())
            {
                this._series_RepetitionCount = _bookings.Count();
            }
        }
        private DateTime? CheckMinutes(DateTime? _time, int _intervall = 15)
        {
            try
            {
                if (_time != null)
                {
                    int minutesOffset = _time.Value.Minute % _intervall;
                    if (minutesOffset > 0 && minutesOffset >= _intervall / 2)
                    {
                        return _time.Value.AddMinutes(15 - minutesOffset);
                    }
                    else if (minutesOffset > 0 && minutesOffset < _intervall / 2)
                    {
                        return _time.Value.AddMinutes(minutesOffset * -1);
                    }
                }
            }
            catch
            {
            }
            return _time;
        }
        public void Dispose()
        {
            if (Meetings != null && Meetings.Any())
            {
                foreach (TimeFilter_Meeting _meeting in Meetings)
                {
                    _meeting.OnDataChanged -= OnDataChange;
                }
            }
            if (_eventTriggerTimer != null)
            {
                _eventTriggerTimer.Elapsed -= _eventTriggerTimer_Elapsed;
            }
        }
    }
    public enum SeriesType
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2
    }
    public enum SeriesDurationType
    {
        Date = 0,
        Repetitions = 1
    }
    public enum WeekDayOfMonth
    {
        First = 0,
        Second = 1,
        Third = 2,
        Fourth = 3,
        Last = 4
    }
    public enum WeekDay
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
    }
    public enum MonthlyType
    {
        FixedDate = 0,
        WeekRelevant = 1
    }
}
