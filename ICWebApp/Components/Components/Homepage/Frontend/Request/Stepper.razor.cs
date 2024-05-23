using ICWebApp.Application.Interface.Provider;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Homepage.Frontend.Request
{
    public partial class Stepper
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Parameter] public int Step { get; set; }


        protected override void OnParametersSet()
        {
            StateHasChanged();
            base.OnParametersSet();
        }
    }
}
