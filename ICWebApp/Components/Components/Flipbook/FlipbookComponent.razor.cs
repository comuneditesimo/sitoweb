using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Flipbook
{
    public partial class FlipbookComponent
    {
        [Inject] private NavigationManager? NavManager { get; set; }
        [Inject] private IJSRuntime? JSRuntime { get; set; }
        [Parameter] public string PDFDocumentTitle { get; set; } = string.Empty;
        [Parameter] public string PDFDocumentUrl { get; set; } = string.Empty;
        [Parameter] public EventCallback<string> PDFDocumentUrlChanged { get; set; }

        private bool _firstRenderComplete = false;

        protected override async Task OnParametersSetAsync()
        {
            await OpenDocument();
            await base.OnParametersSetAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _firstRenderComplete = true;
                await OpenDocument();
            }
            await base.OnAfterRenderAsync(firstRender);
        }
        private async Task OpenDocument()
        {
            if (JSRuntime != null &&
                _firstRenderComplete == true &&
                !string.IsNullOrEmpty(PDFDocumentUrl) &&
                NavManager != null)
            {
                string url = NavManager.BaseUri + "DocumentCache/" + PDFDocumentUrl;
                if (!url.EndsWith(".pdf"))
                {
                    url = url.Split('.').FirstOrDefault() + ".pdf";
                }
                await JSRuntime.InvokeVoidAsync("OpenFlipbookFromPDF", "flipbook-container", url);
            }
        }
    }
}
