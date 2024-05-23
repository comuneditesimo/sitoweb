using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using ICWebApp.Components.Pages.Form.Frontend;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;

namespace ICWebApp.Components.Components.Homepage.Backend.Appointments
{
    public partial class DatesInput
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Parameter] public HOME_Appointment Appointment { get; set; }
        [Parameter] public ICollection<HOME_Appointment_Dates> Dates { get; set; }
        [Parameter] public EventCallback<ICollection<HOME_Appointment_Dates>> DatesChanged { get; set; }

        protected override async void OnInitialized()
        {
            StateHasChanged();
            base.OnInitialized();
        }
        public void Add()
        {
            if(Dates != null && Appointment != null)
            {
                Dates.Add(new HOME_Appointment_Dates()
                {
                    ID = Guid.NewGuid(),
                    HOME_Appointment_ID = Appointment.ID,
                    DateFrom = new DateTime(DateTime.Today.AddDays(1).Year, DateTime.Today.AddDays(1).Month, DateTime.Today.AddDays(1).Day, DateTime.Now.AddDays(1).Hour, 0, 0),
                    DateTo = new DateTime(DateTime.Today.AddDays(1).Year, DateTime.Today.AddDays(1).Month, DateTime.Today.AddDays(1).Day, DateTime.Now.AddDays(1).AddHours(2).Hour, 0, 0)
                });
            }

            StateHasChanged();
        }
        public async void ValueFromChanged(object value, HOME_Appointment_Dates Item)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                Item.DateFrom = DateTime.Parse(value.ToString());
                await DatesChanged.InvokeAsync(Dates);
            }
        }
        public async void ValueToChanged(object value, HOME_Appointment_Dates Item)
        {
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                Item.DateTo = DateTime.Parse(value.ToString());
                await DatesChanged.InvokeAsync(Dates);
            }
        }
        public async void Remove(HOME_Appointment_Dates Item)
        {
            Dates.Remove(Item);
            await DatesChanged.InvokeAsync(Dates);
            StateHasChanged();
        }
    }
}
