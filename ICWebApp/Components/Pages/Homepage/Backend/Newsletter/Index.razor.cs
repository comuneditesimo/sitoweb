using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Syncfusion.Blazor.Popups;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Telerik.Blazor.Components.Editor;
using Telerik.Blazor.Components;

namespace ICWebApp.Components.Pages.Homepage.Backend.Newsletter
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
        private List<V_HOME_Municipal_Newsletter> Data = new List<V_HOME_Municipal_Newsletter>();
        private List<V_HOME_Municipal_Newsletter_Type> TypeData = new List<V_HOME_Municipal_Newsletter_Type>();
        private bool ShowTypeWindow = false;
        private bool ShowTypeEditWindow = false;
        private string? TypeEditWindowTitle = null;
        private HOME_Municipal_Newsletter_Type? TypeItem;
        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private List<IEditorTool> Tools { get; set; } =
           new List<IEditorTool>()
           {
                new EditorButtonGroup(new Bold(), new Italic(), new Underline()),
                new EditorButtonGroup(new AlignLeft(), new AlignCenter(), new AlignRight()),
                new UnorderedList(),
                new EditorButtonGroup(new CreateLink(), new Unlink()),
                new InsertTable(),
                new DeleteTable(),
                new EditorButtonGroup(new AddRowBefore(), new AddRowAfter(), new DeleteRow(), new MergeCells(), new SplitCell())
           };
        public List<string> RemoveAttributes { get; set; } = new List<string>() { "data-id" };
        public List<string> StripTags { get; set; } = new List<string>() { "font" };

        protected override async Task OnInitializedAsync()
        {
            SessionWrapper.PageTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_NEWSLETTER");
            SessionWrapper.PageSubTitle = null;

            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Newsletter", "MAINMENU_BACKEND_HOMEPAGE_NEWSLETTER", null, null, true);

            Languages = await LangProvider.GetAll();

            await GetData();
            CurrentLanguage = LanguageSettings.German;

            IsDataBusy = false;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async Task<bool> GetData()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Data = await HomeProvider.GetMunicipalNewsletters(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            return true;
        }
        private void Edit(V_HOME_Municipal_Newsletter Item)
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Newsletter/Edit/" + Item.ID.ToString());
            StateHasChanged();
        }
        private async void Delete(V_HOME_Municipal_Newsletter Item)
        {
            if (Item != null)
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("DELETE_ARE_YOU_SURE_NEWSLETTER"), TextProvider.Get("WARNING")))
                    return;

                IsDataBusy = true;
                StateHasChanged();

                await HomeProvider.RemoveMunicipalNewsletter(Item.ID);
                await GetData();

                IsDataBusy = false;
                StateHasChanged();
            }
        }
        private void New()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Newsletter/Edit/New");
            StateHasChanged();
        }
        private async void ManageTypes()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                TypeData = await HomeProvider.GetNewsletter_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                ShowTypeWindow = true;
                StateHasChanged();
            }
        }
        private void CloseManageTypes()
        {
            ShowTypeWindow = false;
            StateHasChanged();
        }
        private void NewType()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                TypeEditWindowTitle = TextProvider.Get("BACKEND_HOMEPAGE_NEWSLETTER_TYPES_NEW");

                TypeItem = new HOME_Municipal_Newsletter_Type();

                TypeItem.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                if (Languages != null)
                {
                    foreach (var l in Languages)
                    {
                        if (TypeItem.HOME_Municipal_Newsletter_Type_Extended == null)
                        {
                            TypeItem.HOME_Municipal_Newsletter_Type_Extended = new List<HOME_Municipal_Newsletter_Type_Extended>();
                        }

                        if (TypeItem.HOME_Municipal_Newsletter_Type_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                        {
                            var dataE = new HOME_Municipal_Newsletter_Type_Extended()
                            {
                                ID = Guid.NewGuid(),
                                HOME_Municipal_Newsletter_Type_ID = TypeItem.ID,
                                LANG_Language_ID = l.ID
                            };

                            TypeItem.HOME_Municipal_Newsletter_Type_Extended.Add(dataE);
                        }
                    }
                }

                ShowTypeEditWindow = true;
                StateHasChanged();
            }
        }
        private async void EditType(V_HOME_Municipal_Newsletter_Type Item)
        {
            TypeItem = await HomeProvider.GetNewsletterType(Item.ID);

            if (TypeItem != null)
            {
                TypeEditWindowTitle = TextProvider.Get("BACKEND_HOMEPAGE_NEWSLETTER_TYPES_EDIT");
                TypeItem.HOME_Municipal_Newsletter_Type_Extended = await HomeProvider.GetNewsletterType_Extended(TypeItem.ID);
            }

            ShowTypeEditWindow = true;
            StateHasChanged();
        }
        private async void SaveType()
        {
            if (TypeItem != null)
            {
                await HomeProvider.SetNewsletterType(TypeItem);

                if (TypeItem.HOME_Municipal_Newsletter_Type_Extended != null)
                {
                    foreach (var ext in TypeItem.HOME_Municipal_Newsletter_Type_Extended)
                    {
                        await HomeProvider.SetNewsletterTypeExtended(ext);
                    }
                }
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                TypeData = await HomeProvider.GetNewsletter_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            TypeItem = null;
            ShowTypeEditWindow = false;
            StateHasChanged();
        }
        private void CancelEditType()
        {
            TypeItem = null;
            ShowTypeEditWindow = false;
            StateHasChanged();
        }
        private async void DeleteType(V_HOME_Municipal_Newsletter_Type Item)
        {
            if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_NEWSLETTER_TYPE_ARE_YOU_SURE"), TextProvider.Get("WARNING")))
                return;

            try
            {
                await HomeProvider.RemoveNewsletterType(Item.ID);
            }
            catch
            {
                if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_NEWSLETTER_TYPE_IN_USE"), TextProvider.Get("WARNING")))
                    return;
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                TypeData = await HomeProvider.GetNewsletter_Types(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());
            }

            StateHasChanged();
        }
    }
}
