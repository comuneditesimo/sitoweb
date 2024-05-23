using Blazored.SessionStorage;
using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Enums.Homepage.Settings;
using ICWebApp.Domain.Models.Homepage.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Canteen.Frontend;

public partial class LandingPageCanteen
{
    [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
    [Inject] ISessionWrapper SessionWrapper { get; set; }
    [Inject] ICANTEENProvider CanteenProvider { get; set; }
    [Inject] ITEXTProvider TextProvider { get; set; }
    [Inject] NavigationManager NavManager { get; set; }
    [Inject] IBreadCrumbService CrumbService { get; set; }
    [Inject] ISessionStorageService SessionStorage { get; set; }
    [Inject] IFILEProvider FileProvider { get; set; }
    [Inject] IEnviromentService EnviromentService { get; set; }
    [Inject] IMyCivisService MyCivisService { get; set; }
    [Inject] ILANGProvider LangProvider { get; set; }
    [Inject] IActionBarService ActionBarService { get; set; }
    [Inject] IAPPProvider AppProvider { get; set; }
    [Inject] IAUTHProvider AuthProvider { get; set; }
    [Inject] IHPServiceHelper HpServiceHelper { get; set; }

    [Parameter] public string Infopage { get; set; }

    private decimal CurrentBalance = 0;
    private List<CANTEEN_Subscriber_Movements> LatestMovements = new();
    private List<CANTEEN_Subscriber> Subscribers = new();
    private CANTEEN_User CurrentCanteenUser = new();
    private List<CANTEEN_Ressources>? DataRessources = new List<CANTEEN_Ressources>();
    private List<CANTEEN_Ressources_Extended> DataResourceExtendeds = new List<CANTEEN_Ressources_Extended>();
    private List<CANTEEN_Property>? DataProperties;
    private List<V_HOME_Theme>? DataThemes;
    private CANTEEN_Configuration? Conf;
    private bool IsDataBusy { get; set; }
    private bool ShowContent { get; set; } = false;
    private bool IsHomepage = false;
    private AUTH_Authority? Authority;
    private V_CANTEEN_Configuration_Extended? Data;
    private bool MetaInitialized = false;

    protected override async Task OnInitializedAsync()
    {
        BusyIndicatorService.IsBusy = true;
        SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_CANTEEN");

        if (SessionWrapper.AUTH_Municipality_ID != null)
        {
            var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Canteen);

            if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
            {
                SessionWrapper.PageSubTitle = pagesubTitle.Description;
            }
            else
            {
                SessionWrapper.PageSubTitle = TextProvider.Get("MAINMENU_CANTEEN_SERVICE_DESCRIPTION");
            }
        }

        SessionWrapper.PageDescription = null;
        SessionWrapper.DataElement = "service-title";


        SessionWrapper.OnCurrentSubUserChanged += SessionWrapper_OnCurrentUserChanged;

        if (SessionWrapper != null && SessionWrapper.CurrentUser != null && SessionWrapper.CurrentSubstituteUser == null)
        {
            Subscribers = await CanteenProvider.GetSubscribersByUserID(SessionWrapper.CurrentUser.ID);

            if (Infopage == null && Subscribers.Count > 0)
            {
                ContinueToService();
                return;
            }

            await LoadData();
        }

        DataRessources = await CanteenProvider.GetRessourceList(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
        foreach (var res in DataRessources)
        {
            var extended = (await CanteenProvider.GetRessourceExtendedList(res.ID, LangProvider.GetCurrentLanguageID())).FirstOrDefault();
            if (extended != null)
                DataResourceExtendeds.Add(extended);
        }
        DataProperties = await CanteenProvider.GetPropertyList(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

        if (MyCivisService.Enabled == true)
        {
            await SessionStorage.SetItemAsync("RedirectURL", "/Canteen/MyCivis/Service");
        }
        else
        {
            await SessionStorage.SetItemAsync("RedirectURL", "/Canteen/Service");
        }

        BusyIndicatorService.IsBusy = false;
        ShowContent = true;
        StateHasChanged();

        if (SessionWrapper != null && SessionWrapper.CurrentUser != null && SessionWrapper.CurrentSubstituteUser == null)
        {
            SessionWrapper.PageButtonAction = ContinueToService;
            SessionWrapper.PageButtonActionTitle = TextProvider.Get("CANTEEN_DASHBOARD_ONLINE_SERVICE");
            SessionWrapper.PageButtonDataElement = "service-online-access";
        }
        else if (SessionWrapper != null && SessionWrapper.CurrentUser == null)
        {
            SessionWrapper.PageButtonAction = RedirectToLogin;
            SessionWrapper.PageButtonActionTitle = TextProvider.Get("CANTEEN_LOGIN");
            SessionWrapper.PageButtonDataElement = "service-online-access";
            SessionWrapper.PageButtonSecondaryAction = OfflineForm;
            SessionWrapper.PageButtonSecondaryActionTitle = TextProvider.Get("FORM_DETAIL_OFFLINE_FORM_BUTTON");
            SessionWrapper.PageButtonSecondaryDataElement = "service-booking-access";
        }

        if (SessionWrapper.AUTH_Municipality_ID != null)
        {
            Data = await CanteenProvider.GetVConfiguration(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            Authority = await AuthProvider.GetAuthorityMensa(SessionWrapper.AUTH_Municipality_ID.Value);
            Conf = await CanteenProvider.GetConfiguration(SessionWrapper.AUTH_Municipality_ID.Value);
            IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);

            if (IsHomepage)
            {
                SessionWrapper.PageSubTitle = null;
                SessionWrapper.ShowTitleSepparation = true;

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Canteen);

                    if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                    {
                        SessionWrapper.PageDescription = pagesubTitle.Description;
                    }
                    else
                    {
                        SessionWrapper.PageDescription = TextProvider.Get("MAINMENU_CANTEEN_SERVICE_DESCRIPTION");
                    }
                }

                SessionWrapper.PageChipValue = TextProvider.Get("HOMEPAGE_SERVICE_ACTIVE");

                ActionBarService.ClearActionBar();
                ActionBarService.ShowShareButton = true;
                ActionBarService.ShowDefaultButtons = true;

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    DataThemes = await CanteenProvider.GetVThemes(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                    ActionBarService.ThemeList = DataThemes;
                }
            }
        }

        CrumbService.ClearBreadCrumb();

        if(IsHomepage == true)
        {
            CrumbService.AddBreadCrumb("/Hp/Services", "HP_MAINMENU_DIENSTE", null);
        }

        if (MyCivisService.Enabled == true)
        {
            CrumbService.AddBreadCrumb("/Canteen/MyCivis", "MAINMENU_CANTEEN", null);
        }
        else
        {
            CrumbService.AddBreadCrumb("/Canteen", "MAINMENU_CANTEEN", null);
        }

        MetaInitialized = false;
        StateHasChanged();
        await base.OnInitializedAsync();
    }
    protected override async void OnAfterRender(bool firstRender)
    {
        if (!MetaInitialized)
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Authority = await AuthProvider.GetAuthorityRooms(SessionWrapper.AUTH_Municipality_ID.Value);

                if (Authority != null)
                {
                    string AddressSmall = "";
                    string cap = "";

                    if (!string.IsNullOrEmpty(Authority.Address))
                    {
                        var data = Authority.Address.Replace(", Autonome Provinz Bozen - Südtirol, Italien", "").Split(',').ToList();

                        if (data.Count() > 2)
                        {
                            AddressSmall = data[0] + " " + data[1];

                            cap = data[2];
                        }
                    }

                    var metaItem = new MetaItem()
                    {
                        Name = TextProvider.Get("MAINMENU_CANTEEN"),
                        Authority = Authority.Description,
                        AudienceType = "citizen",
                        Type = TextProvider.Get("HOMEPAGE_SERVICE_ONLINE_SERVICE"),
                        Address = AddressSmall,
                        Cap = cap,
                        Url = NavManager.Uri
                    };

                    await HpServiceHelper.CreateServiceScript(SessionWrapper.AUTH_Municipality_ID.Value, metaItem);
                    await HpServiceHelper.InjectServiceScript();

                    MetaInitialized = true;
                }
            }
        }

        base.OnAfterRender(firstRender);
    }
    private void SessionWrapper_OnCurrentUserChanged()
    {
        StateHasChanged();
    }
    private async Task<bool> LoadData()
    {
        IsDataBusy = true;

        Subscribers = await CanteenProvider.GetSubscribersByUserID(SessionWrapper.CurrentUser.ID);

        if (Infopage == null && Subscribers.Count > 0) ContinueToService();

        IsDataBusy = false;

        return true;
    }
    private async void RedirectToLogin()
    {
        if (MyCivisService.Enabled == true)
        {
            var RedirectURL = "/Canteen/MyCivis/Service";
            await SessionStorage.SetItemAsync("RedirectURL", RedirectURL);
            NavManager.NavigateTo("/Login/" + RedirectURL.Replace("/", "^"));
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
        }
        else
        {
            var RedirectURL = "/Canteen/Service";
            await SessionStorage.SetItemAsync("RedirectURL", RedirectURL);
            NavManager.NavigateTo("/Login/" + RedirectURL.Replace("/", "^"));
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
        }
    }
    private void ContinueToService()
    {
        if (SessionWrapper != null && SessionWrapper.CurrentUser == null)
        {
            RedirectToLogin();
            return;
        }

        if (MyCivisService.Enabled == true)
        {
            NavManager.NavigateTo("/Canteen/MyCivis/Service");
        }
        else
        {
            NavManager.NavigateTo("/Canteen/Service");
        }

        StateHasChanged();
    }
    private async void DownloadRessource(Guid FILE_Fileinfo_ID, string? Name)
    {
        var fileToDownload = await FileProvider.GetFileInfoAsync(FILE_Fileinfo_ID);

        if (fileToDownload != null)
        {
            FILE_FileStorage? blob = null;
            if (fileToDownload.FILE_FileStorage != null && fileToDownload.FILE_FileStorage.Count() > 0)
            {
                blob = fileToDownload.FILE_FileStorage.FirstOrDefault();
            }
            else
            {
                blob = await FileProvider.GetFileStorageAsync(fileToDownload.ID);
            }

            if (blob != null && blob.FileImage != null)
            {
                if (string.IsNullOrEmpty(Name))
                {
                    await EnviromentService.DownloadFile(blob.FileImage, fileToDownload.FileName + fileToDownload.FileExtension);
                }
                else
                {
                    await EnviromentService.DownloadFile(blob.FileImage, Name + fileToDownload.FileExtension);
                }
            }
        }

        StateHasChanged();
    }
    private void GoToReservations()
    {
        if (!NavManager.Uri.Contains("/Hp/Request"))
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Hp/Request");
            StateHasChanged();
        }
    }
    private void OfflineForm()
    {
        BusyIndicatorService.IsBusy = true;
        NavManager.NavigateTo("/Hp/Request");
        StateHasChanged();
    }
}