using DocumentFormat.OpenXml.Math;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.ImageEditor;

namespace ICWebApp.Components.Components.File
{
    public partial class ImageUpload
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public FILE_FileInfo? FileInfo { get; set; }
        [Parameter] public EventCallback<FILE_FileInfo?> FileInfoChanged { get; set; }
        private FILE_FileInfo? OriginalFileInfo;
        private bool ShowImageCrop = false;
        private SfImageEditor? ImageEditor;
        private List<ImageEditorToolbarItemModel> customToolbarItem = new List<ImageEditorToolbarItemModel>() { };
        private bool Rerender = false;

        protected override void OnInitialized()
        {
            if(FileInfo != null && FileInfo.FILE_FileStorage != null && FileInfo.FILE_FileStorage.Any())
            {
                OriginalFileInfo = new FILE_FileInfo();

                OriginalFileInfo.FILE_FileStorage = new List<FILE_FileStorage>(){ FileInfo.FILE_FileStorage.FirstOrDefault()};

                OriginalFileInfo.FileName = FileInfo.FileName;
                OriginalFileInfo.FileExtension = FileInfo.FileExtension;

                StateHasChanged();
            }

            base.OnInitialized();
        }
        protected override void OnParametersSet()
        {
            if (FileInfo != null)
            {
                OriginalFileInfo = FileInfo;
                StateHasChanged();
            }

            base.OnParametersSet();
        }
        private void OnUpload()
        {
            if(OriginalFileInfo != null)
            {
                StateHasChanged();
                ShowImageCrop = true;
            }
        }
        private async void OnRemove()
        {
            FileInfo = null;
            await FileInfoChanged.InvokeAsync(FileInfo);
            StateHasChanged();
        }
        private async void OpenAsync()
        {
            if (ImageEditor != null && OriginalFileInfo != null && OriginalFileInfo.FILE_FileStorage.Any())
            {
                await Task.Delay(100);

                var path = "data:image/" + OriginalFileInfo.FileExtension.Replace(".", "") + ";base64," + Convert.ToBase64String(OriginalFileInfo.FILE_FileStorage.FirstOrDefault().FileImage);

                await ImageEditor.OpenAsync(path);
            }
        }
        private async void FileOpenAsync()
        {
            if (ImageEditor != null)
            {
                await ImageEditor.SelectAsync("16:9");
            }
        }
        private async void SaveImage()
        {
            if(ImageEditor != null && OriginalFileInfo != null && OriginalFileInfo.FILE_FileStorage.Any())
            {
                ShowImageCrop = false;
                StateHasChanged();
                await ImageEditor.CropAsync();
                OriginalFileInfo.FILE_FileStorage.FirstOrDefault().FileImage = await ImageEditor.GetImageDataAsync();
                FileInfo = OriginalFileInfo;
                await FileInfoChanged.InvokeAsync(FileInfo);
                StateHasChanged();
            }
        }
        private async void Cancel()
        {
            ShowImageCrop = false;
            StateHasChanged();

            if (FileInfo != null)
            {
                Rerender = true;
                OriginalFileInfo = null;
                StateHasChanged();
                await Task.Delay(1);

                Rerender = false;
                OriginalFileInfo = FileInfo;
            }
            else
            {
                OriginalFileInfo = null;
            }
            StateHasChanged();
        }
    }
}
