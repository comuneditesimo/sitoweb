using ICWebApp.Domain.Models.Rooms;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Application.Interface.Services
{
    public interface IBookingService
    {
        public event Action? OnValuesChanged;

        public string GetDatesInString(TimeFilter TimeFilter);
        public string GetRoomsInString(List<V_ROOM_Rooms> RoomList, List<V_ROOM_RoomOptions>? Options = null);
        public Task<string?> CheckRoomAvailability(V_ROOM_Rooms Room, Booking _booking, bool IncludeRoomName = false);
        public List<Booking> GetDatesToBook(TimeFilter TimeFilter);
        public void NotifyValuesChanged();
        public Task<decimal?> GetRoomCost(Guid ROOM_Room_ID, Guid? AUTH_Company_Type_ID, DateTime StartDate, DateTime EndDate);
        public Task<bool> SendMessage(string BodyTextCode, string TitleTextCode, Guid ROOM_Booking_ID, string BaseUri, Guid MessageType_ID);
        public void SendMessagesToContacts(Guid AUTH_Municipality_ID, Guid BookinGroup_ID, string Title, bool Cancel = false, ROOM_Booking? Booking = null);
    }
}
