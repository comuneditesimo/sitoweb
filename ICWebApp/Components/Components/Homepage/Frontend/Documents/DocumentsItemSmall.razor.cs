using DocumentFormat.OpenXml.Office2010.Excel;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ICWebApp.Components.Components.Homepage.Frontend.Documents
{
    public partial class DocumentsItemSmall
    {
        [Inject] IFILEProvider FileProvider { get; set; }
        [Inject] IHOMEProvider HomeProvider { get; set; }
        [Inject] ISessionWrapper SessionWrapper { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IImageHelper ImageHelper { get; set; }
        [Inject] IBusyIndicatorService BusyIndicatorService { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }
        [Inject] NavigationManager NavManager { get; set; }
        [Parameter] public V_HOME_Document Document { get; set; }
        [Parameter] public bool TypeClickable { get; set; } = true;

        private void OnClick()
        {
            if (Document != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Document/" + Document.ID);
                StateHasChanged();
            }
        }
        private void OnClickType()
        {
            if (Document != null)
            {
                BusyIndicatorService.IsBusy = true;
                NavManager.NavigateTo("/hp/Type/Document/" + Document.Type_ID);
                StateHasChanged();
            }
        }
        private async void Download()
        {
            if (Document != null)
            {
                var ItemData = await HomeProvider.GetDocumentDataByLanguage(Document.ID, LangProvider.GetCurrentLanguageID());

                if (ItemData == null || ItemData.Data == null)
                {
                    ItemData = await HomeProvider.GetDocumentDataByLanguage(Document.ID, LanguageSettings.German);
                }

                if (ItemData != null && ItemData.Data != null)
                {
                    await EnviromentService.DownloadFile(ItemData.Data, Document.Title + ItemData.Fileextension, true);
                }

                StateHasChanged();
            }
        }
    }
}
