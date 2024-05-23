using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Pages.Homepage.Frontend.Newsletter
{
    public partial class Detail
    {
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IActionBarService ActionBarService { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public string? ID {  get; set; }

        private V_HOME_Municipal_Newsletter? Item;

        protected override async void OnParametersSet()
        {
            if (ID == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Newsletter");
                StateHasChanged();
                return;
            }

            Item = await HomeProvider.GetVMunicipal_Newsletter(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

            if (Item == null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Hp/Newsletter");
                StateHasChanged();
                return;
            }

            SessionWrapper.PageTitle = Item.Title;
            SessionWrapper.PageSubTitle = null;
            SessionWrapper.PageDescription = Item.DescriptionShort;
            SessionWrapper.ShowTitleSepparation = false;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Hp/Villagelife", "HP_MAINMENU_DORFLEBEN", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Newsletter", "HOMEPAGE_FRONTEND_MUNICIPAL_NEWSLETTER", null, null, false);
            CrumbService.AddBreadCrumb("/Hp/Newsletter/" + ID, Item.Title, null, null, true);

            ActionBarService.ClearActionBar();
            ActionBarService.ShowDefaultButtons = true;
            ActionBarService.ShowShareButton = true;

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            base.OnParametersSet();
        }
        private async void Download()
        {
            if (Item != null && Item.Download_FILE_FileInfo_ID != null)
            {
                var fileStorage = await FileProvider.GetFileStorageAsync(Item.Download_FILE_FileInfo_ID.Value);

                if (fileStorage != null)
                {
                    await EnviromentService.DownloadFile(fileStorage.FileImage, Item.Title + ".pdf", true);
                }
                StateHasChanged();
            }
        }
        private async Task OpenPDF()
        {
            string pdfName = string.Empty;
            if (Item != null && Item.Download_FILE_FileInfo_ID != null)
            {
                FILE_FileInfo? fileInfo = FileProvider.GetFileInfo(Item.Download_FILE_FileInfo_ID.Value);

                if (!Directory.Exists(@"D:/Comunix/DocumentCache"))
                {
                    Directory.CreateDirectory(@"D:/Comunix/DocumentCache");
                }
                if (fileInfo != null)
                {
                    if (!File.Exists("D:/Comunix/DocumentCache/" + fileInfo.ID + fileInfo.FileExtension))
                    {
                        var store = FileProvider.GetFileStorage(fileInfo.ID);

                        if (store != null)
                        {
                            File.WriteAllBytes("D:/Comunix/DocumentCache/" + fileInfo.ID + fileInfo.FileExtension, store.FileImage);

                            pdfName = fileInfo.ID + fileInfo.FileExtension;
                        }
                    }
                    else
                    {
                        pdfName = fileInfo.ID + fileInfo.FileExtension;
                    }
                    await JSRuntime.InvokeVoidAsync("open", "/Flipbook/" + pdfName + "/" + fileInfo.FileName, "_blank");
                }
            }
            StateHasChanged();
        }
    }
}
