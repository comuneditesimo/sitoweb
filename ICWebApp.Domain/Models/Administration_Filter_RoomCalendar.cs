using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class Administration_Filter_RoomCalendar
    {
        public List<Guid>? Room_ID { get; set; }
        public DateTime? StartDate { get; set; }
        public int? SchedulerView { get; set; }
    }
}
