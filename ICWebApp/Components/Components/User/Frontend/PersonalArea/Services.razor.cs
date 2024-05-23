using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using Syncfusion.Blazor.Popups;
using System.Linq;
using System.Threading.Tasks;
using Telerik.Blazor;
using ICWebApp.Domain.Enums.Homepage.Settings;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.Models.Homepage.Services;


namespace ICWebApp.Components.Components.User.Frontend.PersonalArea
{
    public partial class Services
    {
        [Inject] IChatNotificationService ChatNotificationService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IFORMApplicationProvider FormApplicationProvider { get; set; }
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBookingService BookingService { get; set; }
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] IRoomProvider RoomProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IORGProvider OrgProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        [Inject] public SfDialogService Dialogs { get; set; }

        private List<ServiceDataItem> ServiceListe = new List<ServiceDataItem>();
        private bool IsBusy = true;

        protected override async void OnParametersSet()
        {
            IsBusy = true;
            StateHasChanged();

            AnchorService.ClearAnchors();
            ServiceListe.Clear();

            await GetApplicationData();
            await GetManteinanceData();
            await GetMensaData();
            await GetBookingData();
            await GetRequestData();

            IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
        private async Task GetApplicationData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var data = await FormApplicationProvider.GetApplicationsPersonalArea(SessionWrapper.AUTH_Municipality_ID.Value, GetCurrentUserID(), LangProvider.GetCurrentLanguageID(), FORMCategories.Applications);

                foreach (var d in data)
                {
                    if (!ServiceListe.Select(p => p.ServiceItemID).Contains(d.FORM_Definition_ID))
                    {
                        ServiceListe.Add(new ServiceDataItem()
                        {
                            Title = d.FormName,
                            Description = d.ShortText,
                            ServiceItemID = d.FORM_Definition_ID,
                            DetailAction = (() => GoToUrl("/Form/Detail/" + d.FORM_Definition_ID))
                        });
                    }
                }
            }

            return;
        }
        private async Task GetManteinanceData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var data = await FormApplicationProvider.GetMantainancesPersonalArea(SessionWrapper.AUTH_Municipality_ID.Value, GetCurrentUserID(), LangProvider.GetCurrentLanguageID(), FORMCategories.Maintenance);

                if (data != null && data.Any()) 
                {                    
                    var serviceItem = new ServiceDataItem()
                    {
                        Title = TextProvider.Get("FRONTEND_MANTAINANCE_TITLE"),
                        ServiceItemID = null,
                        DetailAction = (() => GoToUrl("/Mantainance"))
                    };

                    if (SessionWrapper.AUTH_Municipality_ID != null)
                    {
                        var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Maintenance);

                        if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                        {
                            serviceItem.Description = pagesubTitle.Description;
                        }
                        else
                        {
                            serviceItem.Description = TextProvider.Get("HOMEPAGE_FRONTEND_MAINTENANCE_DESCRIPTION");
                        }
                    }

                    ServiceListe.Add(serviceItem);
                }
            }

            return;
        }
        private async Task GetMensaData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Guid _language = LanguageSettings.German;

                if (LangProvider != null)
                {
                    LANG_Languages? _actualLanguage = LangProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

                    if (_actualLanguage != null)
                    {
                        _language = _actualLanguage.ID;
                    }
                }

                List<V_CANTEEN_RequestRefundBalances> _requests = await CanteenProvider.GetRequestRefundBalances(SessionWrapper.AUTH_Municipality_ID.Value, GetCurrentUserID(), _language);

                if (SessionWrapper.CurrentUser != null)
                {
                    var subscribers = await CanteenProvider.GetVSubscribersByUserID(SessionWrapper.CurrentUser.ID, LangProvider.GetCurrentLanguageID());

                    if((_requests != null && _requests.Any()) || (subscribers != null && subscribers.Any()))
                    {
                        var serviceItem = new ServiceDataItem()
                        {
                            Title = TextProvider.Get("MAINMENU_CANTEEN"),
                            ServiceItemID = null,
                            DetailAction = (() => GoToUrl("/Canteen"))
                        };

                        if (SessionWrapper.AUTH_Municipality_ID != null)
                        {
                            var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Canteen);

                            if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                            {
                                serviceItem.Description = pagesubTitle.Description;
                            }
                            else
                            {
                                serviceItem.Description = TextProvider.Get("MAINMENU_CANTEEN_SERVICE_DESCRIPTION");
                            }
                        }

                        ServiceListe.Add(serviceItem);
                    }
                }
            }

            return;
        }
        private async Task GetBookingData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var Bookings = await RoomProvider.GetBookingGroupByUser(SessionWrapper.CurrentUser.ID);
                var BackendBookings = await RoomProvider.GetBookingGroupBackendByUser(SessionWrapper.CurrentUser.ID);

                if (SessionWrapper.CurrentSubstituteUser != null)
                {
                    Bookings = await RoomProvider.GetBookingGroupByUser(SessionWrapper.CurrentSubstituteUser.ID);
                    BackendBookings = await RoomProvider.GetBookingGroupBackendByUser(SessionWrapper.CurrentSubstituteUser.ID);
                }

                if ((Bookings != null && Bookings.Any()) || (BackendBookings != null && BackendBookings.Any()))
                {
                    var item = new ServiceDataItem()
                    {
                        Title = TextProvider.Get("FRONTEND_BOOKING_TITLE"),
                        Description = TextProvider.Get("MAINMENU_ROOMS_SERVICE_DESCRIPTION"),
                        ServiceItemID = null,
                        DetailAction = (() => GoToUrl("/Rooms/Landing"))
                    };

                    var pagesubTitle = AuthProvider.GetCurrentPageSubTitle(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID() , (int)PageSubtitleType.Rooms);

                    if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                    {
                        item.Description = pagesubTitle.Description;
                    }
                    else
                    {
                        item.Description = TextProvider.Get("MAINMENU_ROOMS_SERVICE_DESCRIPTION");
                    }

                    ServiceListe.Add(item);
                }
            }

            return;
        }
        private async Task GetRequestData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var requests = await HomeProvider.GetPersonUserRequest(SessionWrapper.CurrentUser.ID);
                var requestsPerson = await HomeProvider.GetUserRequest(SessionWrapper.CurrentUser.ID);

                if (requests != null && requests.Any())
                {
                    var item = new ServiceDataItem()
                    {
                        Title = TextProvider.Get("HOMEPAGE_REQUEST_PERSON_APPOINTMENT_TITLE"),
                        Description = TextProvider.Get("HOMEPAGE_REQUEST_PERSON_APPOINTMENT_DESCRIPTION"),
                        ServiceItemID = null,
                        DetailAction = (() => GoToUrl("/Hp/Person/Request"))
                    };

                    ServiceListe.Add(item);
                }
                if (requestsPerson != null && requestsPerson.Any())
                {
                    var item = new ServiceDataItem()
                    {
                        Title = TextProvider.Get("HOMEPAGE_REQUEST_APPOINTMENT_TITLE"),
                        Description = TextProvider.Get("HOMEPAGE_REQUEST_APPOINTMENT_DESCRIPTION"),
                        ServiceItemID = null,
                        DetailAction = (() => GoToUrl("/Hp/Request"))
                    };

                    ServiceListe.Add(item);
                }
            }

            return;
        }
        private void GoToUrl(string Url)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo(Url);
            StateHasChanged();
        }
        private Guid GetCurrentUserID()
        {
            Guid CurrentUserID = SessionWrapper.CurrentUser.ID;

            if (SessionWrapper.CurrentSubstituteUser != null)
            {
                CurrentUserID = SessionWrapper.CurrentSubstituteUser.ID;
            }

            return CurrentUserID;
        }
    }
}
