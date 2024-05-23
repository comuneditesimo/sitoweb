using System.Globalization;
using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using Telerik.Blazor.Components;
using Telerik.DataSource.Extensions;

namespace  ICWebApp.Components.Components.Canteen.Frontend;

public partial class RequestNewPosCardComponent
{
    [Inject] private IBusyIndicatorService BusyIndicatorService { get; set; }
    [Inject] private ISessionWrapper SessionWrapper { get; set; }
    [Inject] private IMyCivisService MyCivisService { get; set; }
    [Inject] private NavigationManager NavManager { get; set; }
    [Inject] private IBreadCrumbService CrumbService { get; set; }
    [Inject] private ITEXTProvider TextProvider { get; set; }
    [Inject] private ICANTEENProvider CanteenProvider { get; set; }
    [Inject] private IPAYProvider PayProvider { get; set; }
    [Inject] private IMETAProvider MetaProvider { get; set; }
    [Inject] private IAUTHProvider AuthProvider { get; set; }
    [Parameter] public string SubscriberTaxNumber { get; set; }

    private CANTEEN_Subscriber_Card? currentCard = null;
    
    private CANTEEN_Configuration? _conf;
    private bool ShowPayProvider { get; set; } = false;
    private CANTEEN_Subscriber_Card_Request _request = new CANTEEN_Subscriber_Card_Request() { ID = Guid.Empty };
    
    protected override async Task OnInitializedAsync()
    {
        currentCard = await CanteenProvider.GetCurrentSubscriberCard(SubscriberTaxNumber);
        if (currentCard == null || (currentCard.CANTEEN_Subscriber_Card_Status_ID != CanteenCardStatus.Finished && currentCard.CANTEEN_Subscriber_Card_Status_ID != CanteenCardStatus.Disabled) || currentCard.AUTH_Municipality_ID == null)
        {
            ReturnToPreviousPage();
        }
        _conf = await CanteenProvider.GetConfiguration(currentCard!.AUTH_Municipality_ID!.Value);
        if (_conf == null || _conf.PosMode == false)
        {
            ReturnToPreviousPage();
        }
        
        SessionWrapper.PageTitle = TextProvider.Get("CANTEEN_REQUEST_NEW_CARD");
        SessionWrapper.PageDescription = null;

        CrumbService.ClearBreadCrumb();

        if (MyCivisService.Enabled == true)
        {
            CrumbService.AddBreadCrumb("/Canteen/MyCivis/Service", "MAINMENU_CANTEEN", null, null);
            CrumbService.AddBreadCrumb("/Canteen/MyCivis/RequestNewPOSCard", "CANTEEN_REQUEST_NEW_CARD", null, null);
        }
        else
        {
            CrumbService.AddBreadCrumb("/Canteen/Service", "MAINMENU_CANTEEN", null, null);
            CrumbService.AddBreadCrumb("/Canteen/RequestNewPOSCard", "CANTEEN_REQUEST_NEW_CARD", null, null);
        }

        var result = await CanteenProvider.GetUnfinishedCardRequest(SubscriberTaxNumber);
        if (result != null)
            _request = result;
        
        SessionWrapper.OnCurrentSubUserChanged += SessionWrapper_OnCurrentUserChanged;
        BusyIndicatorService.IsBusy = false;
        await base.OnInitializedAsync();
    }
    private void SessionWrapper_OnCurrentUserChanged()
    {
        if (SessionWrapper != null && SessionWrapper.CurrentUser != null)
        {
            BusyIndicatorService.IsBusy = true;

            if (MyCivisService.Enabled == true)
            {
                NavManager.NavigateTo("/Canteen/MyCivis");
            }
            else
            {
                NavManager.NavigateTo("/Canteen");
            }

            StateHasChanged();
        }
    }
    private void ReturnToPreviousPage()
    {
        BusyIndicatorService.IsBusy = true;
        StateHasChanged();

        if (MyCivisService.Enabled == true)
        {
            NavManager.NavigateTo("/Canteen/MyCivis/Service");
        }
        else
        {
            NavManager.NavigateTo("/Canteen/Service");
        }
    }
    private void BackToPreviousPayment()
    {
        ShowPayProvider = false;
        StateHasChanged();
        return;
    }

    private MarkupString GetInfoText()
    {
        string text = TextProvider.Get("CANTEEN_REQUEST_NEW_CARD_INFOS_WITHOUT_COSTS");
        if (_conf?.NewPosCardPrice > 0)
        {
            text = TextProvider.Get("CANTEEN_REQUEST_NEW_CARD_INFOS");
            text = text.Replace("{CardPrice}", string.Format("{0:C2}", _conf?.NewPosCardPrice));
        }
        text = text.Replace("{SubscriberName}", currentCard?.Firstname + " " + currentCard?.Lastname);
        text = text.Replace("{SubscriberTaxNumber}", currentCard?.Taxnumber.ToUpper());
        text = text.Replace("{SubscriberBirthday}", currentCard?.DateOfBirth!.Value.ToString("d", CultureInfo.CurrentCulture));
        return (MarkupString) text;
    }
    private async Task<bool> SaveForm()
    {
        if (SessionWrapper.CurrentUser == null)
            return false;
        if (_conf == null)
            return false;
        
        BusyIndicatorService.IsBusy = true;
        StateHasChanged();

        if (_request.ID == Guid.Empty)
        {
            _request.ID = Guid.NewGuid();
            _request.SubscriberTaxNumber = SubscriberTaxNumber;
            _request.CreatedAt = DateTime.Now;
            _request.AUTH_User_ID = SessionWrapper.CurrentUser.ID;
            if (_request.PAY_Transaction_ID == null)
            {
                var userAnagrafic = await AuthProvider.GetAnagraficByUserID(SessionWrapper.CurrentUser.ID);
                
                var transaction = new PAY_Transaction();
                transaction.ID = Guid.NewGuid();
                transaction.AUTH_Users_ID = SessionWrapper.CurrentUser.ID;
                transaction.AUTH_Municipality_ID = SessionWrapper.AUTH_Municipality_ID;
                transaction.CreationDate = DateTime.Now;
                
                transaction.Description = TextProvider.Get("CANTEEN_POS_CARD");
                transaction.PAY_Type_ID = Guid.Parse("b16d8119-e050-46c8-bb81-a1d08891f298"); //STANDARD
                transaction.DisplayInServices = true;
        
                var transPos = new PAY_Transaction_Position();
                transPos.ID = Guid.NewGuid();
                transPos.PAY_Transaction_ID = transaction.ID;
                transPos.Description = TextProvider.Get("CANTEEN_POS_CARD") + " - " + (userAnagrafic?.FiscalNumber ?? (SessionWrapper.CurrentUser.Firstname + " " + SessionWrapper.CurrentUser.Lastname));
                transPos.IsBollo = false;
                transPos.Amount = _conf!.NewPosCardPrice;
        
                var pagoPaIdentifier = await PayProvider.GetPagoPaMensaIdentifier();
                if(pagoPaIdentifier != null)
                {
                    transPos.PagoPA_Identification = pagoPaIdentifier.PagoPA_Identifier;
                    transPos.TipologiaServizio = pagoPaIdentifier.TipologiaServizio;
                }
        
                transaction.TotalAmount = transPos.Amount;
                await PayProvider.SetTransaction(transaction);
                await PayProvider.SetTransactionPosition(transPos);
                _request.PAY_Transaction_ID = transaction.ID;
            }
            CANTEEN_Subscriber? subscriber = await CanteenProvider.GetActiveCanteenSubscriberByTaxNumber(SubscriberTaxNumber);
            if (subscriber != null)
            {
                _request.Place = subscriber.Child_DomicileMunicipality;
                _request.Address = subscriber.Child_DomicileStreetAddress;
                _request.PLZ = subscriber.Child_DomicilePostalCode;
                _request.Municipality = subscriber.Child_DomicileMunicipality;
            }
            else
            {
                _request.Place = "";
                _request.Address = "";
                _request.PLZ = "";
                _request.Municipality = "";
            }
        }
        else if(_request.PAY_Transaction_ID != null)
        {
            await CanteenProvider.UpdateNewCardRequestPrice(_request.PAY_Transaction_ID.Value, _conf.NewPosCardPrice);
        }
        await CanteenProvider.SetSubscriberCardRequest(_request);
        if (_conf!.NewPosCardPrice > 0)
        {
            ShowPayProvider = true;
            BusyIndicatorService.IsBusy = false;
            StateHasChanged();
        }
        else
        {
            await PaymentCompleted();
        }

        return true;
    }
    private async Task<bool> PaymentCompleted()
    {
        if (_request.PAY_Transaction_ID != null)
        {
            var paymentTransaction = await PayProvider.GetTransaction(_request.PAY_Transaction_ID.Value);

            if (paymentTransaction != null && paymentTransaction.PaymentDate == null)
            {
                paymentTransaction.PaymentDate = DateTime.Now;
                await PayProvider.SetTransaction(paymentTransaction);
            }
            if (MyCivisService.Enabled == true)
            {
                NavManager.NavigateTo("/Canteen/MyCivis/NewCardPayed/" + _request.ID);
            }
            else
            {
                NavManager.NavigateTo("/Canteen/NewCardPayed/" + _request.ID);
            }
            StateHasChanged();
        }
        return false;
    }
}

