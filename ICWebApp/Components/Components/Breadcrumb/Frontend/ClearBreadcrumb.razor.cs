using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Services;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Breadcrumb.Frontend
{
    public partial class ClearBreadcrumb
    {
        [Inject] IBreadCrumbService CrumbService { get; set; }
        protected override void OnInitialized()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.ShowBreadCrumb = false;

            StateHasChanged();
            base.OnInitialized();
        }
    }
}
