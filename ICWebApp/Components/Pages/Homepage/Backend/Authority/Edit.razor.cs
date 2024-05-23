using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;
using Syncfusion.Blazor.Popups;
using static Telerik.Blazor.ThemeConstants;

namespace ICWebApp.Components.Pages.Homepage.Backend.Authority
{
    public partial class Edit
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IBreadCrumbService CrumbService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string ID { get; set; }

        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private AUTH_Authority? Data;
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
        private List<Guid> SelectedThemes { get; set; }

        protected override async Task OnInitializedAsync()
        {
            CrumbService.ClearBreadCrumb();
            CrumbService.AddBreadCrumb("/Backend/Homepage/Authorities", "MANMENU_BACKEND_HOMEPAGE_AUTHORITIES", null, null, false);
            CrumbService.AddBreadCrumb("/Backend/Homepage/Authorities/Edit", "MANMENU_BACKEND_HOMEPAGE_AUTHORITIES_EDIT", null, null, true);

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                Data = await AuthProvider.GetAuthority(Guid.Parse(ID));

                if (Data != null)
                {
                    Data.AUTH_Authority_Extended = await AuthProvider.GetAuthorityExtended(Data.ID);

                    if (Languages != null)
                    {
                        string? address = null;
                        if (SessionWrapper.AUTH_Municipality_ID != null)
                        {
                            AUTH_Municipality? municipality = await AuthProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                            if (municipality != null && !string.IsNullOrEmpty(municipality.Address))
                            {
                                address = municipality.Address;
                            }
                        }
                        foreach (var l in Languages)
                        {
                            AUTH_Authority_Extended? extended = Data.AUTH_Authority_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID);
                            if (extended == null)
                            {
                                Data.AUTH_Authority_Extended = new List<AUTH_Authority_Extended>();
                                var dataE = new AUTH_Authority_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    AUTH_Authority_ID = Data.ID,
                                    LANG_Language_ID = l.ID,
                                    Address = address
                                };

                                await AuthProvider.SetAuthorityExtended(dataE);
                                Data.AUTH_Authority_Extended.Add(dataE);
                            }
                            else if (string.IsNullOrEmpty(extended.Address))
                            {
                                extended.Address = address;
                            }
                        }
                    }
                }
            }

            if (Data != null)
            {
                SessionWrapper.PageTitle = Data.Name;
                SessionWrapper.PageSubTitle = "";

                var themes = await HomeProvider.GetAuthority_Theme(Data.ID);

                SelectedThemes = themes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID.Value).ToList();
            }
            else
            {
                SelectedThemes = new List<Guid>();
            }

            if (CurrentLanguage == null)
                CurrentLanguage = LanguageSettings.German;

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async void Save()
        {
            if (Data != null)
            {
                Data.LastChangeDate = DateTime.Now;

                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var allowed = await HomeProvider.CheckAuthorityHighlight(Data.ID, SessionWrapper.AUTH_Municipality_ID.Value);

                    if (!allowed && Data.Highlight)
                    {
                        if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                            return;

                        Data.Highlight = false;
                    }
                }

                await AuthProvider.SetAuthority(Data);

                if (Data.AUTH_Authority_Extended != null)
                {
                    foreach (var ext in Data.AUTH_Authority_Extended)
                    {
                        await AuthProvider.SetAuthorityExtended(ext);
                    }
                }
                var existingThemes = await HomeProvider.GetAuthority_Theme(Data.ID);

                if (existingThemes != null && SelectedThemes != null)
                {
                    var themesToDelete = existingThemes.Where(p => p.HOME_Theme_ID != null && !SelectedThemes.Contains(p.HOME_Theme_ID.Value)).ToList();
                    var themesToAdd = SelectedThemes.Where(p => !existingThemes.Where(p => p.HOME_Theme_ID != null).Select(p => p.HOME_Theme_ID).Contains(p)).ToList();

                    foreach (var t in themesToDelete)
                    {
                        await HomeProvider.RemoveAuthorityTheme(t);
                    }

                    foreach (var t in themesToAdd)
                    {
                        var newPerson = new AUTH_Authority_Theme();

                        newPerson.ID = Guid.NewGuid();
                        newPerson.HOME_Theme_ID = t;
                        newPerson.AUTH_Authority_ID = Data.ID;

                        await HomeProvider.SetAuthorityTheme(newPerson);
                    }
                }

                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/Backend/Homepage/Authorities");
                StateHasChanged();
            }
        }
        private void Cancel()
        {
            BusyIndicatorService.IsBusy = true;
            NavManager.NavigateTo("/Backend/Homepage/Authorities");
            StateHasChanged();
        }
    }
}
