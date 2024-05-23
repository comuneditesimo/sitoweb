using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class Administration_Filter_RoomBookingGroup
    {
        public string? Text { get; set; }
        public Guid? Auth_User_ID { get; set; }
        public List<Guid>? Booking_Status_ID { get; set; }
        public List<Guid>? Room_ID { get; set; }
        public DateTime? SubmittedFrom { get; set; }
        public DateTime? SubmittedTo { get; set; }
        public bool Archived { get; set; } = false;
    }
}
