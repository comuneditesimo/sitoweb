using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IPAYProvider
    {
        public Task<PAY_Transaction?> GetTransaction(Guid ID);
        public Task<List<PAY_Transaction>?> GetTransactionList(Guid AUTH_Users_ID);
        public Task<List<PAY_Transaction>?> GetTransactionList(bool Payed);
        public Task<List<PAY_Transaction>?> GetTransactionList(List<Guid?> IDs);
        public Task<List<PAY_Transaction>?> GetTransactionListByMunicipality(Guid AUTH_Municipality_ID);
        public Task<PAY_Transaction?> SetTransaction(PAY_Transaction Data);
        public Task<PAY_Transaction?> SetTransactionPayed(Guid PAY_Transaction_ID);
        public Task<bool> SetTransactionsPayedByFamilyID(Guid PAY_Transaction_Family_ID);
        public Task<bool> RemoveTransaction(Guid ID);
        public Task<PAY_Transaction_Position?> GetTransactionPosition(Guid ID);
        public Task<List<PAY_Transaction_Position>?> GetTransactionPositionList(Guid PAY_Transaction_ID);
        public Task<PAY_Transaction_Position?> SetTransactionPosition(PAY_Transaction_Position Data);
        public Task<bool> RemoveTransactionPosition(Guid ID);
        public Task<PAY_Type?> GetType(Guid ID);
        public Task<List<PAY_Type>?> GetTypeList();
        public Task<List<V_PAY_Bollo_To_Pay>> GetVPayBolloToPay();
        public Task<PAY_PagoPA_Log?> SetPagoPALog(PAY_PagoPA_Log Data);
        public Task<PAY_PagoPa_Identifier?> GetPagoPaDefaultIdentifier();
        public Task<PAY_PagoPa_Identifier?> GetPagoPaMensaIdentifier();
        public Task<PAY_PagoPa_Identifier?> GetPagoPaRoomsIdentifier();
        public Task<PAY_PagoPa_Identifier?> GetPagoPaBolloIdentifier();
        public Task<PAY_PagoPa_Identifier?> GetPagoPaApplicaitonsIdentifier();
        public Task<List<PAY_PagoPa_Identifier>> GetAllPagoPaApplicaitonsIdentifiers(Guid? municipalityId);
        public Task<PAY_PagoPa_Identifier?> GetPagoPaIdentifierById(int id);
        public Task<List<PAY_Transaction>?> GetTransationsByFamilyID(Guid PAY_Transaction_Family_ID);
        public string GetPagoPaIdentifierByTransaction(PAY_Transaction transaction);
        public Task<(List<PAY_Transaction> transactions, int count)> GetUserServicesTransactions(Guid userId, bool payed = true, bool unpayed = true, int count = 6);
    }
}
