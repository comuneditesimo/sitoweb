using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Layout
{
    public partial class EmptyLayout
    {
        [Inject] IThemeHelper ThemeHelper { get; set; }
    }
}
