using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Syncfusion.Blazor.PdfViewerServer;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Telerik.Reporting.Cache.File;

namespace ICWebApp.Components.Components.File
{
    public partial class PreviewWindow
    {
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] ITEXTProvider TextProvider{ get; set; }

        private SfPdfViewerServer PDFViewer { get; set; }
        protected override async Task OnInitializedAsync()
        {
            EnviromentService.OnShowDownloadPreviewWindow += EnviromentService_OnShowDownloadPreviewWindow;
            await base.OnInitializedAsync();
        }

        private void EnviromentService_OnShowDownloadPreviewWindow()
        {
            StateHasChanged();
        }
        private void HidePreview()
        {
            EnviromentService.ShowDownloadPreviewWindow = false;
            StateHasChanged();
        }
        private async Task OnPDFViewerCreated()
        {
            if (PDFViewer != null && EnviromentService != null && EnviromentService.PreviewFile.Any())
            {
                await PDFViewer.LoadAsync(EnviromentService.PreviewFile);

                StateHasChanged();
            }
            else if (PDFViewer != null)
            {
                await PDFViewer.UnloadAsync();

                StateHasChanged();
            }
        }
        private async Task PDFViewerResetZoom()
        {
            if (PDFViewer != null)
            {
                await PDFViewer.ZoomAsync(100);
            }
        }
    }
}
