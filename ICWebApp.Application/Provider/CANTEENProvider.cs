using Freshdesk;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System.Globalization;
using System.Xml.Serialization;
using Telerik.Reporting;
using Telerik.Reporting.Processing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static SQLite.SQLite3;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ICWebApp.DataStore.MSSQL.Repositories;
using PdfSharp.Pdf.Content.Objects;
using ICWebApp.Application.Services;
using Stripe;
using static System.Net.Mime.MediaTypeNames;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.Models.Canteen;
using Stripe.Issuing;
using Chilkat;
using PdfSharp.Pdf.Filters;
using System;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ICWebApp.Domain.Models.Canteen.POS;

namespace ICWebApp.Application.Provider;

public class CANTEENProvider : ICANTEENProvider
{
    private IUnitOfWork _unitOfWork;
    public CANTEENProvider(IUnitOfWork _unitOfWork)
    {
        this._unitOfWork = _unitOfWork;
    }
    public async Task<List<CANTEEN_Subscriber>> GetCancledSubscriberListByDate(Guid CANTEEN_Canteen_ID, DateTime Date)
    {
        var WeekDay = Date.DayOfWeek;
        var MealMovementTypeID = Guid.Parse("99fdff31-46b7-47c7-bfaa-eb63b64217cf");
        var MovementList = await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().Where(p => p.CANTEEN_Subscriber != null
                && p.CANTEEN_Subscriber.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID
                && p.RemovedDate == null && p.CANTEEN_Subscriber.RemovedDate == null
                                                                    && p.Date != null && p.Date.Value.DayOfWeek == WeekDay
                                                                    && p.CancelDate != null
                                                                    && p.Fee <= 0
                                                                    && p.CANTEEN_Subscriber_Movement_Type_ID == MealMovementTypeID
            ).ToListAsync();


        if (MovementList != null)
            return MovementList.Where(p => p.CANTEEN_Subscriber != null).Select(p => p.CANTEEN_Subscriber).ToList();

        return new List<CANTEEN_Subscriber>();
    }
    public async Task<CANTEEN_Canteen?> GetCanteen(Guid ID)
    {
        var data = await _unitOfWork.Repository<CANTEEN_Canteen>().Where(p => p.ID == ID && p.RemovedDate == null).Include(p => p.CANTEEN_School).FirstOrDefaultAsync();

        if (data != null && data.CANTEEN_School != null) 
            data.SchoolName = data.CANTEEN_School.Name;

        return data;
    }
    public async Task<List<CANTEEN_Canteen>> GetCanteens(Guid AUTH_Municipality_ID)
    {
        var data = await _unitOfWork.Repository<CANTEEN_Canteen>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedDate == null).Include(p => p.CANTEEN_School).ToListAsync();

        foreach (var d in data)
            if (d != null && d.CANTEEN_School != null)
                d.SchoolName = d.CANTEEN_School.Name;

        return data;
    }
    public async Task<List<CANTEEN_Canteen>> GetCanteensBySchool(Guid AUTH_Municipality_ID, Guid CANTEEN_School_ID)
    {
        var s2c = await _unitOfWork.Repository<CANTEEN_School_Canteen>().Where(p => p.CANTEEN_School_ID == CANTEEN_School_ID).ToListAsync();

        var data = await _unitOfWork.Repository<CANTEEN_Canteen>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedDate == null).Include(p => p.CANTEEN_School_Canteen).Include(p => p.CANTEEN_School).ToListAsync();

        data = data.Where(a => a.CANTEEN_School_Canteen.Any(a => a.CANTEEN_School_ID == CANTEEN_School_ID)).ToList();

        foreach (var d in data)
            if (d != null && d.CANTEEN_School != null)
                d.SchoolName = d.CANTEEN_School.Name;

        return data;
    }
    public async Task<CANTEEN_School?> GetSchool(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_School>().GetByIDAsync(ID);
    }
    public async Task<List<CANTEEN_School>> GetSchools(Guid AUTH_Municipality_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_School>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedDate == null);
    }
    public async Task<List<CANTEEN_School>> GetSchoolsByCanteen(Guid AUTH_Municipality_ID, Guid CANTEEN_Canteen_ID)  
    {
        return await _unitOfWork.Repository<CANTEEN_School>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedDate == null && p.ID == CANTEEN_Canteen_ID);
    }
    public async Task<List<CANTEEN_School_Canteen>?> GetSchoolsToCanteen(Guid CANTEEN_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_School_Canteen>().ToListAsync(p => p.CANTEEN_Canteen_ID == CANTEEN_ID);
    }
    public async Task<CANTEEN_Subscriber?> GetSubscriber(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.ID == ID && p.RemovedDate == null)
                                                                 .Include(p => p.CANTEEN_Period)
                                                                 .Include(p => p.CANTEEN_Canteen)
                                                                 .Include(p => p.CANTEEN_Subscriber_Status)
                                                                 .Include(p => p.CanteenMenu).FirstOrDefaultAsync();
    }
    public async Task<CANTEEN_Subscriber?> GetActiveSubscriber(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.ID == ID && p.RemovedDate == null && p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted)
                                                                 .Include(p => p.CANTEEN_Period)
                                                                 .Include(p => p.CANTEEN_Canteen)
                                                                 .Include(p => p.CANTEEN_Subscriber_Status)
                                                                 .Include(p => p.CanteenMenu).FirstOrDefaultAsync();
    }
    public async Task<CANTEEN_Subscriber?> GetSubscriberWithoutInclude(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefaultAsync(p => p.ID == ID && p.RemovedDate == null);
    }
    public decimal GetUserBalance(Guid AUTH_Users_ID)
    {
        var data = _unitOfWork.Repository<V_CANTEEN_User>().FirstOrDefault(p => p.AUTH_User_ID == AUTH_Users_ID);

        if (data == null || data.Balance == null) return 0;

        return data.Balance.Value;
    }
    public decimal GetOpenPayent(Guid AUTH_Users_ID)
    {
        var data = _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().Where(p => p.RemovedDate == null && p.CancelDate == null &&
                                                                                   p.AUTH_User_ID == AUTH_Users_ID 
                                                                                   && ((p.PaymentTransaction != null && p.PaymentTransaction.PaymentDate != null) || (p.Fee < 0))).Include(p => p.PaymentTransaction).ToList();

        var result = data.Sum(p => p.Fee);

        if (result == null)
            return 0;

        return result.Value;
    }
    public async Task<List<CANTEEN_Subscriber>> GetSubscriberListByDate(Guid AUTH_Municipality_ID, Guid CANTEEN_Canteen_ID, DateTime Date)
    {
        var WeekDay = Date.DayOfWeek;
        var MealMovementTypeID = Guid.Parse("99fdff31-46b7-47c7-bfaa-eb63b64217cf");
        var MovementList = await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().ToListAsync(p => p.CANTEEN_Subscriber != null
            && p.CANTEEN_Subscriber.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID
            && p.CANTEEN_Subscriber.AUTH_Municipality_ID == AUTH_Municipality_ID
            && p.RemovedDate == null && p.CANTEEN_Subscriber.RemovedDate == null
            && p.Date != null && p.Date.Value.DayOfWeek == WeekDay
            && p.CancelDate == null
            && p.CANTEEN_Subscriber_Movement_Type_ID == MealMovementTypeID
        );


        if (MovementList != null)
            return MovementList
                .Where(p => p.CANTEEN_Subscriber != null &&
                            p.CANTEEN_Subscriber.AUTH_Municipality_ID == AUTH_Municipality_ID)
                .Select(p => p.CANTEEN_Subscriber).ToList();

        return new List<CANTEEN_Subscriber>();
    }
    public async Task<CANTEEN_Subscriber_Movements?> GetSubscriberMovement(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefaultAsync(p => p.ID == ID && p.RemovedDate == null);
    }
    public async Task<List<CANTEEN_Subscriber_Movements>> GetSubscriberMovementsByMovementType(Guid CANTEEN_Subscriber_Movement_Type_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().ToListAsync(p => p.RemovedDate == null && p.CANTEEN_Subscriber_Movement_Type_ID == CANTEEN_Subscriber_Movement_Type_ID);
    }
    public async Task<List<CANTEEN_Subscriber_Movements>> GetSubscriberMovementsBySubscriber(Guid CANTEEN_Subscriber_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().ToListAsync(p => p.RemovedDate == null && p.CANTEEN_Subscriber_ID == CANTEEN_Subscriber_ID);
    }

    public async Task<List<CANTEEN_Subscriber_Movements>> GetFutureMovementsBySubscriber(Guid CanteenSubId)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().ToListAsync(p => p.CANTEEN_Subscriber_ID == CanteenSubId && p.Date >= DateTime.Today);
    }
    public async Task<List<V_CANTEEN_Subscriber_Movements>> GetVSubscriberMovementsBySubscriber(Guid CANTEEN_Subscriber_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber_Movements>().ToListAsync(p =>
            p.RemovedDate == null && p.CANTEEN_Subscriber_ID == CANTEEN_Subscriber_ID);
    }
    public async Task<List<V_CANTEEN_Subscriber_Movements>> GetPastVSubscriberMovementsBySubscriber(Guid CANTEEN_Subscriber_ID)
    {
        var User = await _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefaultAsync(p => p.ID == CANTEEN_Subscriber_ID);

        if (User != null && User.AUTH_Municipality_ID != null)
        {
            var config = await _unitOfWork.Repository<CANTEEN_Configuration>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == User.AUTH_Municipality_ID);

            if (config != null && config.PosMode == true)
            {
                var data = await _unitOfWork.Repository<V_CANTEEN_Subscriber_Movements>().Where(
                p => p.RemovedDate == null && (p.CancelDate == null || p.Used)
                     && p.CANTEEN_Subscriber_ID == CANTEEN_Subscriber_ID
                     && ((p.PaymentTransactionID != null && p.PaymentDate != null)
                         || (p.Used == true)
                         || (p.RefundReferenceID != null))
                ).ToListAsync();

                data = data.OrderByDescending(p => p.Date).ToList();

                return data;
            }
            else
            {
                var data = await _unitOfWork.Repository<V_CANTEEN_Subscriber_Movements>().Where(
                p => p.RemovedDate == null
                     && p.CancelDate == null
                     && p.CANTEEN_Subscriber_ID == CANTEEN_Subscriber_ID
                     && ((p.PaymentTransactionID != null && p.PaymentDate != null)
                         || (p.Date != null && p.Date.Value.AddMinutes(630) < DateTime.Now && p.Fee <= 0 && p.CANTEEN_RequestRefundBalancesID == null)
                         || (p.RefundReferenceID != null)
                         || (p.CANTEEN_RequestRefundBalancesID != null && p.Fee <= 0))
                ).ToListAsync();

                data = data.OrderByDescending(p => p.Date).ToList();

                return data;
            }
        }

        return new List<V_CANTEEN_Subscriber_Movements>();
    }
    public async Task<List<V_CANTEEN_Subscriber_Movements>> GetFutureVSubscriberMovementsBySubscriber(Guid CANTEEN_Subscriber_ID)
    {
        List<V_CANTEEN_Subscriber_Movements> data;
        if (DateTime.Now.Hour >= 10)
        {
            data = await _unitOfWork.Repository<V_CANTEEN_Subscriber_Movements>().Where(
                p => p.RemovedDate == null
                     && p.CANTEEN_Subscriber_ID == CANTEEN_Subscriber_ID
                     && p.Date > DateTime.Now
            ).ToListAsync();
        }
        else
        {
            data = await _unitOfWork.Repository<V_CANTEEN_Subscriber_Movements>().Where(
                p => p.RemovedDate == null
                     && p.CANTEEN_Subscriber_ID == CANTEEN_Subscriber_ID
                     && p.Date >= DateTime.Today
            ).ToListAsync();
        }
        data = data.OrderBy(p => p.Date).ToList();
        return data;
    }
    public async Task<List<CANTEEN_Subscriber_Movements>> GetSubscriberMovementsByUser(Guid AUTH_USER_ID, int? Amount = null)
    {
        var data = await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().Where(p => p.RemovedDate == null && p.CancelDate == null && 
                                                                                           p.AUTH_User_ID == AUTH_USER_ID).Include(p => p.PaymentTransaction).ToListAsync();

        data = data.OrderByDescending(p => p.Date).ToList();

        if (Amount != null)
            return data.Take(Amount.Value).ToList();

        return data;
    }
    public async Task<List<V_CANTEEN_Subscriber_Movements>> GetVSubscriberMovementsByUser(Guid AUTH_USER_ID, int? Amount = null)
    {
        var data = await _unitOfWork.Repository<V_CANTEEN_Subscriber_Movements>().Where(p => p.RemovedDate == null && p.CancelDate == null &&
            p.AUTH_User_ID == AUTH_USER_ID).ToListAsync();

        data = data.OrderByDescending(p => p.Date).ToList();

        return Amount != null ? data.Take(Amount.Value).ToList() : data;
    }
    public async Task<List<V_CANTEEN_Subscriber_Movements>> GetPastVSubscriberMovementsByUser(Guid AUTH_USER_ID, int? Amount = null)
    {
        var User = await _unitOfWork.Repository<AUTH_Users>().FirstOrDefaultAsync(p => p.ID == AUTH_USER_ID);

        if(User != null && User.AUTH_Municipality_ID != null)
        {
            var config = await _unitOfWork.Repository<CANTEEN_Configuration>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == User.AUTH_Municipality_ID);

            if (config != null && config.PosMode == true)
            {
                var data = await _unitOfWork.Repository<V_CANTEEN_Subscriber_Movements>().Where(
                    p => p.RemovedDate == null && (p.CancelDate == null || p.Used)
                            && p.AUTH_User_ID == AUTH_USER_ID
                            && ((p.PaymentTransactionID != null && p.PaymentDate != null)
                                || (p.Date != null && p.Date.Value.AddMinutes(630) < DateTime.Now && p.Fee <= 0 && p.CANTEEN_RequestRefundBalancesID == null)
                                || (p.RefundReferenceID != null)
                                || (p.CANTEEN_RequestRefundBalancesID != null && p.Fee <= 0)
                                || (p.Used == true))
                ).ToListAsync();
                data = data.OrderByDescending(p => p.Date).ToList();

                return Amount != null ? data.Take(Amount.Value).ToList() : data;
            }
            else
            {
                var data = await _unitOfWork.Repository<V_CANTEEN_Subscriber_Movements>().Where(
                    p => p.RemovedDate == null
                            && p.CancelDate == null
                            && p.AUTH_User_ID == AUTH_USER_ID
                            && ((p.PaymentTransactionID != null && p.PaymentDate != null)
                                || (p.Date != null && p.Date.Value.AddMinutes(630) < DateTime.Now && p.Fee <= 0 && p.CANTEEN_RequestRefundBalancesID == null)
                                || (p.RefundReferenceID != null)
                                || (p.CANTEEN_RequestRefundBalancesID != null && p.Fee <= 0))
                ).ToListAsync();
                data = data.OrderByDescending(p => p.Date).ToList();

                return Amount != null ? data.Take(Amount.Value).ToList() : data;
            }
        }

        return new List<V_CANTEEN_Subscriber_Movements>();
    }
    public async Task<CANTEEN_Subscriber_Movements?> GetSubscriberMovementById(Guid Id)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().GetByIDAsync(Id);
    }
    public async Task<CANTEEN_Subscriber_Movements?> GetSubscriberMovementByRequestID(Guid RequestRefundBalanceID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefaultAsync(p => p.CANTEEN_RequestRefundBalancesID == RequestRefundBalanceID && p.RemovedDate == null);
    }
    public async Task<List<CANTEEN_Subscriber_Movements>> GetSubscriberMovementsByCanteen(Guid CANTEEN_ID, bool WithRemoved = false)
    {
        if (WithRemoved)
        {
            return await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().Where(p => p.CANTEEN_Subscriber.CANTEEN_Canteen_ID == CANTEEN_ID).Include(p => p.CANTEEN_Subscriber).ToListAsync();
        }

        return await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().Where(p => p.CANTEEN_Subscriber.CANTEEN_Canteen_ID == CANTEEN_ID && p.RemovedDate == null).Include(p => p.CANTEEN_Subscriber).ToListAsync();
    }
    public async Task<List<CANTEEN_Subscriber_Movements>> GetSubscriberMovementsBySchool(Guid School_ID, Guid? SchoolClass_ID = null, bool WithRemoved = false)
    {
        var data = _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().Where(p => p.CANTEEN_Subscriber.CANTEEN_School_ID == School_ID);

        if (SchoolClass_ID != null)
        {
            data = data.Where(p => p.CANTEEN_Subscriber.SchoolClassID == SchoolClass_ID);
        }

        if (WithRemoved == false)
        {
            data = data.Where(p => p.RemovedDate == null);
        }

        return await data.Include(p => p.CANTEEN_Subscriber).ToListAsync();
    }
    public async Task<List<V_CANTEEN_Daily_Movements>> GetSubscriberMovementsDaily(Guid AUTH_Municipality_ID, Guid LanguageID, DateTime? Day = null)
    {
        if (Day == null)
        {
            Day = DateTime.Now;
        }

        return await _unitOfWork.Repository<V_CANTEEN_Daily_Movements>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_LanguagesID == LanguageID
                                                                                     && p.Date != null && p.Date.Value.Year == Day.Value.Year && p.Date.Value.Month == Day.Value.Month
                                                                                     && p.Date.Value.Day == Day.Value.Day);
    }
    public async Task<List<V_CANTEEN_Statistic_Students>> GetStatisticStudents(Guid AUTH_Municipality_ID, Guid LanguageID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Statistic_Students>().ToListAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_LanguagesID == LanguageID);
    }
    public async Task<List<CANTEEN_Subscriber_Movement_Type>> GetSubscriberMovementTypes()
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Movement_Type>().ToListAsync();
    }
    public async Task<List<CANTEEN_Subscriber>> GetSubscribersByCanteenID(Guid CANTEEN_Canteen_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.RemovedDate == null && p.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID 
                                                                             && p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted).Include(p => p.CANTEEN_School).ToListAsync();
    }
    public async Task<List<CANTEEN_Subscriber>> GetSubscribersBySchoolID(Guid CANTEEN_School_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.RemovedDate == null && p.CANTEEN_School_ID == CANTEEN_School_ID)
                                                                 .Include(p => p.CANTEEN_School)
                                                                 .Include(p => p.CANTEEN_Canteen)
                                                                 .Include(p => p.CANTEEN_Subscriber_Status)
                                                                 .OrderByDescending(o => o.FirstName)
                                                                 .ThenBy(o => o.LastName)
                                                                 .ToListAsync();
    }
    public async Task<List<CANTEEN_Subscriber>> GetSubscribersByStatusID(Guid CANTEEN_Subscriber_Status_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.RemovedDate == null && p.CANTEEN_Subscriber_Status_ID == CANTEEN_Subscriber_Status_ID)
                                                                 .Include(p => p.CANTEEN_School)
                                                                 .Include(p => p.CANTEEN_Canteen)
                                                                 .Include(p => p.CANTEEN_Subscriber_Status)
                                                                 .Include(p => p.CanteenMenu)
                                                                 .OrderByDescending(o => o.CreationDate)
                                                                 .ToListAsync();
    }
    public async Task<List<V_CANTEEN_Subscriber>> GetVSubscribersByStatusID(Guid CANTEEN_Subscriber_Status_ID, Guid LanguageID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber>().Where(p => p.RemovedDate == null && p.CANTEEN_Subscriber_Status_ID == CANTEEN_Subscriber_Status_ID && p.LANG_LanguagesID == LanguageID)
                                                                   .OrderByDescending(o => o.CreationDate).ToListAsync();
    }
    public async Task<V_CANTEEN_Subscriber?> GetVSubscriber(Guid CANTEEN_Subscriber_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber>().FirstOrDefaultAsync(p => p.ID == CANTEEN_Subscriber_ID);
    }
    public async Task<V_CANTEEN_Subscriber?> GetVSubscriber(string Taxnumber)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber>().Where(p => p.TaxNumber == Taxnumber && p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted
                                                                            && p.SuccessorSubscriptionID == null).OrderByDescending(p => p.CreationDate).FirstOrDefaultAsync();
    }
    public async Task<List<CANTEEN_Subscriber>> GetSubscribersByFamilyID(Guid CANTEEN_Subscriber_Family_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.RemovedDate == null && p.SubscriptionFamilyID == CANTEEN_Subscriber_Family_ID).AsNoTracking()
                                                                 .Include(p => p.CANTEEN_School)
                                                                 .Include(p => p.CANTEEN_Canteen)
                                                                 .Include(p => p.CANTEEN_Subscriber_Status)
                                                                 .Include(p => p.CanteenMenu)
                                                                 .OrderByDescending(o => o.CreationDate)
                                                                 .ToListAsync();
    }
    public async Task<List<CANTEEN_Subscriber>> GetSubscribersByUserID(Guid AUTH_Users_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.RemovedDate == null && p.AUTH_Users_ID == AUTH_Users_ID)
                                                                 .Include(p => p.CANTEEN_School)
                                                                 .Include(p => p.CANTEEN_Canteen)
                                                                 .Include(p => p.CANTEEN_Subscriber_Status)
                                                                 .Include(p => p.CanteenMenu)
                                                                 .OrderByDescending(o => o.CreationDate)
                                                                 .ToListAsync();
    }
    public async Task<List<V_CANTEEN_Subscriber>> GetVSubscribersByUserID(Guid AUTH_Users_ID, Guid LanguageID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber>().Where(p => p.RemovedDate == null && p.AUTH_Users_ID == AUTH_Users_ID && p.LANG_LanguagesID == LanguageID)
                                                                   .OrderByDescending(o => o.CreationDate).ToListAsync();
    }
    public async Task<List<CANTEEN_Subscriber>> GetSubscribersByWeekDay(Guid CANTEEN_Canteen_ID, DayOfWeek WeekDay)
    {
        if (WeekDay == DayOfWeek.Monday)
            return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p =>
                    p.RemovedDate == null && p.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID && p.DayMo)
                .Include(p => p.CANTEEN_School)
                .Include(p => p.CANTEEN_Canteen)
                .Include(p => p.CANTEEN_Subscriber_Status)
                .ToListAsync();
        if (WeekDay == DayOfWeek.Tuesday)
            return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p =>
                    p.RemovedDate == null && p.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID && p.DayTue)
                .Include(p => p.CANTEEN_School)
                .Include(p => p.CANTEEN_Canteen)
                .Include(p => p.CANTEEN_Subscriber_Status)
                .ToListAsync();
        if (WeekDay == DayOfWeek.Wednesday)
            return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p =>
                    p.RemovedDate == null && p.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID && p.DayWed)
                .Include(p => p.CANTEEN_School)
                .Include(p => p.CANTEEN_Canteen)
                .Include(p => p.CANTEEN_Subscriber_Status)
                .ToListAsync();
        if (WeekDay == DayOfWeek.Thursday)
            return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p =>
                    p.RemovedDate == null && p.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID && p.DayThu)
                .Include(p => p.CANTEEN_School)
                .Include(p => p.CANTEEN_Canteen)
                .Include(p => p.CANTEEN_Subscriber_Status)
                .ToListAsync();
        if (WeekDay == DayOfWeek.Friday)
            return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p =>
                    p.RemovedDate == null && p.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID && p.DayFri)
                .Include(p => p.CANTEEN_School)
                .Include(p => p.CANTEEN_Canteen)
                .Include(p => p.CANTEEN_Subscriber_Status)
                .ToListAsync();
        if (WeekDay == DayOfWeek.Saturday)
            return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p =>
                    p.RemovedDate == null && p.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID && p.DaySat)
                .Include(p => p.CANTEEN_School)
                .Include(p => p.CANTEEN_Canteen)
                .Include(p => p.CANTEEN_Subscriber_Status)
                .ToListAsync();
        if (WeekDay == DayOfWeek.Sunday)
            return await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p =>
                    p.RemovedDate == null && p.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID && p.DaySun)
                .Include(p => p.CANTEEN_School)
                .Include(p => p.CANTEEN_Canteen)
                .Include(p => p.CANTEEN_Subscriber_Status)
                .ToListAsync();

        return new List<CANTEEN_Subscriber>();
    }
    public async Task<List<CANTEEN_Subscriber_Status>> GetSubscriberStatuses()
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Status>().Where().OrderBy(o => o.SortOrder).ToListAsync();
    }
    public async Task<bool> RemoveCanteen(Guid ID)
    {
        var item = await _unitOfWork.Repository<CANTEEN_Canteen>().GetByIDAsync(ID);

        if(item != null)
        {
            item.RemovedDate = DateTime.Now;

            await _unitOfWork.Repository<CANTEEN_Canteen>().InsertOrUpdateAsync(item);
        }

        return true;
    }
    public async Task<bool> RemoveSchool(Guid ID)
    {
        var item = await _unitOfWork.Repository<CANTEEN_School>().GetByIDAsync(ID);

        if (item != null)
        {
            item.RemovedDate = DateTime.Now;

            await _unitOfWork.Repository<CANTEEN_School>().InsertOrUpdateAsync(item);
        }

        return true;
    }
    public async Task<bool> RemoveSchoolsToCanteen(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_School_Canteen>().DeleteAsync(ID);
    }
    public async Task<bool> RemoveSubscriber(Guid ID, bool force = false)
    {
        var item = await _unitOfWork.Repository<CANTEEN_Subscriber>().GetByIDAsync(ID);

        if (item != null)
        {
            if (!force)
            {
                item.RemovedDate = DateTime.Now;

                await _unitOfWork.Repository<CANTEEN_Subscriber>().InsertOrUpdateAsync(item);
            }
            else
            {

                await _unitOfWork.Repository<CANTEEN_Subscriber>().DeleteAsync(ID);
            }
        }

        return true;
    }
    public async Task<bool> RemoveSubscriberMovement(Guid ID)
    {
        var item = await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().GetByIDAsync(ID);

        if (item != null)
        {
            item.RemovedDate = DateTime.Now;
            //Not the error source
            await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdateAsync(item);
        }

        return true;
    }
    public async Task<CANTEEN_Canteen?> SetCanteen(CANTEEN_Canteen Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Canteen>().InsertOrUpdateAsync(Data);
    }
    public async Task<CANTEEN_School?> SetSchool(CANTEEN_School Data)
    {
        return await _unitOfWork.Repository<CANTEEN_School>().InsertOrUpdateAsync(Data);
    }
    public async Task<CANTEEN_School_Canteen?> SetSchoolsToCanteen(CANTEEN_School_Canteen Data)
    {
        return await _unitOfWork.Repository<CANTEEN_School_Canteen>().InsertOrUpdateAsync(Data);
    }
    public async Task<CANTEEN_Subscriber?> SetSubscriber(CANTEEN_Subscriber Data)
    {
        if (Data.FirstName != null)
            Data.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.FirstName.ToLower());

        if (Data.LastName != null)
            Data.LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.LastName.ToLower());

        if (Data.Child_PlaceOfBirth != null)
            Data.Child_PlaceOfBirth = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.Child_PlaceOfBirth.ToLower());

        if (Data.Child_PlaceOfBirth != null)
            Data.Child_PlaceOfBirth = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.Child_PlaceOfBirth.ToLower());

        if (Data.Child_DomicileStreetAddress != null)
            Data.Child_DomicileStreetAddress = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Data.Child_DomicileStreetAddress.ToLower());

        if (Data.Version == null)
        {
            Data.Version = 1;
        }

        if (string.IsNullOrEmpty(Data.TelCode))
        {
            Random r = new Random();
            bool ok = false;
            string strPin = "";

            Data.TelCode = "0000";

            while (ok == false)
            {
                int pin = r.Next(1, 9999);
                strPin = pin.ToString("D4");
                var usr = _unitOfWork.Repository<CANTEEN_User>().FirstOrDefault(a => a.TelPin == strPin);
                if (usr == null)
                {
                    string strPinInner = pin.ToString("D4");
                    ok = true;
                    Data.TelCode = strPinInner;
                }
            }
        }

        if (Data.ID == Guid.Empty)
        {
            Data.ID = Guid.NewGuid();
        }

        Data.CANTEEN_Period = null;
        Data.CANTEEN_Canteen = null;
        Data.CANTEEN_School = null;
        Data.CANTEEN_Subscriber_Status = null;
        Data.CanteenMenu = null;
        Data.CANTEEN_Subscriber_Movements = null;

        var result = await _unitOfWork.Repository<CANTEEN_Subscriber>().InsertOrUpdateAsync(Data);

        var user = await _unitOfWork.Repository<CANTEEN_User>().Where(p => p.AUTH_User_ID == Data.AUTH_Users_ID && p.MunicipalityID == Data.AUTH_Municipality_ID).FirstOrDefaultAsync();

        if (user == null)
        {
            user = new CANTEEN_User();
            user.AUTH_User_ID = Data.AUTH_Users_ID;
            user.MunicipalityID = Data.AUTH_Municipality_ID;
            user.Creationdate = DateTime.Now;
            user.FullName = Data.UserFirstName + " " + Data.UserLastName;
            user.TaxNumber = Data.UserTaxNumber;

            await SetCanteenUser(user);
        }


        return result;
    }
    public async Task<CANTEEN_Subscriber?> CloneAndArchiveSubscriber(Guid SubscriberID)
    {
        var Data = await _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefaultAsync(p => p.RemovedDate == null && p.ID == SubscriberID);

        if (Data != null)
        {
            var newSubscriber = new CANTEEN_Subscriber();

            newSubscriber.ID = Guid.NewGuid();
            newSubscriber.CreationDate = DateTime.Now;
            newSubscriber.AUTH_Users_ID = Data.AUTH_Users_ID;
            newSubscriber.AUTH_Municipality_ID = Data.AUTH_Municipality_ID;
            newSubscriber.FirstName = Data.FirstName;
            newSubscriber.LastName = Data.LastName;
            newSubscriber.TaxNumber = Data.TaxNumber;
            newSubscriber.Begindate = DateTime.Today;
            newSubscriber.Enddate = Data.Enddate;
            newSubscriber.DayMo = Data.DayMo;
            newSubscriber.DayTue = Data.DayTue;
            newSubscriber.DayWed = Data.DayWed;
            newSubscriber.DayThu = Data.DayThu;
            newSubscriber.DayFri = Data.DayFri;
            newSubscriber.DaySat = Data.DaySat;
            newSubscriber.DaySun = Data.DaySun;
            newSubscriber.IsVegan = Data.IsVegan;
            newSubscriber.IsVegetarian = Data.IsVegetarian;
            newSubscriber.IsGlutenIntolerance = Data.IsGlutenIntolerance;
            newSubscriber.IsBothParentEmployed = Data.IsBothParentEmployed;
            newSubscriber.IsManualInput = Data.IsManualInput;
            newSubscriber.IsLactoseIntolerance = Data.IsLactoseIntolerance;
            newSubscriber.FILE_FileInfo_SpecialMenu_ID = Data.FILE_FileInfo_SpecialMenu_ID;
            newSubscriber.IsWithoutPorkMeat = Data.IsWithoutPorkMeat;
            newSubscriber.DistanceFromSchool = Data.DistanceFromSchool;
            newSubscriber.AdditionalIntolerance = Data.AdditionalIntolerance;
            newSubscriber.CANTEEN_Subscriber_Status_ID = CanteenStatus.Incomplete;
            newSubscriber.ReferenceID = Data.ReferenceID;
            newSubscriber.CANTEEN_Canteen_ID = Data.CANTEEN_Canteen_ID;
            newSubscriber.CANTEEN_School_ID = Data.CANTEEN_School_ID;
            newSubscriber.SchoolClassID = Data.SchoolClassID;
            newSubscriber.RemovedDate = Data.RemovedDate;
            newSubscriber.CANTEEN_Period_ID = Data.CANTEEN_Period_ID;
            newSubscriber.SubscriptionFamilyID = Guid.NewGuid();
            newSubscriber.SignedDate = null;
            newSubscriber.AUTH_Municipality_ID = Data.AUTH_Municipality_ID;
            newSubscriber.UserAdress = Data.UserAdress;
            newSubscriber.UserCountryOfBirth = Data.UserCountryOfBirth;
            newSubscriber.UserDateOfBirth = Data.UserDateOfBirth;
            newSubscriber.UserDomicileMunicipality = Data.UserDomicileMunicipality;
            newSubscriber.UserDomicileNation = Data.UserDomicileNation;
            newSubscriber.UserDomicilePostalCode = Data.UserDomicilePostalCode;
            newSubscriber.UserDomicileProvince = Data.UserDomicileProvince;
            newSubscriber.UserDomicileStreetAdress = Data.UserDomicileStreetAdress;
            newSubscriber.UserEmail = Data.UserEmail;
            newSubscriber.UserFirstName = Data.UserFirstName;
            newSubscriber.UserLastName = Data.UserLastName;
            newSubscriber.UserTaxNumber = Data.UserTaxNumber;
            newSubscriber.UserGender = Data.UserGender;
            newSubscriber.UserCountryOfBirth = Data.UserCountryOfBirth;
            newSubscriber.UserDateOfBirth = Data.UserDateOfBirth;
            newSubscriber.UserMobilePhone = Data.UserMobilePhone;
            newSubscriber.SchoolyearDescription = Data.SchoolyearDescription;
            newSubscriber.SchoolyearID = Data.SchoolyearID;
            newSubscriber.SchoolClass = Data.SchoolClass;
            newSubscriber.SchoolName = Data.SchoolName;
            newSubscriber.CanteenMenuID = Data.CanteenMenuID;
            newSubscriber.TelCode = Data.TelCode;
            newSubscriber.MenuName = Data.MenuName;
            newSubscriber.DayString = Data.DayString;
            newSubscriber.PreviousSubscriptionID = Data.ID;
            newSubscriber.CreationDate = DateTime.Now;
            newSubscriber.Version = (Data.Version ?? 1) + 1;
            newSubscriber.PrivacyDate = Data.PrivacyDate;
            newSubscriber.Child_PlaceOfBirth = Data.Child_PlaceOfBirth;
            newSubscriber.Child_DateOfBirth = Data.Child_DateOfBirth;
            newSubscriber.Child_DomicileMunicipality = Data.Child_DomicileMunicipality;
            newSubscriber.Child_DomicileNation = Data.Child_DomicileNation;
            newSubscriber.Child_DomicilePostalCode = Data.Child_DomicilePostalCode;
            newSubscriber.Child_DomicileProvince = Data.Child_DomicileProvince;
            newSubscriber.Child_DomicileStreetAddress = Data.Child_DomicileStreetAddress;
            newSubscriber.Child_Domicile_Municipal_ID = Data.Child_Domicile_Municipal_ID;

            Data.SuccessorSubscriptionID = newSubscriber.ID;

            var result = await _unitOfWork.Repository<CANTEEN_Subscriber>().InsertOrUpdateAsync(newSubscriber);
            await _unitOfWork.Repository<CANTEEN_Subscriber>().InsertOrUpdateAsync(Data);

            var user = await _unitOfWork.Repository<CANTEEN_User>().Where(p => p.AUTH_User_ID == Data.AUTH_Users_ID && p.MunicipalityID == Data.AUTH_Municipality_ID).FirstOrDefaultAsync();

            if (user == null)
            {
                user = new CANTEEN_User();
                user.AUTH_User_ID = Data.AUTH_Users_ID;
                user.MunicipalityID = Data.AUTH_Municipality_ID;
                user.Creationdate = DateTime.Now;
                user.FullName = Data.UserFirstName + " " + Data.UserLastName;
                user.TaxNumber = Data.UserTaxNumber;

                await SetCanteenUser(user);
            }

            return result;

        }

        return null;
    }
    public List<CANTEEN_Subscriber> GetSubscribersByUserIDSync(Guid AUTH_Users_ID)
    {
        return _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.RemovedDate == null && p.AUTH_Users_ID == AUTH_Users_ID && p.SuccessorSubscriptionID == null)
                .Include(p => p.CANTEEN_School)
                .Include(p => p.CANTEEN_Canteen)
                .Include(p => p.CANTEEN_Subscriber_Status)
                .Include(p => p.CanteenMenu)
                .OrderByDescending(o => o.CreationDate)
                .ToList();
    }
    public List<CANTEEN_Subscriber> GetActiveSubscribersByUserIDSync(Guid AUTH_Users_ID)
    {
        return _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.RemovedDate == null && p.AUTH_Users_ID == AUTH_Users_ID && p.SuccessorSubscriptionID == null && p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted)
                .Include(p => p.CANTEEN_School)
                .Include(p => p.CANTEEN_Canteen)
                .Include(p => p.CANTEEN_Subscriber_Status)
                .Include(p => p.CanteenMenu)
                .OrderByDescending(o => o.CreationDate)
                .ToList();
    }
    public async Task<CANTEEN_Subscriber_Movements?> GetRemovedRefundMovement(Guid MovementID)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefaultAsync(p => p.RemovedDate != null && p.RefundReferenceID == MovementID && p.CANTEEN_Subscriber_Movement_Type_ID == Guid.Parse("b02530c0-3564-4fbc-b1b2-cdee66ebd3ea"));
    }
    public async Task<CANTEEN_Subscriber_Movements?> SetSubscriberMovement(CANTEEN_Subscriber_Movements Data)
    {
        //maybe error source
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> TaxNumberExists(Guid AUTH_Municipality_ID, Guid CANTEEN_Schoolyear_ID, string TaxNumber)
    {
        var result = await _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefaultAsync(p =>
                p.AUTH_Municipality_ID == AUTH_Municipality_ID &&
                p.SchoolyearID == CANTEEN_Schoolyear_ID &&
                p.RemovedDate == null && p.TaxNumber == TaxNumber && p.Archived == null && p.CANTEEN_Subscriber_Status_ID != CanteenStatus.Denied && p.CANTEEN_Subscriber_Status_ID != CanteenStatus.Archived &&  p.CANTEEN_Subscriber_Status_ID != CanteenStatus.Disabled);

        if (result != null) return true;

        return false;
    }
    public async Task<List<CANTEEN_Period>> GetPeriods(Guid AUTH_Municipality_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Period>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedDate == null).ToListAsync();
    }
    public async Task<CANTEEN_Period?> GetPeriod(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Period>().FirstOrDefaultAsync(p => p.RemovedDate == null && p.ID == ID);
    }
    public async Task<CANTEEN_Period?> GetPeriodByCanteen(Guid CANTEEN_Canteen_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Period>().FirstOrDefaultAsync(p => p.RemovedDate == null && p.CANTEEN_Canteen_ID == CANTEEN_Canteen_ID && p.Active && p.ToDate > DateTime.Now);
    }
    public async Task<CANTEEN_Period?> SetPeriod(CANTEEN_Period Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Period>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemovePeriod(Guid ID)
    {
        var item = await _unitOfWork.Repository<CANTEEN_Period>().GetByIDAsync(ID);

        if (item != null)
        {
            item.RemovedDate = DateTime.Now;

            await _unitOfWork.Repository<CANTEEN_Period>().InsertOrUpdateAsync(item);
        }

        return true;
    }
    public async Task<long> GetNextSubscriberNummer()
    {
        try
        {
            var lastNumber = await _unitOfWork.Repository<CANTEEN_Subscriber>().Where().MaxAsync(p => p.ReferenceID);
            return lastNumber + 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return 1;
        }
    }
    public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
    {
        for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            yield return day;
    }
    public async Task<bool> CreateSubscriber_Movements(Guid SubscriberID)
    {
        var sub = await _unitOfWork.Repository<CANTEEN_Subscriber>().GetByIDAsync(SubscriberID);
        var canteen = await _unitOfWork.Repository<CANTEEN_Canteen>().GetByIDAsync(sub.CANTEEN_Canteen_ID);

        if (canteen != null)
        {
            var hollidays = await _unitOfWork.Repository<CANTEEN_Period>().Where(p => p.AUTH_Municipality_ID == canteen.AUTH_Municipality_ID && p.RemovedDate == null).ToListAsync();

            hollidays = hollidays.Where(a => a.CANTEEN_Canteen_ID == sub.CANTEEN_Canteen_ID ||
                                            (a.SchoolID == sub.CANTEEN_School_ID && a.SchoolClassID == null) ||
                                            (a.SchoolID == sub.CANTEEN_School_ID && a.SchoolClassID == sub.SchoolClassID)).ToList();

            var movements = await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().Where(p => p.RemovedDate == null && p.CANTEEN_Subscriber_ID == SubscriberID)
                                                                                        .Include(p => p.PaymentTransaction).ToListAsync();

            var startdate = sub.Begindate ?? DateTime.Today;
            if (startdate <= DateTime.Today) startdate = DateTime.Today.AddDays(1);

            foreach (var day in EachDay(startdate, sub.Enddate ?? DateTime.Today))
            {
                //Problem --> Status wert net auf archiviert gsetzt wenn nuie request ungnummen wert also werdn für archivierte 
                //subscriber olle movements noamol erstellt.
                //also hon i filter für archived und removed date dazua getun.
                if (sub.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted && sub.Archived == null && sub.RemovedDate == null) //ACCEPTED
                {
                    var dayoftheWeek = day.DayOfWeek;

                    if (sub.DayMo && dayoftheWeek == DayOfWeek.Monday ||
                        sub.DayTue && dayoftheWeek == DayOfWeek.Tuesday ||
                        sub.DayWed && dayoftheWeek == DayOfWeek.Wednesday ||
                        sub.DayThu && dayoftheWeek == DayOfWeek.Thursday ||
                        sub.DayFri && dayoftheWeek == DayOfWeek.Friday ||
                        sub.DaySat && dayoftheWeek == DayOfWeek.Saturday ||
                        sub.DaySun && dayoftheWeek == DayOfWeek.Sunday)
                        if (movements.Where(a => a.Date == day).Count() == 0 &&
                            hollidays.Where(a => a.FromDate == day && a.PeriodType == "CLOSED").Count() == 0
                           )
                        {
                            var mov = new CANTEEN_Subscriber_Movements();
                            mov.CANTEEN_Subscriber_ID = SubscriberID;
                            mov.Date = day;
                            mov.Fee = (canteen.PricePerUse ?? 0) * -1;
                            mov.Description = canteen.Name + " " + sub.FirstName + " " + sub.LastName;
                            mov.CANTEEN_Subscriber_Movement_Type_ID = Guid.Parse("99fdff31-46b7-47c7-bfaa-eb63b64217cf"); //CANTEEN_SUBSCRIBER_MOVEMENT_TYPE_MEAL
                            mov.AUTH_User_ID = sub.AUTH_Users_ID;
                            //maybe error source
                            await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdateAsync(mov);
                        }
                }
            }
        }

        return true;
    }
    public async Task<List<V_CANTEEN_Schoolyear_Current>> GetSchoolsyearsCurrent(Guid AuthMunicipalityID, bool ByRegistration = false)
    {
        var actYear = DateTime.Today.Year;
        var actMonth = DateTime.Today.Month;

        if (ByRegistration)
        {
            return await _unitOfWork.Repository<V_CANTEEN_Schoolyear_Current>().Where(a => a.AUTH_Municipality_ID == AuthMunicipalityID && (a.RegisterBeginDate <= DateTime.Today 
                                                                                        && a.RegisterEndDate >= DateTime.Today))
                                                                               .OrderBy(o => o.BeginYear).ToListAsync();
        }
        else
        {
            return await _unitOfWork.Repository<V_CANTEEN_Schoolyear_Current>().Where(a => a.AUTH_Municipality_ID == AuthMunicipalityID && (a.BeginDate <= DateTime.Today 
                                                                                        && a.EndDate >= DateTime.Today))
                                                                               .OrderBy(o => o.BeginYear).ToListAsync();
        }
    }
    public async Task<List<V_CANTEEN_Schoolyear>> GetSchoolsyearAll(Guid AuthMunicipalityID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Schoolyear>().Where(a => a.AUTH_Municipality_ID == AuthMunicipalityID).OrderBy(o => o.BeginYear).ToListAsync();
    }
    public CANTEEN_User? GetCanteenUserByID(Guid AUTH_User_ID, Guid municipalityID)
    {
        return _unitOfWork.Repository<CANTEEN_User>().Where(p => p.AUTH_User_ID == AUTH_User_ID && p.MunicipalityID == municipalityID).FirstOrDefault();
    }
    public async Task<List<CANTEEN_User>> GetCanteenUserList(Guid municipalityID)
    {
        return await _unitOfWork.Repository<CANTEEN_User>().Where(p => p.MunicipalityID == municipalityID).ToListAsync();
    }
    public async Task<CANTEEN_User?> SetCanteenUser(CANTEEN_User Data)
    {
        if (string.IsNullOrEmpty(Data.TelPin))
        {
            Random r = new Random();
            bool ok = false;
            string strPin = "";
            while (ok == false)
            {
                int pin = r.Next(1, 99999999);
                strPin = pin.ToString("D8");
                var usr = await _unitOfWork.Repository<CANTEEN_User>().FirstOrDefaultAsync(a => a.TelPin == strPin);
                if (usr == null)
                {
                    ok = true;
                    Data.TelPin = strPin;
                }
            }
        }

        return await _unitOfWork.Repository<CANTEEN_User>().InsertOrUpdateAsync(Data);
    }
    public async Task<CANTEEN_MealMenu?> GetCANTEEN_MealMenuByID(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_MealMenu>().GetByIDAsync(ID);
    }
    public async Task<List<CANTEEN_MealMenu>> GetCANTEEN_MealMenuList(Guid Municipality_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_MealMenu>().Where(p => p.AUTH_Municipality_ID == Municipality_ID).ToListAsync();
    }
    public async Task<CANTEEN_MealMenu?> SetCANTEEN_MealMenu(CANTEEN_MealMenu Data)
    {
        return await _unitOfWork.Repository<CANTEEN_MealMenu>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveCANTEEN_MealMenu(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_MealMenu>().DeleteAsync(ID);
    }
    public string? GetApiOperationsByPhoneCode(string parentpin, string childcode, string operation)
    {
        var user = _unitOfWork.Repository<CANTEEN_User>().FirstOrDefault(a => a.TelPin == parentpin);
        if (user == null)
        {
            return null;
        }

        var child = _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.RemovedDate == null && p.AUTH_Users_ID == user.AUTH_User_ID &&
                p.TelCode == childcode && p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted)
                                                                .Include(p => p.CANTEEN_School)
                                                                .Include(p => p.CANTEEN_Canteen)
                                                                .Include(p => p.CANTEEN_Subscriber_Status)
                                                                .Include(p => p.CanteenMenu)
                                                                .FirstOrDefault();

        if (child == null)
        {
            return null;
        }

        var nextMovements = _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().Where(p => p.RemovedDate == null &&
                                                                                              p.AUTH_User_ID == user.AUTH_User_ID &&
                                                                                              p.CANTEEN_Subscriber_ID == child.ID &&
                                                                                              p.Fee <= 0 &&
                                                                                              p.Date >= DateTime.Today)
                                                                                   .OrderBy(p => p.Date)
                                                                                   .ToList();

        if (DateTime.Now.Hour >= 10)
        {
            nextMovements = nextMovements.Where(a => a.Date > DateTime.Today)
                .OrderBy(p => p.Date)
                .ToList();
        }

        if (nextMovements == null || nextMovements.Count == 0)
        {
            return null;
        }

        if (operation == null)
        {
            return null;
        }


        //GetDate
        if (operation == "GetDate1")
        {
            var first = nextMovements
                .OrderBy(p => p.Date).FirstOrDefault();
            if (first == null) { return null; }
            return first.Date.Value.ToString("dd.MM.yyyy");
        }

        if (operation == "GetDate2")
        {
            var first = nextMovements
                .OrderBy(p => p.Date).FirstOrDefault();

            if (first == null) { return null; }
            var second = nextMovements.OrderBy(p => p.Date).FirstOrDefault(a => a.Date > first.Date);
            if (second == null) { return null; }
            return second.Date.Value.ToString("dd.MM.yyyy");
        }

        if (operation == "GetDate3")
        {
            var first = nextMovements.OrderBy(p => p.Date).FirstOrDefault();
            if (first == null) { return null; }
            var second = nextMovements.OrderBy(p => p.Date).FirstOrDefault(a => a.Date > first.Date);
            if (second == null) { return null; }
            var third = nextMovements.OrderBy(p => p.Date).FirstOrDefault(a => a.Date > second.Date);
            if (third == null) { return null; }
            return third.Date.Value.ToString("dd.MM.yyyy");
        }



        //GetStatus
        if (operation == "GetStatus1")
        {
            var first = nextMovements.OrderBy(p => p.Date).FirstOrDefault();
            if (first == null) { return null; }
            if (first.CancelDate == null)
            {
                return "active";
            }
            else
            {
                return "not active";
            }

        }

        if (operation == "GetStatus2")
        {
            var first = nextMovements.OrderBy(p => p.Date).FirstOrDefault();
            if (first == null) { return null; }
            var second = nextMovements.OrderBy(p => p.Date).FirstOrDefault(a => a.Date > first.Date);
            if (second == null) { return null; }
            if (second.CancelDate == null)
            {
                return "active";
            }
            else
            {
                return "not active";
            }
        }

        if (operation == "GetStatus3")
        {
            var first = nextMovements.OrderBy(p => p.Date).FirstOrDefault();
            if (first == null) { return null; }
            var second = nextMovements.OrderBy(p => p.Date).FirstOrDefault(a => a.Date > first.Date);
            if (second == null) { return null; }
            var third = nextMovements.OrderBy(p => p.Date).FirstOrDefault(a => a.Date > second.Date);
            if (third == null) { return null; }
            if (third.CancelDate == null)
            {
                return "active";
            }
            else
            {
                return "not active";
            }
        }

        //SetStatus
        if (operation == "SetStatus1")
        {
            var first = nextMovements.OrderBy(p => p.Date).FirstOrDefault();

            if (first == null) { return null; }

            if (first.CancelDate == null)
            {
                first.CancelDate = DateTime.Now;

                var currentItem = _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefault(a => a.ID == first.ID);

                if (currentItem != null)
                {
                    currentItem.CancelDate = first.CancelDate;
                    //not the error source
                    _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdate(currentItem);
                }

                return "deactivated";
            }
            else
            {
                first.CancelDate = null;

                var currentItem = _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefault(a => a.ID == first.ID);

                if (currentItem != null)
                {
                    currentItem.CancelDate = first.CancelDate;
                    //not the error source
                    _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdate(currentItem);
                }


                return "activated";
            }

        }

        if (operation == "SetStatus2")
        {
            var first = nextMovements.OrderBy(p => p.Date).FirstOrDefault();
            if (first == null) { return null; }
            var second = nextMovements.OrderBy(p => p.Date).FirstOrDefault(a => a.Date > first.Date);
            if (second == null) { return null; }
            if (second.CancelDate == null)
            {
                second.CancelDate = DateTime.Now;

                var currentItem = _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefault(a => a.ID == second.ID);

                if (currentItem != null)
                {
                    currentItem.CancelDate = second.CancelDate;
                    //not the error source
                    _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdate(currentItem);
                }

                return "deactivated";
            }
            else
            {
                second.CancelDate = null;

                var currentItem = _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefault(a => a.ID == second.ID);

                if (currentItem != null)
                {
                    currentItem.CancelDate = second.CancelDate;
                    //not the error source
                    _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdate(currentItem);
                }

                return "activated";
            }
        }

        if (operation == "SetStatus3")
        {
            var first = nextMovements.OrderBy(p => p.Date).FirstOrDefault();
            if (first == null) { return null; }
            var second = nextMovements.OrderBy(p => p.Date).FirstOrDefault(a => a.Date > first.Date);
            if (second == null) { return null; }
            var third = nextMovements.OrderBy(p => p.Date).FirstOrDefault(a => a.Date > second.Date);
            if (third == null) { return null; }
            if (third.CancelDate == null)
            {
                third.CancelDate = DateTime.Now;

                var currentItem = _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefault(a => a.ID == third.ID);

                if (currentItem != null)
                {
                    currentItem.CancelDate = third.CancelDate;
                    //not the error source
                    _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdate(currentItem);
                }

                return "deactivated";
            }
            else
            {
                third.CancelDate = null;

                var currentItem = _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefault(a => a.ID == third.ID);

                if (currentItem != null)
                {
                    currentItem.CancelDate = third.CancelDate;
                    //not the error source
                    _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdate(currentItem);
                }

                return "activated";
            }
        }

        return null;
    }

    public bool ParentCodeExists(string parentpin)
    {
        return _unitOfWork.Repository<CANTEEN_User>().FirstOrDefault(p => p.TelPin == parentpin) != null;
    }

    public bool ParentAndChildCodesExist(string parentPin, string childCode)
    {
        var user = _unitOfWork.Repository<CANTEEN_User>().FirstOrDefault(p => p.TelPin == parentPin);
        if (user == null)
            return false;
        var child = _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefault(p =>
            p.RemovedDate == null && p.AUTH_Users_ID == user.AUTH_User_ID && p.TelCode == childCode);
        return child != null;
    }
    public async Task<CANTEEN_SchoolClass?> GetSchoolClassByID(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_SchoolClass>().FirstOrDefaultAsync(p => p.ID == ID);
    }
    public async Task<List<CANTEEN_SchoolClass>?> GetSchoolClassList()
    {
        return await _unitOfWork.Repository<CANTEEN_SchoolClass>().Where().OrderBy(o => o.Name).ToListAsync();
    }
    public async Task<CANTEEN_SchoolType?> GetSchoolTypeByID(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_SchoolType>().FirstOrDefaultAsync(p => p.id == ID);
    }
    public async Task<List<V_CANTEEN_SchoolType>?> GetSchoolTypeList(Guid LanguageID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_SchoolType>().Where(p => p.LANG_LanguagesID == LanguageID).ToListAsync();
    }
    public async Task<List<CANTEEN_Subscriber?>> GetSubscriptions(Guid AUTH_Municipality_ID, Guid Current_AUTH_Users_ID, Administration_Filter_CanteenSubscriptions? Filter)
    {
        var result = new List<CANTEEN_Subscriber>();

        bool filtered = false;

        var applications = _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);


        if (Filter != null && Filter.ManualInput != null)
        {
            applications = applications.Where(p => p.IsManualInput == Filter.ManualInput);
            filtered = true;
        }
        if (Filter != null && Filter.SubmittedFrom != null)
        {
            applications = applications.Where(p => p.SubmitAt >= Filter.SubmittedFrom);
            filtered = true;
        }
        if (Filter != null && Filter.SubmittedTo != null)
        {
            applications = applications.Where(p => p.SubmitAt <= Filter.SubmittedTo);
            filtered = true;
        }
        if (Filter != null && Filter.Auth_User_ID != null)
        {
            applications = applications.Where(p => p.AUTH_Users_ID == Filter.Auth_User_ID);
            filtered = true;
        }
        if (Filter != null && Filter.Archived != true)
        {
            if (Filter.Archived == false)
            {
                applications = applications.Where(p => p.Archived == null);
                filtered = true;
            }
        }
        if (Filter != null && Filter.MaxDistnanceFromSchool != null && Filter.MaxDistnanceFromSchool > 0)
        {
            if (Filter.Archived == false)
            {
                applications = applications.Where(p => p.DistanceFromSchool <= Filter.MaxDistnanceFromSchool);
                filtered = true;
            }
        }
        if (Filter != null && Filter.MinDistnanceFromSchool != null && Filter.MinDistnanceFromSchool > 0)
        {
            if (Filter.Archived == false)
            {
                applications = applications.Where(p => p.DistanceFromSchool >= Filter.MinDistnanceFromSchool);
                filtered = true;
            }
        }


        var items = await applications.Include(p => p.CANTEEN_School)
                                      .Include(p => p.CANTEEN_Canteen)
                                      .Include(p => p.CANTEEN_Subscriber_Status)
                                      .Include(p => p.CanteenMenu).ToListAsync();

        if (Filter != null && Filter.Text != null && Filter.Text != "")
        {
            items = items.Where(p => p.SearchBox != null && p.SearchBox.Contains(Filter.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
            filtered = true;
        }

        result = result.UnionBy(items, p => p.ID).ToList();

        if (Filter != null && Filter.Subscription_Status_ID != null && Filter.Subscription_Status_ID.Count() > 0)
        {
            result = result.Where(p => p.CANTEEN_Subscriber_Status_ID != null && Filter.Subscription_Status_ID.Contains(p.CANTEEN_Subscriber_Status_ID.Value)).ToList();
            filtered = true;
        }

        if (Filter != null && Filter.Canteen_ID != null && Filter.Canteen_ID.Count() > 0)
        {
            result = result.Where(p => p.CANTEEN_Canteen_ID != null && Filter.Canteen_ID.Contains(p.CANTEEN_Canteen_ID.Value)).ToList();
            filtered = true;
        }

        if (Filter != null && Filter.School_ID != null && Filter.School_ID.Count() > 0)
        {
            result = result.Where(p => p.CANTEEN_School_ID != null && Filter.School_ID.Contains(p.CANTEEN_School_ID.Value)).ToList();
            filtered = true;
        }

        if (Filter != null && Filter.SchoolYear_ID != null && Filter.SchoolYear_ID.Count() > 0)
        {
            result = result.Where(p => p.SchoolyearID != null && Filter.SchoolYear_ID.Contains(p.SchoolyearID.Value)).ToList();
            filtered = true;
        }

        if (Filter != null && Filter.Menu_ID != null && Filter.Menu_ID.Count() > 0)
        {
            result = result.Where(p => p.CanteenMenuID != null && Filter.Menu_ID.Contains(p.CanteenMenuID.Value)).ToList();
            filtered = true;
        }


        if (filtered)
            return result;

        return result.Take(500).ToList();
    }
    public async Task<List<V_CANTEEN_Subscriber?>> GetVSubscriptions(Guid AUTH_Municipality_ID, Guid Current_AUTH_Users_ID, Administration_Filter_CanteenSubscriptions? Filter, Guid LanguageID)
    {
        var result = new List<V_CANTEEN_Subscriber>();

        bool filtered = false;

        var applications = _unitOfWork.Repository<V_CANTEEN_Subscriber>().Where(p => p.LANG_LanguagesID == LanguageID && p.AUTH_Municipality_ID == AUTH_Municipality_ID);

        if (Filter != null && Filter.ManualInput != null)
        {
            applications = applications.Where(p => p.IsManualInput == Filter.ManualInput);
            filtered = true;
        }
        if (Filter != null && Filter.SubmittedFrom != null)
        {
            applications = applications.Where(p => p.SubmitAt >= Filter.SubmittedFrom);
            filtered = true;
        }
        if (Filter != null && Filter.SubmittedTo != null)
        {
            applications = applications.Where(p => p.SubmitAt <= Filter.SubmittedTo);
            filtered = true;
        }
        if (Filter != null && Filter.Auth_User_ID != null)
        {
            applications = applications.Where(p => p.AUTH_Users_ID == Filter.Auth_User_ID);
            filtered = true;
        }
        if (Filter != null && Filter.Archived != true)
        {
            if (Filter.Archived == false)
            {
                applications = applications.Where(p => p.Archived == null);
                filtered = true;
            }
        }
        if (Filter != null && Filter.MaxDistnanceFromSchool != null && Filter.MaxDistnanceFromSchool > 0)
        {
            if (Filter.Archived == false)
            {
                applications = applications.Where(p => p.DistanceFromSchool <= Filter.MaxDistnanceFromSchool);
                filtered = true;
            }
        }
        if (Filter != null && Filter.MinDistnanceFromSchool != null && Filter.MinDistnanceFromSchool > 0)
        {
            if (Filter.Archived == false)
            {
                applications = applications.Where(p => p.DistanceFromSchool >= Filter.MinDistnanceFromSchool);
                filtered = true;
            }
        }


        var items = await applications.ToListAsync();

        if (Filter != null && Filter.Text != null && Filter.Text != "")
        {
            items = items.Where(p => p.SearchBox != null && p.SearchBox.Contains(Filter.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
            filtered = true;
        }

        result = result.UnionBy(items, p => p.ID).ToList();

        if (Filter != null && Filter.Subscription_Status_ID != null && Filter.Subscription_Status_ID.Count() > 0)
        {
            result = result.Where(p => p.CANTEEN_Subscriber_Status_ID != null && Filter.Subscription_Status_ID.Contains(p.CANTEEN_Subscriber_Status_ID.Value)).ToList();
            filtered = true;
        }

        if (Filter != null && Filter.Canteen_ID != null && Filter.Canteen_ID.Count() > 0)
        {
            result = result.Where(p => p.CANTEEN_Canteen_ID != null && Filter.Canteen_ID.Contains(p.CANTEEN_Canteen_ID.Value)).ToList();
            filtered = true;
        }

        if (Filter != null && Filter.School_ID != null && Filter.School_ID.Count() > 0)
        {
            result = result.Where(p => p.CANTEEN_School_ID != null && Filter.School_ID.Contains(p.CANTEEN_School_ID.Value)).ToList();
            filtered = true;
        }

        if (Filter != null && Filter.SchoolYear_ID != null && Filter.SchoolYear_ID.Count() > 0)
        {
            result = result.Where(p => p.SchoolyearID != null && Filter.SchoolYear_ID.Contains(p.SchoolyearID.Value)).ToList();
            filtered = true;
        }

        if (Filter != null && Filter.Menu_ID != null && Filter.Menu_ID.Count() > 0)
        {
            result = result.Where(p => p.CanteenMenuID != null && Filter.Menu_ID.Contains(p.CanteenMenuID.Value)).ToList();
            filtered = true;
        }


        if (filtered)
            return result;

        return result.Take(500).ToList();
    }
    public async Task<List<V_CANTEEN_Subscriber>> GetVSubscriptions(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, string Taxnumber)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber>().Where(p => p.LANG_LanguagesID == LANG_Language_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.TaxNumber == Taxnumber).ToListAsync();
    }
    public async Task<List<V_CANTEEN_Fiscal_Year_Sum>> GetCANTEEN_YEAR_Balance(Guid AuthUserID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Fiscal_Year_Sum>().Where(a => a.AUTH_Users_ID == AuthUserID).ToListAsync();
    }
    public async Task<CANTEEN_Schoolyear?> GetSchoolyear(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Schoolyear>().FirstOrDefaultAsync(a => a.ID == ID);
    }
    public async Task<CANTEEN_Schoolyear?> SetSchoolyear(CANTEEN_Schoolyear Data, List<CANTEEN_Schoolyear_RegistrationPeriods> Periods)
    {
        List<CANTEEN_Schoolyear_RegistrationPeriods> _alreadyExistingPeriods = await _unitOfWork.Repository<CANTEEN_Schoolyear_RegistrationPeriods>().Where(p => p.CANTEEN_Schoolyear_ID == Data.ID).OrderByDescending(p => p.BeginDate).ToListAsync();
        await _unitOfWork.Repository<CANTEEN_Schoolyear>().InsertOrUpdateAsync(Data);
        if (_alreadyExistingPeriods != null && _alreadyExistingPeriods.Any())
        {
            List<Guid> _alreadyExistingPeriodIDs = _alreadyExistingPeriods.Select(p => p.ID).Distinct().ToList();
            List<Guid> _savedPeriodIDs = Periods.Select(p => p.ID).Distinct().ToList();
            List<Guid> _periodsToRemove = _alreadyExistingPeriodIDs.Where(p => !_savedPeriodIDs.Contains(p)).ToList();
            if (_periodsToRemove != null && _periodsToRemove.Any())
            {
                foreach (Guid _periodToRemove in _periodsToRemove)
                {
                    await _unitOfWork.Repository<CANTEEN_Schoolyear_RegistrationPeriods>().DeleteAsync(_periodToRemove);
                }
            }
        }
        foreach (CANTEEN_Schoolyear_RegistrationPeriods _period in Periods)
        {
            await _unitOfWork.Repository<CANTEEN_Schoolyear_RegistrationPeriods>().InsertOrUpdateAsync(_period);
        }
        return await _unitOfWork.Repository<CANTEEN_Schoolyear>().FirstOrDefaultAsync(p => p.ID == Data.ID);
    }
    public async Task<bool> RemoveSchoolyear(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Schoolyear>().DeleteAsync(ID);
    }
    public async Task<List<CANTEEN_Schoolyear_Base_Informations>> GetSchoolyear_BaseInformations()
    {
        return await _unitOfWork.Repository<CANTEEN_Schoolyear_Base_Informations>().Where().OrderBy(p => p.Startdate).ToListAsync();
    }
    public async Task<List<V_CANTEEN_Subscriber_Previous>> GetPreviousSubscriber(Guid AUTH_Users_ID, Guid LanguageID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber_Previous>().Where(p => p.LANG_LanguagesID == LanguageID && p.AUTH_Users_ID == AUTH_Users_ID).ToListAsync();
    }
    public async Task<List<CANTEEN_School_AdditionalPersonal>> GetAdditionalPersonalList(Guid CANTEEN_School_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_School_AdditionalPersonal>().Where(p => p.CANTEEN_School_ID == CANTEEN_School_ID).ToListAsync();
    }
    public async Task<List<CANTEEN_School_AdditionalPersonal>> GetAdditionalPersonalByMeal(Guid CANTEEN_School_ID, Guid CANTEEN_MealMenu_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_School_AdditionalPersonal>().Where(p => p.CANTEEN_School_ID == CANTEEN_School_ID && p.CANTEEN_Canteen_MealMenu_ID == CANTEEN_MealMenu_ID).ToListAsync();
    }
    public async Task<CANTEEN_School_AdditionalPersonal?> SetAdditionalPersonal(CANTEEN_School_AdditionalPersonal Data)
    {
        return await _unitOfWork.Repository<CANTEEN_School_AdditionalPersonal>().InsertOrUpdateAsync(Data);
    }
    public async Task<CANTEEN_School_AdditionalPersonal?> GetAdditionalPersonal(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_School_AdditionalPersonal>().GetByIDAsync(ID);
    }
    public async Task<bool> RemoveAdditionalPersonal(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_School_AdditionalPersonal>().DeleteAsync(ID);
    }
    public byte[]? GetStundentListFile(Guid LANG_Language_ID, Guid CANTEEN_Canteen_ID, List<Guid>? CANTEEN_School_ID = null)
    {
        string? SchoolID = null;

        if (CANTEEN_School_ID != null && CANTEEN_School_ID.Count() > 0)
        {
            foreach (var schoolID in CANTEEN_School_ID)
            {
                SchoolID += schoolID.ToString() + ",";
            }
        }

        if (SchoolID != null)
        {
            SchoolID = SchoolID.TrimEnd(',');
        }

        var ReportParameters = new Dictionary<string, object>();

        var reportPackager = new ReportPackager();
        var reportSource = new InstanceReportSource();

        string reportFileName = "Mensa_Studentlist.trdp";

        if (!string.IsNullOrEmpty(SchoolID))
        {
            reportFileName = "Mensa_Studentlist_School.trdp";
        }

        var BasePath = @"D:\Comunix\Reports\" + reportFileName;

        using (var sourceStream = System.IO.File.OpenRead(BasePath))
        {
            var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);
            report.ReportParameters.Clear();
            report.ReportParameters.Add(new ReportParameter("Canteen_ID", ReportParameterType.String, CANTEEN_Canteen_ID.ToString()));
            
            if (CANTEEN_School_ID != null && CANTEEN_School_ID.Count() > 0)
            {
                report.ReportParameters.Add(new ReportParameter("School_ID", ReportParameterType.String, SchoolID));
            }
            else
            {
                report.ReportParameters.Add(new ReportParameter("School_ID", ReportParameterType.String, ""));
            }

            report.ReportParameters.Add(new ReportParameter("Language_ID", ReportParameterType.String , LANG_Language_ID.ToString()));

            reportSource.ReportDocument = report;

            var reportProcessor = new ReportProcessor();
            var deviceInfo = new System.Collections.Hashtable();

            deviceInfo.Add("ComplianceLevel", "PDF/A-2b");

            RenderingResult result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);

            var ms = new MemoryStream(result.DocumentBytes);
            ms.Position = 0;

            return ms.ToArray();
        }
    }
    public async Task<List<CANTEEN_Property>> GetPropertyList(Guid AUTH_Municipality_ID, Guid LanguageID)
    {
        var data = await _unitOfWork.Repository<CANTEEN_Property>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).Include(p => p.CANTEEN_Property_Extended).ToListAsync();

        foreach (var d in data)
        {
            if (d != null)
            {
                var extended = d.CANTEEN_Property_Extended.FirstOrDefault(p => p.LANG_Languages_ID == LanguageID);

                if (extended != null)
                {
                    d.Title = extended.Title;
                    d.Description = extended.Description;
                }
            }
        }

        return data;
    }
    public async Task<CANTEEN_Property?> GetProperty(Guid ID, Guid LanguageID)
    {
        var data = await _unitOfWork.Repository<CANTEEN_Property>().Where(p => p.ID == ID).Include(p => p.CANTEEN_Property_Extended).FirstOrDefaultAsync();

        if (data != null)
        {
            var extended = data.CANTEEN_Property_Extended.FirstOrDefault(p => p.LANG_Languages_ID == LanguageID);

            if (extended != null)
            {
                data.Title = extended.Title;
                data.Description = extended.Description;
            }
        }
        return data;
    }
    public async Task<CANTEEN_Property?> SetProperty(CANTEEN_Property Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Property>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveProperty(Guid ID, bool force = false)
    {
        return await _unitOfWork.Repository<CANTEEN_Property>().DeleteAsync(ID);
    }
    public async Task<List<CANTEEN_Property_Extended>> GetPropertyExtendedList(Guid CANTEEN_Property_ID, Guid LANG_Language_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Property_Extended>().Where(p => p.CANTEEN_Property_ID == CANTEEN_Property_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
    }
    public async Task<CANTEEN_Property_Extended?> GetPropertyExtended(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Property_Extended>().GetByIDAsync(ID);
    }
    public async Task<CANTEEN_Property_Extended?> SetPropertyExtended(CANTEEN_Property_Extended Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Property_Extended>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemovePropertyExtended(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Property_Extended>().DeleteAsync(ID);
    }
    public async Task<List<CANTEEN_Ressources>> GetRessourceList(Guid AUTH_Municipality_ID, Guid LanguageID)
    {
        var data = await _unitOfWork.Repository<CANTEEN_Ressources>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).Include(p => p.CANTEEN_Ressources_Extended).ToListAsync();

        foreach (var d in data)
        {
            if (d != null)
            {
                var extended = d.CANTEEN_Ressources_Extended.FirstOrDefault(p => p.LANG_Language_ID == LanguageID);

                if (extended != null)
                {
                    d.Description = extended.Description;
                }
            }
        }

        return data;
    }
    public async Task<CANTEEN_Ressources?> GetRessource(Guid ID, Guid LanguageID)
    {
        var data = await _unitOfWork.Repository<CANTEEN_Ressources>().Where(p => p.ID == ID).Include(p => p.CANTEEN_Ressources_Extended).FirstOrDefaultAsync();

        if (data != null)
        {
            var extended = data.CANTEEN_Ressources_Extended.FirstOrDefault(p => p.LANG_Language_ID == LanguageID);

            if (extended != null)
            {
                data.Description = extended.Description;
            }
        }

        return data;
    }
    public async Task<CANTEEN_Ressources?> SetRessource(CANTEEN_Ressources Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Ressources>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRessource(Guid ID, bool force = false)
    {
        return await _unitOfWork.Repository<CANTEEN_Ressources>().DeleteAsync(ID);
    }
    public async Task<List<CANTEEN_Ressources_Extended>> GetRessourceExtendedList(Guid CANTEEN_Ressources_ID, Guid LANG_Language_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Ressources_Extended>().Where(p => p.CANTEEN_Ressources_ID == CANTEEN_Ressources_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
    }
    public async Task<CANTEEN_Ressources_Extended?> GetRessourceExtended(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Ressources_Extended>().GetByIDAsync(ID);
    }
    public async Task<CANTEEN_Ressources_Extended?> SetRessourceExtended(CANTEEN_Ressources_Extended Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Ressources_Extended>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRessourceExtended(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Ressources_Extended>().DeleteAsync(ID);
    }
    public async Task<CANTEEN_Configuration?> GetConfiguration(Guid AUTH_Municipality_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Configuration>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID);
    }
    public async Task<V_CANTEEN_Configuration_Extended?> GetVConfiguration(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Configuration_Extended>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);
    }
    public async Task<List<CANTEEN_Configuration_Extended>> GetConfigurationExtended(Guid CANTEEN_Configuration_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Configuration_Extended>().Where(p => p.CANTEEN_Configuration_ID == CANTEEN_Configuration_ID).ToListAsync();
    }
    public async Task<CANTEEN_Configuration_Extended?> SetConfigurationExtended(CANTEEN_Configuration_Extended Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Configuration_Extended>().InsertOrUpdateAsync(Data);
    }
    public async Task<CANTEEN_Configuration?> SetConfiguration(CANTEEN_Configuration Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Configuration>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveConfiguration(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Configuration>().DeleteAsync(ID);
    }
    public async Task<List<V_CANTEEN_User>> GetVUserList(Guid AUTH_Municipality_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_User>().Where(p => p.MunicipalityID == AUTH_Municipality_ID).ToListAsync();
    }
    public async Task<List<CANTEEN_Theme>> GetThemes(Guid AUTH_Municipality_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();
    }
    public async Task<List<V_HOME_Theme>> GetVThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
    {
        var existingElements = await _unitOfWork.Repository<CANTEEN_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();

        var themes = await _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();

        return themes.Where(p => existingElements.Select(x => x.HOME_THEME_ID).Contains(p.ID)).ToList();
    }
    public async Task<bool> RemoveTheme(CANTEEN_Theme Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Theme>().DeleteAsync(Data);
    }
    public async Task<CANTEEN_Theme?> SetTheme(CANTEEN_Theme Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Theme>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> HasTheme(Guid HOME_Theme_ID)
    {
        var exists = await _unitOfWork.Repository<CANTEEN_Theme>().FirstOrDefaultAsync(p => p.HOME_THEME_ID == HOME_Theme_ID);

        if(exists != null)
        {
            return true;
        }

        return false;
    }
    public async Task<V_CANTEEN_User?> GetVUser(Guid AUTH_Users_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_User>().FirstOrDefaultAsync(p => p.AUTH_User_ID == AUTH_Users_ID);
    }
    public async Task<List<CANTEEN_Schoolyear_RegistrationPeriods>> GetRegistrationPeriodList(Guid CANTEEN_Schoolyear_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Schoolyear_RegistrationPeriods>().Where(p => p.CANTEEN_Schoolyear_ID == CANTEEN_Schoolyear_ID).OrderByDescending(p => p.BeginDate).ToListAsync();
    }
    public async Task<CANTEEN_Schoolyear_RegistrationPeriods?> GetRegistrationPeriod(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_Schoolyear_RegistrationPeriods>().GetByIDAsync(ID);
    }
    public async Task<CANTEEN_Schoolyear_RegistrationPeriods?> SetRegistrationPeriod(CANTEEN_Schoolyear_RegistrationPeriods Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Schoolyear_RegistrationPeriods>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveRegistrationPeriod(Guid ID, bool force = false)
    {
        return await _unitOfWork.Repository<CANTEEN_Schoolyear_RegistrationPeriods>().DeleteAsync(ID);
    }
    public long GetLatestProgressivNumber(Guid AUTH_Municipality_ID, int Year)
    {
        var number = _unitOfWork.Repository<CANTEEN_Subscriber>().Where(p => p.ProgressivYear == Year && p.AUTH_Municipality_ID == AUTH_Municipality_ID).Max(p => p.ProgressivNumber);

        if (number != null)
        {
            return number.Value;
        }

        return 0;
    }
    public async Task<List<V_CANTEEN_Statistik_Meals>> GetStatistikMeals(Guid AUTH_Municipality_ID, DateTime FromDate, DateTime ToDate, Guid LanguageID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Statistik_Meals>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_LanguagesID == LanguageID && p.Date >= FromDate && p.Date <= ToDate).ToListAsync();
    }
    public async Task<List<V_CANTEEN_Statistik_Subscribers>> GetStatistikSubscribers(Guid AUTH_Municipality_ID, Guid SchoolyearID, Guid LanguageID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Statistik_Subscribers>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_LanguagesID == LanguageID && p.SchoolyearID == SchoolyearID).ToListAsync();
    }
    public async Task<List<CANTEEN_User_Documents>> GetUserDocumentList(Guid AUTH_Users_ID)
    {
        var canteenUser = await _unitOfWork.Repository<CANTEEN_User>().FirstOrDefaultAsync(p => p.AUTH_User_ID == AUTH_Users_ID);

        if (canteenUser != null)
        {
            var data = await _unitOfWork.Repository<CANTEEN_User_Documents>().Where(p => p.CANTEEN_User_ID == canteenUser.ID).ToListAsync();

            return data;
        }

        return new List<CANTEEN_User_Documents>();
    }
    public async Task<CANTEEN_User_Documents?> GetUserDocument(Guid ID)
    {
        return await _unitOfWork.Repository<CANTEEN_User_Documents>().GetByIDAsync(ID);
    }
    public async Task<CANTEEN_User_Documents?> SetUserDocument(CANTEEN_User_Documents Data)
    {
        return await _unitOfWork.Repository<CANTEEN_User_Documents>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> RemoveUserDocument(Guid ID, bool force = false)
    {
        return await _unitOfWork.Repository<CANTEEN_User_Documents>().DeleteAsync(ID);
    }
    public async Task<string> ReplaceKeywords(Guid CANTEEN_Subscriber_ID, Guid LANG_Language_ID, string Text, Guid? PreviousStatus_ID = null)
    {
        var subscriber = await _unitOfWork.Repository<V_CANTEEN_Subscriber>().FirstOrDefaultAsync(p => p.ID == CANTEEN_Subscriber_ID && p.LANG_LanguagesID == LANG_Language_ID);

        if (subscriber != null)
        {
            Text = Text.Replace("{Name Schüler}", subscriber.FirstName + " " + subscriber.LastName);
            Text = Text.Replace("{Nome alunno/a}", subscriber.FirstName + " " + subscriber.LastName);

            Text = Text.Replace("{Steuernummer Schüler}", subscriber.TaxNumber);
            Text = Text.Replace("{Codice fiscale alunno/a}", subscriber.TaxNumber);

            Text = Text.Replace("{Protokollnummer}", subscriber.ProgressivNumber);
            Text = Text.Replace("{Numero di protocollo}", subscriber.ProgressivNumber);

            var user = await _unitOfWork.Repository<CANTEEN_User>().FirstOrDefaultAsync(p => p.AUTH_User_ID == subscriber.AUTH_Users_ID);

            if(user != null)
            {
                Text = Text.Replace("{Elternkodex}", user.TelPin);
                Text = Text.Replace("{Codice genitore}", user.TelPin);
            }

            var school = await _unitOfWork.Repository<CANTEEN_School>().FirstOrDefaultAsync(p => p.ID == subscriber.CANTEEN_School_ID);

            if (school != null)
            {
                Text = Text.Replace("{Schule}", school.Name);
                Text = Text.Replace("{Scuola}", school.Name);
            }

            var schoolClass = await _unitOfWork.Repository<CANTEEN_SchoolClass>().FirstOrDefaultAsync(p => p.ID == subscriber.SchoolClassID);

            if (schoolClass != null)
            {
                var item = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == schoolClass.TEXT_SystemText_Code && p.LANG_LanguagesID == LANG_Language_ID);

                if (item != null) 
                {
                    Text = Text.Replace("{Klasse}", item.Text);
                    Text = Text.Replace("{Classe}", item.Text);
                }
            }

            var canteen = await _unitOfWork.Repository<CANTEEN_Canteen>().FirstOrDefaultAsync(p => p.ID == subscriber.CANTEEN_Canteen_ID);

            if (canteen != null)
            {
                Text = Text.Replace("{Mensa}", canteen.Name);
            }

            var StatusList = await GetSubscriberStatuses();

            if (PreviousStatus_ID != null)
            {
                var prevStatus = StatusList.FirstOrDefault(p => p.ID == PreviousStatus_ID);

                if (prevStatus != null)
                {
                    var item = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == prevStatus.TEXT_SystemTexts_Code && p.LANG_LanguagesID == LANG_Language_ID);

                    if (item != null) 
                    {
                        Text = Text.Replace("{Bisheriger Status}", item.Text);
                        Text = Text.Replace("{Stato precedente}", item.Text);
                    }
                }
            }

            var newStatus = StatusList.FirstOrDefault(p => p.ID == subscriber.CANTEEN_Subscriber_Status_ID);

            if (newStatus != null)
            {
                var item = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == newStatus.TEXT_SystemTexts_Code && p.LANG_LanguagesID == LANG_Language_ID);

                if (item != null)
                {
                    Text = Text.Replace("{Neuer Status}", item.Text);
                    Text = Text.Replace("{Nuovo stato}", item.Text);
                }
            }

            string weekyDays = "";

            if(subscriber.DayMo == true)
            {
                var monday = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == "MONDAY" && p.LANG_LanguagesID == LANG_Language_ID);

                if (monday != null)
                {
                    weekyDays += monday.Text + ", ";
                }

                var tuesday = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == "TUESDAY" && p.LANG_LanguagesID == LANG_Language_ID);

                if (tuesday != null)
                {
                    weekyDays += tuesday.Text + ", ";
                }

                var wednesday = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == "WEDNESDAY" && p.LANG_LanguagesID == LANG_Language_ID);

                if (wednesday != null)
                {
                    weekyDays += wednesday.Text + ", ";
                }

                var thursday = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == "THURSDAY" && p.LANG_LanguagesID == LANG_Language_ID);

                if (thursday != null)
                {
                    weekyDays += thursday.Text + ", ";
                }

                var friday = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == "FRIDAY" && p.LANG_LanguagesID == LANG_Language_ID);

                if (friday != null)
                {
                    weekyDays += friday.Text + ", ";
                }
            }

            Text = Text.Replace("{Gewünschte Tage}", weekyDays.Trim(' ').Trim(','));
            Text = Text.Replace("{Giorni richiesti}", weekyDays.Trim(' ').Trim(','));
        }

        return Text;
    }
    public async Task<List<CANTEEN_User_Tax_Report>> GetTaxReportsForUser(Guid userId)
    {
        var reports = await _unitOfWork.Repository<CANTEEN_User_Tax_Report>().Where(e => e.AUTH_User_ID == userId).ToListAsync();
        return reports;
    }
    public async Task<CANTEEN_Subscriber?> GetLatestCanteenSubscriberByTaxNumber(string taxNumber)
    {
        var sub = await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(e => e.TaxNumber == taxNumber)
            .OrderByDescending(e => e.CreationDate).FirstOrDefaultAsync();
        return sub;
    }
    public async Task<CANTEEN_Subscriber?> GetActiveCanteenSubscriberByTaxNumber(string taxNumber)
    {
        var sub = await _unitOfWork.Repository<CANTEEN_Subscriber>().Where(e => e.TaxNumber == taxNumber)
            .OrderByDescending(e => e.CreationDate).FirstOrDefaultAsync();
        return sub;
    }
    public async Task<CANTEEN_Subscriber_Card?> CreateSubscriberCard(Guid CANTEEN_Subscriber_ID, CardDeliveryAddress? Address = null)
    {
        var subscriber = await _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefaultAsync(p => p.ID == CANTEEN_Subscriber_ID);

        if(subscriber != null && !string.IsNullOrEmpty(subscriber.TaxNumber) && subscriber.AUTH_Municipality_ID != null)
        {
            var existingCheck = await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefaultAsync(p => p.DisabledDate == null && p.Taxnumber.ToLower() == subscriber.TaxNumber.ToLower());

            if(existingCheck == null)
            {
                var card = new CANTEEN_Subscriber_Card();

                card.ID = Guid.NewGuid();

                card.AUTH_Municipality_ID = subscriber.AUTH_Municipality_ID;
                card.CardID = await GetCardCode();
                card.CreationDate = DateTime.Now;
                card.Taxnumber = subscriber.TaxNumber;
                card.Firstname = subscriber.FirstName;
                card.Lastname = subscriber.LastName;
                card.DateOfBirth = subscriber.Child_DateOfBirth;
                card.PlaceOfBirth = subscriber.Child_PlaceOfBirth;
                card.CANTEEN_Subscriber_Card_Status_ID = CanteenCardStatus.Requested;


                Guid UserLanguage = LanguageSettings.German;
                if (subscriber.AUTH_Users_ID != null)
                {
                    var userSettings = await _unitOfWork.Repository<AUTH_UserSettings>().FirstOrDefaultAsync(p => p.AUTH_UsersID == subscriber.AUTH_Users_ID);


                    if (userSettings != null && userSettings.LANG_Languages_ID != null)
                    {
                        UserLanguage = userSettings.LANG_Languages_ID.Value;
                    }
                }

                var mun = await _unitOfWork.Repository<AUTH_Municipality>().FirstOrDefaultAsync(p => p.ID == subscriber.AUTH_Municipality_ID);

                if (mun != null)
                {                    
                    var municipalText = await _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefaultAsync(p => p.Code == mun.Name_Text_SystemTexts_Code && p.LANG_LanguagesID == UserLanguage);

                    if(municipalText != null)
                    {
                        card.MunicipalityText = municipalText.Text;
                    }
                    //URLUPDATE --OK
                    //var prefixText = await _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefaultAsync(p => p.Code == mun.Prefix_Text_SystemTexts_Code && p.LANG_LanguagesID == UserLanguage);
                    var baseUrl = await UrlService.GetDefaultUrlByMunicipalityStatic(_unitOfWork, mun.ID, UserLanguage); 
                    card.MunicipalityPath = baseUrl;
                }

                if (Address == null)
                {
                    var school = await _unitOfWork.Repository<CANTEEN_School>().FirstOrDefaultAsync(p => p.ID == subscriber.CANTEEN_School_ID);
                    var schoolClass = await _unitOfWork.Repository<CANTEEN_SchoolClass>().FirstOrDefaultAsync(p => p.ID == subscriber.SchoolClassID);

                    if (school != null && schoolClass != null)
                    {
                        var className = await _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefaultAsync(p => p.Code == schoolClass.TEXT_SystemText_Code && p.LANG_LanguagesID == UserLanguage);

                        if (className != null)
                        {
                            card.DEST_Name = school.Name + " - " + className.Text;
                            card.DEST_Address = school.Street + ", " + school.PLZ + " - " + school.Municipality;
                        }
                    }
                }
                else
                {
                    card.DEST_Name = Address.Name;
                    card.DEST_Address = Address.Street + ", " + Address.PLZ + " " + Address.Municipality;
                }

                await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().InsertOrUpdateAsync(card);

                await SetCardStatusLog(card, CanteenCardStatus.Requested);

                return card;
            }
        }

        return null;
    }
    public async Task<bool> RequestNewSubscriberCard(Guid CANTEEN_Subscriber_ID, CardDeliveryAddress Address)
    {
        var subscriber = await _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefaultAsync(p => p.ID == CANTEEN_Subscriber_ID);

        if (subscriber != null && !string.IsNullOrEmpty(subscriber.TaxNumber) && subscriber.AUTH_Municipality_ID != null)
        {
            var existingCheck = await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefaultAsync(p => p.DisabledDate == null && p.Taxnumber.ToLower() == subscriber.TaxNumber.ToLower());

            if (existingCheck != null)
            {
                await DisableSubscriberCard(existingCheck.ID);

                await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().InsertOrUpdateAsync(existingCheck);
            }

            await CreateSubscriberCard(CANTEEN_Subscriber_ID, Address);
        }

        return false;
    }
    private async Task<string> GetCardCode()
    {
        var code = "";

        Random r = new Random();

        code = r.Next(int.MaxValue).ToString("X8");

        var existing = await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefaultAsync(p => p.CardID == code);

        if (existing != null)
        {
            return await GetCardCode();
        }

        return code;
    }
    public async Task<CANTEEN_Subscriber_Card_Status_Log?> SetCardStatusLog(CANTEEN_Subscriber_Card_Status_Log Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Card_Status_Log>().InsertOrUpdateAsync(Data);
    }
    public async Task<CANTEEN_Subscriber_Card_Status_Log?> SetCardStatusLog(CANTEEN_Subscriber_Card Card, int Status)
    {
        var Data = new CANTEEN_Subscriber_Card_Status_Log();

        Data.ID = Guid.NewGuid();
        Data.CANTEEN_Subscriber_Card_ID = Card.ID;
        Data.CANTEEN_Subscriber_Card_Status_ID = Status;
        Data.ChangedDate = DateTime.Now;

        return await _unitOfWork.Repository<CANTEEN_Subscriber_Card_Status_Log>().InsertOrUpdateAsync(Data);
    }
    public async Task<bool> DisableSubscriberCard(Guid CANTEEN_Subscriber_Card_ID)
    {
        var card = await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefaultAsync(p => p.ID == CANTEEN_Subscriber_Card_ID);

        if(card != null)
        {
            card.DisabledDate = DateTime.Now;
            card.CANTEEN_Subscriber_Card_Status_ID = CanteenCardStatus.Disabled;

            await SetCardStatusLog(card, CanteenCardStatus.Disabled);

            await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().InsertOrUpdateAsync(card);

            return true;
        }

        return false;
    }
    public bool SubscriberCardCanBeActivated(Guid CANTEEN_Subscriber_Card_ID)
    {
        var card = _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefault(p => p.ID == CANTEEN_Subscriber_Card_ID);

        if (card != null && !string.IsNullOrEmpty(card.Taxnumber))
        {
            var existing = _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefault(p => p.Taxnumber.ToLower() == card.Taxnumber.ToLower() && p.DisabledDate == null);

            if(existing != null)
            {
                return false;
            }
        }

        return true;
    }
    public async Task<bool> ActivateSubscriberCard(Guid CANTEEN_Subscriber_Card_ID)
    {
        var card = await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefaultAsync(p => p.ID == CANTEEN_Subscriber_Card_ID);

        if (card != null)
        {
            card.DisabledDate = null;
            card.CANTEEN_Subscriber_Card_Status_ID = CanteenCardStatus.Finished;

            await SetCardStatusLog(card, CanteenCardStatus.Finished);

            await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().InsertOrUpdateAsync(card);

            return true;
        }

        return false;
    }
    public async Task<List<V_CANTEEN_Subscriber_Card>> GetSubscriberCards(Guid LANG_Language_ID, string Taxnumber)
    {
        if (!string.IsNullOrEmpty(Taxnumber))
        {
            return await _unitOfWork.Repository<V_CANTEEN_Subscriber_Card>().ToListAsync(p => p.Taxnumber.ToLower() == Taxnumber.ToLower() && p.LANG_LanguagesID == LANG_Language_ID);
        }

        return new List<V_CANTEEN_Subscriber_Card>();
    }
    public async Task<List<V_CANTEEN_Subscriber_Card_Status>> GetCardStatusList(Guid LANG_Language_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber_Card_Status>().ToListAsync(p => p.LANG_LanguagesID == LANG_Language_ID);
    }
    public async Task<List<V_CANTEEN_Subscriber_Card_Status_Log>> GetCardStatusLogList(Guid LANG_Language_ID, Guid CANTEEN_Subscriber_Card_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber_Card_Status_Log>().ToListAsync(p => p.LANG_LanguagesID == LANG_Language_ID && p.CANTEEN_Subscriber_Card_ID == CANTEEN_Subscriber_Card_ID);
    }
    public async Task<List<V_CANTEEN_Subscriber_Card_Log>> GetCardLogList(Guid LANG_Language_ID, Guid CANTEEN_Subscriber_Card_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber_Card_Log>().Where(p => p.LANG_LanguagesID == LANG_Language_ID && p.CANTEEN_Subscriber_Card_ID == CANTEEN_Subscriber_Card_ID).OrderByDescending(p => p.CreatedAt).ToListAsync();
    }
    public async Task<CANTEEN_Subscriber_Card_Log?> SetCardLog(CANTEEN_Subscriber_Card_Log Data)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Card_Log>().InsertOrUpdateAsync(Data);
    }

    public async Task<CANTEEN_RequestRefundBalances?> GetRequestRefundBalance(string RequestRefundBalanceID)
    {
        return await _unitOfWork.Repository<CANTEEN_RequestRefundBalances>().Where(p => p.ID.ToString().ToLower().Trim() == RequestRefundBalanceID.ToLower().Trim()).FirstOrDefaultAsync();
    }
    public async Task<CANTEEN_RequestRefundBalances?> SetRequestRefundBalance(CANTEEN_RequestRefundBalances Data)
    {
        return await _unitOfWork.Repository<CANTEEN_RequestRefundBalances>().InsertOrUpdateAsync(Data);
    }
    public async Task<CANTEEN_RequestRefundBalances_Resource?> SetRequestRessource(CANTEEN_RequestRefundBalances_Resource Data)
    {
        return await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Resource>().InsertOrUpdateAsync(Data);
    }
    public async Task<CANTEEN_RequestRefundBalances_Status_Log?> SetRequestRefundBalanceStatusLog(CANTEEN_RequestRefundBalances_Status_Log RequestStatusLog)
    {
        return await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Status_Log>().InsertOrUpdateAsync(RequestStatusLog);
    }
    public async Task<CANTEEN_RequestRefundBalances_Status_Log_Extended?> SetRequestRefundBalanceStatusLogExtended(CANTEEN_RequestRefundBalances_Status_Log_Extended RequestStatusLog)
    {
        return await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Status_Log_Extended>().InsertOrUpdateAsync(RequestStatusLog);
    }
    public async Task<CANTEEN_RequestRefundBalances_Status_Log?> CreateRequestRefundBalanceStatusLogEntry(CANTEEN_RequestRefundBalances Request)
    {
        if (Request != null && Request.AUTH_User_ID != null && Request.AUTH_User_ID != Guid.Empty && Request.CANTEEN_RequestRefundAllCharge_Status_ID != null && Request.CANTEEN_RequestRefundAllCharge_Status_ID != Guid.Empty)
        {
            CANTEEN_RequestRefundBalances_Status_Log? _newLogEntry = new CANTEEN_RequestRefundBalances_Status_Log();
            _newLogEntry.ID = Guid.NewGuid();
            if (Request.CANTEEN_RequestRefundAllCharge_Status_ID == Canteen_Request_Status.ToSign && Request.Date != null)
            {
                _newLogEntry.ChangeDate = Request.Date.Value;
            }
            else if (Request.CANTEEN_RequestRefundAllCharge_Status_ID == Canteen_Request_Status.Comitted && Request.SignedDate != null)
            {
                _newLogEntry.ChangeDate = Request.SignedDate.Value;
            }
            else
            {
                _newLogEntry.ChangeDate = DateTime.Now;
            }
            _newLogEntry.AUTH_Users_ID = Request.AUTH_User_ID.Value;
            _newLogEntry.CANTEEN_RequestRefundBalances_ID = Request.ID;
            _newLogEntry.CANTEEN_RequestRefundBalances_Status_ID = Request.CANTEEN_RequestRefundAllCharge_Status_ID.Value;
            _newLogEntry = await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Status_Log>().InsertOrUpdateAsync(_newLogEntry);
            if (_newLogEntry != null)
            {
                CANTEEN_RequestRefundBalances_Status? _matchingStatus = await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Status>().FirstOrDefaultAsync(p => p.ID == _newLogEntry.CANTEEN_RequestRefundBalances_Status_ID);
                if (_matchingStatus != null)
                {
                    List<TEXT_SystemTexts> _titles = await _unitOfWork.Repository<TEXT_SystemTexts>().Where(p => p.Code == _matchingStatus.TEXT_SystemTexts_Code).ToListAsync();
                    List<TEXT_SystemTexts> _descriptions = await _unitOfWork.Repository<TEXT_SystemTexts>().Where(p => p.Code == _matchingStatus.Description).ToListAsync();
                    List<Guid> _languageIDs = _titles.Select(p => p.LANG_LanguagesID).Distinct().ToList();
                    foreach (Guid _languageID in _languageIDs)
                    {
                        TEXT_SystemTexts? _title = _titles.FirstOrDefault(p => p.LANG_LanguagesID == _languageID);
                        TEXT_SystemTexts? _description = _descriptions.FirstOrDefault(p => p.LANG_LanguagesID == _languageID);
                        CANTEEN_RequestRefundBalances_Status_Log_Extended? _newExtendedLogEntry = new CANTEEN_RequestRefundBalances_Status_Log_Extended();
                        _newExtendedLogEntry.ID = Guid.NewGuid();
                        if (_description != null && !string.IsNullOrEmpty(_description.Text))
                        {
                            _newExtendedLogEntry.Reason = _description.Text;
                        }
                        if (_title != null && !string.IsNullOrEmpty(_title.Text))
                        {
                            _newExtendedLogEntry.Title = _title.Text;
                        }
                        _newExtendedLogEntry.CANTEEN_RequestRefundBalances_StatusLog_ID = _newLogEntry.ID;
                        _newExtendedLogEntry.LANG_Languages_ID = _languageID;
                        await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Status_Log_Extended>().InsertOrUpdateAsync(_newExtendedLogEntry);
                    }
                }
            }
            return _newLogEntry;
        }
        return null;
    }
    public async Task<CANTEEN_RequestRefundBalances_Status?> GetDefaultRequestRefundBalanceStatus()
    {
        return await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Status>().Where(p => p.DefaultStatus == true).OrderBy(p => p.SortOrder).FirstOrDefaultAsync();
    }
    public async Task<CANTEEN_RequestRefundBalances_Status?> GetSignedRequestRefundBalanceStatus()
    {
        return await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Status>().Where(p => p.SignedStatus == true).OrderBy(p => p.SortOrder).FirstOrDefaultAsync();
    }
    public async Task<List<CANTEEN_RequestRefundBalances_Status>> GetAllRequestRefundBalanceStatus()
    {
        return await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Status>().Where().OrderBy(p => p.SortOrder).ToListAsync();
    }
    public async Task<List<CANTEEN_RequestRefundBalances_Status_Log>> GetAllRequestRefundBalanceStatusLogEntries(Guid CanteenRequestRefundBalancesID, Guid LANG_Language_ID)
    {
        List<CANTEEN_RequestRefundBalances_Status_Log> _logEntries = await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Status_Log>().Where(p => p.CANTEEN_RequestRefundBalances_ID == CanteenRequestRefundBalancesID).OrderByDescending(p => p.ChangeDate).Include(p => p.CANTEEN_RequestRefundBalances_Status_Log_Extended).ToListAsync();
        List<CANTEEN_RequestRefundBalances_Status> _statusList = await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Status>().Where().OrderBy(p => p.SortOrder).ToListAsync();
        List<AUTH_Users> _users = await _unitOfWork.Repository<AUTH_Users>().Where(p => _logEntries.Select(l => l.AUTH_Users_ID).Distinct().Contains(p.ID)).ToListAsync();
        List<AUTH_Municipal_Users> _municipalUsers = await _unitOfWork.Repository<AUTH_Municipal_Users>().Where(p => _logEntries.Select(l => l.AUTH_Users_ID).Distinct().Contains(p.ID)).ToListAsync();

        foreach (CANTEEN_RequestRefundBalances_Status _status in _statusList)
        {
            TEXT_SystemTexts? _text = await _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefaultAsync(p => p.Code == _status.TEXT_SystemTexts_Code && p.LANG_LanguagesID == LANG_Language_ID);
            if (_text != null)
            {
                _status.Name = _text.Text;
            }
        }

        foreach (CANTEEN_RequestRefundBalances_Status_Log _logEntry in _logEntries)
        {
            CANTEEN_RequestRefundBalances_Status_Log_Extended? _matchingExtended = _logEntry.CANTEEN_RequestRefundBalances_Status_Log_Extended.FirstOrDefault(p => p.LANG_Languages_ID == LANG_Language_ID);
            CANTEEN_RequestRefundBalances_Status? _matchingStatus = _statusList.FirstOrDefault(p => p.ID == _logEntry.CANTEEN_RequestRefundBalances_Status_ID);
            AUTH_Users? _matchingUser = _users.FirstOrDefault(p => p.ID == _logEntry.AUTH_Users_ID);
            if (_matchingExtended != null)
            {
                _logEntry.Title = _matchingExtended.Title;
                _logEntry.Reason = _matchingExtended.Reason;
            }
            if (_matchingStatus != null)
            {
                _logEntry.Status = _matchingStatus.Name;
                _logEntry.StatusIcon = _matchingStatus.Icon;
            }
            if (_matchingUser != null)
            {
                _logEntry.User = (_matchingUser.Firstname + " " + _matchingUser.Lastname).Trim();
            }
            else
            {
                AUTH_Municipal_Users? _matchingMunicipalUser = _municipalUsers.FirstOrDefault(p => p.ID == _logEntry.AUTH_Users_ID);
                if ( _matchingMunicipalUser != null )
                {
                    _logEntry.User = (_matchingMunicipalUser.Firstname + " " + _matchingMunicipalUser.Lastname).Trim();
                }
            }
        }

        return _logEntries.OrderByDescending(p => p.ChangeDate).ToList();
    }
    public async Task<List<CANTEEN_RequestRefundBalances_Resource>> GetAllRequestRefundBalanceResource(Guid CanteenRequestRefundBalancesID)
    {
        return await _unitOfWork.Repository<CANTEEN_RequestRefundBalances_Resource>().Where(p => p.CANTEEN_RequestRefundBalances_ID == CanteenRequestRefundBalancesID && p.RemovedAt == null).ToListAsync();
    }
    public async Task<long> GetLatestProgressivNumberRequestRefundBalances(Guid AUTH_Municipality_ID, int Year)
    {
        var number = await _unitOfWork.Repository<CANTEEN_RequestRefundBalances>().Where(p => p.Date != null && p.Date.Value.Year == Year && p.AUTH_Municipality_ID == AUTH_Municipality_ID).MaxAsync(p => p.ProgressivNumber);

        if (number != null)
        {
            return number.Value;
        }

        return 0;
    }
    public async Task<List<V_CANTEEN_RequestRefundBalances>> GetRequestRefundBalances(Guid AUTH_Municipality_ID, Administration_Filter_CanteenRequestRefundBalances? Filter, Guid LANG_Language_ID)
    {
        List<V_CANTEEN_RequestRefundBalances> _results = new List<V_CANTEEN_RequestRefundBalances>();

        _results = await _unitOfWork.Repository<V_CANTEEN_RequestRefundBalances>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID &&
                                                                                              p.LANG_LanguageID == LANG_Language_ID).OrderByDescending(p => p.Date).ToListAsync();

        if (Filter != null && Filter.SignedFrom != null)
        {
            _results = _results.Where(p => p.SignedDate != null && p.SignedDate.Value >= Filter.SignedFrom.Value).ToList();
        }
        if (Filter != null && Filter.SignedTo != null)
        {
            _results = _results.Where(p => p.SignedDate != null && p.SignedDate.Value <= Filter.SignedTo.Value).ToList();
        }
        if (Filter != null && Filter.Auth_User_ID != null)
        {
            _results = _results.Where(p => p.AUTH_User_ID == Filter.Auth_User_ID).ToList();
        }
        if (Filter != null && !string.IsNullOrEmpty(Filter.Text))
        {
            _results = _results.Where(p => p.SearchBox.Contains(Filter.Text.ToLower())).ToList();
        }
        if (Filter != null && Filter.CANTEEN_RequestRefundBalances_Status_ID != null && Filter.CANTEEN_RequestRefundBalances_Status_ID.Any())
        {
            _results = _results.Where(p => p.CANTEEN_RequestRefundAllCharge_Status_ID == null || Filter.CANTEEN_RequestRefundBalances_Status_ID.Contains(p.CANTEEN_RequestRefundAllCharge_Status_ID.Value)).ToList();
        }
        return _results.OrderByDescending(e => e.Date).Take(100).ToList();
    }
    public async Task<List<V_CANTEEN_RequestRefundBalances>> GetRequestRefundBalances(Guid AUTH_Municipality_ID, Guid AUTH_User_ID, Guid LANG_Language_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_RequestRefundBalances>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_User_ID == AUTH_User_ID && p.LANG_LanguageID == LANG_Language_ID).OrderByDescending(p => p.Date).ToListAsync();
    }
    public async Task<List<V_CANTEEN_RequestRefundBalances_UsersWithRequests>> GetRequestRefundBalancesUsers(Guid AUTH_Municipality_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_RequestRefundBalances_UsersWithRequests>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();
    }
    public async Task<bool> UserHasOpenRequest(Guid AUTH_Municipality_ID, Guid AUTH_User_ID)
    {
        return await _unitOfWork.Repository<CANTEEN_RequestRefundBalances>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_User_ID == AUTH_User_ID && p.CANTEEN_RequestRefundAllCharge_Status_ID != Canteen_Request_Status.Declined && p.CANTEEN_RequestRefundAllCharge_Status_ID != Canteen_Request_Status.Accepted) != null;
    }
    public async Task<List<V_CANTEEN_Student>> GetStudents(Guid AUTH_Municipality_ID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Student>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).OrderBy(p => p.Lastname).ThenBy(p => p.Firstname).ToListAsync();
    }
    public async Task<LoginResponse?> CreatePOSSession(string Username, string Password)
    {
        var ExistingPos = await _unitOfWork.Repository<CANTEEN_Canteen_Pos>().FirstOrDefaultAsync(p => p.Username.ToLower() == Username.ToLower() && p.Password == Password);

        if(ExistingPos == null)
        {
            var ErrorResponse = new LoginResponse();

            ErrorResponse.Response = "Wrong credentials";
            ErrorResponse.Code = "401";

            return ErrorResponse;
        }

        var session = await _unitOfWork.Repository<CANTEEN_POS_Session>().FirstOrDefaultAsync(p => p.CANTEEN_Canteen_Pos_ID == ExistingPos.ID && p.ExpirationDate >= DateTime.Now);

        if (session != null)
        {
            session.ExpirationDate = DateTime.Now.AddDays(30);

            await _unitOfWork.Repository<CANTEEN_POS_Session>().InsertOrUpdateAsync(session);
        }
        else
        {
            session = new CANTEEN_POS_Session();

            session.ID = Guid.NewGuid();
            session.SessionToken = Guid.NewGuid();
            session.CreationDate = DateTime.Now;
            session.ExpirationDate = DateTime.Now.AddDays(30);
            session.CANTEEN_Canteen_Pos_ID = ExistingPos.ID;
            session.AUTH_Municipality_ID = ExistingPos.AUTH_Municipality_ID;

            await _unitOfWork.Repository<CANTEEN_POS_Session>().InsertOrUpdateAsync(session);
        }

        var response = new LoginResponse();

        response.Response = "Success";
        response.Code = "200";

        if (session.SessionToken != null)
        {
            response.SessionToken = session.SessionToken.ToString();
        }

        return response;
    }
    public async Task<CANTEEN_POS_Session?> GetPOSSession(Guid SessionToken)
    {
        var session = await _unitOfWork.Repository<CANTEEN_POS_Session>().FirstOrDefaultAsync(p => p.SessionToken == SessionToken && p.ExpirationDate >= DateTime.Now);

        return session;
    }
    public async Task<CANTEEN_POS_Session?> UpdateSession(CANTEEN_POS_Session Data)
    {
        var session = await _unitOfWork.Repository<CANTEEN_POS_Session>().InsertOrUpdateAsync(Data);

        return session;
    }
    public async Task<CANTEEN_POS_Session?> ExpireSession(Guid SessionToken)
    {
        var session = await _unitOfWork.Repository<CANTEEN_POS_Session>().FirstOrDefaultAsync(p => p.SessionToken == SessionToken && p.ExpirationDate >= DateTime.Now);

        if(session != null)
        {
            session.ExpirationDate = DateTime.Now;

            await _unitOfWork.Repository<CANTEEN_POS_Session>().InsertOrUpdateAsync(session);
        }

        return session;
    }
    public async Task<CardResponse> CheckCard(string CardID, Guid Auth_Municipality_ID, bool Manual = false)
    {
        var checkSystem = await _unitOfWork.Repository<CANTEEN_Configuration>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == Auth_Municipality_ID);

        if (checkSystem == null || checkSystem.PosMode != true)
        {
            return GetResponse("500", "POS-Mode disabled");
        }

        var subscriberCard = await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefaultAsync(p => p.CardID == CardID && p.AUTH_Municipality_ID == Auth_Municipality_ID && 
                                                                                                             (p.CANTEEN_Subscriber_Card_Status_ID == CanteenCardStatus.Finished ||
                                                                                                              p.CANTEEN_Subscriber_Card_Status_ID == CanteenCardStatus.InProgress ||
                                                                                                              p.CANTEEN_Subscriber_Card_Status_ID == CanteenCardStatus.Requested)
                                                                                                        );

        if(subscriberCard == null)
        {
            return GetResponse("501", "Card not Found");
        }

        var subscriber = await _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefaultAsync(p => p.TaxNumber == subscriberCard.Taxnumber && p.CANTEEN_Subscriber_Status_ID == CanteenStatus.Accepted);

        if(subscriber == null)
        {
            await LogCard(subscriberCard.ID, "CANTEEN_POS_NO_ACTIVE_SUBSCRIPTION_TITLE", "CANTEEN_POS_NO_ACTIVE_SUBSCRIPTION_MESSAGE");

            return GetResponse("502", "No active subscription");
        }

        var datetoCheck = DateTime.Now;

        var checkMovementState = await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().FirstOrDefaultAsync(p => p.CANTEEN_Subscriber_ID == subscriber.ID && 
                                                                                                                       p.Date != null && p.RemovedDate == null &&
                                                                                                                       p.Date.Value.Year == datetoCheck.Year &&
                                                                                                                       p.Date.Value.Month == datetoCheck.Month &&
                                                                                                                       p.Date.Value.Day == datetoCheck.Day &&
                                                                                                                       p.CANTEEN_Subscriber_Movement_Type_ID == CanteenMovementType.Meal);

        if (checkMovementState == null)
        {
            await LogCard(subscriberCard.ID, "CANTEEN_POS_NO_MOVEMENT_FOUND_TITLE", "CANTEEN_POS_NO_MOVEMENT_FOUND_MESSAGE");

            return GetResponse("503", "No movement found", subscriber.ID, subscriberCard);
        }

        if(checkMovementState.Used == true)
        {
            await LogCard(subscriberCard.ID, "CANTEEN_POS_ALREADY_USED_TITLE", "CANTEEN_POS_ALREADY_USED_MESSAGE");

            return GetResponse("504", "Movement already booked", subscriber.ID, subscriberCard);
        }

        //Von Fabian angeordnet die Karte trotzdem stempeln lassen
        /*if (checkMovementState.BalanceToLow == true)
        {
            await LogCard(subscriberCard.ID, "CANTEEN_POS_BALANCE_LOW_TITLE", "CANTEEN_POS_BALANCE_LOW_MESSAGE");

            return GetResponse("505", "Balance to low, disabled", subscriber.ID, subscriberCard);
        }*/

        if (checkMovementState.CancelDate != null)
        {
            checkMovementState.Used = true;
            checkMovementState.UsedWhileCanceled = true;
            if (Manual)
                checkMovementState.ManualCheckIn = true;

            await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdateAsync(checkMovementState);

            await LogCard(subscriberCard.ID, "CANTEEN_POS_USED_WITH_EXCEPTION_TITLE", "CANTEEN_POS_USED_WITH_EXCEPTION_MESSAGE");

            return GetResponse("201", "Movement ok, with exception", subscriber.ID, subscriberCard);
        }

        if (Manual)
            checkMovementState.ManualCheckIn = true;
        checkMovementState.Used = true;

        await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().InsertOrUpdateAsync(checkMovementState);

        await LogCard(subscriberCard.ID, "CANTEEN_POS_USED_TITLE", "CANTEEN_POS_USED_MESSAGE");

        return GetResponse("200", "Movement ok", subscriber.ID, subscriberCard);
    }
    private async Task<bool> LogCard(Guid CANTEEN_Subscriber_Card_ID, string TitleCode, string MessageCode)
    {
        var log = new CANTEEN_Subscriber_Card_Log();

        log.ID = Guid.NewGuid();
        log.CANTEEN_Subscriber_Card_ID = CANTEEN_Subscriber_Card_ID;
        log.CreatedAt = DateTime.Now;
        log.TitleCode = TitleCode;
        log.MessageCode = MessageCode;

        await _unitOfWork.Repository<CANTEEN_Subscriber_Card_Log>().InsertOrUpdateAsync(log);

        return true;
    }
    private CardResponse GetResponse(string Code, string Message, Guid? CANTEEN_Subscriber_ID = null, CANTEEN_Subscriber_Card? Card = null)
    {
        var response = new CardResponse();

        response.Code = Code;
        response.Response = Message;

        if(Card != null)
        {
            response.Card = Card;
        }

        if(CANTEEN_Subscriber_ID != null)
        {
            var sub = _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefault(p => p.ID == CANTEEN_Subscriber_ID.Value);

            if(sub != null)
            {
                response.AUTH_Users_ID = sub.AUTH_Users_ID;
            }
        }
        return response;
    }
    //POS BACKEND
    public async Task<CANTEEN_Subscriber_Card?> GetCurrentSubscriberCard(string taxnumber)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Card>().FirstOrDefaultAsync(e => e.Taxnumber.ToLower() == taxnumber.ToLower() && e.DisabledDate == null && e.CANTEEN_Subscriber_Card_Status_ID == CanteenCardStatus.Finished);
    }
    public async Task<List<V_CANTEEN_Subscriber_Movements>> GetTodaysCanteenMovements(Guid canteenId, DateTime? date = null)
    {
        if (date == null)
        {
            date = DateTime.Today;
        }
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber_Movements>().Where(e =>
            e.Date != null && e.Date.Value.Date == date.Value.Date && e.RemovedDate == null
            && e.CANTEEN_Subscriber_Movement_Type_ID == CanteenMovementType.Meal && e.CANTEEN_Canteen_ID == canteenId).OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();
    }
    public async Task<bool> ManualCheckIn(Guid movementId)
    {
        var movement = await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>()
            .FirstOrDefaultAsync(e => e.ID == movementId);
        if (movement == null)
            return false;
        movement.Used = true;
        movement.UsedWhileCanceled = movement.CancelDate != null;
        movement.ManualCheckIn = true;
        await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().UpdateAsync(movement);
        return true;
    }
    public async Task<bool> ManualCheckOut(Guid movementId)
    {
        var movement = await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>()
            .FirstOrDefaultAsync(e => e.ID == movementId);
        if (movement == null)
            return false;
        movement.Used = false;
        movement.UsedWhileCanceled = false;
        movement.ManualCheckIn = false;
        await _unitOfWork.Repository<CANTEEN_Subscriber_Movements>().UpdateAsync(movement);
        return true;
    }
    public async Task<CANTEEN_Canteen?> GetCanteenByAccessToken(Guid token)
    {
        return await _unitOfWork.Repository<CANTEEN_Canteen>().FirstOrDefaultAsync(e =>
            e.ExternalCheckInAccessToken == token && e.ExternalAccessTokenExpirationDate > DateTime.Now);
    }

    public async Task<CANTEEN_Subscriber_Card_Request?> GetUnfinishedCardRequest(string subscriberTaxNumber)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Card_Request>()
            .FirstOrDefaultAsync(e => e.SubscriberTaxNumber.ToLower() == subscriberTaxNumber.ToLower() && e.PayedAt == null);
    }

    public async Task<bool> SetSubscriberCardRequest(CANTEEN_Subscriber_Card_Request request)
    {
        await _unitOfWork.Repository<CANTEEN_Subscriber_Card_Request>().InsertOrUpdateAsync(request);
        return true;
    }

    public async Task<CANTEEN_Subscriber_Card_Request?> GetSubscriberCardRequest(Guid requestId)
    {
        return await _unitOfWork.Repository<CANTEEN_Subscriber_Card_Request>()
            .FirstOrDefaultAsync(e => e.ID == requestId);
    }

    public async Task<CANTEEN_Subscriber_Card?> SetSubscriberCardRequestPayed(Guid requestId)
    {
        var request = await _unitOfWork.Repository<CANTEEN_Subscriber_Card_Request>()
            .FirstOrDefaultAsync(e => e.ID == requestId);

        if (request == null)
            return null;

        var prevCard = await GetCurrentSubscriberCard(request.SubscriberTaxNumber);
        if (prevCard == null)
            return null;

        var sub = await GetActiveCanteenSubscriberByTaxNumber(request.SubscriberTaxNumber);
        if (sub == null)
            return null;

        request.PayedAt = DateTime.Now;
        await _unitOfWork.Repository<CANTEEN_Subscriber_Card_Request>().UpdateAsync(request);

        await DisableSubscriberCard(prevCard.ID);
        var card = await CreateSubscriberCard(sub.ID, new CardDeliveryAddress() { Name = request.Place, Municipality = request.Municipality, Street = request.Address, PLZ = request.PLZ });
        return card;
    }

    public async Task<List<V_CANTEEN_Subscriber_Card>> GetAllCards(Guid municipalityID, Guid languageID)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber_Card>().Where(p => p.AUTH_Municipality_ID == municipalityID && p.LANG_LanguagesID == languageID).ToListAsync();
    }

    public async Task<List<V_CANTEEN_Subscriber_Card_Request>> GetPayedCardRequests(Guid municipalityId)
    {
        return await _unitOfWork.Repository<V_CANTEEN_Subscriber_Card_Request>()
            .Where(e => e.MunicipalityId == municipalityId && e.PayedAt != null).OrderByDescending(e => e.PayedAt).ToListAsync();
    }

    public async Task<List<V_CANTEEN_Subscriber_Card_Request>> GetPayedCardRequests(Guid municipalityId, Administration_Filter_CanteenRequestCardList filter)
    {
        List<V_CANTEEN_Subscriber_Card_Request> _results = new List<V_CANTEEN_Subscriber_Card_Request>();

        _results = await _unitOfWork.Repository<V_CANTEEN_Subscriber_Card_Request>().Where(e => e.MunicipalityId == municipalityId && e.PayedAt != null).OrderByDescending(e => e.PayedAt).ToListAsync();

        if (filter != null && !string.IsNullOrEmpty(filter.Text))
        {
            _results = _results.Where(p => p.SearchBox.ToLower().Contains(filter.Text.ToLower())).ToList();
        }
        if (filter != null && filter.CANTEEN_Card_Status_ID != null && filter.CANTEEN_Card_Status_ID.Any())
        {
            _results = _results.Where(p => p.CANTEEN_Subscriber_Card_Status_ID == null || filter.CANTEEN_Card_Status_ID.Contains(p.CANTEEN_Subscriber_Card_Status_ID.Value)).ToList();
        }
        return _results;
    }
    public async Task<bool> UpdateNewCardRequestPrice(Guid transactionId, decimal price)
    {
        var trans = await _unitOfWork.Repository<PAY_Transaction>().FirstOrDefaultAsync(e => e.ID == transactionId);

        if (trans == null || trans.PaymentDate != null)
            return false;

        if (trans.TotalAmount == price)
            return true;

        var pos = await _unitOfWork.Repository<PAY_Transaction_Position>()
            .FirstOrDefaultAsync(e => e.PAY_Transaction_ID == transactionId);
        
        if (pos != null)
        {
            pos.Amount = price;
            trans.TotalAmount = price;
            await _unitOfWork.Repository<PAY_Transaction_Position>().UpdateAsync(pos);
            await _unitOfWork.Repository<PAY_Transaction>().UpdateAsync(trans);
            return true;
        }
        return false;
    }

    public async Task<MSG_Mailer?> GetChildListLinkMsg(CANTEEN_Canteen canteen, string destinationAddress,
        DateTime expirationTime,
        Guid languageId, string basePath)
    {
        var updatedCanteen = await GetCanteen(canteen.ID);
        if (updatedCanteen == null)
            return null;

        if (updatedCanteen.ExternalCheckInAccessToken == null ||
            updatedCanteen.ExternalAccessTokenExpirationDate < DateTime.Now)
        {
            updatedCanteen.ExternalCheckInAccessToken = Guid.NewGuid();
            updatedCanteen.ExternalAccessTokenExpirationDate = expirationTime;
            updatedCanteen = await SetCanteen(updatedCanteen);
        } else if (updatedCanteen.ExternalAccessTokenExpirationDate < expirationTime)
        {
            updatedCanteen.ExternalAccessTokenExpirationDate = expirationTime;
            updatedCanteen = await SetCanteen(updatedCanteen);
        }
        
        
        if (updatedCanteen == null || updatedCanteen.ExternalCheckInAccessToken == null || updatedCanteen.ExternalAccessTokenExpirationDate == null)
            return null;
        var url = basePath + "External/Canteen/CheckInListExternal/" + updatedCanteen.ExternalCheckInAccessToken;
        await InsertLinkHistory(updatedCanteen.ID, url, updatedCanteen.ExternalCheckInAccessToken.Value,
            updatedCanteen.ExternalAccessTokenExpirationDate.Value);

        var mailText = (await _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefaultAsync(e => e.Code == "CANTEEN_SENDCHILDLISTLINK_EMAIL_TEXT" && e.LANG_LanguagesID == languageId))?.Text;
        if (mailText == null)
            return null;
        if (mailText.ToLower().Contains("{link}"))
        {
            mailText = mailText.Replace("{link}", url).Replace("{Link}", url);
        }
        if (mailText.Contains("{QRCode}"))
        {
            var imageString = await GetExternalListQrCode(url);
            mailText = mailText.Replace("{QRCode}", "<img src=\"" + imageString + "\">");
        }

        if (mailText.Contains("{ExpirationDate}"))
        {
            mailText = mailText.Replace("{ExpirationDate}", updatedCanteen.ExternalAccessTokenExpirationDate.Value.ToString("dd.MM.yyyy"));
        }

        if (mailText.Contains("{ExpirationTime}"))
        {
            mailText = mailText.Replace("{ExpirationTime}", updatedCanteen.ExternalAccessTokenExpirationDate.Value.ToString("HH:mm"));
        }
        MSG_Mailer mail = new MSG_Mailer();
        mail.ToAdress = destinationAddress;
        mail.Subject = (await _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefaultAsync(e =>
            e.Code == "CANTEEN_SENDCHILDLISTLINK_EMAIL_TITLE" && e.LANG_LanguagesID == languageId))?.Text;
        mail.Body = mailText;
        mail.PlannedSendDate = DateTime.Now;
        return mail;
    }

    private async Task<bool> InsertLinkHistory(Guid canteenId, string url, Guid token, DateTime exp)
    {
        CANTEEN_Canteen_ExternalAccessLinkHistory newHistoryEntry = new CANTEEN_Canteen_ExternalAccessLinkHistory();
        newHistoryEntry.ID = Guid.NewGuid();
        newHistoryEntry.CANTEEN_Canteen_ID = canteenId;
        newHistoryEntry.CreateDate = DateTime.Now;
        newHistoryEntry.ActionSystemTextCode = "CANTEEN_EXTERNALACCESLINK_CREATED";
        newHistoryEntry.Link = url;
        newHistoryEntry.ExternalAccessToken = token;
        newHistoryEntry.ExternalAccessTokenExpirationTime = exp;
        return await _unitOfWork.Repository<CANTEEN_Canteen_ExternalAccessLinkHistory>().InsertOrUpdateAsync(newHistoryEntry) != null;
    }
    public async Task<bool> ArchiveOldSubscribers(Guid? municipalityId = null)
    {
        //Theoretisch waret a DB Instruction viel schneller
        //update CANTEEN_Subscriber set Archived = getdate(), CANTEEN_Subscriber_Status_ID = '77593932-E209-43C0-A27E-4FA24B1ACC1A' where ID in (select ID from V_CANTEEN_Subscribers_To_Archive av where av.MunicipalityId = @municipalityId)
        var subsToBeArchived = await _unitOfWork.Repository<V_CANTEEN_Subscribers_To_Archive>()
            .ToListAsync(e => municipalityId == null || e.MunicipalityId == municipalityId);
        foreach (var sub in subsToBeArchived)
        {
            var dbSub = await _unitOfWork.Repository<CANTEEN_Subscriber>().FirstOrDefaultAsync(e => e.ID == sub.ID);
            if(dbSub == null)
                continue;
            dbSub.CANTEEN_Subscriber_Status_ID = CanteenStatus.Archived;
            dbSub.Archived = DateTime.Now;
            await _unitOfWork.Repository<CANTEEN_Subscriber>().UpdateAsync(dbSub);
        }
        return true;
    }
    public async Task<bool> SubsToArchiveExist(Guid municipalityId)
    {
        return (await _unitOfWork.Repository<V_CANTEEN_Subscribers_To_Archive>()
            .FirstOrDefaultAsync(e => e.MunicipalityId == municipalityId)) != null;
    }
    public async Task<string?> GetExternalListQrCode(string url)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var apiUrl = "https://api.qr-code-generator.com/v1/create?access-token=R7_4CCAgLnTpcY2V_njamOEj06Tp_0qJ6E-ZapIYIt6nY4YpeNU3JPlzmopN3Gjt"; // Replace with your API endpoint
    
            var jsonContent = @"{
                    ""frame_name"": ""no-frame"",
                    ""qr_code_text"": ""{url}"",
                    ""image_format"": ""PNG"",
                    ""qr_code_logo"": ""no-logo""
                }";
            jsonContent = jsonContent.Replace("{url}", url);
    
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(apiUrl, content);
        
                if (response.IsSuccessStatusCode)
                {
                    var img = await response.Content.ReadAsByteArrayAsync();
                    var b64string = "data:image/png;base64," + Convert.ToBase64String(img);
                    /*if (!Directory.Exists("D:/Comunix/Resources/Canteen/QR"))
                        Directory.CreateDirectory("D:/Comunix/Resources/Canteen/QR");
                    using var file = File.OpenWrite("D:/Comunix/Resources/Canteen/QR/" + "test.png");
                    await file.WriteAsync(img);
                    return baseUrl + "Resources/Canteen/QR/" + "test.png";*/
                    return b64string;
                }
                else
                {
                    Console.WriteLine($"HTTP Request Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            return null;
        }
    }
}