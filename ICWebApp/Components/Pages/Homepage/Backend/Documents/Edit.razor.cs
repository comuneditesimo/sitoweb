using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Connections;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor.Components.Editor;

namespace ICWebApp.Components.Pages.Homepage.Backend.Documents
{
    public partial class Edit
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ID { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Documents", "MAINMENU_BACKEND_HOMEPAGE_DOCUMENTS", null, null, false);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                if (ID != "New")
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_DOCUMENTS_EDIT");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Documents/Edit", "MAINMENU_BACKEND_HOMEPAGE_DOCUMENTS_EDIT", null, null, true);                   
                }
                else
                {
                    SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_DOCUMENTS_NEW");
                    CrumbService.AddBreadCrumb("/Backend/Homepage/Documents/Edit", "MAINMENU_BACKEND_HOMEPAGE_DOCUMENTS_NEW", null, null, true);
                }
            }
         
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void Save()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Documents");
            StateHasChanged();            
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Documents");
            StateHasChanged();
        }
    }
}
