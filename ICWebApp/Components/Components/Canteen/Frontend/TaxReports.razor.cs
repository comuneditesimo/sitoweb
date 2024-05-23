using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Canteen.Frontend
{
    public partial class TaxReports
    {
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IMyCivisService MyCivisService { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        private List<CANTEEN_User_Tax_Report> _reports = new List<CANTEEN_User_Tax_Report>();
        private List<CANTEEN_User_Tax_Report> _reportsToDisplay = new List<CANTEEN_User_Tax_Report>();
        
        private int SelectedLanguage { get; set; }

        private Guid AccordionID = Guid.NewGuid();
        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("CANTEEN_TAX_REPORT_PAGE_TITLE");
            SessionWrapper.PageDescription = null;
            CrumbService.ClearBreadCrumb();

            if (MyCivisService.Enabled == true)
            {
                CrumbService.AddBreadCrumb("/Canteen/MyCivis/Service", "MAINMENU_CANTEEN", null, null);
                CrumbService.AddBreadCrumb("/Canteen/MyCivis/TaxReports", "CANTEEN_DASHBOARD_TAXREPORTS", null, null);
            }
            else
            {
                CrumbService.AddBreadCrumb("/Canteen/Service", "MAINMENU_CANTEEN", null, null);
                CrumbService.AddBreadCrumb("/Canteen/TaxReports", "CANTEEN_DASHBOARD_TAXREPORTS", null, null);
            }
            
            _reports = await CanteenProvider.GetTaxReportsForUser(SessionWrapper.CurrentUser.ID);
            foreach (var report in _reports)
            {
                if(_reportsToDisplay.All(e => e.Year != report.Year))
                    _reportsToDisplay.Add(report);
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
        }
        
        private async void DownloadItem(Guid reportId)
        {
            var report = _reports.FirstOrDefault(e => e.ID == reportId);
            if (report != null)
            {
                await DownloadRessource(report.FILE_FileInfo_ID, report.Year + "_" + report.UserTaxNumber);
            }
        }

        private async Task DownloadRessource(Guid FILE_Fileinfo_ID, string? Name)
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
                        await EnviromentService.DownloadFile(blob.FileImage,
                            fileToDownload.FileName + fileToDownload.FileExtension);
                    }
                    else
                    {
                        await EnviromentService.DownloadFile(blob.FileImage, Name + fileToDownload.FileExtension, true);
                    }
                }
            }

            StateHasChanged();
        }
        private void ReturnToPreviousPage()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            if (MyCivisService.Enabled == true)
            {
                NavManager.NavigateTo("/Canteen/MyCivis/Service");
            }
            else
            {
                NavManager.NavigateTo("/Canteen/Service");
            }
        }
    }
}