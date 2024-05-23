using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Rooms
{
    public class UserBookingItem
    {
        public Guid ID { get; set; }
        public bool IsBackend { get; set; }
        public Guid? ROOM_BookingStatus_ID { get; set; }
        public string IconCSS { get; set; }
        public string Rooms { get; set; }
        public string Days { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? StartDate { get; set; }
    }
}
