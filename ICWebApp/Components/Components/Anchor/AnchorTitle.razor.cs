using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using System.Runtime.CompilerServices;
using System.Security;

namespace ICWebApp.Components.Components.Anchor
{
    public partial class AnchorTitle
    {
        [Inject] IAnchorService AnchorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Parameter] public string Title { get; set; }
        [Parameter] public string ID { get; set; }
        [Parameter] public int Order { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                if (!string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(ID))
                {
                    AnchorService.AddAnchor(Title, ID.Replace(" ", "-").Trim().ToLower(), Order);
                }

                StateHasChanged();
            }

            base.OnAfterRender(firstRender);
        }
        protected override void OnParametersSet()
        {
            StateHasChanged();
            base.OnParametersSet();
        }
    }
}
