using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Syncfusion.Blazor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor;

namespace ICWebApp.Components.Pages.Canteen.Backend.Students
{
    public partial class StudentList
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IMessageService MessageService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }

        private bool IsDataBusy { get; set; } = true;
        private bool ShowSubscriptionWindow = false;
        private bool ShowCardWindow = false;
        private bool ShowMovementWindow = false;
        private bool ShowAbsenceWindow = false;
        private CANTEEN_Configuration? Configuration;
        private List<V_CANTEEN_Student> Data = new List<V_CANTEEN_Student>();
        private List<V_CANTEEN_Subscriber>? Subscriptions;
        private V_CANTEEN_Subscriber? Subscriber;
        private List<V_CANTEEN_Subscriber_Movements>? Movements;
        private List<V_CANTEEN_Subscriber_Movements>? NextMovements;

        protected override async Task OnInitializedAsync()
        {
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            if (SessionWrapper.AUTH_Municipality_ID == null)
                NavManager.NavigateTo("/Backend/Landing");


            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_CATEEN_STUDENTS_LIST");

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Students/List", "MAINMENU_BACKEND_CATEEN_STUDENTS_LIST", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Configuration = await CanteenProvider.GetConfiguration(SessionWrapper.AUTH_Municipality_ID.Value);
            }

            await GetData();

            BusyIndicatorService.IsBusy = false;
            IsDataBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async Task GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Data = await CanteenProvider.GetStudents(SessionWrapper.AUTH_Municipality_ID.Value);
            }

            StateHasChanged();
        }
        private async Task ShowSubcriptions(V_CANTEEN_Student Item)
        {
            if(Item != null && !string.IsNullOrEmpty(Item.Taxnumber) && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Subscriptions = await CanteenProvider.GetVSubscriptions(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID(), Item.Taxnumber);
                ShowSubscriptionWindow = true;
                StateHasChanged();
            }
        }
        private void HideSubscriptions()
        {
            Subscriptions = null;
            ShowSubscriptionWindow = false;
            StateHasChanged();
        }
        private async Task ShowCards(V_CANTEEN_Student Item)
        {
            if (Item != null && !string.IsNullOrEmpty(Item.Taxnumber) && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Subscriber = await CanteenProvider.GetVSubscriber(Item.Taxnumber);
                ShowCardWindow = true;
                StateHasChanged();
            }
        }
        private void HideCards()
        {
            Subscriber = null;
            ShowCardWindow = false;
            StateHasChanged();
        }
        private async Task ShowMovements(V_CANTEEN_Student Item)
        {
            if (Item != null && !string.IsNullOrEmpty(Item.Taxnumber) && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Subscriber = await CanteenProvider.GetVSubscriber(Item.Taxnumber);

                if(Subscriber != null)
                {
                    Movements = await CanteenProvider.GetPastVSubscriberMovementsBySubscriber(Subscriber.ID);
                }

                ShowMovementWindow = true;
                StateHasChanged();
            }
        }
        private void HideMovements()
        {
            Subscriber = null;
            Movements = null;
            ShowMovementWindow = false;
            StateHasChanged();
        }
        private async Task ShowAbsences(V_CANTEEN_Student Item)
        {
            if (Item != null && !string.IsNullOrEmpty(Item.Taxnumber) && SessionWrapper.AUTH_Municipality_ID != null)
            {
                Subscriber = await CanteenProvider.GetVSubscriber(Item.Taxnumber);

                if (Subscriber != null)
                {
                    NextMovements = await CanteenProvider.GetFutureVSubscriberMovementsBySubscriber(Subscriber.ID);
                }

                ShowAbsenceWindow = true;
                StateHasChanged();
            }
        }
        private void HideAbsences()
        {
            Subscriber = null;
            NextMovements = null;
            ShowAbsenceWindow = false;
            StateHasChanged();
        }
        private async void DailyCancellation(V_CANTEEN_Subscriber_Movements item)
        {
            if (item != null)
            {
                if (item.CancelDate != null)
                {
                    item.CancelDate = null;
                    var movement = await CanteenProvider.GetSubscriberMovementById(item.ID);
                    if (movement != null)
                    {
                        movement.CancelDate = null;
                        await CanteenProvider.SetSubscriberMovement(movement);
                    }
                }
                else
                {
                    if (item.Date == DateTime.Today && DateTime.Now.Hour >= 10)
                    {
                        return;
                    }
                    item.CancelDate = DateTime.Now;
                    var movement = await CanteenProvider.GetSubscriberMovementById(item.ID);
                    if (movement != null)
                    {
                        movement.CancelDate = DateTime.Now;
                        await CanteenProvider.SetSubscriberMovement(movement);
                    }
                }

                NextMovements = await CanteenProvider.GetFutureVSubscriberMovementsBySubscriber(Subscriber.ID);
            }

            StateHasChanged();
        }
    }
}
