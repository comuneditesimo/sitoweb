using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.DBModels;
using Microsoft.AspNetCore.Components;

namespace ICWebApp.Components.Components.Payments;

public partial class PaymentOverview
{
    [Inject] ITEXTProvider TextProvider { get; set; }
    [Inject] IPAYProvider PayProvider { get; set; }
    [Inject] ISessionWrapper SessionWrapper { get; set; }
    [Inject] NavigationManager NavManager { get; set; }

    [Parameter] public bool OpenPayments { get; set; } = true;

    private List<PAY_Transaction>? _transactions = null;
    private int _transactionCount = 0;
    private int _showCount = 6;
    
    protected override async Task OnInitializedAsync()
    {
        if (SessionWrapper.CurrentUser != null)
        {
            if (OpenPayments)
            {
                var open = await PayProvider.GetUserServicesTransactions(SessionWrapper.CurrentUser.ID, payed: false,
                    unpayed: true, _showCount);
                _transactions = open.transactions;
                _transactionCount = open.count;
            }
            else
            {
                var completed = await PayProvider.GetUserServicesTransactions(SessionWrapper.CurrentUser.ID, payed: true, unpayed: false, count: _showCount);
                _transactions = completed.transactions;
                _transactionCount = completed.count;
                
            }
        }
        await base.OnInitializedAsync();
    }
    
    private void GoToPaymentPage(PAY_Transaction transaction)
    {
        if(transaction.PaymentDate == null)
        {
            NavManager.NavigateTo("/Payment/" + transaction.ID.ToString() + "/" + Uri.EscapeDataString("/" + NavManager.Uri.Replace(NavManager.BaseUri, "")));
            StateHasChanged();
            return;
        }
    }

    private async void ShowMore()
    {
        _showCount += 6;
        if (OpenPayments)
        {
            var open = await PayProvider.GetUserServicesTransactions(SessionWrapper.CurrentUser!.ID, payed: false,
                unpayed: true, _showCount);
            _transactions = open.transactions;
        }
        else
        {
            var transactions = await PayProvider.GetUserServicesTransactions(SessionWrapper.CurrentUser!.ID, payed: true, unpayed: false, count: _showCount);
            _transactions = transactions.transactions;
        }
        
        StateHasChanged();
    }

}

