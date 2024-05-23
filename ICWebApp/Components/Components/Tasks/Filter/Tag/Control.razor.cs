using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Services;
using ICWebApp.Domain.DBModels;

namespace ICWebApp.Components.Components.Tasks.Filter.Tag
{
    public partial class Control : IDisposable
    {
        [Inject] ITASKService TaskService { get; set; }
        [Inject] ITASKProvider TASKProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] IEnviromentService EnviromentService { get; set; }

        [Parameter] public List<Guid> TagList { get; set; }
        [Parameter] public EventCallback<Guid> ItemAdded { get; set; }
        [Parameter] public EventCallback<Guid> ItemRemoved { get; set; }

        private List<V_TASK_Tag?> AllTags = new List<V_TASK_Tag>();
        private List<V_TASK_Tag?> TagsToAdd = new List<V_TASK_Tag>();
        private bool TagsDropdownVisibility = false;

        protected override async Task OnInitializedAsync()
        {
            if(TagList == null)
            {
                TagList = new List<Guid>();
            }
            EnviromentService.CloseAllCustomDropdowns += HideTagsDropdown;

            AllTags = await TaskService.GetTagList(TaskService.TASK_Context_ID, true);
            TagsToAdd = AllTags.Where(p => !TagList.Contains(p.ID)).ToList();

            StateHasChanged();
            await base.OnInitializedAsync();
        }
        private async void AddTag(V_TASK_Tag Tag)
        {
            TagList.Add(Tag.ID);
            TagsToAdd = AllTags.Where(p => !TagList.Contains(p.ID)).ToList();

            HideTagsDropdown();

            await ItemAdded.InvokeAsync(Tag.ID);
            StateHasChanged();
        }
        private async void RemoveTag(V_TASK_Tag Tag)
        {
            var tagToRemove = TagList.FirstOrDefault(p => p == Tag.ID);

            if(tagToRemove != null)
            {
                TagList.Remove(tagToRemove);
            }

            TagsToAdd = AllTags.Where(p => !TagList.Contains(p.ID)).ToList();

            await ItemRemoved.InvokeAsync(tagToRemove);
            StateHasChanged();
        }
        private void ToggleTagsDropdown()
        {
            if (TagsDropdownVisibility != true)
            {
                EnviromentService.NotifyCloseAllCustomDropdowns();
                TagsDropdownVisibility = true;
            }
            else
            {
                TagsDropdownVisibility = false;
            }
            StateHasChanged();
        }
        private void HideTagsDropdown()
        {
            TagsDropdownVisibility = false;
            StateHasChanged();
        }
        public void Dispose()
        {
            EnviromentService.CloseAllCustomDropdowns -= HideTagsDropdown;
        }
    }
}
