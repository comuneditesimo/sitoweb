using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using Telerik.Blazor.Components.Editor;

namespace ICWebApp.Components.Pages.Municipality
{
    public partial class PrivacySettings
    {
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IPRIVProvider PrivProvider { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }

        private List<LANG_Languages>? Languages { get; set; }
        private PRIV_Privacy? Privacy;
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
        private Guid? CurrentLanguage { get; set; }
        protected override async Task OnInitializedAsync()
        {
            Privacy = await GetPrivacy();
            Languages = await LangProvider.GetAll();

            if (Privacy == null)
            {
                Privacy = new PRIV_Privacy();
                Privacy.ID = Guid.NewGuid();
                Privacy.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID;

                await PrivProvider.SetPrivacy(Privacy);

                if (Languages != null)
                {
                    foreach (var l in Languages)
                    {
                        var dataE = new PRIV_Privacy_Extended()
                        {
                            FORM_Definition_Privacy_ID = Privacy.ID,
                            LANG_Languages_ID = l.ID
                        };

                        await PrivProvider.SetPrivacyExtended(dataE);
                        Privacy.PRIV_Privacy_Extended.Add(dataE);

                    }
                }
            }
            else
            {
                if (Privacy != null && Languages != null && Privacy.PRIV_Privacy_Extended != null &&
                    Privacy.PRIV_Privacy_Extended.Count < Languages.Count)
                {
                    foreach (var l in Languages)
                    {
                        if (Privacy.PRIV_Privacy_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                        {
                            var dataE = new PRIV_Privacy_Extended()
                            {
                                ID = Guid.NewGuid(),
                                FORM_Definition_Privacy_ID = Privacy.ID,
                                LANG_Languages_ID = l.ID
                            };

                            Privacy.PRIV_Privacy_Extended.Add(dataE);
                        }
                    }
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

        public async Task<PRIV_Privacy?> GetPrivacy()
        {
            if (SessionWrapper != null)
            {
                return await PrivProvider.GetPrivacy(SessionWrapper.AUTH_Municipality_ID.Value);
            }

            return null;
        }
        private async void SaveForm()
        {
            if (Privacy != null)
            {
                BusyIndicatorService.IsBusy = true;
                StateHasChanged();

                foreach (var e in Privacy.PRIV_Privacy_Extended)
                {
                    await PrivProvider.SetPrivacyExtended(e);
                }

                await PrivProvider.SetPrivacy(Privacy);

                BusyIndicatorService.IsBusy = false;
                StateHasChanged();
            }
        }
    }
}
