using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;

namespace ICWebApp.Application.Interface.Provider;

public interface IRoomProvider
{
    public Task<List<AUTH_MunicipalityDistricts>> GetDistricts(Guid municipalityID);
    public Task<List<ROOM_Rooms>> GetRooms(Guid municipalityID);
    public Task<ROOM_Rooms?> GetRoomByID(Guid? roomID);
    public Task<ROOM_Booking> GetBooking(Guid ID);
    public Task<List<V_ROOM_Booking>> VerfiyBookings(Guid RoomID, DateTime Startdate, DateTime Enddate);
    public Task<bool> SetBooking(ROOM_Booking Data);
    public Task<bool> SetBookingGroupBackend(ROOM_BookingGroupBackend Data);
    public Task<List<ROOM_RoomGalerie>> GetRoomGalerie(Guid? id, Guid? roomID);
    public List<ROOM_RoomGalerie> GetRoomGalerieSync(Guid? id, Guid? roomID);
    public Task<List<ROOM_RoomPricing>> GetRoomPricing(Guid RoomID);
    public Task<List<V_ROOM_Booking_Status>> GetBookingStatusList();
    public Task<List<ROOM_TimeTable>> GetTimeTable(int? startMin);
    public Task<List<V_ROOM_RoomOptions>> GetAllRoomOptions(Guid? authMunicipalityId);
    public Task<List<V_ROOM_Rooms_Contact>> GetVRoomsContacts(Guid RoomID);
    public Task<bool> UpdateBooking(ROOM_Booking booking);
    public Task<bool> UpdateRoom(ROOM_Rooms room);
    public Task<bool> UpdateRoomOption(ROOM_RoomOptions room);
    public Task<bool> UpdateRoomGalerie(ROOM_RoomGalerie item);
    public Task<bool> UpdateRoomPricing(ROOM_RoomPricing price);
    public Task<bool> RemoveBooking(ROOM_Booking item);
    public Task<bool> RemoveOption(ROOM_RoomOptions item);
    public Task<bool> RemovePrice(ROOM_RoomPricing item);
    public Task<bool> RemoveGalerie(ROOM_RoomGalerie item);
    public Task<bool> RemoveRoomOption(ROOM_RoomOptions item);
    public Task<List<V_ROOM_Rooms>> GetRoomGroups(Guid municipalityID, Guid? districtID);
    public Task<List<V_ROOM_Rooms>> GetRoomsFromRoomGroup(Guid roomGroupID);
    public Task<List<ROOM_Rooms>> GetSubRoomsFromRoom(Guid roomID);
    public Task<List<V_ROOM_Rooms>> GetVSubRoomsFromRoom(Guid roomID);
    public Task<List<ROOM_BookingGroup>> GetBookingGroupList(Guid municipalityID);
    public Task<bool> SetBookingGroup(ROOM_BookingGroup group);
    public Task<bool> RemoveBookingGroup(ROOM_BookingGroup item);
    //ROOM
    public Task<List<V_ROOM_Rooms>> GetAllBuildings(Guid AUTH_Municipality_ID);
    public Task<List<V_ROOM_Rooms>> GetAllRooms(Guid AUTH_Municipality_ID);
    public Task<List<V_ROOM_Rooms>> GetAllRoomsByBuildingID(Guid ROOM_Room_ID);
    public Task<List<V_ROOM_Rooms>> GetAllRoomsWithoutBuildings(Guid AUTH_Municipality_ID);
    public Task<V_ROOM_Rooms?> GetVRoom(Guid ID);
    public Task<ROOM_Rooms?> GetRoom(Guid ID);
    public Task<ROOM_Rooms?> SetRoom(ROOM_Rooms Data);
    public Task<bool> RemoveRoom(Guid ID);

    //Definition Paymenting Extended
    public Task<List<ROOM_Rooms_Extended>> GetRoomExtendedList(Guid ROOM_Rooms_ID, Guid LANG_Language_ID);
    public Task<List<ROOM_Rooms_Extended>> GetRoomExtendedList(Guid ROOM_Rooms_ID);
    public Task<ROOM_Rooms_Extended?> GetRoomExtended(Guid ID);
    public Task<ROOM_Rooms_Extended?> SetRoomExtended(ROOM_Rooms_Extended Data);
    public Task<bool> RemoveRoomExtended(Guid ID);

    //ROOM OPTIONS
    public Task<List<V_ROOM_RoomOptions>> GetRoomOptionsList(Guid ROOM_Rooms_ID);
    public Task<ROOM_RoomOptions?> GetRoomOption(Guid ID);
    public Task<V_ROOM_RoomOptions?> GetVRoomOption(Guid ID);
    public Task<bool> ToggleEnableOptions(Guid ID);
    public Task<bool> ToggleDailyPayment(Guid ID);
    public Task<ROOM_RoomOptions?> SetRoomOption(ROOM_RoomOptions Data);
    public Task<bool> RemoveRoomOption(Guid ID);
    public Task<List<ROOM_RoomOptions_Extended>> GetRoomOptionsExtendedList(Guid ROOM_RoomsOption_ID, Guid LANG_Language_ID);

    public Task<ROOM_RoomOptions_Extended?> GetRoomOptionsExtended(Guid ID);
    public Task<ROOM_RoomOptions_Extended?> SetRoomOptionsExtended(ROOM_RoomOptions_Extended Data);
    public Task<bool> RemoveRoomOptionsExtended(Guid ID);

    //ROOM OPTIONS POSITIONS
    public Task<List<V_ROOM_RoomOptions_Positions>> GetRoomOptionsPositionList(Guid ROOM_RoomsInventory_ID);
    public Task<ROOM_RoomOptions_Positions?> GetRoomOptionsPosition(Guid ID);
    public Task<ROOM_RoomOptions_Positions?> SetRoomOptionsPosition(ROOM_RoomOptions_Positions Data);
    public Task<bool> RemoveRoomOptionsPosition(Guid ID);

    //ROOM Booking Transactions
    public Task<List<ROOM_BookingTransactions>> GetBookingTransactionList(Guid ROOM_Booking_Group_ID);
    public Task<ROOM_BookingTransactions?> GetBookingTransaction(Guid ID);
    public Task<ROOM_BookingTransactions?> SetBookingTransaction(ROOM_BookingTransactions Data);
    public Task<bool> RemoveBookingTransaction(Guid ID);

    //ROOM Booking
    public Task<List<V_ROOM_Booking>> GetBookings(Guid ROOM_Room_ID);
    public Task<List<V_ROOM_Booking_Group>> GetBookingGroups(Guid AUTH_Municipality_ID, Administration_Filter_RoomBookingGroup? Filter);
    public Task<bool> CheckBookingOpenPaymentsByUser(Guid AUTH_Users_ID);
    public Task<List<V_ROOM_Booking_Group>> GetBookingGroupByUser(Guid AUTH_Users_ID);
    public Task<List<V_ROOM_Booking_Group_Backend>> GetBookingGroupBackendByUser(Guid AUTH_Users_ID);
    public Task<List<V_ROOM_Booking>> GetVBookingsByGroupID(Guid ROOM_RoomGroup_ID, Guid AUTH_Municipality_ID);
    public Task<List<V_ROOM_Booking>> GetVBlockedBookings(List<Guid> ROOM_Room_IDs, DateTime FromDate);
    public Task<List<V_ROOM_Booking>> GetVBookings(DateTime FromDate, Guid Auth_Municipality_ID);
    public Task<List<ROOM_Booking>> GetBookingsByGroupID(Guid ROOM_RoomGroup_ID);
    public Task<ROOM_BookingGroup?> GetBookingGroup(Guid ID);
    public Task<ROOM_BookingGroupBackend?> GetBookingGroupBackend(Guid ID);
    public Task<V_ROOM_Booking_Group?> GetVBookingGroup(Guid ID);
    public Task<List<V_ROOM_Booking_Group_Users>> GetBookingGroupUsers(Guid AUTH_Municipality_ID);

    //ROOM STATUS LOG
    public Task<List<V_ROOM_Booking_Status_Log>> GetRoomStatusLogList(Guid ROOM_BookingGroup_ID);
    public Task<ROOM_Booking_Status_Log?> GetRoomStatusLog(Guid ID);
    public Task<ROOM_Booking_Status_Log?> SetRoomStatusLog(ROOM_Booking_Status_Log Data);
    public Task<ROOM_Booking_Status_Log_Extended?> GetRoomStatusLogExtended(Guid ID);
    public Task<ROOM_Booking_Status_Log_Extended?> SetRoomStatusLogExtended(ROOM_Booking_Status_Log_Extended Data);

    //ROOM RESSOURCE
    public Task<List<ROOM_Booking_Ressource>> GetRoomRessourceList(Guid ROOM_BookingGroup_ID);
    public Task<ROOM_Booking_Ressource?> GetRoomRessource(Guid ID);
    public Task<ROOM_Booking_Ressource?> SetRoomRessource(ROOM_Booking_Ressource Data);
    public Task<bool> RemoveRoomRessource(Guid ID, bool force = false);
    public Task<List<V_ROOM_RoomPricing_Type>> GetRoomPricingType();
    //DOCUMENT
    public Task<FILE_FileInfo> GetDocument(Guid ROOM_BookingGroupID, Guid LANG_Language_ID, bool localhost = false);
    //Progressiv Number
    public long GetLatestProgressivNumber(Guid AUTH_Municipality_ID, int Year);
    public Task<string> ReplaceKeywords(Guid ROOM_BookingGroup_ID, Guid LANG_Language_ID, string Text, Guid? PreviousStatus_ID = null);
    public Task<List<V_ROOM_RoomOptionsCategory>> GetRoomOptionsCategories();
    public Task<List<ROOM_RoomOptions_Contact>> GetRoomOptionContacts(Guid ROOM_RoomOption_ID);
    public Task<ROOM_RoomOptions_Contact?> SetRoomOptionContacts(ROOM_RoomOptions_Contact Data);
    public Task<bool> RemoveRoomOptionContacts(Guid ROOM_RoomOption_Contact_ID);
    public Task<List<V_ROOM_RoomOptions_Contact>> GetVRoomOptionContacts(Guid ROOM_RoomOption_ID);
    public Task<List<ROOM_Rooms_Contact>> GetRoomsContacts(Guid ROOM_Rooms_ID);
    public Task<(List<Guid?>, List<Guid?>)> GetBookingOrBookingGroupContactsToNotify(Guid? RoomBookingId,
        ROOM_Booking booking);
    public Task<ROOM_Rooms_Contact?> SetRoomsContacts(ROOM_Rooms_Contact Data);
    public Task<bool> RemoveRoomsContacts(Guid ROOM_Rooms_Contact_ID);
    public Task<List<V_ROOM_Contact_Type>> GetContactTypes();
    public Task<List<ROOM_Rooms_Contact_Type>> GetRoomContactTypes(Guid ROOM_Rooms_Contact_ID);
    public Task<ROOM_Rooms_Contact_Type?> SetRoomContactType(ROOM_Rooms_Contact_Type Data);
    public Task<bool> RemoveRoomContactType(ROOM_Rooms_Contact_Type Data);
    public Task<ROOM_Settings?> GetSettings(Guid AUTH_Municipality_ID);
    public Task<V_ROOM_Settings?> GetVSettings(Guid AUTH_Municipality_ID, Guid LANG_Language_ID);
    public Task<ROOM_Settings?> SetSettings(ROOM_Settings Data);
    public Task<List<ROOM_Settings_Extended>> GetSettingsExtended(Guid ROOM_Settings_ID);
    public Task<ROOM_Settings_Extended?> SetSettingsExtended(ROOM_Settings_Extended Data);
    public Task<bool> CreateBookingStatusLog(ROOM_BookingGroup BookingGroup, Guid ROOM_BookingGroup_Status_ID);
    public Task<List<ROOM_BookingOptions>> GetBookingOptions(Guid ROOM_BookingGroup_ID);
    public Task<ROOM_BookingOptions?> SetBookingOptions(ROOM_BookingOptions Options);
    public Task<bool> SetBookingComitted(ROOM_BookingGroup BookingGroup, string BaseUri, bool protocolMailSent = false);
    public Task<bool> SetBookingCanceled(Guid BookingGroup_ID, string BaseUri);
    public Task<bool> VerifyBookingsState(Guid AUTH_Users_ID, string BaseUri);
    public Task<List<V_ROOM_Booking_Bookings>> GetBookingPositions(Guid ROOM_BookingGroup_ID);
    public Task<List<V_ROOM_Booking_Type>> GetRoomBookingTypes();
    //THEMES
    public Task<List<V_HOME_Theme>> GetVThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID);
    public Task<List<ROOM_Theme>> GetThemes(Guid AUTH_Municipality_ID);
    public Task<bool> RemoveTheme(ROOM_Theme Data);
    public Task<ROOM_Theme?> SetTheme(ROOM_Theme Data);
    public Task<bool> HasTheme(Guid HOME_Theme_ID);
    // Properties
    public Task<List<ROOM_Property>> GetPropertyList(Guid AUTH_Municipality_ID, Guid LanguageID);
    public Task<ROOM_Property?> GetProperty(Guid ID, Guid LanguageID);
    public Task<ROOM_Property?> SetProperty(ROOM_Property Data);
    public Task<bool> RemoveProperty(Guid ID, bool force = false);
    public Task<List<ROOM_Property_Extended>> GetPropertyExtendedList(Guid ROOM_Property_ID, Guid LANG_Language_ID);
    public Task<ROOM_Property_Extended?> GetPropertyExtended(Guid ID);
    public Task<ROOM_Property_Extended?> SetPropertyExtended(ROOM_Property_Extended Data);
    public Task<bool> RemovePropertyExtended(Guid ID);
    // Resources
    public Task<List<ROOM_Ressources>> GetRessourceList(Guid AUTH_Municipality_ID, Guid LanguageID);
    public Task<ROOM_Ressources?> GetRessource(Guid ID, Guid LanguageID);
    public Task<ROOM_Ressources?> SetRessource(ROOM_Ressources Data);
    public Task<bool> RemoveRessource(Guid ID, bool force = false);
    public Task<List<ROOM_Ressources_Extended>> GetRessourceExtendedList(Guid ROOM_Ressources_ID, Guid LANG_Language_ID);
    public Task<ROOM_Ressources_Extended?> GetRessourceExtended(Guid ID);
    public Task<ROOM_Ressources_Extended?> SetRessourceExtended(ROOM_Ressources_Extended Data);
    public Task<bool> RemoveRessourceExtended(Guid ID);
}