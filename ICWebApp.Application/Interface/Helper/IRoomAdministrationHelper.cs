using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IRoomAdministrationHelper
    {
        public Administration_Filter_RoomBookingGroup? Filter { get; set; }
        public Administration_Filter_RoomCalendar? CalendarFilter { get; set; }
    }
}
