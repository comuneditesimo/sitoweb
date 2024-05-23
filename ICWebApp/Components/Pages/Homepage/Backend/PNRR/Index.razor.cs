using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Syncfusion.Blazor.Popups;
using static Telerik.Blazor.ThemeConstants;

namespace ICWebApp.Components.Pages.Homepage.Backend.PNRR
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }

        private bool IsDataBusy { get; set; } = true;
        private List<V_HOME_PNRR> Data = new List<V_HOME_PNRR>();

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_PNRR");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/PNRR", "MAINMENU_BACKEND_HOMEPAGE_PNRR", null, null, true);

            await GetData();

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async Task<bool> GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Data = await HomeProvider.GetPNRR(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            return true;
        }
        private void New()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/PNRR/Edit/New");
            StateHasChanged();
        }
        private void Edit(V_HOME_PNRR Item)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/PNRR/Edit/" + Item.ID.ToString());
            StateHasChanged();
        }
        private async void Delete(V_HOME_PNRR Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE_PNRR"), TextProvider.Get("WARNING")))
                    return;

                IsDataBusy = true;
                StateHasChanged();

                await HomeProvider.RemovePNRR(Item.ID);
                await GetData();

                IsDataBusy = false;
                StateHasChanged();
            }
        }
        private async void MoveUp(V_HOME_PNRR item)
        {
            if (Data != null)
            {
                int? currentOrder = item.SortOrder;

                var dbItem = await HomeProvider.GetPNRR(item.ID);

                if (dbItem != null)
                {
                    var otherItem = Data.FirstOrDefault(p => p.SortOrder == dbItem.SortOrder - 1);

                    if (otherItem != null)
                    {
                        var dbOtherItem = await HomeProvider.GetPNRR(otherItem.ID);

                        if (dbOtherItem != null)
                        {
                            dbOtherItem.SortOrder = dbItem.SortOrder;
                            dbItem.SortOrder = dbItem.SortOrder - 1;

                            await HomeProvider.SetPNRR(dbOtherItem);
                            await HomeProvider.SetPNRR(dbItem);
                            await GetData();

                            StateHasChanged();
                        }
                    }
                }
            }
        }
        private async void MoveDown(V_HOME_PNRR item)
        {
            if (Data != null)
            {
                int? currentOrder = item.SortOrder;

                var dbItem = await HomeProvider.GetPNRR(item.ID);

                if (dbItem != null)
                {
                    var otherItem = Data.FirstOrDefault(p => p.SortOrder == dbItem.SortOrder + 1);

                    if (otherItem != null)
                    {
                        var dbOtherItem = await HomeProvider.GetPNRR(otherItem.ID);

                        if (dbOtherItem != null)
                        {
                            dbOtherItem.SortOrder = dbItem.SortOrder;
                            dbItem.SortOrder = dbItem.SortOrder + 1;

                            await HomeProvider.SetPNRR(dbOtherItem);
                            await HomeProvider.SetPNRR(dbItem);
                            await GetData();

                            StateHasChanged();
                        }
                    }
                }
            }
        }
    }
}
