using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Rooms
{
    public class TimeFilter_Meeting
    {
        private DateTime? _fromDate;
        private DateTime? _fromHour;
        private DateTime? _toDate;
        private DateTime? _toHour;
        private System.Timers.Timer? _eventTriggerTimer;
        public event Action? OnDataChanged; 
        public DateTime? FromDate
        {
            get
            {
                return _fromDate;
            }
            set
            {
                _fromDate = value;

                if (_toDate < _fromDate)
                {
                    _toDate = value;
                }

                NotifyDataChanged();
            }
        }
        public DateTime? FromHour
        {
            get
            {
                return _fromHour;
            }
            set
            {
                _fromHour = CheckMinutes(value);

                NotifyDataChanged(); 
            }
        }
        public DateTime? From_Combined
        {
            get
            {
                if (FromDate != null && FromHour != null)
                {
                    return new DateTime(FromDate.Value.Year, FromDate.Value.Month, FromDate.Value.Day, FromHour.Value.Hour, FromHour.Value.Minute, 0);
                }
                else if (FromDate != null)
                {
                    return new DateTime(FromDate.Value.Year, FromDate.Value.Month, FromDate.Value.Day, 0, 0, 0);
                }

                return null;
            }
        }
        public DateTime? ToDate
        {
            get
            {
                return _toDate;
            }
            set
            {
                _toDate = value;

                if (_toDate < _fromDate)
                {
                    _fromDate = value;
                }

                NotifyDataChanged();
            }
        }
        public DateTime? ToHour
        {
            get
            {
                return _toHour;
            }
            set
            {
                _toHour = CheckMinutes(value);
                NotifyDataChanged();
            }
        }
        public DateTime? To_Combined
        {
            get
            {
                if (ToDate != null && ToHour != null)
                {
                    return new DateTime(ToDate.Value.Year, ToDate.Value.Month, ToDate.Value.Day, ToHour.Value.Hour, ToHour.Value.Minute, 0);
                }
                else if (ToDate != null)
                {
                    return new DateTime(ToDate.Value.Year, ToDate.Value.Month, ToDate.Value.Day, 0, 0, 0);
                }

                return null;
            }
        }
        public void NotifyDataChanged()
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
            {
                _eventTriggerTimer.Stop();
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
    }
}
