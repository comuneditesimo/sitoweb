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

namespace ICWebApp.Components.Pages.Homepage.Backend.Person
{
    public partial class OfficeHours
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ID { get; set; }

        private HOME_Person? Person;
        private List<HOME_Person_Office_Hours> Times = new List<HOME_Person_Office_Hours>();
        private bool IsDataBusy = false;

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Person", "MAINMENU_BACKEND_HOMEPAGE_PERSON", null, null, false);
            CrumbService.AddBreadCrumb("/Backend/Homepage/Person/OfficeHours", "MAINMENU_BACKEND_HOMEPAGE_PERSON_OFFICE_HOURS", null, null, true);

            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_PERSON_OFFICE_HOURS");
            SessionWrapper.PageSubTitle = "";

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Person = await HomeProvider.GetPerson(Guid.Parse(ID));

                Times = await HomeProvider.GetPersonOfficeHours(Guid.Parse(ID));
            }

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Person");
            StateHasChanged();
        }
        private void AddTime(int Weekday)
        {
            Times.Add(new HOME_Person_Office_Hours()
            {
                ID = Guid.NewGuid(),
                HOME_Person_ID = Guid.Parse(ID),
                Weekday = Weekday
            });

            StateHasChanged();
        }
        private void RemoveTime(HOME_Person_Office_Hours item)
        {
            Times.Remove(item);
            StateHasChanged();
        }
        private async void Save()
        {
            var existing = await HomeProvider.GetPersonOfficeHours(Guid.Parse(ID));

            if (existing != null && Times != null)
            {
                var toDelete = existing.Where(p => !Times.Select(x => x.ID).Contains(p.ID)).ToList();
                var toAdd = Times.Where(p => !existing.Select(p => p.ID).Contains(p.ID)).ToList();


                foreach (var p in toDelete)
                {
                    await HomeProvider.RemovePersonOfficeHours(p.ID);
                }

                foreach (var p in toAdd)
                {
                    if (p.TimeFrom != null)
                    {
                        await HomeProvider.SetPersonOfficeHours(p);
                    }
                }
            }

            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Person");
            StateHasChanged();
        }     
    }
}
