using ICWebApp.Domain.DBModels;

namespace ICWebApp.Application.Interface.Helper;

public interface ID3Helper
{
    public Task ProtocolNewCanteenSubscription(Guid fileInfoId, IEnumerable<CANTEEN_Subscriber> canteenSubscriber);
    public Task ProtocolNewFormApplication(FORM_Application application);
    public Task ProtocolRoomBooking(ROOM_BookingGroup booking);
    public Task ProtocolNewOrganization(ORG_Request request);
}