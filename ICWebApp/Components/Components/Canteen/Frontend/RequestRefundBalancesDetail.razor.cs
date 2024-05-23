using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Application.Provider;

namespace ICWebApp.Components.Components.Canteen.Frontend
{
    public partial class RequestRefundBalancesDetail : IDisposable
    {
        [Inject] IBusyIndicatorService? BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService? BreadCrumbService { get; set; }
        [Inject] IEnviromentService? EnviromentService { get; set; }
        [Inject] ICANTEENProvider? CanteenProvider { get; set; }
        [Inject] IBreadCrumbService? CrumbService { get; set; }
        [Inject] ISessionWrapper? SessionWrapper { get; set; }
        [Inject] IMyCivisService? MyCivisService { get; set; }
        [Inject] NavigationManager? NavManager { get; set; }
        [Inject] IFILEProvider? FileProvider { get; set; }
        [Inject] ITEXTProvider? TextProvider { get; set; }
        [Inject] ILANGProvider? LangProvider { get; set; }

        [Parameter] public string? ID { get; set; }

        private CANTEEN_RequestRefundBalances? CurrentRequest;
        private FILE_FileInfo? CurrentFileInfo;
        private List<CANTEEN_RequestRefundBalances_Status> StatusListRequest = new List<CANTEEN_RequestRefundBalances_Status>();
        private List<CANTEEN_RequestRefundBalances_Status_Log> CurrentStatusLogList = new List<CANTEEN_RequestRefundBalances_Status_Log>();
        private bool IsDataBusy { get; set; } = true;
        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper != null)
            {
                SessionWrapper.OnCurrentUserChanged += OnCurrentUserChanged;
                if (TextProvider != null)
                {
                    SessionWrapper.PageTitle = TextProvider.Get("CANTEEN_REQUESTREFUND_DETAIL_TITLE");
                    SessionWrapper.PageDescription = null;
                }
            }
            SetBreadCrumb();

            await base.OnInitializedAsync();
        }
        protected override async Task OnParametersSetAsync()
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
            }
            IsDataBusy = true;
            StateHasChanged();
            if (string.IsNullOrEmpty(ID))
            {
                if (NavManager != null)
                {
                    NavManager.NavigateTo("/");
                }
                return;
            }
            await base.OnParametersSetAsync();

            CANTEEN_RequestRefundBalances? _currentRequest = null;
            FILE_FileInfo? _currentFileInfo = null;
            List<CANTEEN_RequestRefundBalances_Status> _statusListRequest = new List<CANTEEN_RequestRefundBalances_Status>();
            List<CANTEEN_RequestRefundBalances_Status_Log> _currentStatusLogList = new List<CANTEEN_RequestRefundBalances_Status_Log>();

            if (CanteenProvider != null)
            {
                _currentRequest = await CanteenProvider.GetRequestRefundBalance(ID);
                _statusListRequest = await CanteenProvider.GetAllRequestRefundBalanceStatus();
                if (_currentRequest != null)
                {
                    if (FileProvider != null && _currentRequest.FILE_FileInfoID != null)
                    {
                        _currentFileInfo = FileProvider.GetFileInfo(_currentRequest.FILE_FileInfoID.Value);
                    }
                    _currentStatusLogList = await CanteenProvider.GetAllRequestRefundBalanceStatusLogEntries(_currentRequest.ID, LangProvider.GetCurrentLanguageID());
                    if (LangProvider != null)
                    {
                        _currentStatusLogList = await CanteenProvider.GetAllRequestRefundBalanceStatusLogEntries(_currentRequest.ID, LangProvider.GetCurrentLanguageID());
                    }
                    else
                    {
                        _currentStatusLogList = await CanteenProvider.GetAllRequestRefundBalanceStatusLogEntries(_currentRequest.ID, LanguageSettings.German);
                    }
                }
            }

            CurrentRequest = _currentRequest;
            CurrentFileInfo = _currentFileInfo;
            StatusListRequest = _statusListRequest;
            CurrentStatusLogList = _currentStatusLogList;

            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = false;
            }
            IsDataBusy = false;
            StateHasChanged();
        }
        public void OnCurrentUserChanged()
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
            }

            ReturnToPreviousPage();
        }
        private void SetBreadCrumb()
        {
            if (BreadCrumbService != null)
            {
                BreadCrumbService.ClearBreadCrumb();
                if (MyCivisService != null && MyCivisService.Enabled == true)
                {
                    BreadCrumbService.AddBreadCrumb("/Canteen/MyCivis/Service", "MAINMENU_CANTEEN", null, null);
                    BreadCrumbService.AddBreadCrumb("/Canteen/MyCivis/RequestRefundBalances/Details", "CANTEEN_REQUESTREFUND_DETAIL_TITLE", null, null, false);
                }
                else
                {
                    BreadCrumbService.AddBreadCrumb("/Canteen/Service", "MAINMENU_CANTEEN", null, null);
                    BreadCrumbService.AddBreadCrumb("/Canteen/RequestRefundBalances/Details", "CANTEEN_REQUESTREFUND_DETAIL_TITLE", null, null, false);
                }
            }
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
        private async void DownloadRessource(Guid FILE_Fileinfo_ID, string? Name)
        {
            if (FileProvider != null && EnviromentService != null)
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
            }

            StateHasChanged();
        }
        private void BackToPrevious()
        {
            if (BusyIndicatorService != null)
            {
                BusyIndicatorService.IsBusy = true;
            }

            if (MyCivisService != null && NavManager != null && MyCivisService.Enabled)
            {
                NavManager.NavigateTo("/Canteen/MyCivis/Service");
            }
            else if (NavManager != null)
            {
                NavManager.NavigateTo("/Canteen/Service");
            }

            StateHasChanged();
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
