using ICWebApp.Application.Interface.Helper;
using ICWebApp.Domain.Models.Rooms;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Application.Helper
{
    public class RoomBookingHelper : IRoomBookingHelper
    {
        public event Action? OnShowBookingChanged;
        private bool _ShowBookingSideBar = false;
        public bool ShowBookingSideBar 
        {
            get
            {
                return _ShowBookingSideBar;
            }
            set
            {
                _ShowBookingSideBar = value;
                NotifyShowBookingChanged();
            }
        }
        public List<Booking> Bookings { get; set; }
        public TimeFilter? TimeFilter { get; set; }
        public void NotifyShowBookingChanged()
        {
            OnShowBookingChanged?.Invoke();
        }
        public List<V_ROOM_Rooms> RoomList { get; set; } = new List<V_ROOM_Rooms>();
        public List<V_ROOM_RoomOptions> RoomOptions { get; set; } = new List<V_ROOM_RoomOptions>();
    }
}
