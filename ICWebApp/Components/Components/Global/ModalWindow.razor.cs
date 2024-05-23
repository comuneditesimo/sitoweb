using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using static ICWebApp.Domain.Models.ModalWindowParameters;

namespace ICWebApp.Components.Components.Global
{
    public partial class ModalWindow
    {
        [Inject] public IEnviromentService EnviromentService { get; set; }
        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public RenderFragment? Content { get; set; }
        [Parameter] public ModalWindowParameters? Parameters { get; set; }
        [Parameter] public EventCallback OnWindowClosed { get; set; }

        protected override void OnInitialized()
        {
            EnviromentService.OnIsMobileChanged += EnviromentService_OnIsMobileChanged;

            StateHasChanged();
            base.OnInitialized();
        }
        private void EnviromentService_OnIsMobileChanged()
        {
            StateHasChanged();
        }
    }
}
