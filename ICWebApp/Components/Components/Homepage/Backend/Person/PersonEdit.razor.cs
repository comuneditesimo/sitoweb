using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components.Editor;
using Telerik.Blazor.Components;
using ICWebApp.Application.Interface.Services;
using Syncfusion.Blazor.Popups;
using ICWebApp.Application.Settings;

namespace ICWebApp.Components.Components.Homepage.Backend.Person
{
    public partial class PersonEdit
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Inject] public SfDialogService Dialogs { get; set; }
        [Parameter] public string PersonID{ get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }
        [Parameter] public bool IsPopUp { get; set; } = false;

        private Guid? CurrentLanguage { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private HOME_Person? Data;
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
        private FILE_FileInfo? ExistingFile { get; set; }
        private List<Guid> SelectedAuthorities { get; set; }
        private List<Guid> SelectedDocuments { get; set; }
        private List<V_HOME_Person_Type> Types = new List<V_HOME_Person_Type>();
        private bool MaleSelected
        {
            get
            {
                if (Data != null)
                    return !Data.Gender;


                return true;
            }
            set
            {
                if (Data != null)
                    Data.Gender = false;
            }
        }
        private bool FemaleSelected
        {
            get
            {
                if (Data != null)
                    return Data.Gender;


                return false;
            }
            set
            {
                if (Data != null)
                    Data.Gender = true;
            }
        }
        protected override async Task OnInitializedAsync()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Languages = await LangProvider.GetAll();

                if (PersonID != "New")
                {
                    Data = await HomeProvider.GetPerson(Guid.Parse(PersonID));

                    if (Data != null)
                    {
                        Data.HOME_Person_Extended = await HomeProvider.GetPerson_Extended(Data.ID);

                        if (Languages != null)
                        {
                            if (Data.HOME_Person_Extended != null && Data.HOME_Person_Extended.Count < Languages.Count)
                            {
                                foreach (var l in Languages)
                                {
                                    if (Data.HOME_Person_Extended == null)
                                    {
                                        Data.HOME_Person_Extended = new List<HOME_Person_Extended>();
                                    }

                                    if (Data.HOME_Person_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                                    {
                                        var dataE = new HOME_Person_Extended()
                                        {
                                            ID = Guid.NewGuid(),
                                            HOME_Person_ID = Data.ID,
                                            LANG_Language_ID = l.ID
                                        };

                                        await HomeProvider.SetPersonExtended(dataE);
                                        Data.HOME_Person_Extended.Add(dataE);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Data = new HOME_Person();

                    Data.ID = Guid.NewGuid();
                    Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                    if (Languages != null)
                    {
                        foreach (var l in Languages)
                        {
                            if (Data.HOME_Person_Extended == null)
                            {
                                Data.HOME_Person_Extended = new List<HOME_Person_Extended>();
                            }

                            if (Data.HOME_Person_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new HOME_Person_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    HOME_Person_ID = Data.ID,
                                    LANG_Language_ID = l.ID
                                };

                                Data.HOME_Person_Extended.Add(dataE);
                            }
                        }
                    }

                    if (Languages != null && Data != null && Data.HOME_Person_Extended != null)
                    {
                        foreach (var lang in Languages)
                        {
                            var mun = await HomeProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value, lang.ID);

                            if (mun != null)
                            {
                                Data.HOME_Person_Extended.FirstOrDefault(p => p.LANG_Language_ID == lang.ID).Address = mun.Address;
                                Data.PhoneNr = mun.Telefon;
                                Data.HOME_Person_Extended.FirstOrDefault(p => p.LANG_Language_ID == lang.ID).PECEmail = mun.PecEMail;
                                Data.HOME_Person_Extended.FirstOrDefault(p => p.LANG_Language_ID == lang.ID).EMail = mun.EMail;
                            }
                        }
                    }

                    if(Data != null && Data.HOME_Person_Extended != null)
                    {
                        var mun = await HomeProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value);

                        if(mun != null)
                        {
                            Data.PhoneNr = mun.Telefon;

                            foreach(var ext in Data.HOME_Person_Extended)
                            {
                                var munl = await HomeProvider.GetMunicipality(SessionWrapper.AUTH_Municipality_ID.Value, ext.LANG_Language_ID.Value);

                                if(munl != null)
                                {
                                    ext.Address = munl.Address;
                                    ext.PECEmail = munl.PecEMail;
                                    ext.EMail = munl.EMail;
                                }
                            }
                        }
                    }
                }
            }

            if (Data != null)
            {
                var authorities = await HomeProvider.GetPerson_Authorities(Data.ID);

                SelectedAuthorities = authorities.Where(p => p.AUTH_Authority_ID != null).Select(p => p.AUTH_Authority_ID.Value).ToList();

                if (Data.FILE_FileImage_ID != null)
                {
                    ExistingFile = await FileProvider.GetFileInfoAsync(Data.FILE_FileImage_ID.Value);
                }

                var documents = await HomeProvider.GetPerson_Documents(Data.ID);

                SelectedDocuments = documents.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID.Value).ToList();
            }
            else
            {
                SelectedAuthorities = new List<Guid>();
                SelectedDocuments = new List<Guid>();
            }

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                Types = await HomeProvider.GetPerson_Types(LangProvider.GetCurrentLanguageID());
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

                if (ExistingFile != null)
                {
                    await FileProvider.SetFileInfo(ExistingFile);

                    Data.FILE_FileImage_ID = ExistingFile.ID;
                }
                else
                {
                    if (Data.FILE_FileImage_ID != null)
                    {
                        await FileProvider.RemoveFileInfo(Data.FILE_FileImage_ID.Value);

                        Data.FILE_FileImage_ID = null;
                    }
                }


                if (SessionWrapper.AUTH_Municipality_ID != null)
                {
                    var allowed = await HomeProvider.CheckPersonHighlight(Data.ID, SessionWrapper.AUTH_Municipality_ID.Value);

                    if (!allowed && Data.Highlight)
                    {
                        if (!await Dialogs.ConfirmAsync(TextProvider.Get("BACKEND_HOMEPAGE_HIGHLIGHT_NOT_POSSIBLE"), TextProvider.Get("WARNING")))
                            return;

                        Data.Highlight = false;
                    }
                }

                await HomeProvider.SetPerson(Data);

                if (Data.HOME_Person_Extended != null)
                {
                    foreach (var ext in Data.HOME_Person_Extended)
                    {
                        await HomeProvider.SetPersonExtended(ext);
                    }
                }

                var existingAuthorities = await HomeProvider.GetPerson_Authorities(Data.ID);

                if (existingAuthorities != null && SelectedAuthorities != null)
                {
                    var itemsToDelete = existingAuthorities.Where(p => p.AUTH_Authority_ID != null && !SelectedAuthorities.Contains(p.AUTH_Authority_ID.Value)).ToList();
                    var itemsToAdd = SelectedAuthorities.Where(p => !existingAuthorities.Where(p => p.AUTH_Authority_ID != null).Select(p => p.AUTH_Authority_ID).Contains(p)).ToList();

                    foreach (var t in itemsToDelete)
                    {
                        await HomeProvider.RemovePersonAuthority(t);
                    }

                    foreach (var t in itemsToAdd)
                    {
                        var newItem = new HOME_Person_Authority();

                        newItem.ID = Guid.NewGuid();
                        newItem.AUTH_Authority_ID = t;
                        newItem.HOME_Person_ID = Data.ID;

                        await HomeProvider.SetPersonAuthority(newItem);
                    }
                }

                var existingDocuments = await HomeProvider.GetPerson_Documents(Data.ID);

                if (existingDocuments != null && SelectedDocuments != null)
                {
                    var docsToDelete = existingDocuments.Where(p => p.HOME_Document_ID != null && !SelectedDocuments.Contains(p.HOME_Document_ID.Value)).ToList();
                    var docsToAdd = SelectedDocuments.Where(p => !existingDocuments.Where(p => p.HOME_Document_ID != null).Select(p => p.HOME_Document_ID).Contains(p)).ToList();

                    foreach (var t in docsToDelete)
                    {
                        await HomeProvider.RemovePersonDocument(t);
                    }

                    foreach (var t in docsToAdd)
                    {
                        var newItem = new HOME_Person_Document();

                        newItem.ID = Guid.NewGuid();
                        newItem.HOME_Document_ID = t;
                        newItem.HOME_Person_ID = Data.ID;

                        await HomeProvider.SetPersonDocument(newItem);
                    }
                }

                await OnSave.InvokeAsync();
            }
        }
        private async void Cancel()
        {
            await OnCancel.InvokeAsync();
        }

    }
}
