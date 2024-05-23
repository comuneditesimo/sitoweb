using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.Models.Rooms;
using ICWebApp.Domain.DBModels;
using Blazored.SessionStorage;
using Microsoft.JSInterop;
using Telerik.Reporting;
using System.Text.Json;
using Telerik.Blazor;
using Syncfusion.Blazor.Popups;
using ICWebApp.Domain.Enums.Homepage.Settings;

namespace ICWebApp.Components.Pages.Rooms.Frontend
{
    public partial class Booking : IDisposable
    {
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IRoomProvider RoomProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IRoomBookingHelper RoomBookingHelper { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IBookingService BookingService { get; set; }
        [Inject] IPRIVProvider PrivProvider { get; set; }
        [Inject] ISessionStorageService SessionStorage { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ID3Helper D3Helper { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IJSRuntime JsRuntime { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string SessionKey { get; set; }

        private bool IsDataBusy = false;
        private bool FirstStep = true;
        private List<string> BookingErrorList = new List<string>();
        private ROOM_BookingGroup CurrentBooking = new ROOM_BookingGroup();
        private PRIV_Privacy? Privacy;
        private List<V_ROOM_RoomOptions>? RoomOptionsList = new List<V_ROOM_RoomOptions>();
        private bool IsValid = true;
        private bool ShowBookingWarning = false;
        private List<BookingPrice> BookingPrices = new List<BookingPrice>();
        private DateTime LastNotificationEvent = DateTime.Now;

        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                CrumbService.ClearBreadCrumb();
                CrumbService.AddBreadCrumb("/Rooms", "MAINMENU_ROOMS", null, null);
                CrumbService.AddBreadCrumb("/Rooms/Service", "FRONTEND_BOOKING_TITLE", null, null, true);

                SessionWrapper.PageTitle = TextProvider.Get("FRONTEND_BOOKING_TITLE");


                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Rooms);

                    if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                    {
                        SessionWrapper.PageSubTitle = pagesubTitle.Description;
                    }
                    else
                    {
                        SessionWrapper.PageSubTitle = TextProvider.Get("MAINMENU_ROOMS_SERVICE_DESCRIPTION");
                    }
                }
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Privacy = await PrivProvider.GetPrivacy(SessionWrapper.AUTH_Municipality_ID.Value);
            }
            else
            {
                Privacy = null;
            }

            RoomBookingHelper.TimeFilter = new TimeFilter();
            RoomBookingHelper.ShowBookingSideBar = true;
            if (RoomBookingHelper.TimeFilter != null)
            {
                RoomBookingHelper.TimeFilter.OnDataChanged += TimeFilter_OnDataChanged;
            }
            BookingService.OnValuesChanged += BookingService_OnValuesChanged;

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async void BookingService_OnValuesChanged()
        {
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (SessionKey != null)
                {
                    var timeString = await SessionStorage.GetItemAsync<string>(SessionKey + "_Time");
                    var weekString = await SessionStorage.GetItemAsync<string>(SessionKey + "_Weekdays");
                    var roomString = await SessionStorage.GetItemAsync<string>(SessionKey + "_Rooms");

                    if (!string.IsNullOrEmpty(timeString) && !string.IsNullOrEmpty(roomString))
                    {
                        var timefilter = JsonSerializer.Deserialize<TimeFilter>(timeString);
                        var roomfilter = JsonSerializer.Deserialize<List<V_ROOM_Rooms>>(roomString);

                        if (timefilter != null && roomfilter != null)
                        {
                            RoomBookingHelper.RoomList = roomfilter;
                            if (RoomBookingHelper.RoomOptions != null && RoomBookingHelper.RoomOptions.Any())
                            {
                                RoomBookingHelper.RoomOptions.Clear();
                                BookingService.NotifyValuesChanged();
                            }
                            RoomBookingHelper.TimeFilter = timefilter;

                            if(!string.IsNullOrEmpty(weekString))
                            {
                                var weekfilter = JsonSerializer.Deserialize<List<WeekDay>>(weekString);

                                if(weekfilter != null)
                                {
                                    RoomBookingHelper.TimeFilter.Weekly_Weekdays = weekfilter;
                                }
                            }

                            if (RoomBookingHelper.TimeFilter != null)
                            {
                                RoomBookingHelper.TimeFilter.OnDataChanged += TimeFilter_OnDataChanged;
                            }

                            GoToSecondStep();
                        }
                        else
                        {
                            RoomBookingHelper.RoomList = await RoomProvider.GetAllRooms(SessionWrapper.AUTH_Municipality_ID.Value);
                            if (RoomBookingHelper.RoomOptions != null && RoomBookingHelper.RoomOptions.Any())
                            {
                                RoomBookingHelper.RoomOptions.Clear();
                                BookingService.NotifyValuesChanged();
                            }
                            RoomBookingHelper.TimeFilter = new TimeFilter();
                            if (RoomBookingHelper.TimeFilter != null)
                            {
                                RoomBookingHelper.TimeFilter.OnDataChanged += TimeFilter_OnDataChanged;
                            }
                        }

                        BookingService.NotifyValuesChanged();
                    }
                }
                else
                {
                    RoomBookingHelper.RoomList = await RoomProvider.GetAllRooms(SessionWrapper.AUTH_Municipality_ID.Value);
                    RoomBookingHelper.TimeFilter = new TimeFilter();
                    if (RoomBookingHelper.TimeFilter != null)
                    {
                        RoomBookingHelper.TimeFilter.OnDataChanged += TimeFilter_OnDataChanged;
                    }
                }

                IsDataBusy = false;
                StateHasChanged();
            }

            await base.OnAfterRenderAsync(firstRender);
        }
        private void TimeFilter_OnDataChanged()
        {
            Task.Run(async () => {
                if (RoomBookingHelper.TimeFilter != null)
                {
                    List<Domain.Models.Rooms.Booking> _bookings = BookingService.GetDatesToBook(RoomBookingHelper.TimeFilter);
                    if (_bookings != null && _bookings.Any())
                    {
                        RoomBookingHelper.TimeFilter.UpdateSeriesRepitionOrEndTime(_bookings);
                        RoomBookingHelper.Bookings = _bookings;
                    }
                    else
                    {
                        RoomBookingHelper.Bookings = new List<Domain.Models.Rooms.Booking>();
                    }
                }
                else
                {
                    RoomBookingHelper.Bookings = new List<Domain.Models.Rooms.Booking>();
                }
                BookingService.NotifyValuesChanged();
            }).ConfigureAwait(false);
        }
            
        private async void OnSearch()
        {
            IsDataBusy = true;
            StateHasChanged();

            if(RoomBookingHelper.RoomList != null && RoomBookingHelper.RoomList.Count() > 0 && RoomBookingHelper.TimeFilter != null)
            {
                List<Domain.Models.Rooms.Booking> DatesToBook = BookingService.GetDatesToBook(RoomBookingHelper.TimeFilter);

                foreach (var room in RoomBookingHelper.RoomList)
                {
                    if (DatesToBook != null && DatesToBook.Any())
                    {
                        List<string> _resultList = new List<string>();

                        foreach (Domain.Models.Rooms.Booking date in DatesToBook.OrderBy(p => p.StartDate).ToList())
                        {
                            string? _result = await BookingService.CheckRoomAvailability(room, date);
                            if (!string.IsNullOrEmpty(_result))
                            {
                                _resultList.Add(_result);
                            }
                        }

                        if (_resultList != null && _resultList.Any())
                        {
                            room.BookableErrors = _resultList;
                        }
                        else
                        {
                            room.BookableErrors = new List<string>();
                        }
                    }
                    else
                    {
                        room.BookableErrors = null;
                    }
                }
            }

            BookingService.NotifyValuesChanged();
            StateHasChanged();

            IsDataBusy = false;
            StateHasChanged();
        }
        private async void GoToSecondStep()
        {
            if (RoomBookingHelper.RoomList != null && RoomBookingHelper.RoomList.Count() > 0 && RoomBookingHelper.TimeFilter != null)
            {
                IsDataBusy = true;
                StateHasChanged();

                bool HasErrors = false;
                var DatesToBook = BookingService.GetDatesToBook(RoomBookingHelper.TimeFilter);
                BookingErrorList = new List<string>();
                BookingPrices = new List<BookingPrice>();

                foreach (var room in RoomBookingHelper.RoomList.Where(p => p.IsSelected == true).ToList())
                {
                    List<string> resultList = new List<string>();

                    foreach (Domain.Models.Rooms.Booking date in DatesToBook.OrderBy(p => p.StartDate).ToList())
                    {
                        string? _result = await BookingService.CheckRoomAvailability(room, date, true);

                        if(!string.IsNullOrEmpty(_result))
                        {
                            resultList.Add(_result);
                        }
                        else if (date.StartDate != null && date.EndDate != null)
                        {
                            var price = new BookingPrice();

                            price.ROOM_Room_ID = room.ID;
                            price.StartDate = date.StartDate.Value;
                            price.EndDate = date.EndDate.Value;

                            BookingPrices.Add(price);
                        }
                    }

                    if (resultList.Count > 0)
                    {
                        room.BookableErrors = resultList;
                        HasErrors = true;
                        BookingErrorList.AddRange(resultList);
                    }
                    else
                    {
                        room.BookableErrors = new List<string>();
                    }
                }

                if(HasErrors == true)
                {
                    ShowBookingWarning = true;
                    StateHasChanged();
                }
                else
                {
                    FirstStepOk();
                }
            }
        }
        private void CancelSwitch()
        {
            ShowBookingWarning = false;
            IsDataBusy = false;
            StateHasChanged();
        }
        private async void FirstStepOk()
        {
            ShowBookingWarning = false;

            RoomOptionsList = new List<V_ROOM_RoomOptions>();

            foreach (var room in RoomBookingHelper.RoomList.Where(p => p.IsSelected == true && (p.BookableErrors == null || p.BookableErrors.Count() == 0)).ToList())
            {
                var data = await RoomProvider.GetRoomOptionsList(room.ID);

                RoomOptionsList.AddRange(data.Where(p => p.Enabled == true && p.ROOM_RoomOptions_Category_ID == ROOMOptionCategory.Chargable).ToList());
            }
            if (RoomBookingHelper.RoomOptions != null && RoomBookingHelper.RoomOptions.Any())
            {
                RoomBookingHelper.RoomOptions.Clear();
                BookingService.NotifyValuesChanged();
            }

            foreach (var span in BookingPrices)
            {
                if (span.StartDate != null && span.EndDate != null)
                {
                    Guid? CompanyTypeID = Guid.Parse("5FFD4839-10D6-47DF-AF5A-7D3A228CBAF5");

                    if(SessionWrapper.CurrentSubstituteUser != null)
                    {
                        CompanyTypeID = SessionWrapper.CurrentSubstituteUser.AUTH_Company_Type_ID;
                    }

                    var price = await BookingService.GetRoomCost(span.ROOM_Room_ID, CompanyTypeID, span.StartDate.Value, span.EndDate.Value);

                    if (price == null)
                        span.Price = 0;
                    else
                        span.Price = price.Value;
                }
            }
            await JsRuntime.InvokeVoidAsync("enviromentHelper_SmoothScrollToTop");
            FirstStep = false;
            IsDataBusy = false;
            StateHasChanged();
        }
        private void OnOptionSelectionChanged(V_ROOM_RoomOptions Option)
        {
            Option.IsSelected = !Option.IsSelected;

            if (RoomOptionsList != null)
            {
                RoomBookingHelper.RoomOptions = RoomOptionsList;
                BookingService.NotifyValuesChanged();
            }
            var price = BookingPrices.FirstOrDefault(p => p.ROOM_Option_ID == Option.ID);

            if (price == null)
            {
                if (Option.BasePrice != null)
                {
                    List<DateTime> _daysBooking = new List<DateTime>();
                    foreach (BookingPrice _date in BookingPrices.Where(p => p.ROOM_Room_ID == Option.ROOM_Room_ID).OrderBy(p => p.StartDate))
                    {
                        if (_date != null && _date.StartDate != null && _date.EndDate != null && _date.EndDate >= _date.StartDate)
                        {
                            for (DateTime _dateToAdd = _date.StartDate.Value.Date; _dateToAdd <= _date.EndDate.Value.Date; _dateToAdd = _dateToAdd.AddDays(1))
                            {
                                _daysBooking.Add(_dateToAdd);
                            }
                        }
                    }
                    if (Option.DailyPayment == true && _daysBooking.Count() > 1)
                    {
                        foreach (DateTime _date in _daysBooking)
                        {
                            BookingPrices.Add(new BookingPrice()
                            {
                                ROOM_Room_ID = Option.ROOM_Room_ID,
                                ROOM_Option_ID = Option.ID,
                                Price = decimal.Parse(Option.BasePrice.Value.ToString()),
                                Description = Option.Name + " - " + _date.ToString("dd.MM.yyyy")
                            });
                        }
                    }
                    else
                    {
                        BookingPrices.Add(new BookingPrice()
                        {
                            ROOM_Room_ID = Option.ROOM_Room_ID,
                            ROOM_Option_ID = Option.ID,
                            Price = decimal.Parse(Option.BasePrice.Value.ToString()),
                            Description = Option.Name
                        });
                    }
                }
            }
            else
            {
                BookingPrices.Remove(price);
            }

            StateHasChanged();
        }
        private void ReturnToFirstStep()
        {
            if (RoomBookingHelper.RoomOptions != null && RoomBookingHelper.RoomOptions.Any())
            {
                RoomBookingHelper.RoomOptions.Clear();
                BookingService.NotifyValuesChanged();
            }
            IsDataBusy = false;
            FirstStep = true;
            StateHasChanged();
        }
        private async void SaveSessionAndLogin()
        {
            if (RoomBookingHelper.TimeFilter != null && RoomBookingHelper.RoomList != null && RoomBookingHelper.RoomList.Count() > 0)
            {
                string SessionKey = Guid.NewGuid().ToString();

                var timefilter = JsonSerializer.Serialize(RoomBookingHelper.TimeFilter);
                var roomfilter = JsonSerializer.Serialize(RoomBookingHelper.RoomList);
                var weekfilter = JsonSerializer.Serialize(RoomBookingHelper.TimeFilter.Weekly_Weekdays);

                await SessionStorage.SetItemAsync(SessionKey + "_Time", timefilter);
                await SessionStorage.SetItemAsync(SessionKey + "_Rooms", roomfilter);
                await SessionStorage.SetItemAsync(SessionKey + "_Weekdays", weekfilter);

                await SessionStorage.SetItemAsync("RedirectURL", NavManager.BaseUri + "Rooms/Service/" + SessionKey);
                NavManager.NavigateTo("/Login/" + (NavManager.BaseUri + "Rooms/Service/" + SessionKey).Replace("/", "^"));
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }
        }
        private async void Commit()
        {
            if (CurrentBooking != null && CurrentBooking.PrivacyDate == null && Privacy != null)
            {
                Privacy.PrivacyErrorCSS = "error";
                IsValid = false;
                StateHasChanged();

                return;
            }
            else
            {
                IsValid = true;
            }

            if (!await Dialogs.ConfirmAsync(TextProvider.Get("FRONTEND_BOOKING_COMMIT_ARE_YOU_SURE"), TextProvider.Get("INFORMATION")))
                return;

            if(CurrentBooking != null) 
            { 
                CurrentBooking.ID = Guid.NewGuid();
                CurrentBooking.ROOM_BookingStatus_ID = Guid.Parse("C2D6504A-09E8-4AA2-8625-0D5B3E0D0871"); //Draft
                CurrentBooking.AUTH_MunicipalityID = await SessionWrapper.GetMunicipalityID();

                if (SessionWrapper.CurrentUser != null)
                {
                    AUTH_Users_Anagrafic? dbUser = null;

                    if (SessionWrapper.CurrentSubstituteUser != null)
                    {
                        dbUser = await AuthProvider.GetAnagraficByUserID(SessionWrapper.CurrentSubstituteUser.ID);
                    }
                    else
                    {
                        dbUser = await AuthProvider.GetAnagraficByUserID(SessionWrapper.CurrentUser.ID);
                    }

                    if (dbUser != null)
                    {
                        CurrentBooking.FirstName = dbUser.FirstName;
                        CurrentBooking.LastName = dbUser.LastName;
                        CurrentBooking.FiscalNumber = dbUser.FiscalNumber;
                        CurrentBooking.Email = dbUser.Email;
                        CurrentBooking.CountyOfBirth = dbUser.CountyOfBirth;
                        CurrentBooking.PlaceOfBirth = dbUser.PlaceOfBirth;
                        CurrentBooking.DateOfBirth = dbUser.DateOfBirth;
                        CurrentBooking.Address = dbUser.Address;
                        CurrentBooking.DomicileMunicipality = dbUser.DomicileMunicipality;
                        CurrentBooking.DomicileNation = dbUser.DomicileNation;
                        CurrentBooking.DomicilePostalCode = dbUser.DomicilePostalCode;
                        CurrentBooking.DomicileProvince = dbUser.DomicileProvince;
                        CurrentBooking.DomicileStreetAddress = dbUser.DomicileStreetAddress;
                        CurrentBooking.Gender = dbUser.Gender;
                        CurrentBooking.MobilePhone = dbUser.MobilePhone;
                        CurrentBooking.Cancellation_IBAN = dbUser.IBAN;
                        CurrentBooking.Cancellation_Banc = dbUser.Bankname;
                        CurrentBooking.Cancellation_Owner = dbUser.KontoInhaber;
                    }

                    if (SessionWrapper.CurrentSubstituteUser != null)
                    {
                        var dbRootUser = await AuthProvider.GetAnagraficByUserID(SessionWrapper.CurrentUser.ID);

                        if (dbRootUser != null)
                        {
                            CurrentBooking.ROOT_AUTH_User_ID = dbRootUser.AUTH_Users_ID;
                            CurrentBooking.ROOT_FirstName = dbRootUser.FirstName;
                            CurrentBooking.ROOT_LastName = dbRootUser.LastName;
                            CurrentBooking.ROOT_FiscalCode = dbRootUser.FiscalNumber;
                            CurrentBooking.ROOT_CountyOfBirth = dbRootUser.CountyOfBirth;
                            CurrentBooking.ROOT_PlaceOfBirth = dbRootUser.PlaceOfBirth;
                            CurrentBooking.ROOT_DateOfBirth = dbRootUser.DateOfBirth;
                            CurrentBooking.ROOT_Address = dbRootUser.Address;
                            CurrentBooking.ROOT_DomicileMunicipality = dbRootUser.DomicileMunicipality;
                            CurrentBooking.ROOT_DomicileNation = dbRootUser.DomicileNation;
                            CurrentBooking.ROOT_DomicilePostalCode = dbRootUser.DomicilePostalCode;
                            CurrentBooking.ROOT_DomicileProvince = dbRootUser.DomicileProvince;
                            CurrentBooking.ROOT_DomicileStreetAddress = dbRootUser.DomicileStreetAddress;
                            CurrentBooking.ROOT_Gender = dbRootUser.Gender;
                            CurrentBooking.ROOT_MobilePhone = dbRootUser.MobilePhone;
                            CurrentBooking.ROOT_Email = dbRootUser.Email;
                            CurrentBooking.ROOT_Phone = dbRootUser.Phone;
                        }

                        if (dbUser != null && dbUser.GV_AUTH_Users_ID != null)
                        {
                            var dbGVUser = await AuthProvider.GetAnagraficByUserID(dbUser.GV_AUTH_Users_ID.Value);

                            if (dbGVUser != null)
                            {
                                CurrentBooking.GV_AUTH_User_ID = dbGVUser.AUTH_Users_ID;
                                CurrentBooking.GV_FirstName = dbGVUser.FirstName;
                                CurrentBooking.GV_LastName = dbGVUser.LastName;
                                CurrentBooking.GV_FiscalCode = dbGVUser.FiscalNumber;
                                CurrentBooking.GV_CountyOfBirth = dbGVUser.CountyOfBirth;
                                CurrentBooking.GV_PlaceOfBirth = dbGVUser.PlaceOfBirth;
                                CurrentBooking.GV_DateOfBirth = dbGVUser.DateOfBirth;
                                CurrentBooking.GV_Address = dbGVUser.Address;
                                CurrentBooking.GV_DomicileMunicipality = dbGVUser.DomicileMunicipality;
                                CurrentBooking.GV_DomicileNation = dbGVUser.DomicileNation;
                                CurrentBooking.GV_DomicilePostalCode = dbGVUser.DomicilePostalCode;
                                CurrentBooking.GV_DomicileProvince = dbGVUser.DomicileProvince;
                                CurrentBooking.GV_DomicileStreetAddress = dbGVUser.DomicileStreetAddress;
                                CurrentBooking.GV_Gender = dbGVUser.Gender;
                                CurrentBooking.GV_MobilePhone = dbGVUser.MobilePhone;
                                CurrentBooking.GV_Email = dbGVUser.Email;
                                CurrentBooking.GV_Phone = dbGVUser.Phone;
                            }
                        }
                    }

                    if (SessionWrapper.CurrentSubstituteUser != null)
                    {
                        CurrentBooking.AUTH_User_ID = SessionWrapper.CurrentSubstituteUser.ID;
                    }
                    else
                    {
                        CurrentBooking.AUTH_User_ID = SessionWrapper.CurrentUser.ID;
                    }

                    CurrentBooking.CreationDate = DateTime.Now;
                    CurrentBooking.ROOM_Booking_Type_ID = ROOMBookingType.External;

                    await RoomProvider.SetBookingGroup(CurrentBooking);

                    foreach (var day in BookingPrices.Where(p => p.ROOM_Option_ID == null).OrderBy(p => p.ROOM_Room_ID).ThenBy(p => p.StartDate).ToList())
                    {
                        if (day.StartDate != null && day.EndDate != null)
                        {
                            var booking = new ROOM_Booking();

                            booking.ID = Guid.NewGuid();
                            booking.AUTH_Municipality_ID = CurrentBooking.AUTH_MunicipalityID;
                            booking.ROOM_BookingGroup_ID = CurrentBooking.ID;
                            booking.ROOM_Room_ID = day.ROOM_Room_ID;
                            booking.CreationDate = DateTime.Now;
                            booking.StartDate = day.StartDate;
                            booking.EndDate = day.EndDate;
                            booking.Price = day.Price;

                            await RoomProvider.SetBooking(booking);
                        }
                    }

                    foreach (var opt in BookingPrices.Where(p => p.ROOM_Option_ID != null).OrderBy(p => p.ROOM_Room_ID).ToList())
                    {
                        var option = new ROOM_BookingOptions();

                        option.ID = Guid.NewGuid();
                        option.ROOM_RoomOption_ID = opt.ROOM_Option_ID;
                        option.ROOM_Room_ID = opt.ROOM_Room_ID;
                        option.CreationDate = DateTime.Now;
                        option.ROOM_BookingGroup_ID = CurrentBooking.ID;
                        option.Description = opt.Description;
                        option.Price = (((decimal?)opt.Price) ?? 0).ToString();

                        await RoomProvider.SetBookingOptions(option);
                    }

                    bool ToPay = false;
                    bool ToSign = false;

                    foreach (var roomID in BookingPrices.Select(p => p.ROOM_Room_ID).Distinct())
                    {
                        var room = await RoomProvider.GetRoom(roomID);

                        if (room != null && room.HasDirectPay == true)
                        {
                            ToPay = true;
                        }
                        if (room != null && room.HasSigning == true)
                        {
                            ToSign = true;
                        }
                    }

                    bool localhost = false;

                    if (NavManager.BaseUri.Contains("localhost"))
                    {
                        localhost = true;
                    }

                    await RoomProvider.CreateBookingStatusLog(CurrentBooking, ROOMStatus.Created);

                    CurrentBooking.ROOM_BookingStatus_ID = ROOMStatus.Created;
                    
                    await RoomProvider.SetBookingGroup(CurrentBooking);


                    var file = await RoomProvider.GetDocument(CurrentBooking.ID, LangProvider.GetCurrentLanguageID(), localhost);

                    if (CurrentBooking != null && file != null)
                    {
                        CurrentBooking.FILE_FileInfo_ID = file.ID;

                        await RoomProvider.SetBookingGroup(CurrentBooking);
                    }

                    if (ToPay)
                    {
                        CurrentBooking.ROOM_BookingStatus_ID = ROOMStatus.ToPay;

                        await RoomProvider.CreateBookingStatusLog(CurrentBooking, ROOMStatus.ToPay);
                        await RoomProvider.SetBookingGroup(CurrentBooking);

                        BusyIndicatorService.IsBusy = true;
                        StateHasChanged();
                        NavManager.NavigateTo("/Room/Payment/" + CurrentBooking.ID);
                    }
                    else if (ToSign)
                    {
                        CurrentBooking.ROOM_BookingStatus_ID = ROOMStatus.ToSign;

                        await RoomProvider.CreateBookingStatusLog(CurrentBooking, ROOMStatus.ToSign);
                        await RoomProvider.SetBookingGroup(CurrentBooking);

                        NavManager.NavigateTo("/Room/Sign/" + CurrentBooking.ID);
                        StateHasChanged();
                    }
                    else
                    {
                        await RoomProvider.SetBookingComitted(CurrentBooking, NavManager.BaseUri, true);

                        //D3! rooms ok
                        Task.Run(async () => await D3Helper.ProtocolRoomBooking(CurrentBooking)).ConfigureAwait(false);
                        /************************/

                        NavManager.NavigateTo("/Room/Comitted/" + CurrentBooking.ID);
                        StateHasChanged();
                    }
                }
            } 
        }
        private void PrivacyChanged()
        {

            if (Privacy != null)
            {
                CurrentBooking.PrivacyBool = !CurrentBooking.PrivacyBool;

                if (CurrentBooking != null && CurrentBooking.PrivacyDate == null)
                {
                    Privacy.PrivacyErrorCSS = "error";
                    IsValid = false;
                }
                else
                {
                    Privacy.PrivacyErrorCSS = null;
                    IsValid = true;
                }

                StateHasChanged();
            }
        }
        public void Dispose()
        {
            if (RoomBookingHelper.TimeFilter != null)
            {
                RoomBookingHelper.TimeFilter.OnDataChanged -= TimeFilter_OnDataChanged;
            }
        }
    }
}
