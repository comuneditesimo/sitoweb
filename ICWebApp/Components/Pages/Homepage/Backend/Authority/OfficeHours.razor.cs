using DocumentFormat.OpenXml.Drawing.Charts;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Newtonsoft.Json.Bson;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor.Components.Editor;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Telerik.Blazor.ThemeConstants;

namespace ICWebApp.Components.Pages.Homepage.Backend.Authority
{
    public partial class OfficeHours
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ID { get; set; }

        private AUTH_Authority? Authority;
        private List<AUTH_Authority_Office_Hours> Times = new List<AUTH_Authority_Office_Hours>();
        private bool IsDataBusy = false;

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Authorities", "MANMENU_BACKEND_HOMEPAGE_AUTHORITIES", null, null, false);
            CrumbService.AddBreadCrumb("/Backend/Homepage/Authorities/OfficeHours", "MAINMENU_BACKEND_HOMEPAGE_AUTHORITY_OFFICE_HOURS", null, null, true);

            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_AUTHORITY_OFFICE_HOURS");
            SessionWrapper.PageSubTitle = "";

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Authority = await AuthProvider.GetAuthority(Guid.Parse(ID));

                Times = await AuthProvider.GetOfficeHours(Guid.Parse(ID));
            }

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Authorities");
            StateHasChanged();
        }
        private void AddTime(int Weekday)
        {
            Times.Add(new AUTH_Authority_Office_Hours()
            {
                ID = Guid.NewGuid(),
                AUTH_Authority_ID = Guid.Parse(ID),
                Weekday = Weekday
            });

            StateHasChanged();
        }
        private void RemoveTime(AUTH_Authority_Office_Hours item)
        {
            Times.Remove(item);
            StateHasChanged();
        }
        private async void Save()
        {
            var existing = await AuthProvider.GetOfficeHours(Guid.Parse(ID));

            if (existing != null && Times != null)
            {
                var toDelete = existing.Where(p => !Times.Select(x => x.ID).Contains(p.ID)).ToList();
                var toAdd = Times.Where(p => !existing.Select(p => p.ID).Contains(p.ID)).ToList();


                foreach (var p in toDelete)
                {
                    await AuthProvider.RemoveOfficeHours(p.ID);
                }

                foreach (var p in toAdd)
                {
                    if (p.TimeFrom != null)
                    {
                        await AuthProvider.SetOfficeHours(p);
                    }
                }
            }

            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Authorities");
            StateHasChanged();
        }     
    }
}
