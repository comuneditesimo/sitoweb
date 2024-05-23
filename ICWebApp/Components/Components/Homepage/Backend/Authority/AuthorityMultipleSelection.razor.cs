using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Backend.Authority
{
    public partial class AuthorityMultipleSelection
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Parameter] public List<Guid>? SelectedAuthorities { get; set; }
        [Parameter] public EventCallback<List<Guid>?> SelectedAuthoritiesChanged { get; set; }

        private List<V_HOME_Authority>? AuthorityList = null;
        private List<Guid>? SelectedList
        {
            get
            {
                return SelectedAuthorities;
            }
            set
            {
                SelectedAuthorities = value;
                SelectedAuthoritiesChanged.InvokeAsync(SelectedAuthorities);
            }
        }
        protected override async void OnParametersSet()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && AuthorityList == null)
            {
                AuthorityList = await HOMEProvider.GetAuthorities(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
            base.OnParametersSet();
        }
    }
}
