using Blazored.LocalStorage;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class SessionWrapper : ISessionWrapper
    {
        private string _login_UserID;
        private string _login_AuthToken;
        private string _login_AuthTimestamp;
        private bool _pageIsRendered = false;
        private bool _pageIsPublic = true;
        private bool _initializing = true;
        private bool _showTitleSepparation = true;
        private bool _showTitleSmall = false;
        private bool _showBreadcrumb = true;
        private AUTH_Users? _currentUser;
        private AUTH_Municipal_Users? _currentMunicipalUser;
        private AUTH_Users? _currentSubstituteUser;
        private string _pageTitle;
        private string _pageSubTitle;
        private string _pageDescription;
        private string? _pageChipValue;
        private DateTime? _releaseDate;
        private int? _readDuration;
        public event Action OnLanguageChanged;
        public Guid? _aUTH_Municipality_ID;
        public Guid? _sessionkeyID;
        public Action? _pageButtonAction = null;
        public string? _pageButtonActionTitle = null;
        public Action? _pageButtonSecondaryAction = null;
        public string? _pageButtonSecondaryActionTitle = null;
        public string? _pageButtonDataElement = null;
        public string? _pageButtonSecondaryDataElement = null;
        public bool _showHelpSection = false;
        public bool _showQuestioneer = true;
        public string? _dataElement;
        public event Action ShowQuestioneerChanged;
        public event Action OnShowHelpSectionChanged;
        public event Action OnPageTitleChanged;
        public event Action OnPageChipValueChanged;
        public event Action OnPageSubTitleChanged;
        public event Action OnAUTHMunicipalityIDChanged;
        public event Action OnCurrentUserChanged;
        public event Action OnCurrentMunicipalUserChanged;
        public event Action OnInitialized;
        public event Action OnCurrentSubUserChanged;
        public event Action OnPageDescriptionChanged;
        public event Action OnShowBreadcrumbChanged;
        public event Action OnDataElementChanged;
        public event Action OnReleaseChanged;

        private List<AUTH_MunicipalityApps> _MunicipalityApps;
        public List<AUTH_MunicipalityApps> MunicipalityApps
        {
            get
            {
                return _MunicipalityApps;
            }
            set
            {
                _MunicipalityApps = value;
            }
        }

        private Blazored.SessionStorage.ISessionStorageService sessionStorage;
        private ILocalStorageService localStorageService;
        private IUnitOfWork unitOfWork;

        public SessionWrapper(Blazored.SessionStorage.ISessionStorageService sessionStorage, ILocalStorageService localStorageService, IUnitOfWork unitOfWork)
        {
            this.sessionStorage = sessionStorage;
            this.localStorageService = localStorageService;
            this.unitOfWork = unitOfWork;
        }

        public AUTH_Users? CurrentUser
        {
            get
            {
                return _currentUser;
            }
            set
            {
                if (_currentUser != null && value != null)
                {
                    if (_currentUser.ID != value.ID)
                    {
                        _currentUser = value;

                        if (unitOfWork != null && _currentUser != null && unitOfWork.Auth_Users_ID != _currentUser.ID)
                        {
                            unitOfWork.SetAuditLogdata(_currentUser.ID, _currentUser.AUTH_Municipality_ID);
                        }

                        NotifyCurrentUserChanged();
                    }
                }
                else if (CurrentUser != value)
                {
                    _currentUser = value;

                    if (unitOfWork != null && _currentUser != null && unitOfWork.Auth_Users_ID != _currentUser.ID)
                    {
                        unitOfWork.SetAuditLogdata(_currentUser.ID, _currentUser.AUTH_Municipality_ID);
                    }

                    NotifyCurrentUserChanged();
                }
            }
        }
        public AUTH_Municipal_Users? CurrentMunicipalUser
        {
            get
            {
                return _currentMunicipalUser;
            }
            set
            {
                if (_currentMunicipalUser != null && value != null)
                {
                    if (_currentMunicipalUser.ID != value.ID)
                    {
                        _currentMunicipalUser = value;

                        if (unitOfWork != null && _currentMunicipalUser != null && unitOfWork.Auth_Users_ID != _currentMunicipalUser.ID)
                        {
                            unitOfWork.SetAuditLogdata(_currentMunicipalUser.ID, _currentMunicipalUser.AUTH_Municipality_ID);
                        }

                        NotifyCurrentMunicipalUserChanged();
                    }
                }
                else if (_currentMunicipalUser != value)
                {
                    _currentMunicipalUser = value;

                    if (unitOfWork != null && _currentMunicipalUser != null && unitOfWork.Auth_Users_ID != _currentMunicipalUser.ID)
                    {
                        unitOfWork.SetAuditLogdata(_currentMunicipalUser.ID, _currentMunicipalUser.AUTH_Municipality_ID);
                    }

                    NotifyCurrentMunicipalUserChanged();
                }
            }
        }
        public AUTH_Users? CurrentSubstituteUser
        {
            get
            {
                return _currentSubstituteUser;
            }
            set
            {
                _currentSubstituteUser = value;
                NotifyCurrentSubUserChanged();
            }
        }
        public string Login_AuthToken { get => _login_AuthToken; set => _login_AuthToken = value; }
        public string Login_UserID { get => _login_UserID; set => _login_UserID = value; }
        public bool PageIsRendered { get => _pageIsRendered; set => _pageIsRendered = value; }
        public string Login_AuthTimestamp { get => _login_AuthTimestamp; set => _login_AuthTimestamp = value; }
        public bool PageIsPublic { get => _pageIsPublic; set => _pageIsPublic = value; }
        public bool Initializing
        {
            get => _initializing;
            set
            {
                _initializing = value;

                if (_initializing == false)
                {
                    NotifyOnInitialized();
                }
            }
        }
        public Guid? AUTH_Municipality_ID
        {
            get
            {
                return _aUTH_Municipality_ID;
            }
            set
            {
                _aUTH_Municipality_ID = value;

                if (unitOfWork != null && CurrentUser == null)
                {
                    unitOfWork.SetAuditLogdata(null, _aUTH_Municipality_ID);
                }

                NotifyMunicipalityChanged();
            }
        }
        public string PageTitle
        {
            get
            {
                return _pageTitle;
            }
            set
            {
                _pageTitle = value;
                NotifyPageTitleChanged();
            }
        }
        public string PageSubTitle
        {
            get
            {
                return _pageSubTitle;
            }
            set
            {
                _pageSubTitle = value;
                NotifyPageSubTitleChanged();
            }
        }
        public Action? PageButtonAction
        {
            get
            {
                return _pageButtonAction;
            }
            set
            {
                _pageButtonAction = value;
                NotifyPageTitleChanged();
            }
        }
        public string? PageButtonActionTitle
        {
            get
            {
                return _pageButtonActionTitle;
            }
            set
            {
                _pageButtonActionTitle = value;
                NotifyPageTitleChanged();
            }
        }
        public Action? PageButtonSecondaryAction
        {
            get
            {
                return _pageButtonSecondaryAction;
            }
            set
            {
                _pageButtonSecondaryAction = value;
                NotifyPageTitleChanged();
            }
        }
        public string? PageButtonSecondaryActionTitle
        {
            get
            {
                return _pageButtonSecondaryActionTitle;
            }
            set
            {
                _pageButtonSecondaryActionTitle = value;
                NotifyPageTitleChanged();
            }
        }
        public string? PageButtonDataElement
        {
            get
            {
                return _pageButtonDataElement;
            }
            set
            {
                _pageButtonDataElement = value;
                NotifyPageTitleChanged();
            }
        }
        public string? PageButtonSecondaryDataElement
        {
            get
            {
                return _pageButtonSecondaryDataElement;
            }
            set
            {
                _pageButtonSecondaryDataElement = value;
                NotifyPageTitleChanged();
            }
        }
        public bool ShowTitleSepparation
        {
            get
            {
                return _showTitleSepparation;
            }
            set
            {
                _showTitleSepparation = value;
                NotifyPageTitleChanged();
            }
        }
        public bool ShowTitleSmall
        {
            get
            {
                return _showTitleSmall;
            }
            set
            {
                _showTitleSmall = value;
                NotifyPageTitleChanged();
            }
        }
        public bool ShowHelpSection
        {
            get
            {
                return _showHelpSection;
            }
            set
            {
                _showHelpSection = value;
                NotifyOnShowHelpSectionChanged();
            }
        }
        public bool ShowQuestioneer 
        {
            get
            {
                return _showQuestioneer;
            }
            set
            {
                _showQuestioneer = value;
                NotifyOnShowQuestioneerChanged();
            }
        }
        public string PageDescription 
        {
            get 
            { 
                return _pageDescription; 
            }
            set
            {
                _pageDescription = value;
                NotifyPageDescriptionChanged();
            }
        }
        public bool ShowBreadcrumb
        {
            get
            {
                return _showBreadcrumb;
            }
            set
            {
                _showBreadcrumb = value;
                NotifyShowBreadcrumbChanged();
            }
        }
        public DateTime? ReleaseDate 
        {
            get
            {
                return _releaseDate;
            }
            set 
            { 
                _releaseDate = value;
                NotifyOnReleaseChanged();
            }
        }
        public int? ReadDuration
        {
            get
            {
                return _readDuration;
            }
            set
            {
                _readDuration = value;
                NotifyOnReleaseChanged();
            }
        }
        public string? PageChipValue
        {
            get
            {
                return _pageChipValue;
            }
            set
            {
                _pageChipValue = value;
                NotifyPageChipValueChanged();
            }
        }
        public string? DataElement
        {
            get
            {
                return _dataElement;
            }
            set
            {
				_dataElement = value;
				NotifyOnDataElementChanged();
            }
        }
        private void NotifyPageTitleChanged() => OnPageTitleChanged?.Invoke();
        private void NotifyPageSubTitleChanged() => OnPageSubTitleChanged?.Invoke();
        private void NotifyPageDescriptionChanged() => OnPageDescriptionChanged?.Invoke();
        private void NotifyMunicipalityChanged() => OnAUTHMunicipalityIDChanged?.Invoke();
        private void NotifyCurrentUserChanged() => OnCurrentUserChanged?.Invoke();
        private void NotifyCurrentMunicipalUserChanged() => OnCurrentMunicipalUserChanged?.Invoke();
        private void NotifyCurrentSubUserChanged() => OnCurrentSubUserChanged?.Invoke();
        private void NotifyOnInitialized() => OnInitialized?.Invoke();
        private void NotifyShowBreadcrumbChanged() => OnShowBreadcrumbChanged?.Invoke();
        private void NotifyOnReleaseChanged() => OnReleaseChanged?.Invoke();
        private void NotifyOnShowHelpSectionChanged() => OnShowHelpSectionChanged?.Invoke();
        private void NotifyPageChipValueChanged() => OnPageChipValueChanged?.Invoke();
        private void NotifyOnShowQuestioneerChanged() => ShowQuestioneerChanged?.Invoke();
        private void NotifyOnDataElementChanged() => OnDataElementChanged?.Invoke();
        public async Task<Guid?> GetMunicipalityID()
        {
            return _aUTH_Municipality_ID;
        }
        public async Task<Guid?> SessionKeyID()
        {
            if (_sessionkeyID == null)
            {
                var result = await sessionStorage.GetItemAsync<string>("sessionkeyID");

                if (result != null)
                {
                    _sessionkeyID = Guid.Parse(result);
                }
                else
                {
                    _sessionkeyID = Guid.NewGuid();
                    await sessionStorage.SetItemAsync("sessionkeyID", _sessionkeyID.ToString());
                }

            }

            return _sessionkeyID;
        }
    }
}
