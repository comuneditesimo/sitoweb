using ICWebApp.Application.Interface.Provider;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Canteen.External
{
    public partial class SingleSelectFilterComponent
    {
        [Inject] public ITEXTProvider? TextProvider { get; set; }

        [Parameter] public List<string> ItemList { get; set; } = new List<string>();
        [Parameter] public string SelectedItem
        {
            get
            {
                return this._selectedItem;
            }
            set
            {
                if (value != _selectedItem)
                {
                    _selectedItem = value;
                    OnSelectionChanged().ConfigureAwait(false);
                }
            }
        }
        [Parameter] public EventCallback<string> SelectedItemChanged { get; set; }
        [Parameter] public EventCallback<string> OnFilterChange { get; set; }

        private string _selectedItem = "";
        private async Task OnSelectionChanged()
        {
            await SelectedItemChanged.InvokeAsync(SelectedItem);
            await OnFilterChange.InvokeAsync(SelectedItem);
            StateHasChanged();
        }
        private void ClearFilter()
        {
            SelectedItem = "";
        }
    }
}
