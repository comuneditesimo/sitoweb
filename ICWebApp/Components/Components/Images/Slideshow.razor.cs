using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Images
{
    public partial class Slideshow
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Parameter] public List<Guid> FileImageList { get; set; }

        protected override void OnParametersSet()
        {
            StateHasChanged();
            base.OnParametersSet();
        }
    }
}
