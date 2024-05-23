using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;
using RestSharp.Extensions;

namespace ICWebApp.Components.Components.InputFields.Phone
{
    public partial class CustomPhone
    {
        [Inject] IMETAProvider MetaProvider { get; set; }
        [Parameter] public string Value { get; set; }
        [Parameter] public EventCallback<string> ValueChanged { get; set; }
        private List<META_PhonePrefix> Countries = new List<META_PhonePrefix>();
        private META_PhonePrefix? Prefix { get; set; }   
        private string Phone
        {
            get
            {
                if (Value == null)
                {
                    Value = "";
                }

                Value = Value.Trim();

                if (Prefix != null)
                {
                    Value = Value.Replace(Prefix.Value, "");
                }

                return Value;
            }
            set
            {
                Value = value.Trim();
                Value = Value.Replace(" ", "");

                if (Prefix != null)
                {
                    Value = Value.Replace(Prefix.Value, "");

                    Value = Prefix.Value + Value;
                }

                ValueChanged.InvokeAsync(Value);
            }
        }
        protected override async Task OnInitializedAsync()
        {
            Countries = await MetaProvider.GetPhonePrefixes();

            Prefix = Countries.FirstOrDefault(p => p.Code == "it");

            StateHasChanged();
            await base.OnInitializedAsync();
        }
    }
}
