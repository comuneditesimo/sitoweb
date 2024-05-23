using ICWebApp.Application.Interface.Provider;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Canteen.External
{
    public partial class MultiSelectFilterComponent
    {
        [Inject] public ITEXTProvider? TextProvider { get; set; }

        [Parameter] public List<string> ItemList { get; set; } = new List<string>();
        [Parameter] public List<string> SelectedItemList { get; set; } = new List<string>();
        [Parameter] public EventCallback<List<string>> SelectedItemListChanged { get; set; }
        [Parameter] public EventCallback<List<string>> OnFilterChange { get; set; }

        private async Task OnItemSelect(string item)
        {
            if (!SelectedItemList.Select(p => p.ToLower()).Contains(item.ToLower()))
            {
                SelectedItemList.Add(item);
            }
            else
            {
                SelectedItemList.Remove(item);
            }

            await SelectedItemListChanged.InvokeAsync(SelectedItemList);
            await OnFilterChange.InvokeAsync(SelectedItemList);
            StateHasChanged();
        }
        private async Task ClearSelectedItems()
        {
            SelectedItemList = new List<string>();

            await SelectedItemListChanged.InvokeAsync(SelectedItemList);
            await OnFilterChange.InvokeAsync(SelectedItemList);
            StateHasChanged();
        }
        private async Task SelectAllItems()
        {
            SelectedItemList = new List<string>();
            SelectedItemList.AddRange(ItemList);

            await SelectedItemListChanged.InvokeAsync(SelectedItemList);
            await OnFilterChange.InvokeAsync(SelectedItemList);
            StateHasChanged();
        }
    }
}
