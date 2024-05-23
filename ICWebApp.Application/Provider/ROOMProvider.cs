using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.EntityFrameworkCore;
using ICWebApp.Application.Settings;
using Telerik.Reporting.Processing;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System.Globalization;
using Telerik.Reporting;

namespace ICWebApp.Application.Provider;

public class RoomProvider : IRoomProvider
{
    private IUnitOfWork _unitOfWork;
    private ILANGProvider _langProvider;
    private ITEXTProvider _textProvider;
    private IFILEProvider _FileProvider;
    private IMessageService _messageService;

    public RoomProvider(IUnitOfWork _unitOfWork, ILANGProvider _langProvider, ITEXTProvider _textProvider, IFILEProvider _FileProvider, IMessageService _messageService)
    {
        this._unitOfWork = _unitOfWork;
        this._langProvider = _langProvider;
        this._textProvider = _textProvider;
        this._FileProvider = _FileProvider;
        this._messageService = _messageService;
    }
    public async Task<ROOM_Rooms?> GetRoomByID(Guid? roomID)
    {
        return await _unitOfWork.Repository<ROOM_Rooms>().GetByIDAsync(roomID);
    }
    public async Task<List<ROOM_Rooms>> GetRooms(Guid municipalityID)
    {
        return await _unitOfWork.Repository<ROOM_Rooms>().Where(a => a.MunicipalityID == municipalityID).ToListAsync();
    }
    public async Task<List<V_ROOM_Booking>> VerfiyBookings(Guid RoomID, DateTime Startdate, DateTime Enddate)
    {
        List<V_ROOM_Booking> data = await _unitOfWork.Repository<V_ROOM_Booking>().Where(a => a.ROOM_Room_ID == RoomID && a.StartDateWithBufferTime != null && a.EndDateWithBufferTime != null && Startdate < a.EndDateWithBufferTime.Value && a.StartDateWithBufferTime.Value < Enddate).ToListAsync();

        return data;
    }
    public async Task<ROOM_Booking> GetBooking(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Booking>().FirstOrDefaultAsync(p => p.ID == ID);
    }
    public async Task<bool> SetBooking(ROOM_Booking Data)
    {
        await _unitOfWork.Repository<ROOM_Booking>().InsertOrUpdateAsync(Data);

        return true;
    }
    public async Task<bool> SetBookingGroupBackend(ROOM_BookingGroupBackend Data)
    {
        await _unitOfWork.Repository<ROOM_BookingGroupBackend>().InsertOrUpdateAsync(Data);

        return true;
    }
    public async Task<List<ROOM_RoomOptions>> GetRoomOptions(Guid? id, Guid? roomID)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions>().Where(a => (a.ID == id || id == null)).Include(i => i.ROOM_RoomOptions_Extended).ToListAsync();
    }
    public async Task<List<V_ROOM_RoomOptions>> GetAllRoomOptions(Guid? authMunicipalityId)
    {
        return await _unitOfWork.Repository<V_ROOM_RoomOptions>().ToListAsync(a => a.AUTH_Municipality_ID == authMunicipalityId);
    }
    public async Task<List<ROOM_RoomGalerie>> GetRoomGalerie(Guid? id, Guid? roomID)
    {
        return await _unitOfWork.Repository<ROOM_RoomGalerie>().Where(a => (a.ID == id || id == null) && (a.ROOM_Room_ID == roomID || roomID == null)).ToListAsync();
    }
    public List<ROOM_RoomGalerie> GetRoomGalerieSync(Guid? id, Guid? roomID)
    {
        return _unitOfWork.Repository<ROOM_RoomGalerie>().Where(a => (a.ID == id || id == null) && (a.ROOM_Room_ID == roomID || roomID == null)).ToList();
    }
    public async Task<List<ROOM_RoomPricing>> GetRoomPricing(Guid RoomID)
    {
        return await _unitOfWork.Repository<ROOM_RoomPricing>().Where(a => a.ROOM_Rooms_ID == RoomID).Include(i => i.AUTH_Company_Type).ToListAsync();
    }
    public async Task<List<V_ROOM_Rooms_Contact>> GetVRoomsContacts(Guid RoomID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Rooms_Contact>().Where(a => a.ROOM_Rooms_ID == RoomID && a.LANG_Languages_ID == Language.ID).ToListAsync();
    }
    public async Task<List<ROOM_TimeTable>> GetTimeTable(int? startMin)
    {
        return await _unitOfWork.Repository<ROOM_TimeTable>().Where(a => a.StartMinuteOfTheDay > startMin || startMin == null).ToListAsync();
    }
    public async Task<List<AUTH_MunicipalityDistricts>> GetDistricts(Guid municipalityID)
    {
        return await _unitOfWork.Repository<AUTH_MunicipalityDistricts>().Where(a => a.AUTH_MunicipalityID == municipalityID).ToListAsync();
    }
    public async Task<List<V_ROOM_Booking_Status>> GetBookingStatusList()
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Booking_Status>().Where(p => p.LANG_LanguagesID == Language.ID).OrderBy(p => p.SortOrder).ToListAsync();
    }
    public async Task<bool> UpdateBooking(ROOM_Booking booking)
    {
        await _unitOfWork.Repository<ROOM_Booking>().InsertOrUpdateAsync(booking);

        return true;
    }
    public async Task<bool> UpdateRoom(ROOM_Rooms room)
    {
        await _unitOfWork.Repository<ROOM_Rooms>().InsertOrUpdateAsync(room);

        return true;
    }
    public async Task<bool> UpdateRoomOption(ROOM_RoomOptions item)
    {
        await _unitOfWork.Repository<ROOM_RoomOptions>().InsertOrUpdateAsync(item);

        return true;
    }
    public async Task<bool> UpdateRoomGalerie(ROOM_RoomGalerie item)
    {
        await _unitOfWork.Repository<ROOM_RoomGalerie>().InsertOrUpdateAsync(item);

        return true;
    }
    public async Task<bool> UpdateRoomPricing(ROOM_RoomPricing item)
    {
        await _unitOfWork.Repository<ROOM_RoomPricing>().InsertOrUpdateAsync(item);

        return true;
    }
    public async Task<bool> RemoveBooking(ROOM_Booking item)
    {
        item.RemovedDate = DateTime.Now;

        await _unitOfWork.Repository<ROOM_Booking>().InsertOrUpdateAsync(item);

        return true;
    }
    public async Task<bool> RemoveOption(ROOM_RoomOptions item)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions>().DeleteAsync(item);
    }
    public async Task<bool> RemovePrice(ROOM_RoomPricing item)
    {
        return await _unitOfWork.Repository<ROOM_RoomPricing>().DeleteAsync(item);
    }
    public async Task<bool> RemoveGalerie(ROOM_RoomGalerie item)
    {
        return await _unitOfWork.Repository<ROOM_RoomGalerie>().DeleteAsync(item);
    }
    public async Task<bool> RemoveRoomOption(ROOM_RoomOptions item)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions>().DeleteAsync(item);
    }
    public async Task<List<V_ROOM_Rooms>> GetRoomGroups(Guid municipalityID, Guid? districtID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        var data = await _unitOfWork.Repository<V_ROOM_Rooms>().Where(a => (a.MunicipalityID == municipalityID) && a.LANG_Languages_ID == Language.ID && (a.RoomGroupFamilyID == null)).ToListAsync();

        return data.Where(a => a.QuantityOfSubRooms > 0 || a.RoomGroupFamilyID == null).ToList();
    }
    public async Task<List<V_ROOM_Rooms>> GetRoomsFromRoomGroup(Guid roomGroupID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Rooms>().Where(a => (a.RoomGroupFamilyID == roomGroupID || (a.RoomGroupFamilyID == null && a.ID == roomGroupID)) &&
                                                                       (a.HasRooms != true) && (a.LANG_Languages_ID == Language.ID)).ToListAsync();
    }
    public async Task<List<ROOM_Rooms>> GetSubRoomsFromRoom(Guid roomID)
    {
        return await _unitOfWork.Repository<ROOM_Rooms>().Where(a => a.RoomGroupFamilyID == roomID).ToListAsync();
    }
    public async Task<List<V_ROOM_Rooms>> GetVSubRoomsFromRoom(Guid roomID)
    {
        return await _unitOfWork.Repository<V_ROOM_Rooms>().Where(a => a.RoomGroupFamilyID == roomID).ToListAsync();
    }
    public async Task<List<ROOM_BookingGroup>> GetBookingGroupList(Guid municipalityID)
    {
        return await _unitOfWork.Repository<ROOM_BookingGroup>().Where(a => a.AUTH_MunicipalityID == municipalityID).ToListAsync();
    }
    public async Task<bool> SetBookingGroup(ROOM_BookingGroup group)
    {
        await _unitOfWork.Repository<ROOM_BookingGroup>().InsertOrUpdateAsync(group);

        return true;
    }
    public async Task<bool> RemoveBookingGroup(ROOM_BookingGroup item)
    {
        return await _unitOfWork.Repository<ROOM_BookingGroup>().DeleteAsync(item);
    }
    public async Task<List<V_ROOM_Rooms>> GetAllBuildings(Guid AUTH_Municipality_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Rooms>().Where(p => p.MunicipalityID == AUTH_Municipality_ID && p.LANG_Languages_ID == Language.ID && p.RoomGroupFamilyID == null).ToListAsync();
    }
    public async Task<List<V_ROOM_Rooms>> GetAllRooms(Guid AUTH_Municipality_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Rooms>().Where(p => p.MunicipalityID == AUTH_Municipality_ID && p.LANG_Languages_ID == Language.ID && p.Enabled == true).ToListAsync();
    }
    public async Task<List<V_ROOM_Rooms>> GetAllRoomsByBuildingID(Guid ROOM_Room_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Rooms>().Where(p => p.RoomGroupFamilyID == ROOM_Room_ID && p.LANG_Languages_ID == Language.ID).ToListAsync();
    }
    public async Task<List<V_ROOM_Rooms>> GetAllRoomsWithoutBuildings(Guid AUTH_Municipality_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Rooms>().Where(p => p.MunicipalityID == AUTH_Municipality_ID && p.LANG_Languages_ID == Language.ID && (p.RoomGroupFamilyID != null || p.HasRooms != true)).ToListAsync();
    }
    public async Task<V_ROOM_Rooms?> GetVRoom(Guid ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Rooms>().Where(p => p.LANG_Languages_ID == Language.ID).FirstOrDefaultAsync(p => p.ID == ID);
    }
    public async Task<ROOM_Rooms?> GetRoom(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Rooms>().Where(p => p.ID == ID).Include(p => p.ROOM_Rooms_Extended).FirstOrDefaultAsync();
    }
    public async Task<ROOM_Rooms?> SetRoom(ROOM_Rooms Data)
    {
        return await _unitOfWork.Repository<ROOM_Rooms>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRoom(Guid ID)
    {
        var itemexists = _unitOfWork.Repository<ROOM_Rooms>().Where(p => p.ID == ID).FirstOrDefault();

        if (itemexists != null)
        {
            itemexists.RemovedAt = DateTime.Now;

            await _unitOfWork.Repository<ROOM_Rooms>().InsertOrUpdateAsync(itemexists);
        }

        return true;
    }
    public async Task<List<ROOM_Rooms_Extended>> GetRoomExtendedList(Guid ROOM_Rooms_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<ROOM_Rooms_Extended>().Where(p => p.ROOM_Rooms_ID == ROOM_Rooms_ID && p.LANG_Languages_ID == Language.ID).ToListAsync();
    }
    public async Task<List<ROOM_Rooms_Extended>> GetRoomExtendedList(Guid ROOM_Rooms_ID, Guid LANG_Language_ID)
    {
        return await _unitOfWork.Repository<ROOM_Rooms_Extended>().Where(p => p.ROOM_Rooms_ID == ROOM_Rooms_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
    }
    public async Task<ROOM_Rooms_Extended?> GetRoomExtended(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Rooms_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
    }
    public async Task<ROOM_Rooms_Extended?> SetRoomExtended(ROOM_Rooms_Extended Data)
    {
        return await _unitOfWork.Repository<ROOM_Rooms_Extended>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRoomExtended(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Rooms_Extended>().DeleteAsync(ID);
    }
    public async Task<List<V_ROOM_RoomOptions>> GetRoomOptionsList(Guid ROOM_Rooms_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_RoomOptions>().Where(p => p.ROOM_Room_ID == ROOM_Rooms_ID && p.LANG_Language_ID == Language.ID).ToListAsync();
    }
    public async Task<ROOM_RoomOptions?> GetRoomOption(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions>().Where(p => p.ID == ID).Include(p => p.ROOM_RoomOptions_Extended).FirstOrDefaultAsync();
    }
    public async Task<bool> ToggleEnableOptions(Guid ID)
    {
        ROOM_RoomOptions? options = await _unitOfWork.Repository<ROOM_RoomOptions>().FirstOrDefaultAsync(p => p.ID == ID);
        if (options != null)
        {
            if (options.Enabled == null)
            {
                options.Enabled = false;
            }
            options.Enabled = !options.Enabled;
            await _unitOfWork.Repository<ROOM_RoomOptions>().InsertOrUpdateAsync(options);
            return true;
        }
        return false;
    }
    public async Task<bool> ToggleDailyPayment(Guid ID)
    {
        ROOM_RoomOptions? options = await _unitOfWork.Repository<ROOM_RoomOptions>().FirstOrDefaultAsync(p => p.ID == ID);
        if (options != null)
        {
            if  (options.DailyPayment == null)
            {
                options.DailyPayment = false;
            }
            options.DailyPayment = !options.DailyPayment;
            await _unitOfWork.Repository<ROOM_RoomOptions>().InsertOrUpdateAsync(options);
            return true;
        }
        return false;
    }
    public async Task<ROOM_RoomOptions?> SetRoomOption(ROOM_RoomOptions Data)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions>().InsertOrUpdateAsync(Data);
    }
    public async Task<V_ROOM_RoomOptions?> GetVRoomOption(Guid ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_RoomOptions>().Where(p => p.ID == ID && p.LANG_Language_ID == Language.ID).FirstOrDefaultAsync();
    }
    public async Task<bool> RemoveRoomOption(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions>().DeleteAsync(ID);
    }
    public async Task<List<ROOM_RoomOptions_Extended>> GetRoomOptionsExtendedList(Guid ROOM_RoomsOption_ID, Guid LANG_Language_ID)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions_Extended>().Where(p => p.ROOM_RoomOptions_ID == ROOM_RoomsOption_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
    }
    public async Task<ROOM_RoomOptions_Extended?> GetRoomOptionsExtended(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
    }
    public async Task<ROOM_RoomOptions_Extended?> SetRoomOptionsExtended(ROOM_RoomOptions_Extended Data)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions_Extended>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRoomOptionsExtended(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions_Extended>().DeleteAsync(ID);
    }
    public async Task<List<V_ROOM_RoomOptions_Positions>> GetRoomOptionsPositionList(Guid ROOM_RoomsOptions_ID)
    {
        return await _unitOfWork.Repository<V_ROOM_RoomOptions_Positions>().Where(p => p.ROOM_Rooms_Options_ID == ROOM_RoomsOptions_ID).ToListAsync();
    }
    public async Task<ROOM_RoomOptions_Positions?> GetRoomOptionsPosition(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions_Positions>().FirstOrDefaultAsync(p => p.ID == ID);
    }
    public async Task<ROOM_RoomOptions_Positions?> SetRoomOptionsPosition(ROOM_RoomOptions_Positions Data)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions_Positions>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRoomOptionsPosition(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions_Positions>().DeleteAsync(ID);
    }
    public async Task<List<ROOM_BookingTransactions>> GetBookingTransactionList(Guid ROOM_Booking_Group_ID)
    {
        return await _unitOfWork.Repository<ROOM_BookingTransactions>().Where(p => p.ROOM_BookingGroupID == ROOM_Booking_Group_ID).ToListAsync();
    }
    public async Task<ROOM_BookingTransactions?> GetBookingTransaction(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_BookingTransactions>().FirstOrDefaultAsync(p => p.ID == ID);
    }
    public async Task<ROOM_BookingTransactions?> SetBookingTransaction(ROOM_BookingTransactions Data)
    {
        return await _unitOfWork.Repository<ROOM_BookingTransactions>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveBookingTransaction(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_BookingTransactions>().DeleteAsync(ID);
    }
    public async Task<List<V_ROOM_Booking>> GetBookings(Guid ROOM_Room_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Booking>().Where(p => p.ROOM_Room_ID == ROOM_Room_ID && p.LANG_Languages_ID == Language.ID).ToListAsync();
    }
    public async Task<V_ROOM_Booking_Group?> GetVBookingGroup(Guid ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Booking_Group>().Where(p => p.ID == ID && p.LANG_LanguagesID == Language.ID).FirstOrDefaultAsync();
    }
    public async Task<ROOM_BookingGroup?> GetBookingGroup(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_BookingGroup>().Where(p => p.ID == ID).FirstOrDefaultAsync();
    }
    public async Task<ROOM_BookingGroupBackend?> GetBookingGroupBackend(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_BookingGroupBackend>().Where(p => p.ID == ID).FirstOrDefaultAsync();
    }
    public async Task<List<V_ROOM_Booking>> GetVBookingsByGroupID(Guid ROOM_RoomGroup_ID, Guid AUTH_Municipality_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Booking>().Where(p => p.ROOM_BookingGroup_ID == ROOM_RoomGroup_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Languages_ID == Language.ID).ToListAsync();
    }
    public async Task<List<V_ROOM_Booking>> GetVBlockedBookings(List<Guid> ROOM_Room_IDs, DateTime FromDate)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Booking>().Where(p => p.ROOM_Room_ID != null && ROOM_Room_IDs.Contains(p.ROOM_Room_ID.Value) && p.EndDate >= FromDate && p.LANG_Languages_ID == Language.ID).ToListAsync();
    }
    public async Task<List<V_ROOM_Booking>> GetVBookings(DateTime FromDate, Guid Auth_Municipality_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Booking>().Where(p => p.ROOM_Room_ID != null && p.EndDate >= FromDate && p.AUTH_MunicipalityID == Auth_Municipality_ID && p.LANG_Languages_ID == Language.ID).ToListAsync();
    }
    public async Task<List<ROOM_Booking>> GetBookingsByGroupID(Guid ROOM_RoomGroup_ID)
    {
        return await _unitOfWork.Repository<ROOM_Booking>().Where(p => p.ROOM_BookingGroup_ID == ROOM_RoomGroup_ID || p.ROOM_BookingGroupBackend_ID == ROOM_RoomGroup_ID).ToListAsync();
    }
    public async Task<List<V_ROOM_Booking_Group>> GetBookingGroups(Guid AUTH_Municipality_ID, Administration_Filter_RoomBookingGroup? Filter)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        var data = _unitOfWork.Repository<V_ROOM_Booking_Group>().Where(p => p.LANG_LanguagesID == Language.ID && p.AUTH_MunicipalityID == AUTH_Municipality_ID);

        //if (Filter != null && Filter.Archived != true)
        //{
        //    data = data.Where(p => p. == null || p.LastDate >= DateTime.Now.AddDays(-60));
        //}
        if (Filter != null && Filter.Auth_User_ID != null)
        {
            data = data.Where(p => p.AUTH_User_ID == Filter.Auth_User_ID);
        }
        if (Filter != null && Filter.Booking_Status_ID != null && Filter.Booking_Status_ID.Count() > 0)
        {
            data = data.Where(p => p.ROOM_BookingStatus_ID != null && Filter.Booking_Status_ID.Contains(p.ROOM_BookingStatus_ID.Value));
        }
        if (Filter != null && Filter.SubmittedFrom != null)
        {
            data = data.Where(p => p.CreationDate >= Filter.SubmittedFrom);
        }
        if (Filter != null && Filter.SubmittedTo != null)
        {
            data = data.Where(p => p.CreationDate <= Filter.SubmittedTo);
        }
        if (Filter != null && Filter.Room_ID != null && Filter.Room_ID.Count() > 0)
        {
            //var groupIDbyRooms = _unitOfWork.Repository<V_ROOM_Booking>().Where(p => p.RoomID != null && p.BookingGroupID != null && p.MunicipalityID == AUTH_Municipality_ID && Filter.Room_ID.Contains(p.RoomID.Value)).Select(p => p.BookingGroupID).Distinct().ToList();

            //if (groupIDbyRooms != null)
            //{
            //    data = data.Where(p => groupIDbyRooms.Contains(p.ID));
            //}
            //else
            //{
            //    data = data.Where(p => true == false);
            //}
        }
        if (Filter != null && !string.IsNullOrEmpty(Filter.Text))
        {
            data = data.Where(p => p.SearchText != null && p.SearchText.ToLower().Contains(Filter.Text.ToLower()));
        }

        return await data.OrderByDescending(o => o.CreationDate).ToListAsync();
    }
    public async Task<List<V_ROOM_Booking_Group>> GetBookingGroupByUser(Guid AUTH_Users_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        var data = _unitOfWork.Repository<V_ROOM_Booking_Group>().Where(p => p.LANG_LanguagesID == Language.ID && p.AUTH_User_ID == AUTH_Users_ID);

        return await data.OrderByDescending(o => o.CreationDate).ToListAsync();
    }
    public async Task<bool> CheckBookingOpenPaymentsByUser(Guid AUTH_Users_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        V_ROOM_Booking_Group? data = await _unitOfWork.Repository<V_ROOM_Booking_Group>().FirstOrDefaultAsync(p => p.LANG_LanguagesID == Language.ID &&
                                                                                                             p.AUTH_User_ID == AUTH_Users_ID &&
                                                                                                             p.ROOM_BookingStatus_ID == ROOMStatus.ToPay);

        return data != null;
    }
    public async Task<List<V_ROOM_Booking_Group_Backend>> GetBookingGroupBackendByUser(Guid AUTH_Users_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        var data = _unitOfWork.Repository<V_ROOM_Booking_Group_Backend>().Where(p => p.LANG_LanguagesID == Language.ID && p.AUTH_Users_ID == AUTH_Users_ID);

        return await data.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }
    public async Task<List<V_ROOM_Booking_Group_Users>> GetBookingGroupUsers(Guid AUTH_Municipality_ID)
    {
        return await _unitOfWork.Repository<V_ROOM_Booking_Group_Users>().Where(p => p.AUTH_MunicipalityID == AUTH_Municipality_ID).ToListAsync();
    }
    public async Task<List<V_ROOM_Booking_Status_Log>> GetRoomStatusLogList(Guid ROOM_BookingGroup_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Booking_Status_Log>().Where(p => p.ROOM_BookingGroup_ID == ROOM_BookingGroup_ID && p.LANG_Languages_ID == Language.ID).ToListAsync();
    }
    public async Task<ROOM_Booking_Status_Log?> GetRoomStatusLog(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Booking_Status_Log>().Where(p => p.ID == ID).Include(p => p.ROOM_Booking_Status_Log_Extended).FirstOrDefaultAsync();
    }
    public async Task<ROOM_Booking_Status_Log?> SetRoomStatusLog(ROOM_Booking_Status_Log Data)
    {
        return await _unitOfWork.Repository<ROOM_Booking_Status_Log>().InsertOrUpdateAsync(Data);
    }
    public async Task<ROOM_Booking_Status_Log_Extended?> GetRoomStatusLogExtended(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Booking_Status_Log_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
    }
    public async Task<ROOM_Booking_Status_Log_Extended?> SetRoomStatusLogExtended(ROOM_Booking_Status_Log_Extended Data)
    {
        return await _unitOfWork.Repository<ROOM_Booking_Status_Log_Extended>().InsertOrUpdateAsync(Data);
    }
    public async Task<List<ROOM_Booking_Ressource>> GetRoomRessourceList(Guid ROOM_BookingGroup_ID)
    {
        return await _unitOfWork.Repository<ROOM_Booking_Ressource>().Where(p => p.ROOM_BookingGroup_ID == ROOM_BookingGroup_ID).ToListAsync();
    }
    public async Task<ROOM_Booking_Ressource?> GetRoomRessource(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Booking_Ressource>().FirstOrDefaultAsync(p => p.ID == ID);
    }
    public async Task<ROOM_Booking_Ressource?> SetRoomRessource(ROOM_Booking_Ressource Data)
    {
        return await _unitOfWork.Repository<ROOM_Booking_Ressource>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRoomRessource(Guid ID, bool force = false)
    {
        return await _unitOfWork.Repository<ROOM_Booking_Ressource>().DeleteAsync(ID);
    }
    public async Task<List<V_ROOM_RoomPricing_Type>> GetRoomPricingType()
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_RoomPricing_Type>().Where(p => p.LANG_LanguagesID == Language.ID).ToListAsync();
    }
    public async Task<List<ROOM_RoomPricing>> GetRoomPricing(Guid roomID, Guid authCompanyTypeID)
    {
        var pricing = await _unitOfWork.Repository<ROOM_RoomPricing>().Where(a => a.ROOM_Rooms_ID == roomID && a.AUTH_Company_Type_ID == authCompanyTypeID).OrderBy(p => p.MinPrice).ToListAsync();

        return pricing.ToList();
    }
    public async Task<FILE_FileInfo> GetDocument(Guid ROOM_BookingGroupID, Guid LANG_Language_ID, bool localhost = false)
    {
        Guid LangID = _langProvider.GetCurrentLanguageID();

        var reportPackager = new ReportPackager();
        var reportSource = new InstanceReportSource();

        var CurrentLanguage = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        string reportFileName = "RoomBooking_" + CurrentLanguage.Code + ".trdp";

        var BasePath = @"D:\Comunix\Reports\" + reportFileName;

        if (localhost == true)
        {
            BasePath = @"C:\VisualStudioProjects\Comunix\ICWebApp.Reporting\wwwroot\Reports\" + reportFileName;
        }

        using (var sourceStream = System.IO.File.OpenRead(BasePath))
        {
            var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);
            report.ReportParameters.Clear();
            report.ReportParameters.Add(new ReportParameter("BookingGroupID", ReportParameterType.String, ROOM_BookingGroupID.ToString().ToUpper()));
            report.ReportParameters.Add(new ReportParameter("LanguageID", ReportParameterType.String, LangID.ToString().ToUpper()));

            reportSource.ReportDocument = report;

            var reportProcessor = new ReportProcessor();
            var deviceInfo = new System.Collections.Hashtable();

            deviceInfo.Add("ComplianceLevel", "PDF/A-2b");

            RenderingResult result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);

            var ms = new MemoryStream(result.DocumentBytes);
            ms.Position = 0;

            var BookingGroup = await GetBookingGroup(ROOM_BookingGroupID);

            FILE_FileInfo fi = new FILE_FileInfo();
            fi.ID = Guid.NewGuid();
            fi.FileName = "RoomBooking";

            if (CurrentLanguage.Code.Contains("DE"))
            {
                fi.FileName = "Raumbuchung";
            }
            if (CurrentLanguage.Code.Contains("IT"))
            {
                fi.FileName = "PrenotazioneSala";
            }


            fi.CreationDate = DateTime.Now;
            fi.AUTH_Users_ID = BookingGroup.AUTH_User_ID;
            fi.FileExtension = ".pdf";
            fi.Size = ms.Length;
            var fstorage = new FILE_FileStorage();

            fstorage.ID = Guid.NewGuid();
            fstorage.CreationDate = DateTime.Now;
            fstorage.FILE_FileInfo_ID = fi.ID;

            fstorage.FileImage = ms.ToArray();

            fi.FILE_FileStorage = new List<FILE_FileStorage>();
            fi.FILE_FileStorage.Add(fstorage);

            await _FileProvider.SetFileInfo(fi);

            return fi;
        }
    }
    public long GetLatestProgressivNumber(Guid AUTH_Municipality_ID, int Year)
    {
        var number = _unitOfWork.Repository<ROOM_BookingGroup>().Where(p => p.ProgressivYear == Year && p.AUTH_MunicipalityID == AUTH_Municipality_ID).Max(p => p.ProgressivNumber);

        if (number != null)
        {
            return number.Value;
        }

        return 0;
    }
    public async Task<string> ReplaceKeywords(Guid ROOM_BookingGroup_ID, Guid LANG_Language_ID, string Text, Guid? PreviousStatus_ID = null)
    {
        var booking = await _unitOfWork.Repository<V_ROOM_Booking_Group>().FirstOrDefaultAsync(p => p.ID == ROOM_BookingGroup_ID && p.LANG_LanguagesID == LANG_Language_ID);

        if (booking != null)
        {
            Text = Text.Replace("{ApplicantName}", booking.FirstName + " " + booking.LastName);

            if (booking.ProgressivNumber != null)
            {
                Text = Text.Replace("{Protokollnummer}", booking.ProgressivNumber.ToString());
                Text = Text.Replace("{Numero di protocollo}", booking.ProgressivNumber.ToString());
            }

            Text = Text.Replace("{Titel Veranstaltung}", booking.Title);
            Text = Text.Replace("{Titolo evento}", booking.Title);

            Text = Text.Replace("{Notiz}", booking.Description);
            Text = Text.Replace("{Annotazioni}", booking.Description);

            Text = Text.Replace("{Datum}", booking.Days);
            Text = Text.Replace("{Data}", booking.Days);

            Text = Text.Replace("{Räume}", booking.Rooms);
            Text = Text.Replace("{Sale}", booking.Rooms);

            var StatusList = await GetBookingStatusList();

            if (PreviousStatus_ID != null)
            {
                var prevStatus = StatusList.FirstOrDefault(p => p.ID == PreviousStatus_ID);

                if (prevStatus != null)
                {
                    var item = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == prevStatus.TEXT_SystemTextsCode && p.LANG_LanguagesID == LANG_Language_ID);

                    if (item != null)
                    {
                        Text = Text.Replace("{Bisheriger Status}", item.Text);
                        Text = Text.Replace("{Stato precedente}", item.Text);
                    }
                }
            }

            var newStatus = StatusList.FirstOrDefault(p => p.ID == booking.ROOM_BookingStatus_ID);

            if (newStatus != null)
            {
                var item = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == newStatus.TEXT_SystemTextsCode && p.LANG_LanguagesID == LANG_Language_ID);

                if (item != null)
                {
                    Text = Text.Replace("{Neuer Status}", item.Text);
                    Text = Text.Replace("{Nuovo stato}", item.Text);
                }
            }
        }

        return Text;
    }
    public async Task<List<V_ROOM_RoomOptionsCategory>> GetRoomOptionsCategories()
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_RoomOptionsCategory>().Where(p => p.LANG_LanguagesID == Language.ID).ToListAsync();
    }
    public async Task<List<ROOM_RoomOptions_Contact>> GetRoomOptionContacts(Guid ROOM_RoomOption_ID)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions_Contact>().Where(p => p.ROOM_RoomOptions_ID == ROOM_RoomOption_ID).Include(p => p.CONT_Contact).ToListAsync();
    }
    public async Task<ROOM_RoomOptions_Contact?> SetRoomOptionContacts(ROOM_RoomOptions_Contact Data)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions_Contact>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRoomOptionContacts(Guid ROOM_RoomOption_Contact_ID)
    {
        return await _unitOfWork.Repository<ROOM_RoomOptions_Contact>().DeleteAsync(ROOM_RoomOption_Contact_ID);
    }
    public async Task<List<V_ROOM_RoomOptions_Contact>> GetVRoomOptionContacts(Guid ROOM_RoomOption_ID)
    {
        return await _unitOfWork.Repository<V_ROOM_RoomOptions_Contact>().Where(p => p.ROOM_RoomOptions_ID == ROOM_RoomOption_ID).ToListAsync();
    }
    public async Task<List<ROOM_Rooms_Contact>> GetRoomsContacts(Guid ROOM_Rooms_ID)
    {
        return await _unitOfWork.Repository<ROOM_Rooms_Contact>().Where(p => p.ROOM_Rooms_ID == ROOM_Rooms_ID).Include(p => p.CONT_Contact).Include(p => p.ROOM_Rooms_Contact_Type).ToListAsync();
    }

    public async Task<(List<Guid?>, List<Guid?>)> GetBookingOrBookingGroupContactsToNotify(Guid? RoomBookingId,
        ROOM_Booking booking)
    {
        var emailContactList = new List<Guid?>();
        var smsContactList = new List<Guid?>();
        
        var contacts = new List<ROOM_Rooms_Contact>();
        //Room or building people
        var room = await GetVRoom(booking.ROOM_Room_ID.Value);
        if (room != null) {
            var roomContacts = await GetRoomsContacts(room.ID);

            if(room.RoomGroupFamilyID != null)
            {
                roomContacts = await GetRoomsContacts(room.RoomGroupFamilyID.Value);
            }
            contacts.AddRange(roomContacts);
        }

        emailContactList.AddRange(contacts.Where(e => e.SendEmail && e.CONT_Contact_ID != null).Select(e => e.CONT_Contact_ID));
        smsContactList.AddRange(contacts.Where(e => e.SendSMS && e.CONT_Contact_ID != null).Select(e => e.CONT_Contact_ID));

        if (RoomBookingId != null)
        {
            var options = await GetBookingOptions(RoomBookingId.Value, booking.ROOM_Room_ID.Value);
            var optionContacts = new List<ROOM_RoomOptions_Contact>();
            foreach (var option in options)
            {
                if (option.ROOM_Room_ID != null)
                {
                    var temp = await GetRoomOptionContacts(option.ROOM_RoomOption_ID!.Value);
                    optionContacts.AddRange(temp);
                }
            }
            emailContactList.AddRange(optionContacts.Where(e => e.SendEmail && e.CONT_Contact_ID != null).Select(e => e.CONT_Contact_ID));
            smsContactList.AddRange(optionContacts.Where(e => e.SendSMS && e.CONT_Contact_ID != null).Select(e => e.CONT_Contact_ID));
        }
        
        
        return (emailContactList.Distinct().ToList(), smsContactList.Distinct().ToList());
    }
    public async Task<ROOM_Rooms_Contact?> SetRoomsContacts(ROOM_Rooms_Contact Data)
    {
        return await _unitOfWork.Repository<ROOM_Rooms_Contact>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRoomsContacts(Guid ROOM_Rooms_Contact_ID)
    {
        return await _unitOfWork.Repository<ROOM_Rooms_Contact>().DeleteAsync(ROOM_Rooms_Contact_ID);
    }
    public async Task<List<V_ROOM_Contact_Type>> GetContactTypes()
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Contact_Type>().ToListAsync(p => p.LANG_LanguagesID == Language.ID);
    }
    public async Task<List<ROOM_Rooms_Contact_Type>> GetRoomContactTypes(Guid ROOM_Rooms_Contact_ID)
    {
        return await _unitOfWork.Repository<ROOM_Rooms_Contact_Type>().ToListAsync(p => p.ROOM_Rooms_Contact_ID == ROOM_Rooms_Contact_ID);
    }
    public async Task<ROOM_Rooms_Contact_Type?> SetRoomContactType(ROOM_Rooms_Contact_Type Data)
    {
        return await _unitOfWork.Repository<ROOM_Rooms_Contact_Type>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRoomContactType(ROOM_Rooms_Contact_Type Data)
    {
        return await _unitOfWork.Repository<ROOM_Rooms_Contact_Type>().DeleteAsync(Data);
    }
    public async Task<ROOM_Settings?> GetSettings(Guid AUTH_Municipality_ID)
    {
        return await _unitOfWork.Repository<ROOM_Settings>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
    }
    public async Task<V_ROOM_Settings?> GetVSettings(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
    {
        return await _unitOfWork.Repository<V_ROOM_Settings>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
    }
    public async Task<ROOM_Settings?> SetSettings(ROOM_Settings Data)
    {
        return await _unitOfWork.Repository<ROOM_Settings>().InsertOrUpdateAsync(Data);
    }
    public async Task<List<ROOM_Settings_Extended>> GetSettingsExtended(Guid ROOM_Settings_ID)
    {
        return await _unitOfWork.Repository<ROOM_Settings_Extended>().Where(p => p.ROOM_Settings_ID == ROOM_Settings_ID).ToListAsync();
    }
    public async Task<ROOM_Settings_Extended?> SetSettingsExtended(ROOM_Settings_Extended Data)
    {
        return await _unitOfWork.Repository<ROOM_Settings_Extended>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> CreateBookingStatusLog(ROOM_BookingGroup BookingGroup, Guid ROOM_BookingGroup_Status_ID)
    {
        var status = await _unitOfWork.Repository<ROOM_BookingStatus>().FirstOrDefaultAsync(p => p.ID == ROOM_BookingGroup_Status_ID);

        if (status != null)
        {
            var StatusLog = new ROOM_Booking_Status_Log();

            StatusLog.ID = Guid.NewGuid();

            StatusLog.ROOM_BookingGroup_ID = BookingGroup.ID;
            StatusLog.AUTH_Users_ID = BookingGroup.AUTH_User_ID;
            StatusLog.ROOM_BookingStatus = ROOM_BookingGroup_Status_ID;
            StatusLog.ChangeDate = DateTime.Now;

            await _unitOfWork.Repository<ROOM_Booking_Status_Log>().InsertOrUpdateAsync(StatusLog);

            var Languages = await _unitOfWork.Repository<LANG_Languages>().ToListAsync();

            if (Languages != null)
            {
                foreach (var l in Languages)
                {
                    var dataE = new ROOM_Booking_Status_Log_Extended()
                    {
                        ROOM_Booking_Status_Log_ID = StatusLog.ID,
                        LANG_Languages_ID = l.ID
                    };

                    if (l.ID == LanguageSettings.German)
                    {
                        var text = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == status.TEXT_SystemTextsCode && p.LANG_LanguagesID == LanguageSettings.German);

                        if (text != null)
                        {
                            dataE.Title = text.Text;
                        }
                    }
                    else if (l.ID == LanguageSettings.Italian)
                    {
                        var text = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == status.TEXT_SystemTextsCode && p.LANG_LanguagesID == LanguageSettings.Italian);

                        if (text != null)
                        {
                            dataE.Title = text.Text;
                        }
                    }
                    else
                    {
                        var text = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == status.TEXT_SystemTextsCode && p.LANG_LanguagesID == LanguageSettings.German);

                        if (text != null)
                        {
                            dataE.Title = text.Text;
                        }
                    }

                    await _unitOfWork.Repository<ROOM_Booking_Status_Log_Extended>().InsertOrUpdateAsync(dataE);
                }
            }
        }

        return true;
    }
    public async Task<List<ROOM_BookingOptions>> GetBookingOptions(Guid ROOM_BookingGroup_ID)
    {
        return await _unitOfWork.Repository<ROOM_BookingOptions>().ToListAsync(p => p.ROOM_BookingGroup_ID == ROOM_BookingGroup_ID);
    }
    
    public async Task<List<ROOM_BookingOptions>> GetBookingOptions(Guid ROOM_BookingGroup_ID, Guid RoomId)
    {
        return await _unitOfWork.Repository<ROOM_BookingOptions>().ToListAsync(p => p.ROOM_BookingGroup_ID == ROOM_BookingGroup_ID && p.ROOM_Room_ID == RoomId);
    }
    public async Task<ROOM_BookingOptions?> SetBookingOptions(ROOM_BookingOptions Options)
    {
        return await _unitOfWork.Repository<ROOM_BookingOptions>().InsertOrUpdateAsync(Options);
    }
    public async Task<bool> SetBookingComitted(ROOM_BookingGroup BookingGroup, string BaseUri, bool protocolMailSent = false)
    {
        BookingGroup.ROOM_BookingStatus_ID = ROOMStatus.Comitted;

        if (BookingGroup.AUTH_MunicipalityID != null)
        {
            var lastNumber = GetLatestProgressivNumber(BookingGroup.AUTH_MunicipalityID.Value, DateTime.Now.Year);

            if (BookingGroup.ProgressivNumber == null || BookingGroup.ProgressivNumber == 0)
            {
                BookingGroup.ProgressivYear = DateTime.Now.Year;

                BookingGroup.ProgressivNumber = lastNumber + 1;
            }
        }

        await CreateBookingStatusLog(BookingGroup, ROOMStatus.Comitted);
        if(protocolMailSent)
        {
            BookingGroup.ProtocolDate = DateTime.Now;
        }
        await SetBookingGroup(BookingGroup);

        var msg = await GetCitizenCommittedMessage(BookingGroup);

        if (BookingGroup.AUTH_User_ID != null && BookingGroup.AUTH_MunicipalityID != null)
        {
            var bookings = await GetBookingsByGroupID(BookingGroup.ID);

            if (bookings != null)
            {
                string contactString = "";
                bool existingContacts = false;
                var contactList = new List<V_ROOM_Rooms_Contact>();

                foreach (var roomID in bookings.Select(p => p.ROOM_Room_ID).Distinct())
                {
                    if (roomID != null)
                    {
                        var room = await GetRoom(roomID.Value);

                        if (room != null)
                        {
                            var contacts = await GetVRoomsContacts(room.ID);

                            var roomContacts = contacts.Where(p => p.ShowOnline == true).ToList();

                            if (room.RoomGroupFamilyID != null)
                            {
                                var parentContacts = await GetVRoomsContacts(room.RoomGroupFamilyID.Value);

                                parentContacts = parentContacts.Where(p => p.ShowOnline == true).ToList();

                                roomContacts.AddRange(parentContacts);
                            }

                            contactList.AddRange(roomContacts);
                        }
                    }
                }

                contactString += _textProvider.Get("ROOM_CONTACTS") + "<br><br>";

                foreach (var contact in contactList.Distinct().ToList())
                {
                    contact.Phone = contact.Phone.Replace(" ", "").Replace("+", "");

                    contactString += "<b>" + contact.FirstName + " " + contact.LastName + "</b><br>";
                    contactString += contact.ContactType + "<br>";
                    contactString += String.Format("{0:(+##) ###-###-####}", long.Parse(contact.Phone)) + "<br>";
                    contactString += contact.EMail + "<br><br>";

                    existingContacts = true;
                }

                if (!existingContacts)
                {
                    contactString = "";
                }

                if (msg != null)
                {
                    msg.Messagetext = msg.Messagetext.Replace("{Contacts}", contactString);
                }
            }

            if (msg != null)
            {
                await _messageService.SendMessage(msg, BaseUri + "/User/Services");
            }

            //SEND TO AUTHORITY
            var authorityMsg = await GetMunCommittedMessage(BookingGroup);

            var authorityList = await _unitOfWork.Repository<AUTH_Authority>().ToListAsync(p => p.AUTH_Municipality_ID == BookingGroup.AUTH_MunicipalityID);

            var subsititutionAuthority = authorityList.FirstOrDefault(p => p.IsRooms == true);

            if (authorityMsg != null && subsititutionAuthority != null)
            {
                await _messageService.SendMessageToAuthority(subsititutionAuthority.ID, authorityMsg, BaseUri + "/RoomBooking/Detail/" + BookingGroup.ID);
            }
        }

        return true;
    }
    public async Task<bool> SetBookingCanceled(Guid BookingGroup_ID, string BaseUri)
    {
        var dbBookingGroup = await GetBookingGroup(BookingGroup_ID);

        if (dbBookingGroup != null)
        {

            dbBookingGroup.ROOM_BookingStatus_ID = ROOMStatus.CancelledByCitizen;

            await CreateBookingStatusLog(dbBookingGroup, ROOMStatus.CancelledByCitizen);
            await SetBookingGroup(dbBookingGroup);

            var msg = await GetCitizenCanceledMessage(dbBookingGroup);

            if (dbBookingGroup.AUTH_User_ID != null && dbBookingGroup.AUTH_MunicipalityID != null)
            {
                if (msg != null)
                {
                    await _messageService.SendMessage(msg, BaseUri + "/User/Services");
                }

                //SEND TO AUTHORITY
                var authorityMsg = await GetMunCanceledMessage(dbBookingGroup);

                var authorityList = await _unitOfWork.Repository<AUTH_Authority>().ToListAsync(p => p.AUTH_Municipality_ID == dbBookingGroup.AUTH_MunicipalityID);

                var subsititutionAuthority = authorityList.FirstOrDefault(p => p.IsRooms == true);

                if (authorityMsg != null && subsititutionAuthority != null)
                {
                    await _messageService.SendMessageToAuthority(subsititutionAuthority.ID, authorityMsg, BaseUri + "/RoomBooking/Detail/" + dbBookingGroup.ID);
                }
            }
        }

        return true;
    }
    public async Task<bool> VerifyBookingsState(Guid AUTH_Users_ID, string BaseUri)
    {
        var bookingsToPay = await _unitOfWork.Repository<ROOM_BookingGroup>().ToListAsync(p => p.AUTH_User_ID == AUTH_Users_ID && p.ROOM_BookingStatus_ID == ROOMStatus.ToPay);

        if (bookingsToPay != null && bookingsToPay.Count() > 0)
        {
            foreach (var booking in bookingsToPay)
            {
                bool Payed = false;
                var payTransactions = await _unitOfWork.Repository<ROOM_BookingTransactions>().ToListAsync(p => p.ROOM_BookingGroupID == booking.ID);

                foreach (var appTrans in payTransactions)
                {
                    if (appTrans.PAY_Transaction_ID != null)
                    {
                        var payTrans = await _unitOfWork.Repository<PAY_Transaction>().FirstOrDefaultAsync(p => p.ID == appTrans.PAY_Transaction_ID.Value);

                        if (payTrans != null && payTrans.PaymentDate != null)
                        {
                            Payed = true;
                        }
                        else
                        {
                            Payed = false;
                            break;
                        }
                    }
                }

                if (Payed == true)
                {
                    booking.PayedAt = DateTime.Now;

                    await SetBookingGroup(booking);

                    bool ToSign = false;

                    var bookings = await GetBookingsByGroupID(booking.ID);

                    foreach (var roomID in bookings.Select(p => p.ROOM_Room_ID).Distinct())
                    {
                        if (roomID != null)
                        {
                            var room = await GetRoom(roomID.Value);

                            if (room != null && room.HasSigning == true)
                            {
                                ToSign = true;
                            }
                        }
                    }

                    if (ToSign == true)
                    {
                        booking.ROOM_BookingStatus_ID = ROOMStatus.ToSign;

                        await CreateBookingStatusLog(booking, ROOMStatus.ToSign);
                        await SetBookingGroup(booking);
                    }
                    else
                    {
                        await SetBookingComitted(booking, BaseUri);
                    }
                }
            }
        }

        var bookingsToSign = await _unitOfWork.Repository<ROOM_BookingGroup>().ToListAsync(p => p.AUTH_User_ID == AUTH_Users_ID && p.ROOM_BookingStatus_ID == ROOMStatus.ToSign);

        if (bookingsToSign != null && bookingsToSign.Count() > 0)
        {
            foreach (var booking in bookingsToSign)
            {
                var doc = await _unitOfWork.Repository<FILE_FileInfo>().FirstOrDefaultAsync(p => p.ID == booking.FILE_FileInfo_ID);

                if (doc != null && doc.Signed == true)
                {
                    booking.SignedAt = DateTime.Now;
                    await SetBookingGroup(booking);

                    await SetBookingComitted(booking, BaseUri);
                }
            }
        }

        return true;
    }
    public async Task<List<V_ROOM_Booking_Bookings>> GetBookingPositions(Guid ROOM_BookingGroup_ID)
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Booking_Bookings>().ToListAsync(p => p.ROOM_BookingGroup_ID == ROOM_BookingGroup_ID && p.LANG_Languages_ID == Language.ID);
    }
    public async Task<List<V_ROOM_Booking_Type>> GetRoomBookingTypes()
    {
        var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

        return await _unitOfWork.Repository<V_ROOM_Booking_Type>().ToListAsync(p => p.LANG_LanguagesID == Language.ID);
    }
    private async Task<MSG_Message?> GetCitizenCommittedMessage(ROOM_BookingGroup BookingGroup)
    {
        var msg = await _messageService.GetMessage(BookingGroup.AUTH_User_ID.Value, BookingGroup.AUTH_MunicipalityID.Value,
            "NOTIF_ROOM_COMITTED_TEXT", "NOTIF_ROOM_COMITTED_SHORTTEXT", "NOTIF_ROOM_COMITTED_TITLE", Guid.Parse("7d03e491-5826-4131-a6a1-06c99be991c9"), true);
        
        if (msg == null)
            return null;

        var userId = BookingGroup.AUTH_User_ID;

        var lang = (await _unitOfWork.Repository<AUTH_UserSettings>().FirstOrDefaultAsync(e => e.AUTH_UsersID == userId))?.LANG_Languages_ID;

        if (lang == null)
            lang = LanguageSettings.German;

        var vBookingGroup = await _unitOfWork.Repository<V_ROOM_Booking_Group>().FirstOrDefaultAsync(e => e.ID == BookingGroup.ID && e.LANG_LanguagesID == lang);
        
        if (vBookingGroup == null)
            return null;

        msg.Messagetext = msg.Messagetext.Replace("{EventTitle}", vBookingGroup.Title);
        msg.Messagetext = msg.Messagetext.Replace("{Locations}", vBookingGroup.Rooms);
        msg.Messagetext = msg.Messagetext.Replace("{TimeSpan}", vBookingGroup.Days);

        return msg;
    }
    private async Task<MSG_Message?> GetMunCommittedMessage(ROOM_BookingGroup BookingGroup)
    {
        var msg = await _messageService.GetMessage(BookingGroup.AUTH_User_ID.Value, BookingGroup.AUTH_MunicipalityID.Value,
            "NOTIF_MUN_ROOM_COMITTED_TEXT", "NOTIF_MUN_ROOM_COMITTED_SHORTTEXT", "NOTIF_MUN_ROOM_COMITTED_TITLE", Guid.Parse("7d03e491-5826-4131-a6a1-06c99be991c9"), true);

        if (msg == null)
            return null;

        var userId = BookingGroup.AUTH_User_ID;

        var lang = (await _unitOfWork.Repository<AUTH_UserSettings>().FirstOrDefaultAsync(e => e.AUTH_UsersID == userId))?.LANG_Languages_ID;

        if (lang == null)
            lang = LanguageSettings.German;

        var vBookingGroup = await _unitOfWork.Repository<V_ROOM_Booking_Group>().FirstOrDefaultAsync(e => e.ID == BookingGroup.ID && e.LANG_LanguagesID == lang);

        if (vBookingGroup == null)
            return null;

        msg.Messagetext = msg.Messagetext.Replace("{ProtocolNumber}", vBookingGroup.ProgressivNumberCombined);
        msg.Messagetext = msg.Messagetext.Replace("{Locations}", vBookingGroup.Rooms);
        msg.Messagetext = msg.Messagetext.Replace("{TimeSpan}", vBookingGroup.Days);
        msg.Messagetext = msg.Messagetext.Replace("{ApplicantName}", vBookingGroup.Fullname);
        msg.Messagetext = msg.Messagetext.Replace("{ApplicantTaxCode}", vBookingGroup.FiscalNumber);

        return msg;
    }
    private async Task<MSG_Message?> GetCitizenCanceledMessage(ROOM_BookingGroup BookingGroup)
    {
        var msg = await _messageService.GetMessage(BookingGroup.AUTH_User_ID.Value, BookingGroup.AUTH_MunicipalityID.Value,
            "NOTIF_ROOM_CANCELED_TEXT", "NOTIF_ROOM_CANCELED_SHORTTEXT", "NOTIF_ROOM_CANCELED_TITLE", Guid.Parse("7d03e491-5826-4131-a6a1-06c99be991c9"), true);

        if (msg == null)
            return null;

        var userId = BookingGroup.AUTH_User_ID;

        var lang = (await _unitOfWork.Repository<AUTH_UserSettings>().FirstOrDefaultAsync(e => e.AUTH_UsersID == userId))?.LANG_Languages_ID;

        if (lang == null)
            lang = LanguageSettings.German;

        var vBookingGroup = await _unitOfWork.Repository<V_ROOM_Booking_Group>().FirstOrDefaultAsync(e => e.ID == BookingGroup.ID && e.LANG_LanguagesID == lang);

        if (vBookingGroup == null)
            return null;

        msg.Messagetext = msg.Messagetext.Replace("{EventTitle}", vBookingGroup.Title);
        msg.Messagetext = msg.Messagetext.Replace("{Locations}", vBookingGroup.Rooms);
        msg.Messagetext = msg.Messagetext.Replace("{TimeSpan}", vBookingGroup.Days);

        return msg;
    }
    private async Task<MSG_Message?> GetMunCanceledMessage(ROOM_BookingGroup BookingGroup)
    {
        var msg = await _messageService.GetMessage(BookingGroup.AUTH_User_ID.Value, BookingGroup.AUTH_MunicipalityID.Value,
            "NOTIF_MUN_ROOM_CANCELED_TEXT", "NOTIF_MUN_ROOM_CANCELED_SHORTTEXT", "NOTIF_MUN_ROOM_CANCELED_TITLE", Guid.Parse("7d03e491-5826-4131-a6a1-06c99be991c9"), true);

        if (msg == null)
            return null;

        var userId = BookingGroup.AUTH_User_ID;

        var lang = (await _unitOfWork.Repository<AUTH_UserSettings>().FirstOrDefaultAsync(e => e.AUTH_UsersID == userId))?.LANG_Languages_ID;

        if (lang == null)
            lang = LanguageSettings.German;

        var vBookingGroup = await _unitOfWork.Repository<V_ROOM_Booking_Group>().FirstOrDefaultAsync(e => e.ID == BookingGroup.ID && e.LANG_LanguagesID == lang);

        if (vBookingGroup == null)
            return null;

        msg.Messagetext = msg.Messagetext.Replace("{ProtocolNumber}", vBookingGroup.ProgressivNumberCombined);
        msg.Messagetext = msg.Messagetext.Replace("{Locations}", vBookingGroup.Rooms);
        msg.Messagetext = msg.Messagetext.Replace("{TimeSpan}", vBookingGroup.Days);
        msg.Messagetext = msg.Messagetext.Replace("{ApplicantName}", vBookingGroup.Fullname);
        msg.Messagetext = msg.Messagetext.Replace("{ApplicantTaxCode}", vBookingGroup.FiscalNumber);

        return msg;
    }
    public async Task<List<V_HOME_Theme>> GetVThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
    {
        var existingElements = await _unitOfWork.Repository<ROOM_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();

        var themes = await _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();

        return themes.Where(p => existingElements.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToList();
    }
    public async Task<List<ROOM_Theme>> GetThemes(Guid AUTH_Municipality_ID)
    {
        return await _unitOfWork.Repository<ROOM_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();
    }
    public async Task<bool> RemoveTheme(ROOM_Theme Data)
    {
        return await _unitOfWork.Repository<ROOM_Theme>().DeleteAsync(Data);
    }
    public async Task<ROOM_Theme?> SetTheme(ROOM_Theme Data)
    {
        return await _unitOfWork.Repository<ROOM_Theme>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> HasTheme(Guid HOME_Theme_ID)
    {
        var exists = await _unitOfWork.Repository<ROOM_Theme>().FirstOrDefaultAsync(p => p.HOME_Theme_ID == HOME_Theme_ID);

        if (exists != null)
        {
            return true;
        }

        return false;
    }
    public async Task<List<ROOM_Property>> GetPropertyList(Guid AUTH_Municipality_ID, Guid LanguageID)
    {
        var data = await _unitOfWork.Repository<ROOM_Property>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).Include(p => p.ROOM_Property_Extended).ToListAsync();

        foreach (var d in data)
        {
            if (d != null)
            {
                var extended = d.ROOM_Property_Extended.FirstOrDefault(p => p.LANG_Languages_ID == LanguageID);

                if (extended != null)
                {
                    d.Title = extended.Title;
                    d.Description = extended.Description;
                }
            }
        }

        return data;
    }
    public async Task<ROOM_Property?> GetProperty(Guid ID, Guid LanguageID)
    {
        var data = await _unitOfWork.Repository<ROOM_Property>().Where(p => p.ID == ID).Include(p => p.ROOM_Property_Extended).FirstOrDefaultAsync();

        if (data != null)
        {
            var extended = data.ROOM_Property_Extended.FirstOrDefault(p => p.LANG_Languages_ID == LanguageID);

            if (extended != null)
            {
                data.Title = extended.Title;
                data.Description = extended.Description;
            }
        }
        return data;
    }
    public async Task<ROOM_Property?> SetProperty(ROOM_Property Data)
    {
        return await _unitOfWork.Repository<ROOM_Property>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveProperty(Guid ID, bool force = false)
    {
        return await _unitOfWork.Repository<ROOM_Property>().DeleteAsync(ID);
    }
    public async Task<List<ROOM_Property_Extended>> GetPropertyExtendedList(Guid ROOM_Property_ID, Guid LANG_Language_ID)
    {
        return await _unitOfWork.Repository<ROOM_Property_Extended>().Where(p => p.ROOM_Property_ID == ROOM_Property_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
    }
    public async Task<ROOM_Property_Extended?> GetPropertyExtended(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Property_Extended>().GetByIDAsync(ID);
    }
    public async Task<ROOM_Property_Extended?> SetPropertyExtended(ROOM_Property_Extended Data)
    {
        return await _unitOfWork.Repository<ROOM_Property_Extended>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemovePropertyExtended(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Property_Extended>().DeleteAsync(ID);
    }
    public async Task<List<ROOM_Ressources>> GetRessourceList(Guid AUTH_Municipality_ID, Guid LanguageID)
    {
        var data = await _unitOfWork.Repository<ROOM_Ressources>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).Include(p => p.ROOM_Ressources_Extended).ToListAsync();

        foreach (var d in data)
        {
            if (d != null)
            {
                var extended = d.ROOM_Ressources_Extended.FirstOrDefault(p => p.LANG_Language_ID == LanguageID);

                if (extended != null)
                {
                    d.Description = extended.Description;
                }
            }
        }

        return data;
    }
    public async Task<ROOM_Ressources?> GetRessource(Guid ID, Guid LanguageID)
    {
        var data = await _unitOfWork.Repository<ROOM_Ressources>().Where(p => p.ID == ID).Include(p => p.ROOM_Ressources_Extended).FirstOrDefaultAsync();

        if (data != null)
        {
            var extended = data.ROOM_Ressources_Extended.FirstOrDefault(p => p.LANG_Language_ID == LanguageID);

            if (extended != null)
            {
                data.Description = extended.Description;
            }
        }

        return data;
    }
    public async Task<ROOM_Ressources?> SetRessource(ROOM_Ressources Data)
    {
        return await _unitOfWork.Repository<ROOM_Ressources>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRessource(Guid ID, bool force = false)
    {
        return await _unitOfWork.Repository<ROOM_Ressources>().DeleteAsync(ID);
    }
    public async Task<List<ROOM_Ressources_Extended>> GetRessourceExtendedList(Guid ROOM_Ressources_ID, Guid LANG_Language_ID)
    {
        return await _unitOfWork.Repository<ROOM_Ressources_Extended>().Where(p => p.ROOM_Ressources_ID == ROOM_Ressources_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
    }
    public async Task<ROOM_Ressources_Extended?> GetRessourceExtended(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Ressources_Extended>().GetByIDAsync(ID);
    }
    public async Task<ROOM_Ressources_Extended?> SetRessourceExtended(ROOM_Ressources_Extended Data)
    {
        return await _unitOfWork.Repository<ROOM_Ressources_Extended>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRessourceExtended(Guid ID)
    {
        return await _unitOfWork.Repository<ROOM_Ressources_Extended>().DeleteAsync(ID);
    }
}