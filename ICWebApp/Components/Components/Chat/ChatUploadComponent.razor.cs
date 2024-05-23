using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.FileProviders;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Chat;

public partial class ChatUploadComponent
{
    [Inject] ITEXTProvider TextProvider { get; set; }
    [Inject] ISessionWrapper SessionWrapper { get; set; }
    [Inject] private IEnviromentService EnviromentService { get; set; }
    [Parameter] public EventCallback<List<FILE_FileInfo>> DocsChanged { get; set; }
    [Parameter] public bool Multiple { get; set; } = true;
    [Parameter] public int MaxFileSize { get; set; } = 100000000; //About 100MB
    [Parameter] public int MaxUploadsAtOnce { get; set; } = 10;
    [Parameter] public int MaxFiles { get; set; } = 50;
    [Parameter] public bool Visible { get; set; } = true;
    [Parameter] public bool SummaryVisible { get; set; } = true;
    //Empty list allows all extensions
    [Parameter]
    public List<string> AllowedExtensions { get; set; } = new List<string>()
    {
        ".xlsx",
        ".docx",
        ".pdf",
        ".zip",
        ".7z",
        ".csv",
        ".jpeg",
        ".jpg",
        ".png",
        ".gif",
        ".tiff",
        ".mp4",
        ".mov",
        ".mp3",
        ".wav",
        ".txt",
    };

    private bool _filesLoading = false;
    private List<FILE_FileInfo> _currentFiles = new List<FILE_FileInfo>();

    protected override Task OnParametersSetAsync()
    {
        StateHasChanged();
        return base.OnParametersSetAsync();
    }

    private async Task LoadFiles(InputFileChangeEventArgs e)
    {
        _filesLoading = true;
        StateHasChanged();

        IReadOnlyList<IBrowserFile>? files = null;
        try
        {
            files = e.GetMultipleFiles(MaxUploadsAtOnce);
        }
        catch
        {
            // ignored
        }

        if (files == null)
        {
            _filesLoading = false;
            StateHasChanged();
            return;
        }

        foreach (var file in files)
        {
            try
            {
                if (string.IsNullOrEmpty(file.ContentType))
                {
                    continue;
                }

                var fileExtension = Path.GetExtension(file.Name);
                if (!AllowedExtensions.Contains(fileExtension.ToLower()))
                {
                    continue;
                }

                if (!Multiple)
                {
                    _currentFiles.Clear();
                }
                if (_currentFiles.Count >= MaxFiles)
                    break;

                var trustedFileNameForFileStorage = Path.GetRandomFileName();

                await using var fs = new MemoryStream();
                await file.OpenReadStream(MaxFileSize).CopyToAsync(fs);

                var fi = new FILE_FileInfo
                {
                    ID = Guid.NewGuid(),
                    CreationDate = DateTime.Now,
                    FileExtension = Path.GetExtension(file.Name),
                    FileName = Path.GetFileNameWithoutExtension(file.Name),
                    Size = file.Size
                };

                if (SessionWrapper.CurrentUser != null)
                {
                    fi.AUTH_Users_ID = SessionWrapper.CurrentUser.ID;
                }

                var fstorage = new FILE_FileStorage
                {
                    ID = Guid.NewGuid(),
                    CreationDate = DateTime.Now,
                    FILE_FileInfo_ID = fi.ID,
                    FileImage = fs.ToArray()
                };

                fi.FILE_FileStorage = new List<FILE_FileStorage>();
                fi.FILE_FileStorage.Add(fstorage);

                _currentFiles.Add(fi);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                //ignored
            }
        }

        await DocsChanged.InvokeAsync(_currentFiles);

        _filesLoading = false;
        StateHasChanged();
    }

    private async Task RemoveFile(FILE_FileInfo fi)
    {
        _currentFiles.Remove(fi);
        await DocsChanged.InvokeAsync(_currentFiles);
        StateHasChanged();
    }
    
    public async void ClearData()
    {
        _currentFiles.Clear();
        await DocsChanged.InvokeAsync(_currentFiles);
        StateHasChanged();
    }
    private void DownloadFile(FILE_FileInfo fi)
    {
        var storage = fi.FILE_FileStorage.FirstOrDefault();
        if (storage != null)
        {
            EnviromentService.DownloadFile(storage.FileImage, fi.FileName + fi.FileExtension, true);
        }
        
    }
}