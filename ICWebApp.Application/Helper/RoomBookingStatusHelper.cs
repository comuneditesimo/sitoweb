namespace ICWebApp.Application.Helper;

public static class RoomBookingStatusHelper
{
    public static Guid ToPay => Guid.Parse("000FB92E-3BAF-483D-9FA7-08C79A4DAF8B");
    public static Guid ToSign => Guid.Parse("C9E6E82E-6C9D-4D7D-9C98-BCA8E9B45440");
    public static Guid Declined => Guid.Parse("41082EAC-210C-4632-A063-0C858A98B110");
    public static Guid Archived => Guid.Parse("E8A662A9-B511-4209-9860-116AC5F3FAF5");
    public static Guid AcceptedWithChanges => Guid.Parse("159A9EBC-5E27-4AA6-992F-6E1B35502F04");
    public static Guid Committed => Guid.Parse("B99595E0-B4E1-4F46-A10A-7D42F80C491E");
    public static Guid Accepted => Guid.Parse("55DFD0BE-E6D9-447D-B5CC-7F2013181C75");
    public static Guid Cancelled => Guid.Parse("77A0C136-F145-4F6F-9505-9760B09F37E4");
    public static Guid CancelledByCitizen => Guid.Parse("AA89C50E-82B2-43A6-972D-9DEBBA1C61E7");
    
    public static bool IsCommitted(Guid? roomBookingGroupStatusId)
    {
        if (roomBookingGroupStatusId == null)
            return false;

        return roomBookingGroupStatusId != ToPay && roomBookingGroupStatusId != ToSign;
    }
}