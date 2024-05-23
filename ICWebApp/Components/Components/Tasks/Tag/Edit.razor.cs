﻿using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;

namespace ICWebApp.Components.Components.Tasks.Tag
{
    public partial class Edit
    {
        [Inject] ITASKService TaskService { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }

        [Parameter] public TASK_Tag Tag { get; set; }
        [Parameter] public List<TASK_Tag_Extended> TagExtended { get; set; }
        [Parameter] public EventCallback<(TASK_Tag, List<TASK_Tag_Extended>, List<V_TASK_Context>)> Saved { get; set; }
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
            if(Tag.Color != null && Tag.Color.Length == 9)
            {
                Tag.Color = Tag.Color.Substring(0, 7);
            }

            await TaskService.SetTag(Tag, TagExtended);

            await Saved.InvokeAsync((Tag, TagExtended, _additionalContexts));
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
        private void OnColorChange(ColorPickerEventArgs args)
        {
            if (Tag != null)
            {
                Tag.Color = args.CurrentValue.Hex;
            }
            StateHasChanged();
        }
    }
}