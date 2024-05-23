using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Backend.Theme
{
    public partial class ThemeSelection
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Parameter] public List<Guid>? SelectedThemes { get; set; }
        [Parameter] public EventCallback<List<Guid>?> SelectedThemesChanged { get; set; }

        private List<V_HOME_Theme>? ThemeList = null;
        private List<Guid>? SelectedList 
        {
            get 
            {
                return SelectedThemes;
            } 
            set 
            {
                SelectedThemes = value;
                SelectedThemesChanged.InvokeAsync(SelectedThemes);
            }
        }
        protected override void OnParametersSet()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && ThemeList == null)
            {
                ThemeList = HOMEProvider.GetThemes(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
            base.OnParametersSet();
        }
    }
}
