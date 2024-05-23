using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Tasks.Status
{
    public partial class Edit
    {
        [Inject] ITASKService TaskService { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }

        [Parameter] public TASK_Status Status { get; set; }
        [Parameter] public List<TASK_Status_Extended> StatusExtended { get; set; }
        [Parameter] public EventCallback<(TASK_Status, List<TASK_Status_Extended>, List<V_TASK_Context>)> Saved { get; set; }
        [Parameter] public EventCallback Cancelled { get; set; }
        [Parameter] public bool IsEdit { get; set; }
        [Parameter] public List<V_TASK_Context> ContextList { get; set; }
        
        private List<V_TASK_Context> _additionalContexts = new List<V_TASK_Context>();
        private List<LANG_Languages>? Languages { get; set; }
        private Guid? CurrentLanguage { get; set; }      
        protected override async Task OnParametersSetAsync()
        {
            Languages = await LangProvider.GetAll();

            if (Languages != null)
            {
                CurrentLanguage = Languages.FirstOrDefault().ID;
            }

            StateHasChanged();
            await base.OnParametersSetAsync();
        }
        private async void ReturnToPreviousPage()
        {
            await Cancelled.InvokeAsync();
            StateHasChanged();
        }
        private async void SaveForm()
        {
            await TaskService.SetStatus(Status, StatusExtended);

            await Saved.InvokeAsync((Status, StatusExtended, _additionalContexts));
            StateHasChanged();
        }
        private void ContextCheckBoxValueChanged(V_TASK_Context context, bool value)
        {
            if (value && _additionalContexts.All(e => e.ID != context.ID))
            {
                _additionalContexts.Add(context);
            }
            else
            {
                _additionalContexts = _additionalContexts.Where(e => e.ID != context.ID).ToList();
            }
        }
    }
}
