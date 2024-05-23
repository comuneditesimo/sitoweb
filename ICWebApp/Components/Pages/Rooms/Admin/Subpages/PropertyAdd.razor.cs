using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Telerik.Blazor.Components.Editor;
using Microsoft.AspNetCore.Components;
using ICWebApp.Domain.DBModels;
using Telerik.Blazor.Components;

namespace ICWebApp.Components.Pages.Rooms.Admin.Subpages
{
    public partial class PropertyAdd
    {
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] IRoomProvider RoomProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IAUTHProvider AuthProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] NavigationManager NavManager { get; set; }

        [Parameter] public string ID { get; set; }
        [Parameter] public string ActiveIndex { get; set; }

        private ROOM_Property? Data { get; set; }
        private List<LANG_Languages>? Languages { get; set; }
        private Guid? CurrentLanguage { get; set; }
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
            Languages = await LangProvider.GetAll();

            if (ID == "New")
            {
                Data = new ROOM_Property();
                Data.ID = Guid.NewGuid();
                Data.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID.Value;

                await RoomProvider.SetProperty(Data);

                if (Languages != null)
                {
                    foreach (var l in Languages)
                    {
                        var dataE = new ROOM_Property_Extended()
                        {
                            ROOM_Property_ID = Data.ID,
                            LANG_Languages_ID = l.ID
                        };

                        await RoomProvider.SetPropertyExtended(dataE);
                        Data.ROOM_Property_Extended.Add(dataE);

                    }
                }

                var count = await RoomProvider.GetPropertyList(SessionWrapper.AUTH_Municipality_ID.Value, LangProvider.GetCurrentLanguageID());

                if (count != null && count.Count > 0)
                {
                    Data.SortOrder = count.Count + 1;
                }
                else
                {
                    Data.SortOrder = 1;
                }
            }
            else
            {
                Data = await RoomProvider.GetProperty(Guid.Parse(ID), LangProvider.GetCurrentLanguageID());

                if (Data != null && Languages != null && Data.ROOM_Property_Extended != null &&
                    Data.ROOM_Property_Extended.Count < Languages.Count)
                {
                    foreach (var l in Languages)
                    {
                        if (Data.ROOM_Property_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                        {
                            var dataE = new ROOM_Property_Extended()
                            {
                                ID = Guid.NewGuid(),
                                ROOM_Property_ID = Data.ID,
                                LANG_Languages_ID = l.ID
                            };

                            Data.ROOM_Property_Extended.Add(dataE);
                        }
                    }
                }

                if (Data == null)
                {
                    ReturnToPreviousPage();
                }
            }

            if (Languages != null)
            {
                CurrentLanguage = Languages.FirstOrDefault().ID;
            }

            BusyIndicatorService.IsBusy = false;
            StateHasChanged();

            await base.OnInitializedAsync();
        }
        private async void ReturnToPreviousPage()
        {
            if (ID == "New" && Data != null)
            {
                await RoomProvider.RemoveProperty(Data.ID, true);
            }

            BusyIndicatorService.IsBusy = true;
            StateHasChanged();
            NavManager.NavigateTo("/Admin/Rooms/DetailPage/" + ActiveIndex);
        }
        private async void SaveForm()
        {
            BusyIndicatorService.IsBusy = true;
            StateHasChanged();

            foreach (var e in Data.ROOM_Property_Extended)
            {
                await RoomProvider.SetPropertyExtended(e);
            }

            await RoomProvider.SetProperty(Data);
            NavManager.NavigateTo("/Admin/Rooms/DetailPage/" + ActiveIndex);
        }
        private void LanguageChanged()
        {
            StateHasChanged();
        }
    }
}
