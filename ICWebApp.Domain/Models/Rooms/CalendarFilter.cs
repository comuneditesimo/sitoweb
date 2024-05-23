using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Rooms
{
    public class CalendarFilter
    {
        public List<Guid>? ROOM_Room_IDs { get; set; }
        public DateTime? StartDate { get; set; }
    }
}
