using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor;

namespace ICWebApp.Components.Pages.Rooms.Admin
{
    public partial class RoomDetailPage
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Inject] IRoomProvider ROOMProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }

        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string AktiveIndex { get; set; }
        private int AktiveTabIndex { get; set; } = 0;
        private List<ROOM_Property> Properties { get; set; }
        private List<ROOM_Ressources> Ressources { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_ROOM_SETTINGS");

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb(NavManager.Uri, "MAINMENU_BACKEND_ROOM_SETTINGS", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Properties = await ROOMProvider.GetPropertyList(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
                Ressources = await ROOMProvider.GetRessourceList(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private void AddProperty()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            NavManager.NavigateTo("/Rooms/Property/Add/New/" + AktiveTabIndex);
        }
        private void EditProperty(ROOM_Property Item)
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            NavManager.NavigateTo("/Rooms/Property/Add/" + Item.ID + "/" + AktiveTabIndex);
        }
        private async void DeleteProperty(ROOM_Property Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;

                await ROOMProvider.RemoveProperty(Item.ID);
                Properties = await ROOMProvider.GetPropertyList(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());

                StateHasChanged();
            }
        }
        private async void MoveUpProperty(ROOM_Property opt)
        {
            if (Properties != null && Properties.Count() > 0)
            {
                await ReOrderProperties();
                var newPos = Properties.FirstOrDefault(p => p.SortOrder == opt.SortOrder - 1);

                if (newPos != null)
                {
                    opt.SortOrder = opt.SortOrder - 1;
                    newPos.SortOrder = newPos.SortOrder + 1;
                    await ROOMProvider.SetProperty(opt);
                    await ROOMProvider.SetProperty(newPos);
                }
            }

            StateHasChanged();
        }
        private async void MoveDownProperty(ROOM_Property opt)
        {
            if (Properties != null && Properties.Count() > 0)
            {
                await ReOrderProperties();
                var newPos = Properties.FirstOrDefault(p => p.SortOrder == opt.SortOrder + 1);

                if (newPos != null)
                {
                    opt.SortOrder = opt.SortOrder + 1;
                    newPos.SortOrder = newPos.SortOrder - 1;
                    await ROOMProvider.SetProperty(opt);
                    await ROOMProvider.SetProperty(newPos);
                }
            }

            StateHasChanged();
        }
        private async Task<bool> ReOrderProperties()
        {
            int count = 1;

            foreach (var d in Properties.OrderBy(p => p.SortOrder))
            {
                d.SortOrder = count;

                await ROOMProvider.SetProperty(d);

                count++;
            }

            return true;
        }
        private void AddRessource()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            NavManager.NavigateTo("/Rooms/Ressource/Add/New/" + AktiveTabIndex);
        }
        private void EditRessource(ROOM_Ressources Item)
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            NavManager.NavigateTo("/Rooms/Ressource/Add/" + Item.ID + "/" + AktiveTabIndex);
        }
        private async void DeleteRessource(ROOM_Ressources Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;

                await ROOMProvider.RemoveRessource(Item.ID);
                Ressources = await ROOMProvider.GetRessourceList(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());

                StateHasChanged();
            }
        }
        private async void MoveUpRessources(ROOM_Ressources opt)
        {
            if (Ressources != null && Ressources.Count() > 0)
            {
                await ReOrderRessources();
                var newPos = Ressources.FirstOrDefault(p => p.SortOrder == opt.SortOrder - 1);

                if (newPos != null)
                {
                    opt.SortOrder = opt.SortOrder - 1;
                    newPos.SortOrder = newPos.SortOrder + 1;
                    await ROOMProvider.SetRessource(opt);
                    await ROOMProvider.SetRessource(newPos);
                }
            }

            StateHasChanged();
        }
        private async void MoveDownRessources(ROOM_Ressources opt)
        {
            if (Ressources != null && Ressources.Count() > 0)
            {
                await ReOrderRessources();
                var newPos = Ressources.FirstOrDefault(p => p.SortOrder == opt.SortOrder + 1);

                if (newPos != null)
                {
                    opt.SortOrder = opt.SortOrder + 1;
                    newPos.SortOrder = newPos.SortOrder - 1;
                    await ROOMProvider.SetRessource(opt);
                    await ROOMProvider.SetRessource(newPos);
                }
            }

            StateHasChanged();
        }
        private async Task<bool> ReOrderRessources()
        {
            int count = 1;

            foreach (var d in Ressources.OrderBy(p => p.SortOrder))
            {
                d.SortOrder = count;

                await ROOMProvider.SetRessource(d);

                count++;
            }

            return true;
        }
    }
}
