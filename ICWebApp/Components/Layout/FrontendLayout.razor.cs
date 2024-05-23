using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components.Routing;
using ICWebApp.Application.Settings;
using ICWebApp.Application.Services;
using Microsoft.JSInterop;
using DocumentFormat.OpenXml.Drawing;

namespace ICWebApp.Components.Layout
{
    public partial class FrontendLayout
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IAccountService AccountService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] IMessageService MessageService { get; set; }
        [Inject] IActionBarService ActionBarService { get; set; }
        [Inject] IRoomBookingHelper RoomBookingHelper { get; set; }
        [Inject] IDialogService DialogService { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] IBreadCrumbService BreadCrumbService { get; set; }
        [Inject] IThemeHelper ThemeHelper { get; set; }
        [Inject] IJSRuntime JsRuntime { get; set; }

        private System.Timers.Timer _healthCheckTimer;
        private AUTH_Municipality? Municipality { get; set; }
        private IDisposable registration;
        private bool IsHomepage = false;
        private bool faviconChanged = false;

        protected override async void OnInitialized()
        {
            SessionWrapper.PageIsPublic = true;
            SessionWrapper.OnInitialized += SessionWrapper_OnInitialized;

            SessionWrapper.OnPageTitleChanged += OnValueChanged;
            SessionWrapper.OnPageSubTitleChanged += OnValueChanged;
            SessionWrapper.OnPageDescriptionChanged += OnValueChanged;
            SessionWrapper.OnShowBreadcrumbChanged += OnValueChanged;
            SessionWrapper.OnReleaseChanged += OnValueChanged;
            SessionWrapper.OnShowHelpSectionChanged += OnValueChanged;
            SessionWrapper.OnPageChipValueChanged += OnValueChanged;
            SessionWrapper.OnDataElementChanged += OnValueChanged;
            SessionWrapper.OnAUTHMunicipalityIDChanged += SessionWrapper_OnAUTHMunicipalityIDChanged;
            AnchorService.OnAnchorChanged += AnchorService_OnAnchorChanged;
            AnchorService.OnFoceShowChanged += AnchorService_OnAnchorChanged;
            EnviromentService.OnIsMobileChanged += EnviromentService_OnIsMobileChanged;
            EnviromentService.OnShowPanoramicChanged += EnviromentService_OnShowPanoramicChanged;
            EnviromentService.OnLayoutAdditionalCSSChanged += EnviromentService_OnShowPanoramicChanged;
            EnviromentService.OnShowDownloadPreviewWindow += EnviromentService_OnShowPanoramicChanged;
            NavManager.LocationChanged += NavManager_LocationChanged;
            RoomBookingHelper.OnShowBookingChanged += RoomBookingHelper_OnShowBookingChanged;
            ActionBarService.OnShowShareButtonChanged += OnValueChanged;
            ActionBarService.OnShowDefaultButtonsChanged += OnValueChanged;
            ActionBarService.OnActionBarChanged += OnValueChanged;
            ActionBarService.OnThemeListChanged += OnValueChanged;
            registration = NavManager.RegisterLocationChangingHandler(LocationChangingHandler);
            BusyIndicatorService.OnBusyIndicatorChanged += BusyIndicatorService_OnBusyIndicatorChanged; ;

            _healthCheckTimer = new System.Timers.Timer(120000);
            _healthCheckTimer.Elapsed += _healthCheckTimer_Elapsed;
            _healthCheckTimer.Enabled = true;
            _healthCheckTimer.AutoReset = true;

            _healthCheckTimer.Start();

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);
            }

            base.OnInitialized();
        }

        protected override async void OnAfterRender(bool firstRender)
        {
            if (!faviconChanged && SessionWrapper.AUTH_Municipality_ID != null)
            {
                try
                {
                    await JsRuntime.InvokeVoidAsync("replaceFavicon", ThemeHelper.GetFaviconPath());
                    faviconChanged = true;
                }
                catch { }
            }

            base.OnAfterRender(firstRender);
        }
        private void BusyIndicatorService_OnBusyIndicatorChanged(bool obj)
        {
            StateHasChanged();
        }
        private ValueTask LocationChangingHandler(LocationChangingContext arg)
        {
            SessionWrapper.PageTitle = null;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = null;
            SessionWrapper.ShowBreadcrumb = true;
            SessionWrapper.ReleaseDate = null;
            SessionWrapper.ReadDuration = null;
            SessionWrapper.ShowTitleSepparation = true;
            RoomBookingHelper.ShowBookingSideBar = false;
            EnviromentService.ShowMunicipalPanoramic = false;
            EnviromentService.LayoutAdditionalCSS = "";
            SessionWrapper.PageButtonAction = null;
            SessionWrapper.PageButtonActionTitle = null;
            SessionWrapper.PageButtonDataElement = null;
            SessionWrapper.PageButtonSecondaryAction = null;
            SessionWrapper.PageButtonSecondaryActionTitle = null;
            SessionWrapper.PageButtonSecondaryDataElement = null;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.ShowHelpSection = false;
            SessionWrapper.ShowQuestioneer = true;
            SessionWrapper.PageChipValue = null;
            SessionWrapper.ShowTitleSmall = false;
            SessionWrapper.DataElement = null;
            SessionWrapper.ShowBreadcrumb = true;
            BreadCrumbService.ShowBreadCrumb = true;

            EnviromentService.ScrollToTop();

            ActionBarService.ClearActionBar();

            return ValueTask.CompletedTask;
        }
        public void Dispose()
        {
            registration?.Dispose();
        }
        private void RoomBookingHelper_OnShowBookingChanged()
        {
            StateHasChanged();
        }
        private void EnviromentService_OnShowPanoramicChanged()
        {
            StateHasChanged();
        }
        private async void NavManager_LocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            try
            {
                await EnviromentService.ScrollToTop();
            }
            catch { }
        }
        private async void SessionWrapper_OnAUTHMunicipalityIDChanged()
        {
            if (SessionWrapper != null && SessionWrapper.AUTH_Municipality_ID != null)
            {
                IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);

                if (!IsHomepage)
                {
                    Municipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                    if (Municipality != null && Municipality.PanoramicImage != null)
                    {
                        if (!Directory.Exists("D:/Comunix/NewsImages"))
                        {
                            Directory.CreateDirectory("D:/Comunix/NewsImages");
                        }

                        if (!File.Exists("D:/Comunix/NewsImages/Panormaic_" + Municipality.Name + ".webp"))
                        {
                            File.WriteAllBytes("D:/Comunix/NewsImages/Panormaic_" + Municipality.Name + ".webp", Municipality.PanoramicImage);
                        }
                    }
                }
            }
        }
        private void SessionWrapper_OnInitialized()
        {
            StateHasChanged();
        }
        private void EnviromentService_OnIsMobileChanged()
        {
            StateHasChanged();
        }
        private void AnchorService_OnAnchorChanged()
        {
            StateHasChanged();
        }
        private void _healthCheckTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            AccountService.HealthCheck();
            MessageService.CallRequestRefresh();
        }
        private void OnValueChanged()
        {
            StateHasChanged();
        }
    }
}
