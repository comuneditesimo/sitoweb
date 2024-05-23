using ICWebApp.Domain.DBModels;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IPAYService
    {
        public Task<Customer?> GetOrCreateCustomer(AUTH_Users_Anagrafic User_Anagrafic);
        public Task<Customer?> UpdateCustomer(AUTH_Users_Anagrafic User_Anagrafic);
        public Task<Session?> CreateSession(Guid TransactionFamilyID, List<PAY_Transaction_Position> Positions, AUTH_Users_Anagrafic User_Anagrafic, string ReturnUrl, string CancelUrl);
        public Task<Refund?> CreateRefund(string STRIPE_Session_ID, AUTH_Users_Anagrafic User_Anagrafic, long Amount);
        public Task<string?> PAGOPA_Create(CONF_PagoPA Configuration, string Family_ID, List<PAY_Transaction_Position> Positions, string PagoPANumero, string ReturnUrl, string CancelUrl);
    }
}
