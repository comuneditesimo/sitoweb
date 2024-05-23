using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Provider;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;
using Stripe;

namespace ICWebApp.Components.Components.Canteen.Frontend
{
    public partial class RequestRefundBalances : IDisposable
    {
        [Inject] IBusyIndicatorService? BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService? BreadCrumbService { get; set; }
        [Inject] ICANTEENProvider? CanteenProvider { get; set; }
        [Inject] IMyCivisService? MyCivisService { get; set; }
        [Inject] ISessionWrapper? SessionWrapper { get; set; }
        [Inject] IPRIVProvider? PrivacyProvider { get; set; }
        [Inject] IAUTHProvider? AuthProvider { get; set; }
        [Inject] NavigationManager? NavManager { get; set; }
        [Inject] ITEXTProvider? TextProvider { get; set; }

        private CANTEEN_RequestRefundBalances? Request;
        private PRIV_Privacy? Privacy;
        private decimal MaxBalance = 0;
        private bool ShowValidationErrors = false;

        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper != null)
            {
                SessionWrapper.OnCurrentUserChanged += OnCurrentUserChanged;
                if (TextProvider != null)
                {
                    SessionWrapper.PageTitle = TextProvider.Get("CANTEEN_REQUESTREFUND_TITLE");
                    SessionWrapper.PageDescription = null;
                }
                if (CanteenProvider != null && SessionWrapper.AUTH_Municipality_ID != null && SessionWrapper.CurrentUser != null && await CanteenProvider.UserHasOpenRequest(SessionWrapper.AUTH_Municipality_ID.Value, SessionWrapper.CurrentUser.ID))
                {
                    ReturnToPreviousPage();
                }
            }

            SetBreadCrumb();
            await CreateNewRequest();
            await GetPrivacyMessage();

            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }

            await base.OnInitializedAsync();
        }
        public async void OnCurrentUserChanged()
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }

            await CreateNewRequest();
            await GetPrivacyMessage();

            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }
        }
        private void SetBreadCrumb()
        {
            if (BreadCrumbService != null)
            {
                BreadCrumbService.ClearBreadCrumb();
                if (MyCivisService != null && MyCivisService.Enabled == true)
                {
                    BreadCrumbService.AddBreadCrumb("/Canteen/MyCivis/Service", "MAINMENU_CANTEEN", null, null);
                    BreadCrumbService.AddBreadCrumb("/Canteen/MyCivis/RequestRefundBalances", "CANTEEN_REQUESTREFUND_TITLE", null, null, false);
                }
                else
                {
                    BreadCrumbService.AddBreadCrumb("/Canteen/Service", "MAINMENU_CANTEEN", null, null);
                    BreadCrumbService.AddBreadCrumb("/Canteen/RequestRefundBalances", "CANTEEN_REQUESTREFUND_TITLE", null, null, false);
                }
            }
        }
        public async Task<CANTEEN_RequestRefundBalances?> CreateNewRequest()
        {
            // Definiere lokale Variablen
            CANTEEN_RequestRefundBalances_Status? _defaultStatus = null;
            CANTEEN_RequestRefundBalances? _balance = null;
            AUTH_Users? _currentUser = null;
            AUTH_Users_Anagrafic? _currentUserAnagrafic = null;
            decimal _currentBalance = 0;
            //Setze die lokalen Variablen
            if (SessionWrapper != null && SessionWrapper.CurrentUser != null)
            {
                _currentUser = SessionWrapper.CurrentUser;
                if (_currentUser != null && AuthProvider != null)
                {
                    _currentUserAnagrafic = await AuthProvider.GetAnagraficByUserID(_currentUser.ID);
                }
            }
            if (_currentUser != null && CanteenProvider != null)
            {
                _defaultStatus = await CanteenProvider.GetDefaultRequestRefundBalanceStatus();
                _currentBalance = CanteenProvider.GetUserBalance(_currentUser.ID);
            }
            // Erstelle einen neuen Antrag
            if (_currentUser != null && _currentBalance > 0 && _currentUser.AUTH_Municipality_ID != null)
            {
                _balance = new CANTEEN_RequestRefundBalances();
                _balance.ID = Guid.NewGuid();
                _balance.AUTH_User_ID = _currentUser.ID;
                if (_currentUserAnagrafic != null)
                {
                    _balance.UserFirstName = _currentUserAnagrafic.FirstName;
                    _balance.UserLastName = _currentUserAnagrafic.LastName;
                    _balance.UserTaxNumber = _currentUserAnagrafic.FiscalNumber;
                    _balance.UserGender = _currentUserAnagrafic.Gender;
                    _balance.UserCountryOfBirth = _currentUserAnagrafic.CountyOfBirth;
                    _balance.UserPlaceOfBirth = _currentUserAnagrafic.PlaceOfBirth;
                    _balance.UserDateOfBirth = _currentUserAnagrafic.DateOfBirth;
                    _balance.UserEmail = _currentUserAnagrafic.Email;
                    _balance.UserMobilePhone = _currentUserAnagrafic.MobilePhone;
                    _balance.UserDomicileEntireAdress = _currentUserAnagrafic.Address;
                    _balance.UserDomicileStreetAdress = _currentUserAnagrafic.DomicileStreetAddress;
                    _balance.UserDomicilePostalCode = _currentUserAnagrafic.DomicilePostalCode;
                    _balance.UserDomicileMunicipality = _currentUserAnagrafic.DomicileMunicipality;
                    _balance.UserDomicileProvince = _currentUserAnagrafic.DomicileProvince;
                    _balance.UserDomicileNation = _currentUserAnagrafic.DomicileNation;
                }
                _balance.AUTH_Municipality_ID = _currentUser.AUTH_Municipality_ID;
                _balance.Date = DateTime.Now;
                _balance.Fee = _currentBalance;
                if (_defaultStatus != null)
                {
                    _balance.CANTEEN_RequestRefundAllCharge_Status_ID = _defaultStatus.ID;
                }
            }
            Request = _balance;
            MaxBalance = _currentBalance;
            StateHasChanged();

            return Request;
        }
        public async Task SaveRequest()
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }
            if (Request != null && CanteenProvider != null)
            {
                Request = await CanteenProvider.SetRequestRefundBalance(Request);
                if (Request != null)
                {
                    await CanteenProvider.CreateRequestRefundBalanceStatusLogEntry(Request);
                }
                if (NavManager != null && Request != null && Request.ID != Guid.Empty && MyCivisService != null && MyCivisService.Enabled == true)
                {
                    NavManager.NavigateTo("/Canteen/MyCivis/SignRefundBalances/" + Request.ID);
                }
                else if (NavManager != null && Request != null && Request.ID != Guid.Empty)
                {
                    NavManager.NavigateTo("/Canteen/SignRefundBalances/" + Request.ID);
                }
                else
                {
                    ReturnToPreviousPage();
                }
            }

            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }
        }
        public void SetValidation()
        {
            ShowValidationErrors = true;
            StateHasChanged();
        }
        public async Task<PRIV_Privacy?> GetPrivacyMessage()
        {
            PRIV_Privacy? _privacy = null;
            if (SessionWrapper != null && PrivacyProvider != null)
            {
                Guid? municipalityID = await SessionWrapper.GetMunicipalityID();

                if (municipalityID != null && municipalityID != Guid.Empty)
                {
                    _privacy = await PrivacyProvider.GetPrivacy(municipalityID.Value);
                }
            }
            Privacy = _privacy;
            StateHasChanged();
            return Privacy;
        }
        private void ReturnToPreviousPage()
        {
            if (NavManager != null)
            {
                if (MyCivisService != null && MyCivisService.Enabled == true)
                {
                    NavManager.NavigateTo("/Canteen/MyCivis/Service");
                }
                else
                {
                    NavManager.NavigateTo("/Canteen/Service");
                }
            }
        }
        public void Dispose()
        {
            if (SessionWrapper != null)
            {
                SessionWrapper.OnCurrentUserChanged -= OnCurrentUserChanged;
            }
        }
    }
}
