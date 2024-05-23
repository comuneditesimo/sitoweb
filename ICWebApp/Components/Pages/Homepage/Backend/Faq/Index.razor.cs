using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Syncfusion.Blazor.Popups;
using static Telerik.Blazor.ThemeConstants;

namespace ICWebApp.Components.Pages.Homepage.Backend.Faq
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
        private List<V_HOME_Faq> Data = new List<V_HOME_Faq>();

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MANMENU_BACKEND_HOMEPAGE_FAQ");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Faq", "MANMENU_BACKEND_HOMEPAGE_FAQ", null, null, true);

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
                Data = await HomeProvider.GetFaqs(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            return true;
        }
        private void New()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Faq/Edit/New");
            StateHasChanged();
        }
        private void Edit(V_HOME_Faq Item)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Faq/Edit/" + Item.ID.ToString());
            StateHasChanged();
        }
        private async void Delete(V_HOME_Faq Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE_FAQ"), TextProvider.Get("WARNING")))
                    return;

                IsDataBusy = true;
                StateHasChanged();

                await HomeProvider.RemoveFaq(Item.ID);
                await GetData();

                IsDataBusy = false;
                StateHasChanged();
            }
        }
    }
}
