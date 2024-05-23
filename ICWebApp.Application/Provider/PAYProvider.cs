using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ICWebApp.Application.Provider
{
    public class PAYProvider : IPAYProvider
    {
        private IUnitOfWork _unitOfWork;
        private IAUTHProvider _AuthProvider;
        private ITEXTProvider _TextProvider;
        public PAYProvider(IUnitOfWork _unitOfWork, IAUTHProvider _AuthProvider, ITEXTProvider _TextProvider)
        {
            this._unitOfWork = _unitOfWork;
            this._AuthProvider = _AuthProvider;
            this._TextProvider = _TextProvider;
        }

        public async Task<PAY_Transaction?> GetTransaction(Guid ID)
        {
            var d = await _unitOfWork.Repository<PAY_Transaction>().Where(p => p.ID == ID).Include(p => p.PAY_Transaction_Position).FirstOrDefaultAsync();

            var dataType = await _unitOfWork.Repository<PAY_Type>().ToListAsync();

            if (d != null && dataType != null)
            {
                if (d != null && d.AUTH_Users_ID != null)
                {
                    var user = await _AuthProvider.GetUser(d.AUTH_Users_ID.Value);

                    if (user != null)
                    {
                        d.User = user.Firstname + " " + user.Lastname;
                    }
                }

                if (d != null && d.PAY_Type_ID != null)
                {
                    var type = dataType.FirstOrDefault(p => p.ID == d.PAY_Type_ID.Value);

                    if (type != null && type.TEXT_SystemTexts_Code != null)
                    {
                        d.Type = _TextProvider.Get(type.TEXT_SystemTexts_Code);
                    }
                }                
            }

            return d;
        }
        public async Task<List<PAY_Transaction>?> GetTransactionList(Guid AUTH_Users_ID)
        {
            var data = await _unitOfWork.Repository<PAY_Transaction>().Where(p => p.AUTH_Users_ID == AUTH_Users_ID).Include(p => p.PAY_Transaction_Position).ToListAsync();
            var dataType = await _unitOfWork.Repository<PAY_Type>().ToListAsync();

            if (data != null && dataType != null)
            {
                foreach (var d in data)
                {
                    if (d != null && d.AUTH_Users_ID != null)
                    {
                        var user = await _AuthProvider.GetUser(d.AUTH_Users_ID.Value);

                        if(user != null)
                        {
                            d.User = user.Firstname + " " + user.Lastname;
                        }
                    }

                    if(d != null && d.PAY_Type_ID != null)
                    {
                        var type = dataType.FirstOrDefault(p => p.ID == d.PAY_Type_ID.Value);

                        if (type != null && type.TEXT_SystemTexts_Code != null) 
                        {
                            d.Type = _TextProvider.Get(type.TEXT_SystemTexts_Code);
                        }
                    }
                }
            }

            return data;
        }
        public async Task<List<PAY_Transaction>?> GetTransactionList(bool Payed)
        {
            var data = new List<PAY_Transaction>();

            if (Payed)
            {
                data = await _unitOfWork.Repository<PAY_Transaction>().Where(p => p.PaymentDate != null).Include(p => p.PAY_Transaction_Position).ToListAsync();
            }
            else
            {
                data = await _unitOfWork.Repository<PAY_Transaction>().Where(p => p.PaymentDate == null).Include(p => p.PAY_Transaction_Position).ToListAsync();
            }

            var dataType = await _unitOfWork.Repository<PAY_Type>().ToListAsync();

            if (data != null && dataType != null)
            {
                foreach (var d in data)
                {
                    if (d != null && d.AUTH_Users_ID != null)
                    {
                        var user = await _AuthProvider.GetUser(d.AUTH_Users_ID.Value);

                        if (user != null)
                        {
                            d.User = user.Firstname + " " + user.Lastname;
                        }
                    }

                    if (d != null && d.PAY_Type_ID != null)
                    {
                        var type = dataType.FirstOrDefault(p => p.ID == d.PAY_Type_ID.Value);

                        if (type != null && type.TEXT_SystemTexts_Code != null)
                        {
                            d.Type = _TextProvider.Get(type.TEXT_SystemTexts_Code);
                        }
                    }
                }
            }

            return data;
        }
        public async Task<List<PAY_Transaction>?> GetTransactionList(List<Guid?> IDs)
        {
            var data = await _unitOfWork.Repository<PAY_Transaction>().Where(p => IDs.Contains(p.ID)).Include(p => p.PAY_Transaction_Position).ToListAsync();
            var dataType = await GetTypeList();

            if (data != null && dataType != null)
            {
                foreach (var d in data)
                {
                    if (d != null && d.AUTH_Users_ID != null)
                    {
                        var user = await _AuthProvider.GetUser(d.AUTH_Users_ID.Value);

                        if (user != null)
                        {
                            d.User = user.Firstname + " " + user.Lastname;
                        }
                    }

                    if (d != null && d.PAY_Type_ID != null)
                    {
                        var type = dataType.FirstOrDefault(p => p.ID == d.PAY_Type_ID.Value);

                        if (type != null)
                        {
                            d.Type = type.Name;
                        }
                    }
                }
            }

            return data;
        }
        public async Task<List<PAY_Transaction>?> GetTransactionListByMunicipality(Guid AUTH_Municipality_ID)
        {
            var data = await _unitOfWork.Repository<PAY_Transaction>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).Include(p => p.PAY_Transaction_Position).ToListAsync();
            var dataType = await _unitOfWork.Repository<PAY_Type>().ToListAsync();

            if (data != null && dataType != null)
            {
                foreach (var d in data)
                {
                    if (d != null && d.AUTH_Users_ID != null)
                    {
                        var user = await _AuthProvider.GetUser(d.AUTH_Users_ID.Value);

                        if (user != null)
                        {
                            d.User = user.Firstname + " " + user.Lastname;
                        }
                    }

                    if (d != null && d.PAY_Type_ID != null)
                    {
                        var type = dataType.FirstOrDefault(p => p.ID == d.PAY_Type_ID.Value);

                        if (type != null && type.TEXT_SystemTexts_Code != null)
                        {
                            d.Type = _TextProvider.Get(type.TEXT_SystemTexts_Code);
                        }
                    }
                }
            }

            return data;
        }
        public async Task<PAY_Transaction_Position?> GetTransactionPosition(Guid ID)
        {
            return await _unitOfWork.Repository<PAY_Transaction_Position>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<PAY_Transaction_Position>?> GetTransactionPositionList(Guid PAY_Transaction_ID)
        {
            return await _unitOfWork.Repository<PAY_Transaction_Position>().Where(p => p.PAY_Transaction_ID == PAY_Transaction_ID).ToListAsync();
        }
        public async Task<PAY_Type?> GetType(Guid ID)
        {
            return await _unitOfWork.Repository<PAY_Type>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<PAY_Type>?> GetTypeList()
        {
            var data = await _unitOfWork.Repository<PAY_Type>().ToListAsync();

            if (data != null)
            {
                foreach (var d in data)
                {
                    if (d != null && d.TEXT_SystemTexts_Code != null)
                    {
                        d.Name = _TextProvider.Get(d.TEXT_SystemTexts_Code);
                    }
                }
            }

            return data;
        }
        public async Task<bool> RemoveTransaction(Guid ID)
        {
            return await _unitOfWork.Repository<PAY_Transaction>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveTransactionPosition(Guid ID)
        {
            return await _unitOfWork.Repository<PAY_Transaction_Position>().DeleteAsync(ID);
        }
        public async Task<PAY_Transaction?> SetTransaction(PAY_Transaction Data)
        {
            return await _unitOfWork.Repository<PAY_Transaction>().InsertOrUpdateAsync(Data);
        }
        public async Task<PAY_Transaction?> SetTransactionPayed(Guid PAY_Transaction_ID)
        {
            var trans = await GetTransaction(PAY_Transaction_ID);

            if (trans != null)
            {
                trans.PaymentDate = DateTime.Now;

                return await _unitOfWork.Repository<PAY_Transaction>().InsertOrUpdateAsync(trans);
            }

            return null;
        }
        public async Task<bool> SetTransactionsPayedByFamilyID(Guid PAY_Transaction_Family_ID)
        {
            var transactions = await _unitOfWork.Repository<PAY_Transaction>().Where(p => p.Family_ID == PAY_Transaction_Family_ID).ToListAsync();

            if (transactions != null)
            {
                foreach (var trans in transactions)
                {
                    if (trans != null)
                    {
                        trans.PaymentDate = DateTime.Now;

                        await _unitOfWork.Repository<PAY_Transaction>().InsertOrUpdateAsync(trans);
                    }
                }
            }

            return true;
        }
        public async Task<PAY_Transaction_Position?> SetTransactionPosition(PAY_Transaction_Position Data)
        {
            return await _unitOfWork.Repository<PAY_Transaction_Position>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<V_PAY_Bollo_To_Pay>> GetVPayBolloToPay()
        {
            return await _unitOfWork.Repository<V_PAY_Bollo_To_Pay>().ToListAsync();
        }
        public async Task<PAY_PagoPA_Log?> SetPagoPALog(PAY_PagoPA_Log Data)
        {
            return await _unitOfWork.Repository<PAY_PagoPA_Log>().InsertOrUpdateAsync(Data);
        }
        public async Task<PAY_PagoPa_Identifier?> GetPagoPaDefaultIdentifier()
        {
            return await _unitOfWork.Repository<PAY_PagoPa_Identifier>().Where(p => p.IsDefault == true).FirstOrDefaultAsync();
        }
        public async Task<PAY_PagoPa_Identifier?> GetPagoPaMensaIdentifier()
        {
            return await _unitOfWork.Repository<PAY_PagoPa_Identifier>().Where(p => p.IsMensa == true).FirstOrDefaultAsync();
        }
        public async Task<PAY_PagoPa_Identifier?> GetPagoPaRoomsIdentifier()
        {
            var res = await _unitOfWork.Repository<PAY_PagoPa_Identifier>().Where(p => p.IsRooms == true).ToListAsync();
            return res.FirstOrDefault();
        }

        public async Task<PAY_PagoPa_Identifier?> GetPagoPaBolloIdentifier()
        {
            var res = await _unitOfWork.Repository<PAY_PagoPa_Identifier>().Where(p => p.IsBollo == true).ToListAsync();
            return res.FirstOrDefault();
        }
        public async Task<PAY_PagoPa_Identifier?> GetPagoPaApplicaitonsIdentifier()
        {
            var res = await _unitOfWork.Repository<PAY_PagoPa_Identifier>().Where(p => p.IsApplications == true && p.IsDefault == true).ToListAsync();
            return res.FirstOrDefault();
        }
        public async Task<List<PAY_PagoPa_Identifier>> GetAllPagoPaApplicaitonsIdentifiers(Guid? municipalityId)
        {
            var res = await _unitOfWork.Repository<PAY_PagoPa_Identifier>().Where(p => p.IsApplications == true)
                .ToListAsync();
            if (municipalityId == null)
                return res;
            var munIdents = await _unitOfWork.Repository<PAY_PagoPa_Identifier_Municipalities>()
                .Where(e => e.AUTH_Municipality_ID == municipalityId).ToListAsync();
            return res.Where(e => munIdents.Any(p => p.PAY_PagoPa_Identifier_ID == e.ID)).ToList();
        }
        public async Task<PAY_PagoPa_Identifier?> GetPagoPaIdentifierById(int id)
        {
            return await _unitOfWork.Repository<PAY_PagoPa_Identifier>().FirstOrDefaultAsync(e => e.ID == id);
        }
        public async Task<List<PAY_Transaction>?> GetTransationsByFamilyID(Guid PAY_Transaction_Family_ID)
        {
            return await _unitOfWork.Repository<PAY_Transaction>().Where(p => p.Family_ID == PAY_Transaction_Family_ID).ToListAsync();
        }
        public string GetPagoPaIdentifierByTransaction(PAY_Transaction transaction)
        {
            var pos = _unitOfWork.Repository<PAY_Transaction_Position>()
                .FirstOrDefault(e => e.PAY_Transaction_ID == transaction.ID);
            if (pos != null)
            {
                return pos.PagoPA_Identification ?? "";
            }
            return "";
        }

        public async Task<(List<PAY_Transaction> transactions, int count)> GetUserServicesTransactions(Guid userId, bool payed = true, bool unpayed = true, int count = 6)
        {
            if (payed && unpayed)
            {
                return (await _unitOfWork.Repository<PAY_Transaction>().Where(e =>
                        e.AUTH_Users_ID == userId && e.DisplayInServices == true).Include(e => e.PAY_Transaction_Position).OrderByDescending(e => e.CreationDate)
                    .Take(count).ToListAsync(), await _unitOfWork.Repository<PAY_Transaction>().Where(e =>
                    e.AUTH_Users_ID == userId).CountAsync());
            }
            
            if (payed)
            {
                return (await _unitOfWork.Repository<PAY_Transaction>().Where(e =>
                        e.AUTH_Users_ID == userId && e.PaymentDate != null && e.DisplayInServices == true).Include(e => e.PAY_Transaction_Position).OrderByDescending(e => e.PaymentDate)
                    .Take(count).ToListAsync(), await _unitOfWork.Repository<PAY_Transaction>().Where(e =>
                    e.AUTH_Users_ID == userId && e.PaymentDate != null).CountAsync());
            } 
            
            if (unpayed)
            {
                return (await _unitOfWork.Repository<PAY_Transaction>().Where(e =>
                        e.AUTH_Users_ID == userId && e.PaymentDate == null && e.DisplayInServices == true).Include(e => e.PAY_Transaction_Position).OrderByDescending(e => e.CreationDate)
                    .Take(count).ToListAsync(), await _unitOfWork.Repository<PAY_Transaction>().Where(e =>
                    e.AUTH_Users_ID == userId && e.PaymentDate == null).CountAsync());
            }

            return (new List<PAY_Transaction>(), 0);
        }
    }
}
