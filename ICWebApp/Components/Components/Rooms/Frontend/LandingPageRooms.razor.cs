using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Blazored.SessionStorage;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Domain.Models.Homepage.Services;
using ICWebApp.Components.Pages.Rooms.Admin;
using ICWebApp.Domain.Enums.Homepage.Settings;

namespace ICWebApp.Components.Components.Rooms.Frontend
{
    public partial class LandingPageRooms
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionStorageService SessionStorage { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] IActionBarService ActionBarService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IRoomProvider RoomProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] IHPServiceHelper HpServiceHelper { get; set; }

        private List<ROOM_Ressources>? DataRessources = new List<ROOM_Ressources>();
        private List<ROOM_Ressources_Extended> DataResourceExtendeds = new List<ROOM_Ressources_Extended>();
        private List<ROOM_Property>? DataProperties;
        private bool IsHomepage = false;
        private AUTH_Authority? Authority;
        private List<V_HOME_Theme>? DataThemes;
        private DateTime? LastModificationDate;
        private V_ROOM_Settings? Data;
        private bool MetaInitialized = false;

        protected override async Task OnInitializedAsync()
        {
            BusyIndicatorService.IsBusy = true;
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_ROOMS");

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

            SessionWrapper.PageDescription = null;
            SessionWrapper.DataElement = "service-title";

            SessionWrapper.OnCurrentSubUserChanged += SessionWrapper_OnCurrentUserChanged;

            DataRessources = await RoomProvider.GetRessourceList(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            foreach (var res in DataRessources)
            {
                var extended = (await RoomProvider.GetRessourceExtendedList(res.ID, LangProvider.GetCurrentLanguageID())).FirstOrDefault();
                if (extended != null)
                    DataResourceExtendeds.Add(extended);
            }
            DataProperties = await RoomProvider.GetPropertyList(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

            if(DataProperties != null && DataProperties.Any())
            {
                LastModificationDate = DataProperties.OrderByDescending(p => p.CreationDate).Select(x => x.CreationDate).FirstOrDefault();
            }
            else
            {
                LastModificationDate = DateTime.Now;
            }

            await SessionStorage.SetItemAsync("RedirectURL", "/Rooms/Service");

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            SessionWrapper.PageButtonAction = ContinueToService;
            SessionWrapper.PageButtonActionTitle = TextProvider.Get("ROOMS_DASHBOARD_ONLINE_SERVICE");
            SessionWrapper.PageButtonDataElement = "service-online-access";
            SessionWrapper.PageButtonSecondaryAction = GoToReservations;
            SessionWrapper.PageButtonSecondaryActionTitle = TextProvider.Get("FORM_DETAIL_OFFLINE_FORM_BUTTON");
            SessionWrapper.PageButtonSecondaryDataElement = "service-booking-access";

            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {

                Data = await RoomProvider.GetVSettings(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
                IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);
                Authority = await AuthProvider.GetAuthorityRooms(SessionWrapper.AUTH_Municipality_ID.Value);

                if (IsHomepage)
                {
                    SessionWrapper.PageSubTitle = null;
                    SessionWrapper.ShowTitleSepparation = true;

                    if (SessionWrapper.AUTH_Municipality_ID != null)
                    {
                        var pagesubTitle = await AuthProvider.GetCurrentPageSubTitleAsync(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), (int)PageSubtitleType.Rooms);

                        if (pagesubTitle != null && !string.IsNullOrEmpty(pagesubTitle.Description))
                        {
                            SessionWrapper.PageDescription = pagesubTitle.Description;
                        }
                        else
                        {
                            SessionWrapper.PageDescription = TextProvider.Get("MAINMENU_ROOMS_SERVICE_DESCRIPTION");
                        }
                    }

                    SessionWrapper.PageChipValue = TextProvider.Get("HOMEPAGE_SERVICE_ACTIVE");

                    ActionBarService.ClearActionBar();
                    ActionBarService.ShowShareButton = true;
                    ActionBarService.ShowDefaultButtons = true;

                    if (SessionWrapper.AUTH_Municipality_ID != null)
                    {
                        DataThemes = await RoomProvider.GetVThemes(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                        ActionBarService.ThemeList = DataThemes;
                    }
                }
            }

            CrumbService.ClearBreadCrumb();

            if (IsHomepage == true)
            {
                CrumbService.AddBreadCrumb("/Hp/Services", "HP_MAINMENU_DIENSTE", null);
            }

            CrumbService.AddBreadCrumb("/Rooms", "MAINMENU_ROOMS", null);

            MetaInitialized = false;

            StateHasChanged();
            await base.OnInitializedAsync();
        }
        protected override async void OnAfterRender(bool firstRender)
        {
            if (!MetaInitialized)
            {
                if (Authority != null && SessionWrapper.AUTH_Municipality_ID != null)
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
                        Name = TextProvider.Get("MAINMENU_ROOMS"),
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

            base.OnAfterRender(firstRender);
        }
        private void SessionWrapper_OnCurrentUserChanged()
        {
            StateHasChanged();
        }
        private void ContinueToService()
        {
            BusyIndicatorService.IsBusy = true;

            NavManager.NavigateTo("/Rooms/Service");

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
    }
}
