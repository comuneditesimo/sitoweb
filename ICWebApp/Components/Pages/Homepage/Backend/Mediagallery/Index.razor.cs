using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Wordprocessing;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;
using Telerik.Blazor;

namespace ICWebApp.Components.Pages.Homepage.Backend.Mediagallery
{
    public partial class Index
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IImageHelper ImageHelper {  get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }

        private bool IsDataBusy { get; set; } = true;
        private List<V_HOME_Media> Data = new List<V_HOME_Media>();

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_MEDIA");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Media", "MAINMENU_BACKEND_HOMEPAGE_MEDIA", null, null, true);

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
                Data = await HomeProvider.GetMediaList(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            return true;
        }
        private void Edit(V_HOME_Media Item)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Media/Edit/" + Item.ID.ToString());
            StateHasChanged();
        }
        private async void Delete(V_HOME_Media Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE_MEDIA"), TextProvider.Get("WARNING")))
                    return;

                IsDataBusy = true;
                StateHasChanged();

                await HomeProvider.RemoveMedia(Item.ID);
                await GetData();

                IsDataBusy = false;
                StateHasChanged();
            }
        }
        private void New()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Media/Edit/New");
            StateHasChanged();
        }
        private async void MoveUp(V_HOME_Media item)
        {
            if (Data != null)
            {
                int? currentOrder = item.SortOrder;

                var dbItem = await HomeProvider.GetMedia(item.ID);
                if(dbItem != null) { 
                var otherItem = Data.FirstOrDefault(p => p.SortOrder == dbItem.SortOrder - 1);

                    if (otherItem != null)
                    {
                        var dbOtherItem = await HomeProvider.GetMedia(otherItem.ID);

                        if (dbOtherItem != null)
                        {
                            dbOtherItem.SortOrder = dbItem.SortOrder;
                            dbItem.SortOrder = dbItem.SortOrder - 1;

                            await HomeProvider.SetMedia(dbOtherItem);
                            await HomeProvider.SetMedia(dbItem);
                            await GetData();

                            StateHasChanged();
                        }
                    }
                }
            }
        }
        private async void MoveDown(V_HOME_Media item)
        {
            if (Data != null)
            {
                int? currentOrder = item.SortOrder;

                var dbItem = await HomeProvider.GetMedia(item.ID);
                if (dbItem != null)
                {
                    var otherItem = Data.FirstOrDefault(p => p.SortOrder == dbItem.SortOrder + 1);

                    if (otherItem != null)
                    {
                        var dbOtherItem = await HomeProvider.GetMedia(otherItem.ID);

                        if (dbOtherItem != null)
                        {
                            dbOtherItem.SortOrder = dbItem.SortOrder;
                            dbItem.SortOrder = dbItem.SortOrder + 1;

                            await HomeProvider.SetMedia(dbOtherItem);
                            await HomeProvider.SetMedia(dbItem);
                            await GetData();

                            StateHasChanged();
                        }
                    }
                }
            }
        }        
    }
}
