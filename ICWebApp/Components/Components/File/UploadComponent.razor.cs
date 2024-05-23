using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Drawing;
using System.Drawing.Imaging;
using Telerik.Blazor;
using Syncfusion.Blazor.Popups;

namespace ICWebApp.Components.Components.File
{
    public partial class UploadComponent
    {
        [Inject] public ITEXTProvider TextProvider { get; set; }
        [Inject] public ISessionWrapper SessionWrapper { get; set; }
        [Inject] public IEnviromentService EnviromentService { get; set; }
        [Inject] public IFILEProvider FileProvider { get; set; }

        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public List<FILE_FileInfo> FileInfoList { get; set; }
        [Parameter] public EventCallback<List<FILE_FileInfo>> FileInfoListChanged { get; set; }
        [Parameter] public FILE_FileInfo? FileInfo { get; set; }
        [Parameter] public EventCallback<FILE_FileInfo?> FileInfoChanged { get; set; }
        [Parameter] public bool Multiple { get; set; } = false;
        [Parameter] public EventCallback<Guid> OnRemove { get; set; }
        [Parameter] public EventCallback<Guid> OnUpload { get; set; }
        [Parameter] public string Accept { get; set; } = ".pdf, .docx, .xlsx, .xls, .csv, .jpg, .png";
        [Parameter] public string ID { get; set; }
        [Parameter] public bool SmallStyle { get; set; } = false;
        [Parameter] public bool RemoveAreYouSure { get; set; } = false;
        [Parameter] public bool ReadOnly { get; set; } = false;
        [Parameter] public bool Image { get; set; } = false;
        [Parameter] public int? MinImageWidth { get; set; } = null;
        [Parameter] public int? MinImageHeight { get; set; } = null;

        private List<IBrowserFile> loadedFiles = new();
        private bool isLoading;
        private string? ErrorMessage;
        private InputFile InptuFileElement { get; set; }
        private ElementReference dropZoneElement { get; set; }
        private string SmallStyleCSS
        {
            get
            {
                if (SmallStyle)
                {
                    return "small-style";
                }

                return "";
            } 
        }

        IJSObjectReference _module;
        IJSObjectReference _dropZoneInstance;

        protected override void OnInitialized()
        {
            isLoading = false;

            StateHasChanged();

            base.OnInitialized();
        }
        protected override void OnParametersSet()
        {
            isLoading = false;
            StateHasChanged();
            base.OnParametersSet();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (!ReadOnly)
                {
                    _module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/UploadComponent/upload-component.js");
                    _dropZoneInstance = await _module.InvokeAsync<IJSObjectReference>("initializeFileDropZone", dropZoneElement, InptuFileElement.Element);
                }
            }
        }
        private async Task LoadFiles(InputFileChangeEventArgs e)
        {
            isLoading = true;
            loadedFiles.Clear();
            ErrorMessage = null;
            try
            {
                foreach (var file in e.GetMultipleFiles())
                {
                    try
                    {
                        if (string.IsNullOrEmpty(file.ContentType))
                        {
                            continue;
                        }

                        if (FileInfoList != null && !Multiple)
                        {
                            FileInfoList.Clear();
                            await FileInfoListChanged.InvokeAsync(FileInfoList);
                        }

                        loadedFiles.Add(file);

                        var trustedFileNameForFileStorage = Path.GetRandomFileName();

                        await using MemoryStream fs = new MemoryStream();
                        await file.OpenReadStream(100000000).CopyToAsync(fs);

                        if(Image == true)
                        {
                            if(MinImageWidth != null && MinImageHeight != null)
                            {
                                var image = System.Drawing.Image.FromStream(fs);

                                if(image.Width < MinImageWidth || image.Height < MinImageHeight)
                                {
                                    ErrorMessage = TextProvider.Get("FILE_UPLOAD_IMAGE_WRONG_SIZE").Replace("{0}", MinImageWidth.ToString()).Replace("{1}", MinImageHeight.ToString());
                                    isLoading = false;
                                    StateHasChanged();
                                    return;
                                }
                            }
                        }

                        var fi = new FILE_FileInfo();

                        fi.ID = Guid.NewGuid();
                        fi.CreationDate = DateTime.Now;
                        fi.FileExtension = Path.GetExtension(file.Name);
                        fi.FileName = Path.GetFileNameWithoutExtension(file.Name);
                        fi.Size = file.Size;

                        if (SessionWrapper.CurrentUser != null)
                        {
                            fi.AUTH_Users_ID = SessionWrapper.CurrentUser.ID;
                        }
                        if(fi.AUTH_Users_ID == null)
                        {
                            if(SessionWrapper.CurrentMunicipalUser != null)
                            {
                                fi.AUTH_Users_ID = SessionWrapper.CurrentMunicipalUser.ID;
                            }
                        }

                        var fstorage = new FILE_FileStorage();

                        fstorage.ID = Guid.NewGuid();
                        fstorage.CreationDate = DateTime.Now;
                        fstorage.FILE_FileInfo_ID = fi.ID;
                        fstorage.FileImage = fs.ToArray();

                        fi.FILE_FileStorage = new List<FILE_FileStorage>();
                        fi.FILE_FileStorage.Add(fstorage);

                        if (FileInfoList == null)
                        {
                            FileInfoList = new List<FILE_FileInfo>();
                            await FileInfoListChanged.InvokeAsync(FileInfoList);
                        }

                        FileInfo = fi;
                        await FileInfoChanged.InvokeAsync(FileInfo);

                        FileInfoList.Add(fi);
                        await FileInfoListChanged.InvokeAsync(FileInfoList);
                        await OnUpload.InvokeAsync(fi.ID);

                        StateHasChanged();
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = TextProvider.Get("FILE_UPLOAD_ERROR");
                    }
                }
            }
            catch
            {
                ErrorMessage = TextProvider.Get("FILE_UPLOAD_GENERAL_ERROR");
            }
            StateHasChanged();

            isLoading = false;
            StateHasChanged();
        }
        private async void RemoveFile(Guid FILE_FileInfo_ID)
        {
            if (RemoveAreYouSure)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("FILE_REMOVE_DELETE_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;
            }

            if (FileInfoList != null)
            {
                var itemToRemove = FileInfoList.FirstOrDefault(p => p.ID == FILE_FileInfo_ID);

                if (itemToRemove != null)
                {
                    FileInfoList.Remove(itemToRemove);
                    await FileInfoListChanged.InvokeAsync(FileInfoList);
                }
            }

            FileInfo = null;
            await FileInfoChanged.InvokeAsync(FileInfo);

            await OnRemove.InvokeAsync(FILE_FileInfo_ID);
            StateHasChanged();
        }
        private async void DownloadFile(Guid File_FileInfo_ID)
        {
            FILE_FileInfo? filetoDownload = null;

            if (FileInfoList != null) 
            {
                filetoDownload = FileInfoList.FirstOrDefault(p => p.ID == File_FileInfo_ID);
            }
            else if(FileInfo != null)
            {
                filetoDownload = FileInfo;
            }
            

            if (filetoDownload != null)
            {
                FILE_FileStorage? blob = null;
                if (filetoDownload.FILE_FileStorage != null && filetoDownload.FILE_FileStorage.Count() > 0)
                {
                    blob = filetoDownload.FILE_FileStorage.FirstOrDefault();
                }
                else
                {
                    blob = await FileProvider.GetFileStorageAsync(filetoDownload.ID);
                }

                if (blob != null && blob.FileImage != null) 
                {
                    await EnviromentService.DownloadFile(blob.FileImage, filetoDownload.FileName + filetoDownload.FileExtension);
                }

                StateHasChanged();
            }
        }
        private string? GetBase64Image(FILE_FileInfo FILE_FileInfo) 
        {
            if (FILE_FileInfo != null) 
            {
                var FileStorage = FileProvider.GetFileStorage(FILE_FileInfo.ID);

                if(FILE_FileInfo.FILE_FileStorage != null && FILE_FileInfo.FILE_FileStorage.Count() > 0)
                {
                    return string.Format("data:image/" + FILE_FileInfo.FileExtension.Replace(".", "") + ";base64,{0}", Convert.ToBase64String(FILE_FileInfo.FILE_FileStorage.FirstOrDefault().FileImage));
                }
                else if (FILE_FileInfo.FileExtension != null && FileStorage != null)
                {
                    return string.Format("data:image/" + FILE_FileInfo.FileExtension.Replace(".", "") + ";base64,{0}", Convert.ToBase64String(FileStorage.FileImage));
                }
            }

            return null;
        }
    }
}
