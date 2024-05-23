using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Homepage.Backend.Documents
{
    public partial class DocumentIcon
    {
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Parameter] public V_HOME_Document? Document { get; set; }
        [Parameter] public EventCallback<Guid> OnClick { get; set; }
        [Parameter] public bool EnableClicking { get; set; } = true;

        protected override void OnParametersSet()
        {
            StateHasChanged();
            base.OnParametersSet();
        }
        private async void Clicked()
        {
            if (Document != null)
            {
                await OnClick.InvokeAsync(Document.ID);
            }
        }
    }
}
