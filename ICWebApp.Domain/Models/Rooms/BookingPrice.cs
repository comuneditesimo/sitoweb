using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Rooms
{
    public class BookingPrice
    {
        public Guid ROOM_Room_ID { get; set; }
        public Guid? ROOM_Option_ID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
