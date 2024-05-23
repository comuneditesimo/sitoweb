using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Syncfusion.Blazor.Popups;
using System.Diagnostics;

namespace ICWebApp.Components.Pages.Form.Admin
{
    public partial class DefinitionList
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IFORMDefinitionProvider FormDefinitionProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set;}
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] IAPPProvider AppProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string PageRefreshParam { get; set; }

        private List<V_FORM_Definition> Data = new List<V_FORM_Definition>();
        private bool IsDataBusy = true;
        private bool IsHomepage = false;
        private bool IsApplication = true;

        protected override async Task OnParametersSetAsync()
        {
            if (NavManager.Uri.Contains("Application"))
            {
                SessionWrapper.PageTitle = TextProvider.Get("FORM_DEFINITION_BACKEND_FORM");
                SessionWrapper.PageSubTitle = TextProvider.Get("FORM_DEFINITION_PAGE_TITLE");
                CrumbService.ClearBreadCrumb();
                CrumbService.AddBreadCrumb("/Form/Definition", "FORM_DEFINITION_PAGE_TITLE", null, null, true);
                StateHasChanged();
            }
            else if (NavManager.Uri.Contains("Mantainance"))
            {
                SessionWrapper.PageTitle = TextProvider.Get("FORM_DEFINITION_BACKEND_FORM");
                SessionWrapper.PageSubTitle = TextProvider.Get("FORM_DEFINITION_PAGE_MANTAINANCE_TITLE");
                CrumbService.ClearBreadCrumb();
                CrumbService.AddBreadCrumb("/Form/Definition", "FORM_DEFINITION_PAGE_MANTAINANCE_TITLE", null, null, true);
                StateHasChanged();
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                IsHomepage = await AppProvider.HasApplicationAsync(SessionWrapper.AUTH_Municipality_ID.Value, Applications.Homepage);

            }
            if (NavManager.Uri.Contains("Mantainance"))
            {
                IsApplication = false;
            }

            await GetData();

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnParametersSetAsync();
        }
        private async Task<bool> GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                IsDataBusy = true;
                StateHasChanged();

                if (IsApplication)
                {
                    Data = await FormDefinitionProvider.GetVDefinitionListByCategory(SessionWrapper.AUTH_Municipality_ID.Value, FORMCategories.Applications, LangProvider.GetCurrentLanguageID());
                }
                else
                {
                    Data = await FormDefinitionProvider.GetVDefinitionListByCategory(SessionWrapper.AUTH_Municipality_ID.Value, FORMCategories.Maintenance, LangProvider.GetCurrentLanguageID());
                }

                IsDataBusy = false;
                StateHasChanged();
            }

            return true;
        }
        private void Add()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged(); 

            if (NavManager.Uri.Contains("Application"))
            {
                NavManager.NavigateTo("/Form/Definition/Add/Application/New");
            }
            else
            {
                NavManager.NavigateTo("/Form/Definition/Add/Mantainance/New");
            }
        }
        private void Edit(V_FORM_Definition Item)
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            NavManager.NavigateTo("/Form/Definition/Add/" + Item.ID);
        }
        private async void Copy(V_FORM_Definition Item)
        {
            if (Item != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();
                await FormDefinitionProvider.CopyDefinition(Item.ID);
                await GetData();
                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }
        }
        private async void Delete(V_FORM_Definition Item)
        {
            if(Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                    return;

                await FormDefinitionProvider.RemoveDefinition(Item.ID);
                await GetData();
                StateHasChanged();
            }
        }
        private async void MoveUp(V_FORM_Definition opt)
        {
            if (Data != null && Data.Count() > 0 && opt.ID != null)
            {
                await ReOrder();

                var startField = await FormDefinitionProvider.GetDefinition(opt.ID);

                if (startField != null)
                {
                    var newPos = Data.FirstOrDefault(p => p.SortOrder == startField.SortOrder - 1);

                    if (newPos != null && newPos.ID != null)
                    {
                        var newPosDB = await FormDefinitionProvider.GetDefinition(newPos.ID);

                        if (newPosDB != null)
                        {
                            startField.SortOrder = startField.SortOrder - 1;
                            newPosDB.SortOrder = newPosDB.SortOrder + 1;
                            await FormDefinitionProvider.SetDefinition(startField);
                            await FormDefinitionProvider.SetDefinition(newPosDB);
                        }
                    }
                }
            }

            if (Data != null)
            {
                await GetData();
            }

            StateHasChanged();
        }
        private async void MoveDown(V_FORM_Definition opt)
        {
            if (Data != null && Data.Count() > 0 && opt.ID != null)
            {
                await ReOrder();

                var startField = await FormDefinitionProvider.GetDefinition(opt.ID);

                if (startField != null)
                {

                    var newPos = Data.FirstOrDefault(p => p.SortOrder == startField.SortOrder + 1);

                    if (newPos != null && newPos.ID != null)
                    {
                        var newPosDB = await FormDefinitionProvider.GetDefinition(newPos.ID);

                        if (newPosDB != null)
                        {
                            startField.SortOrder = startField.SortOrder + 1;
                            newPosDB.SortOrder = newPosDB.SortOrder - 1;
                            await FormDefinitionProvider.SetDefinition(startField);
                            await FormDefinitionProvider.SetDefinition(newPosDB);
                        }
                    }
                }
            }

            if (Data != null)
            {
                await GetData();
            }

            StateHasChanged();
        }
        private async Task<bool> ReOrder()
        {
            int count = 1;

            foreach (var d in Data.OrderBy(p => p.SortOrder))
            {
                if (d != null && d.ID != null)
                {
                    var field = await FormDefinitionProvider.GetDefinition(d.ID);

                    if (field != null)
                    {
                        field.SortOrder = count;

                        await FormDefinitionProvider.SetDefinition(field);
                    }
                }
                count++;
            }

            return true;
        }
    }
}
