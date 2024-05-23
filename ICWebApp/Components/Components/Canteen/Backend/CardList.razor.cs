using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Canteen;
using Microsoft.AspNetCore.Components;


namespace ICWebApp.Components.Components.Canteen.Backend
{
    public partial class CardList
    {
        [Inject] ICANTEENProvider CanteenProvider { get; set; }
        [Inject] ILANGProvider LangProvider { get; set; }
        [Inject] ITEXTProvider TextProvider { get; set; }
        [Parameter] public string Taxnumber { get; set; }
        [Parameter] public Guid SubscriberID { get; set; }

        private List<V_CANTEEN_Subscriber_Card>? Data;
        private List<V_CANTEEN_Subscriber_Card_Log>? LogData;
        private bool LogVisible = false;
        private bool RequestNewVisible = false;
        private CardDeliveryAddress? Address;

        protected override async void OnParametersSet()
        {
            if(!string.IsNullOrEmpty(Taxnumber))
            {
                Data = await CanteenProvider.GetSubscriberCards(LangProvider.GetCurrentLanguageID(), Taxnumber);
            }

            StateHasChanged();
            base.OnParametersSet();
        }
        private async void Activate(Guid CANTEEN_Subscriber_Card_ID)
        {
            await CanteenProvider.ActivateSubscriberCard(CANTEEN_Subscriber_Card_ID);

            if (!string.IsNullOrEmpty(Taxnumber))
            {
                Data = await CanteenProvider.GetSubscriberCards(LangProvider.GetCurrentLanguageID(), Taxnumber);
            }
            StateHasChanged();
        }
        private async void Disable(Guid CANTEEN_Subscriber_Card_ID)
        {
            await CanteenProvider.DisableSubscriberCard(CANTEEN_Subscriber_Card_ID);

            if (!string.IsNullOrEmpty(Taxnumber))
            {
                Data = await CanteenProvider.GetSubscriberCards(LangProvider.GetCurrentLanguageID(), Taxnumber);
            }

            StateHasChanged();
        }
        private async void ShowLog(Guid CANTEEN_Subscriber_Card_ID)
        {
            LogData = await CanteenProvider.GetCardLogList(LangProvider.GetCurrentLanguageID(), CANTEEN_Subscriber_Card_ID);
            LogVisible = true;
            StateHasChanged();
        }
        private void HideLog()
        {
            LogData = null;
            LogVisible = false;
            StateHasChanged();
        }
        private async void NewCard()
        {
            var sub = await CanteenProvider.GetVSubscriber(SubscriberID);

            Address = new CardDeliveryAddress();

            if (sub != null) 
            {
                Address.Name = sub.UserFirstName + " " + sub.UserLastName;
                Address.Street = sub.UserDomicileStreetAdress;
                Address.PLZ = sub.UserDomicilePostalCode;
                Address.Name = sub.UserDomicileMunicipality;
            }

            RequestNewVisible = true;
            StateHasChanged();
        }
        private void HideRequestNew()
        {
            Address = null;
            RequestNewVisible = false;
            StateHasChanged();
        }
        private async void SaveRequestNew()
        {
            if (Address != null)
            {
                await CanteenProvider.RequestNewSubscriberCard(SubscriberID, Address);

                if (!string.IsNullOrEmpty(Taxnumber))
                {
                    Data = await CanteenProvider.GetSubscriberCards(LangProvider.GetCurrentLanguageID(), Taxnumber);
                }
            }

            Address = null;
            RequestNewVisible = false;
            StateHasChanged();
        }
    }
}
