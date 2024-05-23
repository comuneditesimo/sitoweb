using DocumentFormat.OpenXml.Office2019.Excel.ThreadedComments;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Components.Components.Homepage.Backend.Person;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Backend.Documents
{
    public partial class DocumentMultipleSelection
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HOMEProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ILANGProvider LANGProvider { get; set; }
        [Parameter] public List<Guid>? SelectedDocuments { get; set; }
        [Parameter] public EventCallback<List<Guid>?> SelectedDocumentsChanged { get; set; }

        private List<V_HOME_Document>? DocumentList = null;
        private bool AddDocumentVisible = false;
        private string? DocumentEditTitle = null;
        private bool ShowDocumentEdit = false;
        private string? DocumentEditID = null;
        private string? SearchText = null;

        protected override async void OnInitialized()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var list = await HOMEProvider.GetDocuments(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
                DocumentList = list.Where(p => p.ReleaseDate == null || p.ReleaseDate.Value <= DateTime.Now).ToList();
            }

            StateHasChanged();
            base.OnInitialized();
        }
        protected override async void OnParametersSet()
        {
            if (SessionWrapper.AUTH_Municipality_ID != null && DocumentList == null)
            {
                var list = await HOMEProvider.GetDocuments(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
                DocumentList = list.Where(p => p.ReleaseDate == null || p.ReleaseDate.Value <= DateTime.Now).ToList();
            }

            StateHasChanged();
            base.OnParametersSet();
        }     
        private async void Remove(V_HOME_Document person)
        {
            if (SelectedDocuments != null) 
            {
                SelectedDocuments.Remove(person.ID);
                await SelectedDocumentsChanged.InvokeAsync(SelectedDocuments);
                StateHasChanged();
            }
        }
        private void AddVisible()
        {
            AddDocumentVisible = true;
            StateHasChanged();
        }
        private void HideAdd()
        {
            AddDocumentVisible = false;
            StateHasChanged();
        }
        private async void Select(V_HOME_Document Item)
        {
            if (SelectedDocuments != null)
            {
                SelectedDocuments.Add(Item.ID);
                await SelectedDocumentsChanged.InvokeAsync(SelectedDocuments);
                AddDocumentVisible = false;
                StateHasChanged();
            }
        }
        private void New()
        {
            ShowDocumentEdit = true;
            DocumentEditID = "New";
            DocumentEditTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_DOCUMENT_NEW");
            StateHasChanged();
        }
        private void Edit(Guid HOME_Document_ID)
        {
            ShowDocumentEdit = true;
            DocumentEditID = HOME_Document_ID.ToString();
            DocumentEditTitle = TextProvider.Get("MAINMENU_BACKEND_HOMEPAGE_DOCUMENT_EDIT");
            StateHasChanged();
        }
        private void Cancel()
        {
            ShowDocumentEdit = false;
            DocumentEditID = null;
            DocumentEditTitle = null;
            StateHasChanged();
        }
        private async void Save()
        {
            ShowDocumentEdit = false;
            DocumentEditID = null;
            DocumentEditTitle = null;

            if (SessionWrapper.AUTH_Municipality_ID != null)
            {
                var list = await HOMEProvider.GetDocuments(SessionWrapper.AUTH_Municipality_ID.Value, LANGProvider.GetCurrentLanguageID());
                DocumentList = list.Where(p => p.ReleaseDate == null || p.ReleaseDate.Value <= DateTime.Now).ToList();
            }

            StateHasChanged();
        }
    }
}
