using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Syncfusion.Blazor.Popups;
using static Telerik.Blazor.ThemeConstants;

namespace ICWebApp.Components.Pages.Homepage.Backend.Impressum
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
        private List<V_HOME_Impressum> Data = new List<V_HOME_Impressum>();

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_IMPRESSUM");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Impressum", "MANMENU_BACKEND_HOMEPAGE_IMPRESSUM", null, null, true);

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
                Data = await HomeProvider.GetImpressum(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            return true;
        }
        private void New()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Impressum/Edit/New");
            StateHasChanged();
        }
        private void Edit(V_HOME_Impressum Item)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Impressum/Edit/" + Item.ID.ToString());
            StateHasChanged();
        }
        private async void Delete(V_HOME_Impressum Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE_IMPRESSUM"), TextProvider.Get("WARNING")))
                    return;

                IsDataBusy = true;
                StateHasChanged();

                await HomeProvider.RemoveImpressum(Item.ID);
                await GetData();

                IsDataBusy = false;
                StateHasChanged();
            }
        }
        private async void MoveUp(V_HOME_Impressum item)
        {
            if (Data != null)
            {
                int? currentOrder = item.SortOrder;

                var dbItem = await HomeProvider.GetImpressum(item.ID);

                if (dbItem != null)
                {
                    var otherItem = Data.FirstOrDefault(p => p.SortOrder == dbItem.SortOrder - 1);

                    if (otherItem != null)
                    {
                        var dbOtherItem = await HomeProvider.GetImpressum(otherItem.ID);

                        if (dbOtherItem != null)
                        {
                            dbOtherItem.SortOrder = dbItem.SortOrder;
                            dbItem.SortOrder = dbItem.SortOrder - 1;

                            await HomeProvider.SetImpressum(dbOtherItem);
                            await HomeProvider.SetImpressum(dbItem);
                            await GetData();

                            StateHasChanged();
                        }
                    }
                }
            }
        }
        private async void MoveDown(V_HOME_Impressum item)
        {
            if (Data != null)
            {
                int? currentOrder = item.SortOrder;

                var dbItem = await HomeProvider.GetImpressum(item.ID);

                if (dbItem != null)
                {
                    var otherItem = Data.FirstOrDefault(p => p.SortOrder == dbItem.SortOrder + 1);

                    if (otherItem != null)
                    {
                        var dbOtherItem = await HomeProvider.GetImpressum(otherItem.ID);

                        if (dbOtherItem != null)
                        {
                            dbOtherItem.SortOrder = dbItem.SortOrder;
                            dbItem.SortOrder = dbItem.SortOrder + 1;

                            await HomeProvider.SetImpressum(dbOtherItem);
                            await HomeProvider.SetImpressum(dbItem);
                            await GetData();

                            StateHasChanged();
                        }
                    }
                }
            }
        }
    }
}
