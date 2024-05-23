using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Application.Provider;

namespace ICWebApp.Components.Components.File
{
    public partial class DownloadCardComponent
    {
        [Inject] IEnviromentService? EnviromentService { get; set; }
        [Inject] ITEXTProvider? TextProvider { get; set; }
        [Inject] IFILEProvider? FILEProvider { get; set; }

        [Parameter] public Guid? FILE_FileInfoID { get; set; }
        [Parameter] public string FileName { get; set; }

        private FILE_FileInfo? FileInfo { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            Guid? fileInfoID = FILE_FileInfoID;
            FILE_FileInfo? fileInfo = null;

            if (FILEProvider != null && fileInfoID != null && (FileInfo == null || fileInfoID != FileInfo.ID))
            {
                fileInfo = await FILEProvider.GetFileInfoAsync(fileInfoID.Value);
            }

            FileInfo = fileInfo;

            await base.OnParametersSetAsync();
        }
        private async void Download(FILE_FileInfo File, bool ForceDownload = false)
        {
            if (File != null && FILEProvider != null && EnviromentService != null)
            {
                var storage = await FILEProvider.GetFileStorageAsync(File.ID);

                if (storage != null)
                {
                    if (!string.IsNullOrEmpty(FileName))
                    {
                        await EnviromentService.DownloadFile(storage.FileImage, FileName + File.FileExtension, ForceDownload);
                    }
                    else
                    {
                        await EnviromentService.DownloadFile(storage.FileImage, File.FileName + File.FileExtension, ForceDownload);
                    }
                }
            }

            StateHasChanged();
        }
    }
}
