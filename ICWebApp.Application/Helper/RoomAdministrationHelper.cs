using ICWebApp.Application.Interface.Helper;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Helper
{
    public class RoomAdministrationHelper : IRoomAdministrationHelper
    {
        private Administration_Filter_RoomBookingGroup? _filter;
        private Administration_Filter_RoomCalendar? _calendarFilter;
        public Administration_Filter_RoomBookingGroup? Filter { get => _filter; set => _filter = value; }
        public Administration_Filter_RoomCalendar? CalendarFilter { get => _calendarFilter; set => _calendarFilter = value; }
    }
}
