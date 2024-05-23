using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Homepage.Request;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Schedule;
using Telerik.Blazor;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;

namespace ICWebApp.Components.Pages.Homepage.Backend.Person.Requests
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ICalendarHelper CalendarHelper { get; set; }
        [Inject] IMailerService MailerService { get; set; }
        [Inject] ISMSService SMSService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }

        private bool IsDataBusy { get; set; } = true;
        private List<V_HOME_Person_Request> Data = new List<V_HOME_Person_Request>();
        private List<V_HOME_Person> People = new List<V_HOME_Person>();
        private List<Guid> SelectedPeople = new List<Guid>();
        private List<HOME_Person_Dates_Timeslots> Timeslots = new List<HOME_Person_Dates_Timeslots>();
        private List<HOME_Person_Dates_Closed> ClosedDates = new List<HOME_Person_Dates_Closed>();
        private V_HOME_Person? SelectedPerson;
        private List<Timeslot> OpenSlots = new List<Timeslot>();
        private List<MonthItem> MonthItems = new List<MonthItem>();
        private bool EditWindowVisible = false;
        private HOME_Person_Request? CurrentRequest;
        private int Month = 0;
        private int? Day;
        private bool IsRequestNew = false;

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_PERSON_APPOINTMENTS");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Person/Appointments", "MAINMENU_BACKEND_PERSON_APPOINTMENTS", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null && SessionWrapper.CurrentMunicipalUser != null)
            {
                People = await HomeProvider.GetPeopleWithTimeslots(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            await GetData();

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async Task<bool> GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var data = await HomeProvider.GetPersonRequests(SessionWrapper.AUTH_Municipality_ID.Value);

                if (SelectedPeople != null && SelectedPeople.Any())
                {
                    Data = data.Where(p => p.HOME_Person_ID != null && SelectedPeople.Contains(p.HOME_Person_ID.Value)).ToList();
                }
                else
                {
                    Data = data.Where(p => p.HOME_Person_ID != null && People.Select(x => x.ID).Contains(p.HOME_Person_ID.Value)).ToList();
                }
            }

            StateHasChanged();
            return true;
        }
        private void CloseEditWindow()
        {
            EditWindowVisible = false;
            StateHasChanged();
        }
        private async void ActionBegin(ActionEventArgs<V_HOME_Person_Request> args)
        {
            if(args.ActionType == ActionType.EventRemove)
            {
                args.Cancel = true;

                foreach (var item in args.DeletedRecords)
                {
                    if (item != null)
                    {
                        if (!string.IsNullOrEmpty(item.EMail))
                        {
                            SendCancelMail(item.EMail, item);
                        }
                        if (!string.IsNullOrEmpty(item.Phone))
                        {
                            SendCancelSms(item.Phone, item);
                        }
                        
                        await HomeProvider.RemovePersonRequest(item.ID);
                    }
                }

                await GetData();
            }
            else if(args.ActionType == ActionType.EventChange ||
                    args.ActionType == ActionType.EventCreate)
            {
                args.Cancel = true;
            }
        }
        private void CellClick(CellClickEventArgs args)
        {
            args.Cancel = true;
        }
        private void EventClick(EventClickArgs<V_HOME_Person_Request> args)
        {
            args.Cancel = true;
        }
        private void CellDoubleClick(CellClickEventArgs args)
        {
            args.Cancel = true;
            IsDataBusy = true;
            StateHasChanged();

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                CurrentRequest = new HOME_Person_Request();

                CurrentRequest.ID = Guid.NewGuid();
                CurrentRequest.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;
                CurrentRequest.CreationDate = DateTime.Now;

                IsRequestNew = true;
                IsDataBusy = false;
                EditWindowVisible = true;
                StateHasChanged();
            }
        }
        private async void EventDoubleClick(EventClickArgs<V_HOME_Person_Request> args)
        {
            args.Cancel = true;

            var app = args.Event;

            CurrentRequest = await HomeProvider.GetPersonRequest(app.ID);

            if (CurrentRequest != null)
            {
                SelectedPerson = People.FirstOrDefault(p => p.ID == CurrentRequest.HOME_Person_ID);

                PersonChanged();

                if (CurrentRequest.DateFrom != null && CurrentRequest.DateTo != null)
                {
                    Month = CurrentRequest.DateFrom.Value.Month;
                    Day = CurrentRequest.DateFrom.Value.Day;
                }
            }

            IsRequestNew = false;
            EditWindowVisible = true;
            StateHasChanged();
        }
        private async void EventSave()
        {
            if (!await Dialogs.ConfirmAsync(TextProvider.Get("HOME_BACKEND_REQUEST_ARE_YOU_SURE_CHANGE"), TextProvider.Get("INFORMATION")))
                return;

            EditWindowVisible = false;
            IsDataBusy = true;
            StateHasChanged();

            if (CurrentRequest != null && SelectedPerson != null)
            {
                var dbRequest = await HomeProvider.GetPersonRequest(CurrentRequest.ID);

                if (OpenSlots != null && OpenSlots.Any())
                {
                    var tm = OpenSlots.FirstOrDefault(p => p.Selected == true);

                    if (Data != null && tm != null)
                    {
                        CurrentRequest.DateFrom = tm.DateFrom;
                        CurrentRequest.DateTo = tm.DateTo;
                    }
                }

                CurrentRequest.Address = SelectedPerson.Address;

                if (CurrentRequest.AUTH_Users_ID == null && !string.IsNullOrEmpty(CurrentRequest.Fiscalnumber))
                {
                    var user = await AuthProvider.GetUser(CurrentRequest.Fiscalnumber);

                    if (user != null)
                    {
                        CurrentRequest.AUTH_Users_ID = user.ID;
                    }
                }

                await HomeProvider.SetPersonRequest(CurrentRequest);

                if (IsRequestNew)
                {
                    if (!string.IsNullOrEmpty(CurrentRequest.EMail))
                    {
                        SendMail(CurrentRequest.EMail);
                    }
                    if (!string.IsNullOrEmpty(CurrentRequest.Phone))
                    {
                        SendSms(CurrentRequest.Phone);
                    }
                }
                else
                {
                    if (dbRequest != null && (
                                              dbRequest.HOME_Person_ID != CurrentRequest.HOME_Person_ID ||
                                              dbRequest.Address != CurrentRequest.Address || 
                                              dbRequest.DateFrom != CurrentRequest.DateFrom || 
                                              dbRequest.DateTo != CurrentRequest.DateTo
                                             )
                       )
                    {
                        if (!string.IsNullOrEmpty(CurrentRequest.EMail))
                        {
                            SendChangeMail(CurrentRequest.EMail);
                        }
                        if (!string.IsNullOrEmpty(CurrentRequest.Phone))
                        {
                            SendChangeSms(CurrentRequest.Phone);
                        }
                    }
                }
            }

            OpenSlots = new List<Timeslot>();
            SelectedPerson = null;
            CurrentRequest = null;
            await GetData();

            IsDataBusy = false;
            StateHasChanged();
        }
        private async void ClearTagFilter()
        {
            if (SelectedPeople != null)
            {
                SelectedPeople = new List<Guid>();
                await GetData();
            }
        }
        private async void AddFilter(Guid HOME_Person_ID)
        {

            if (SelectedPeople == null)
                SelectedPeople = new List<Guid>();

            if (SelectedPeople.Contains(HOME_Person_ID))
            {
                SelectedPeople.Remove(HOME_Person_ID);
            }
            else
            {
                SelectedPeople.Add(HOME_Person_ID);
            }

            await GetData();
        }
        private async void PersonChanged()
        {
            if (CurrentRequest != null && CurrentRequest.HOME_Person_ID != null)
            {
                SelectedPerson = await HomeProvider.GetVPerson(CurrentRequest.HOME_Person_ID.Value, LangProvider.GetCurrentLanguageID());

                if (SelectedPerson != null)
                {
                    Timeslots = await HomeProvider.GetPersonTimes(SelectedPerson.ID);
                    ClosedDates = await HomeProvider.GetPersonClosedDates(SelectedPerson.ID);

                    AvailableTimeslots();
                }
            }
            else
            {
                SelectedPerson = null;
            }

            StateHasChanged();
        }
        public async void AvailableTimeslots()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && SelectedPerson != null)
            {
                var allDates = new List<DateTime>();
                var aktiveDates = new List<DateTime>();
                OpenSlots = new List<Timeslot>();
                DateTime StartDate = DateTime.Today;
                DateTime EndDate = DateTime.Today.AddDays(90);

                if (SelectedPerson != null)
                {
                    StartDate = DateTime.Today.AddDays(SelectedPerson.MinDaysAheadOfBooking);
                    EndDate = DateTime.Today.AddDays(SelectedPerson.MaxDaysBookable);
                }

                for (var dt = StartDate; dt <= EndDate; dt = dt.AddDays(1))
                {
                    allDates.Add(dt);
                }

                foreach (var t in Timeslots.Select(p => p.Weekday).Distinct().ToList())
                {
                    if (t == 0)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Monday).ToList());
                    }
                    else if (t == 1)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Tuesday).ToList());
                    }
                    else if (t == 2)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Wednesday).ToList());
                    }
                    else if (t == 3)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Thursday).ToList());
                    }
                    else if (t == 4)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Friday).ToList());
                    }
                    else if (t == 5)
                    {
                        aktiveDates.AddRange(allDates.Where(p => p.DayOfWeek == DayOfWeek.Saturday).ToList());
                    }
                }

                aktiveDates = aktiveDates.Where(p => ClosedDates.FirstOrDefault(x => x.Date == p) == null).ToList();

                foreach (var t in aktiveDates)
                {
                    var slotsInDay = Timeslots.Where(p => p.DayOfweek != null && p.DayOfweek == t.DayOfWeek).ToList();


                    foreach (var slot in slotsInDay)
                    {
                        if (slot.TimeFrom != null && slot.TimeTo != null)
                        {
                            var DateFrom = new DateTime(t.Year, t.Month, t.Day, slot.TimeFrom.Value.Hour, slot.TimeFrom.Value.Minute, 0);
                            var DateTo = new DateTime(t.Year, t.Month, t.Day, slot.TimeTo.Value.Hour, slot.TimeTo.Value.Minute, 0);

                            var existing = await HomeProvider.GetPersonRequest(SessionWrapper.AUTH_Municipality_ID.Value, SelectedPerson.ID, DateFrom);

                            if (existing == null)
                            {
                                OpenSlots.Add(new Timeslot()
                                {
                                    DateFrom = DateFrom,
                                    DateTo = DateTo
                                });
                            }
                        }
                    }
                }

                if (CurrentRequest != null && CurrentRequest.DateFrom != null && CurrentRequest.DateTo != null)
                {
                    OpenSlots.Add(new Timeslot()
                    {
                        DateFrom = CurrentRequest.DateFrom.Value,
                        DateTo = CurrentRequest.DateTo.Value,
                        Selected = true
                    });
                }

                MonthItems = new List<MonthItem>();

                foreach (var slot in OpenSlots)
                {
                    if (!MonthItems.Select(p => p.MonthInt).Contains(slot.MonthInt))
                    {
                        MonthItems.Add(new MonthItem()
                        {
                            Month = slot.Month,
                            MonthInt = slot.MonthInt,
                            YearMonth = slot.DateFrom.Value.ToString("yyyyMM")
                        });
                    }
                }

                StateHasChanged();
            }
        }
        private void ValueChanged()
        {
            StateHasChanged();
        }
        private void SlotChanged(bool value, Timeslot slot)
        {
            if (slot.Selected != true)
            {
                foreach (var s in OpenSlots)
                {
                    s.Selected = false;
                }

                slot.Selected = value;
            }
            StateHasChanged();
        }
        private async void SendMail(string EMail)
        {
            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = EMail;

            mail.Subject = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_TITLE");
            mail.Body = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_CONTENT");

            if (CurrentRequest != null && SelectedPerson != null && CurrentRequest.DateFrom != null && CurrentRequest.DateTo != null)
            {
                mail.Body = mail.Body.Replace("{Firstname}", CurrentRequest.Firstname);
                mail.Body = mail.Body.Replace("{Lastname}", CurrentRequest.Lastname);
                mail.Body = mail.Body.Replace("{Fiscalcode}", CurrentRequest.Fiscalnumber);

                mail.Body = mail.Body.Replace("{EMail}", CurrentRequest.EMail);
                mail.Body = mail.Body.Replace("{Phone}", CurrentRequest.Phone);

                mail.Body = mail.Body.Replace("{Person}", SelectedPerson.Fullname);
                mail.Body = mail.Body.Replace("{Address}", CurrentRequest.Address);
                mail.Body = mail.Body.Replace("{Date}", CurrentRequest.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + CurrentRequest.DateTo.Value.ToString("HH:mm"));

                var path = CalendarHelper.GetFilePath(CurrentRequest.ID, CurrentRequest.DateFrom.Value, CurrentRequest.DateTo.Value, SelectedPerson.Fullname, Location: CurrentRequest.Address);

                string newpath = Path.Combine(NavManager.BaseUri, path);

                mail.Body = mail.Body.Replace("{Link}", newpath);

                mail.Body = mail.Body.Replace("{Details}", CurrentRequest.Details);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await MailerService.SendMail(mail, null, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
        private async void SendSms(string Phone)
        {
            MSG_SMS sms = new MSG_SMS();

            sms.PhoneNumber = Phone;

            sms.Message = TextProvider.Get("HOME_REQUEST_PERSON_SMS_CONTENT");

            if (CurrentRequest != null && SelectedPerson != null && CurrentRequest.DateFrom != null && CurrentRequest.DateTo != null)
            {
                sms.Message = sms.Message.Replace("{Firstname}", CurrentRequest.Firstname);
                sms.Message = sms.Message.Replace("{Lastname}", CurrentRequest.Lastname);
                sms.Message = sms.Message.Replace("{Fiscalcode}", CurrentRequest.Fiscalnumber);

                sms.Message = sms.Message.Replace("{EMail}", CurrentRequest.EMail);
                sms.Message = sms.Message.Replace("{Phone}", CurrentRequest.Phone);

                sms.Message = sms.Message.Replace("{Person}", SelectedPerson.Fullname);
                sms.Message = sms.Message.Replace("{Address}", CurrentRequest.Address);
                sms.Message = sms.Message.Replace("{Date}", CurrentRequest.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + CurrentRequest.DateTo.Value.ToString("HH:mm"));

                var path = CalendarHelper.GetFilePath(CurrentRequest.ID, CurrentRequest.DateFrom.Value, CurrentRequest.DateTo.Value, SelectedPerson.Fullname, Location: CurrentRequest.Address);

                string newpath = Path.Combine(NavManager.BaseUri, path);

                sms.Message = sms.Message.Replace("{Link}", newpath);

                sms.Message = sms.Message.Replace("{Details}", CurrentRequest.Details);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await SMSService.SendSMS(sms, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
        private async void SendChangeMail(string EMail)
        {
            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = EMail;

            mail.Subject = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_TITLE");
            mail.Body = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_CONTENT");

            if (CurrentRequest != null && SelectedPerson != null && CurrentRequest.DateFrom != null && CurrentRequest.DateTo != null)
            {
                mail.Body = mail.Body.Replace("{Firstname}", CurrentRequest.Firstname);
                mail.Body = mail.Body.Replace("{Lastname}", CurrentRequest.Lastname);
                mail.Body = mail.Body.Replace("{Fiscalcode}", CurrentRequest.Fiscalnumber);

                mail.Body = mail.Body.Replace("{EMail}", CurrentRequest.EMail);
                mail.Body = mail.Body.Replace("{Phone}", CurrentRequest.Phone);

                mail.Body = mail.Body.Replace("{Person}", SelectedPerson.Fullname);
                mail.Body = mail.Body.Replace("{Address}", CurrentRequest.Address);
                mail.Body = mail.Body.Replace("{Date}", CurrentRequest.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + CurrentRequest.DateTo.Value.ToString("HH:mm"));

                var path = CalendarHelper.GetFilePath(CurrentRequest.ID, CurrentRequest.DateFrom.Value, CurrentRequest.DateTo.Value, SelectedPerson.Fullname, Location: CurrentRequest.Address);

                string newpath = Path.Combine(NavManager.BaseUri, path);

                mail.Body = mail.Body.Replace("{Link}", newpath);

                mail.Body = mail.Body.Replace("{Details}", CurrentRequest.Details);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await MailerService.SendMail(mail, null, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
        private async void SendChangeSms(string Phone)
        {
            MSG_SMS sms = new MSG_SMS();

            sms.PhoneNumber = Phone;

            sms.Message = TextProvider.Get("HOME_REQUEST_PERSON_SMS_CONTENT");

            if (CurrentRequest != null && SelectedPerson != null && CurrentRequest.DateFrom != null && CurrentRequest.DateTo != null)
            {
                sms.Message = sms.Message.Replace("{Firstname}", CurrentRequest.Firstname);
                sms.Message = sms.Message.Replace("{Lastname}", CurrentRequest.Lastname);
                sms.Message = sms.Message.Replace("{Fiscalcode}", CurrentRequest.Fiscalnumber);

                sms.Message = sms.Message.Replace("{EMail}", CurrentRequest.EMail);
                sms.Message = sms.Message.Replace("{Phone}", CurrentRequest.Phone);

                sms.Message = sms.Message.Replace("{Person}", SelectedPerson.Fullname);
                sms.Message = sms.Message.Replace("{Address}", CurrentRequest.Address);
                sms.Message = sms.Message.Replace("{Date}", CurrentRequest.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + CurrentRequest.DateTo.Value.ToString("HH:mm"));

                var path = CalendarHelper.GetFilePath(CurrentRequest.ID, CurrentRequest.DateFrom.Value, CurrentRequest.DateTo.Value, SelectedPerson.Fullname, Location: CurrentRequest.Address);

                string newpath = Path.Combine(NavManager.BaseUri, path);

                sms.Message = sms.Message.Replace("{Link}", newpath);

                sms.Message = sms.Message.Replace("{Details}", CurrentRequest.Details);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await SMSService.SendSMS(sms, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
        private async void SendCancelMail(string EMail, V_HOME_Person_Request request)
        {
            MSG_Mailer mail = new MSG_Mailer();

            mail.ToAdress = EMail;

            mail.Subject = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_CANCEL_TITLE");

            mail.Body = TextProvider.Get("HOME_REQUEST_PERSON_MAIL_CANCEL_CONTENT");

            if (request != null && request.DateFrom != null && request.DateTo != null)
            {
                mail.Body = mail.Body.Replace("{Firstname}", request.Firstname);
                mail.Body = mail.Body.Replace("{Lastname}", request.Lastname);
                mail.Body = mail.Body.Replace("{Fiscalcode}", request.Fiscalnumber);

                mail.Body = mail.Body.Replace("{EMail}", request.EMail);
                mail.Body = mail.Body.Replace("{Phone}", request.Phone);

                var person = People.FirstOrDefault(p => p.ID == request.HOME_Person_ID);

                if (person != null)
                {
                    mail.Body = mail.Body.Replace("{Person}", person.Fullname);
                }

                mail.Body = mail.Body.Replace("{Address}", request.Address);
                mail.Body = mail.Body.Replace("{Date}", request.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + request.DateTo.Value.ToString("HH:mm"));

                mail.Body = mail.Body.Replace("{Details}", request.Details);
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await MailerService.SendMail(mail, null, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
        private async void SendCancelSms(string Phone, V_HOME_Person_Request request)
        {
            MSG_SMS sms = new MSG_SMS();

            sms.PhoneNumber = Phone;

            sms.Message = TextProvider.Get("HOME_REQUEST_PERSON_SMS_CANCEL_CONTENT");

            if (request != null && request.DateFrom != null && request.DateTo != null)
            {
                sms.Message = sms.Message.Replace("{Firstname}", request.Firstname);
                sms.Message = sms.Message.Replace("{Lastname}", request.Lastname);
                sms.Message = sms.Message.Replace("{Fiscalcode}", request.Fiscalnumber);

                sms.Message = sms.Message.Replace("{EMail}", request.EMail);
                sms.Message = sms.Message.Replace("{Phone}", request.Phone);

                var person = People.FirstOrDefault(p => p.ID == request.HOME_Person_ID);

                if (person != null)
                {
                    sms.Message = sms.Message.Replace("{Person}", person.Fullname);
                }

                sms.Message = sms.Message.Replace("{Address}", request.Address);
                sms.Message = sms.Message.Replace("{Date}", request.DateFrom.Value.ToString("dddd, dd. MMMM yyyy    HH:mm") + " - " + request.DateTo.Value.ToString("HH:mm"));

                sms.Message = sms.Message.Replace("{Details}", request.Details);
            }



            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                await SMSService.SendSMS(sms, SessionWrapper.AUTH_Municipality_ID.Value);
            }
        }
    }
}
