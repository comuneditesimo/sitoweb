using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Rooms;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IRoomBookingHelper
    {
        public List<Booking> Bookings { get; set; }
        public TimeFilter? TimeFilter {get;set;}
        public bool ShowBookingSideBar { get; set; }
        public List<V_ROOM_Rooms> RoomList { get; set; }
        public List<V_ROOM_RoomOptions> RoomOptions { get; set; }
        public event Action? OnShowBookingChanged;
    }
}
