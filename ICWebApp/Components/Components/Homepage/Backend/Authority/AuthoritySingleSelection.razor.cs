using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Backend.Authority
{
    public partial class AuthoritySingleSelection
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Parameter] public Guid? SelectedAuthorityID { get; set; }
        [Parameter] public EventCallback<Guid?> SelectedAuthorityIDChanged { get; set; }

        private Guid? _selectedAuthority 
        { 
            get 
            {
                return SelectedAuthorityID;
            }
            set
            {
                SelectedAuthorityID = value;
                SelectedAuthorityIDChanged.InvokeAsync(SelectedAuthorityID);
            }
        }

        private List<V_HOME_Authority>? AuthorityList = null;
        private V_HOME_Authority? SelectedAuthority = null;
        protected override async void OnParametersSet()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && AuthorityList == null)
            {
                AuthorityList = await HOMEProvider.GetAuthorities(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
            }

            if (AuthorityList != null)
            {
                SelectedAuthority = AuthorityList.FirstOrDefault(p => p.ID == SelectedAuthorityID);
            }

            StateHasChanged();
            base.OnParametersSet();
        }
    }
}
