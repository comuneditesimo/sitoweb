using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using ICWebApp.Domain.Models.Canteen;
using ICWebApp.Domain.Models.Canteen.POS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface ICANTEENProvider
    {
        //CANTEEN
        public Task<List<CANTEEN_Canteen>> GetCanteens(Guid AUTH_Municipality_ID);
        public Task<List<CANTEEN_Canteen>> GetCanteensBySchool(Guid AUTH_Municipality_ID, Guid CANTEEN_School_ID);
        public Task<CANTEEN_Canteen?> GetCanteen(Guid ID);
        public Task<CANTEEN_Canteen?> SetCanteen(CANTEEN_Canteen Data);
        public Task<bool> RemoveCanteen(Guid ID);
        //SCHOOLS
        public Task<List<CANTEEN_School>> GetSchools(Guid AUTH_Municipality_ID);
        public Task<List<CANTEEN_School>> GetSchoolsByCanteen(Guid AUTH_Municipality_ID, Guid CANTEEN_Canteen_ID);
        public Task<CANTEEN_School?> GetSchool(Guid ID);
        public Task<CANTEEN_School?> SetSchool(CANTEEN_School Data);
        public Task<bool> RemoveSchool(Guid ID);
        //SCHOOLS_TO_CANTEENS
        public Task<List<CANTEEN_School_Canteen>?> GetSchoolsToCanteen(Guid CANTEEN_ID);
        public Task<CANTEEN_School_Canteen?> SetSchoolsToCanteen(CANTEEN_School_Canteen Data);
        public Task<bool> RemoveSchoolsToCanteen(Guid ID);
        //SUBSCRIBER
        public Task<List<CANTEEN_Subscriber>> GetSubscribersByCanteenID(Guid CANTEEN_Canteen_ID);
        public Task<List<CANTEEN_Subscriber>> GetSubscribersBySchoolID(Guid CANTEEN_School_ID);
        public Task<List<CANTEEN_Subscriber>> GetSubscribersByUserID(Guid AUTH_Users_ID);
        public Task<List<V_CANTEEN_Subscriber>> GetVSubscribersByUserID(Guid AUTH_Users_ID, Guid LanguageID);
        public Task<List<CANTEEN_Subscriber>> GetSubscribersByWeekDay(Guid CANTEEN_Canteen_ID, DayOfWeek WeekDay);
        public Task<List<CANTEEN_Subscriber>> GetSubscribersByStatusID(Guid CANTEEN_Subscriber_Status_ID);
        public Task<List<V_CANTEEN_Subscriber>> GetVSubscribersByStatusID(Guid CANTEEN_Subscriber_Status_ID, Guid LanguageID);
        public Task<V_CANTEEN_Subscriber?> GetVSubscriber(Guid CANTEEN_Subscriber_ID);
        public Task<V_CANTEEN_Subscriber?> GetVSubscriber(string Taxnumber);
        public Task<List<CANTEEN_Subscriber>> GetSubscribersByFamilyID(Guid CANTEEN_Subscriber_Family_ID);
        public List<CANTEEN_Subscriber> GetSubscribersByUserIDSync(Guid AUTH_Users_ID);
        public List<CANTEEN_Subscriber> GetActiveSubscribersByUserIDSync(Guid AUTH_Users_ID);
        public Task<CANTEEN_Subscriber_Movements?> GetRemovedRefundMovement(Guid MovementID);
        public Task<CANTEEN_Subscriber_Movements?> GetSubscriberMovementById(Guid Id);
        public Task<CANTEEN_Subscriber_Movements?> GetSubscriberMovementByRequestID(Guid RequestRefundBalanceID);
        public Task<List<CANTEEN_Subscriber_Movements>> GetSubscriberMovementsByCanteen(Guid CANTEEN_ID, bool WithRemoved = false);
        public Task<List<CANTEEN_Subscriber_Movements>> GetSubscriberMovementsBySchool(Guid School_ID, Guid? SchoolClass_ID = null, bool WithRemoved = false);
        public Task<List<V_CANTEEN_Daily_Movements>> GetSubscriberMovementsDaily(Guid AUTH_Municipality_ID, Guid LanguageID, DateTime? Day = null);
        public Task<List<V_CANTEEN_Statistic_Students>> GetStatisticStudents(Guid AUTH_Municipality_ID, Guid LanguageID);
        public Task<CANTEEN_Subscriber?> GetSubscriber(Guid ID);
        public Task<CANTEEN_Subscriber?> GetActiveSubscriber(Guid ID);
        public Task<CANTEEN_Subscriber?> GetSubscriberWithoutInclude(Guid ID);
        public Task<CANTEEN_Subscriber?> SetSubscriber(CANTEEN_Subscriber Data);
        public Task<bool> RemoveSubscriber(Guid ID, bool force = false);
        //SUBSCRIBER MOVEMENT
        public Task<List<CANTEEN_Subscriber_Movements>> GetSubscriberMovementsBySubscriber(Guid CANTEEN_Subscriber_ID);
        public Task<List<CANTEEN_Subscriber_Movements>> GetFutureMovementsBySubscriber(Guid CanteenSubId);
        public Task<List<V_CANTEEN_Subscriber_Movements>> GetVSubscriberMovementsBySubscriber(Guid CANTEEN_Subscriber_ID);
        public Task<List<V_CANTEEN_Subscriber_Movements>> GetPastVSubscriberMovementsBySubscriber(
            Guid CANTEEN_Subscriber_ID);

        public Task<List<V_CANTEEN_Subscriber_Movements>> GetFutureVSubscriberMovementsBySubscriber(
            Guid CANTEEN_Subscriber_ID);
        public Task<List<CANTEEN_Subscriber_Movements>> GetSubscriberMovementsByMovementType(Guid CANTEEN_Subscriber_Movement_Type_ID);
        public Task<CANTEEN_Subscriber_Movements?> GetSubscriberMovement(Guid ID);
        public Task<List<CANTEEN_Subscriber_Movements>> GetSubscriberMovementsByUser(Guid AUTH_USER_ID, int? Amount = null);
        public Task<List<V_CANTEEN_Subscriber_Movements>> GetVSubscriberMovementsByUser(Guid AUTH_USER_ID, int? Amount = null);
        public Task<List<V_CANTEEN_Subscriber_Movements>> GetPastVSubscriberMovementsByUser(Guid AUTH_USER_ID,
            int? Amount = null);
        public Task<CANTEEN_Subscriber_Movements?> SetSubscriberMovement(CANTEEN_Subscriber_Movements Data);
        public Task<bool> RemoveSubscriberMovement(Guid ID);
        //SUBSCRIBER MOVEMENT TYPE
        public Task<List<CANTEEN_Subscriber_Movement_Type>> GetSubscriberMovementTypes();
        //SUBSCRIBER STATUS
        public Task<List<CANTEEN_Subscriber_Status>> GetSubscriberStatuses();
        //PERIOD
        public Task<List<CANTEEN_Period>> GetPeriods(Guid AUTH_Municipality_ID);
        public Task<CANTEEN_Period?> GetPeriod(Guid ID);
        public Task<CANTEEN_Period?> GetPeriodByCanteen(Guid CANTEEN_Canteen_ID);
        public Task<CANTEEN_Period?> SetPeriod(CANTEEN_Period Data);
        public Task<bool> RemovePeriod(Guid ID);
        //MISCELANEOUS
        public decimal GetUserBalance(Guid AUTH_Users_ID);
        public decimal GetOpenPayent(Guid AUTH_Users_ID);
        public Task<bool> TaxNumberExists(Guid AUTH_Municipality_ID, Guid CANTEEN_Schoolyear_ID, string TaxNumber);
        public Task<List<CANTEEN_Subscriber>> GetSubscriberListByDate(Guid AUTH_Municipality_ID, Guid CANTEEN_Canteen_ID, DateTime Date);
        public Task<List<CANTEEN_Subscriber>> GetCancledSubscriberListByDate(Guid CANTEEN_Canteen_ID, DateTime Date);
        public Task<long> GetNextSubscriberNummer();
        public Task<bool> CreateSubscriber_Movements(Guid SubscriberID);
        public Task<List<V_CANTEEN_Schoolyear_Current>> GetSchoolsyearsCurrent(Guid AuthMunicipalityID, bool ByRegistration = false);
        public Task<List<V_CANTEEN_Schoolyear>> GetSchoolsyearAll(Guid AuthMunicipalityID);
        public Task<CANTEEN_SchoolClass?> GetSchoolClassByID(Guid ID);
        public Task<CANTEEN_Schoolyear?> GetSchoolyear(Guid ID);
        public Task<CANTEEN_Schoolyear?> SetSchoolyear(CANTEEN_Schoolyear Data, List<CANTEEN_Schoolyear_RegistrationPeriods> Periods);
        public Task<bool> RemoveSchoolyear(Guid ID);
        public Task<List<CANTEEN_Schoolyear_Base_Informations>> GetSchoolyear_BaseInformations();
        public Task<List<V_CANTEEN_Subscriber_Previous>> GetPreviousSubscriber(Guid AUTH_Users_ID, Guid LanguageID);
        public Task<List<CANTEEN_SchoolClass>?> GetSchoolClassList();
        public Task<CANTEEN_SchoolType?> GetSchoolTypeByID(Guid ID);
        public Task<List<V_CANTEEN_SchoolType>?> GetSchoolTypeList(Guid LanguageID);
        public Task<List<CANTEEN_User>> GetCanteenUserList(Guid municipalityID);
        public CANTEEN_User? GetCanteenUserByID(Guid AUTH_Users_ID, Guid municipalityID);
        public Task<CANTEEN_User?> SetCanteenUser(CANTEEN_User Data);
        public Task<List<CANTEEN_MealMenu?>> GetCANTEEN_MealMenuList(Guid Municipality_ID);
        public Task<CANTEEN_MealMenu?> GetCANTEEN_MealMenuByID(Guid ID);
        public Task<CANTEEN_MealMenu?> SetCANTEEN_MealMenu(CANTEEN_MealMenu Data);
        public Task<bool> RemoveCANTEEN_MealMenu(Guid ID);
        public string? GetApiOperationsByPhoneCode(string parentpin, string childcode, string operation);
        public bool ParentCodeExists(string parentpin);
        public bool ParentAndChildCodesExist(string parentpin, string childcode);
        public Task<List<CANTEEN_Subscriber?>> GetSubscriptions(Guid AUTH_Municipality_ID, Guid Current_AUTH_Users_ID, Administration_Filter_CanteenSubscriptions? Filter);
        public Task<List<V_CANTEEN_Subscriber?>> GetVSubscriptions(Guid AUTH_Municipality_ID, Guid Current_AUTH_Users_ID, Administration_Filter_CanteenSubscriptions? Filter, Guid LanguageID);
        public Task<List<V_CANTEEN_Subscriber>> GetVSubscriptions(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, string Taxnumber);
        public Task<List<V_CANTEEN_Fiscal_Year_Sum>> GetCANTEEN_YEAR_Balance(Guid AuthUserID);
        public Task<CANTEEN_Subscriber?> CloneAndArchiveSubscriber(Guid SubscriberID);
        //AdditionalPersonal
        public Task<List<CANTEEN_School_AdditionalPersonal>> GetAdditionalPersonalList(Guid CANTEEN_School_ID);
        public Task<List<CANTEEN_School_AdditionalPersonal>> GetAdditionalPersonalByMeal(Guid CANTEEN_School_ID, Guid CANTEEN_MealMenu_ID);
        public Task<CANTEEN_School_AdditionalPersonal?> SetAdditionalPersonal(CANTEEN_School_AdditionalPersonal Data);
        public Task<CANTEEN_School_AdditionalPersonal?> GetAdditionalPersonal(Guid ID);
        public Task<bool> RemoveAdditionalPersonal(Guid ID);
        public byte[]? GetStundentListFile(Guid LANG_Language_ID, Guid CANTEEN_Canteen_ID, List<Guid>? CANTEEN_School_ID = null);
        //Property
        public Task<List<CANTEEN_Property>> GetPropertyList(Guid AUTH_Municipality_ID, Guid LanguageID);
        public Task<CANTEEN_Property?> GetProperty(Guid ID, Guid LanguageID);
        public Task<CANTEEN_Property?> SetProperty(CANTEEN_Property Data);
        public Task<bool> RemoveProperty(Guid ID, bool force = false);
        //Property Extended
        public Task<List<CANTEEN_Property_Extended>> GetPropertyExtendedList(Guid CANTEEN_Property_ID, Guid LANG_Language_ID);
        public Task<CANTEEN_Property_Extended?> GetPropertyExtended(Guid ID);
        public Task<CANTEEN_Property_Extended?> SetPropertyExtended(CANTEEN_Property_Extended Data);
        public Task<bool> RemovePropertyExtended(Guid ID);
        //Ressources
        public Task<List<CANTEEN_Ressources>> GetRessourceList(Guid AUTH_Municipality_ID, Guid LanguageID);
        public Task<CANTEEN_Ressources?> GetRessource(Guid ID, Guid LanguageID);
        public Task<CANTEEN_Ressources?> SetRessource(CANTEEN_Ressources Data);
        public Task<bool> RemoveRessource(Guid ID, bool force = false);
        //Ressources Extended
        public Task<List<CANTEEN_Ressources_Extended>> GetRessourceExtendedList(Guid CANTEEN_Ressources_ID, Guid LANG_Language_ID);
        public Task<CANTEEN_Ressources_Extended?> GetRessourceExtended(Guid ID);
        public Task<CANTEEN_Ressources_Extended?> SetRessourceExtended(CANTEEN_Ressources_Extended Data);
        public Task<bool> RemoveRessourceExtended(Guid ID);
        //Configuration
        public Task<CANTEEN_Configuration?> GetConfiguration(Guid AUTH_Municipality_ID);
        public Task<V_CANTEEN_Configuration_Extended?> GetVConfiguration(Guid AUTH_Municipality_ID, Guid LANG_Language_ID);
        public Task<CANTEEN_Configuration?> SetConfiguration(CANTEEN_Configuration Data);
        public Task<List<CANTEEN_Configuration_Extended>> GetConfigurationExtended(Guid CANTEEN_Configuration_ID);
        public Task<CANTEEN_Configuration_Extended?> SetConfigurationExtended(CANTEEN_Configuration_Extended Data);
        public Task<bool> RemoveConfiguration(Guid ID);
        //THEMES
        public Task<List<CANTEEN_Theme>> GetThemes(Guid AUTH_Municipality_ID);
        public Task<List<V_HOME_Theme>> GetVThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID);
        public Task<bool> RemoveTheme(CANTEEN_Theme Data);
        public Task<CANTEEN_Theme?> SetTheme(CANTEEN_Theme Data);
        public Task<bool> HasTheme(Guid HOME_Theme_ID);

        //User View
        public Task<List<V_CANTEEN_User>> GetVUserList(Guid AUTH_Municipality_ID);
        public Task<V_CANTEEN_User?> GetVUser(Guid AUTH_Users_ID);
        //Registration Periods
        public Task<List<CANTEEN_Schoolyear_RegistrationPeriods>> GetRegistrationPeriodList(Guid CANTEEN_Schoolyear_ID);
        public Task<CANTEEN_Schoolyear_RegistrationPeriods?> GetRegistrationPeriod(Guid ID);
        public Task<CANTEEN_Schoolyear_RegistrationPeriods?> SetRegistrationPeriod(CANTEEN_Schoolyear_RegistrationPeriods Data);
        public Task<bool> RemoveRegistrationPeriod(Guid ID, bool force = false);
        //CANTEEN Progressiv Number
        public long GetLatestProgressivNumber(Guid AUTH_Municipality_ID, int Year);
        //CANTEEN Statistik 
        public Task<List<V_CANTEEN_Statistik_Meals>> GetStatistikMeals(Guid AUTH_Municipality_ID, DateTime FromDate, DateTime ToDate, Guid LanguageID);
        public Task<List<V_CANTEEN_Statistik_Subscribers>> GetStatistikSubscribers(Guid AUTH_Municipality_ID, Guid SchoolyearID, Guid LanguageID);
        //CANTEEN USER DOCUMENTS
        public Task<List<CANTEEN_User_Documents>> GetUserDocumentList(Guid AUTH_Users_ID);
        public Task<CANTEEN_User_Documents?> GetUserDocument(Guid ID);
        public Task<CANTEEN_User_Documents?> SetUserDocument(CANTEEN_User_Documents Data);
        public Task<bool> RemoveUserDocument(Guid ID, bool force = false);
        public Task<string> ReplaceKeywords(Guid CANTEEN_Subscriber_ID, Guid LANG_Language_ID, string Text, Guid? PreviousStatus_ID = null);
        public Task<List<CANTEEN_User_Tax_Report>> GetTaxReportsForUser(Guid userId);
        public Task<CANTEEN_Subscriber?> GetLatestCanteenSubscriberByTaxNumber(string taxNumber);
        public Task<CANTEEN_Subscriber?> GetActiveCanteenSubscriberByTaxNumber(string taxNumber);
        //POS
        public Task<CANTEEN_Subscriber_Card?> CreateSubscriberCard(Guid CANTEEN_Subscriber_ID, CardDeliveryAddress? Address = null);
        public Task<bool> RequestNewSubscriberCard(Guid CANTEEN_Subscriber_ID, CardDeliveryAddress Address);
        public Task<bool> DisableSubscriberCard(Guid CANTEEN_Subscriber_Card_ID);
        public bool SubscriberCardCanBeActivated(Guid CANTEEN_Subscriber_Card_ID);
        public Task<bool> ActivateSubscriberCard(Guid CANTEEN_Subscriber_Card_ID);
        public Task<CANTEEN_Subscriber_Card_Status_Log?> SetCardStatusLog(CANTEEN_Subscriber_Card_Status_Log Data);
        public Task<CANTEEN_Subscriber_Card_Status_Log?> SetCardStatusLog(CANTEEN_Subscriber_Card Card, int Status);
        public Task<List<V_CANTEEN_Subscriber_Card>> GetSubscriberCards(Guid LANG_Language_ID, string Taxnumber);
        public Task<List<V_CANTEEN_Subscriber_Card_Status>> GetCardStatusList(Guid LANG_Language_ID);
        public Task<List<V_CANTEEN_Subscriber_Card_Status_Log>> GetCardStatusLogList(Guid LANG_Language_ID, Guid CANTEEN_Subscriber_Card_ID);
        public Task<List<V_CANTEEN_Subscriber_Card_Log>> GetCardLogList(Guid LANG_Language_ID, Guid CANTEEN_Subscriber_Card_ID);
        public Task<CANTEEN_Subscriber_Card_Log?> SetCardLog(CANTEEN_Subscriber_Card_Log Data);

        public Task<CANTEEN_RequestRefundBalances?> GetRequestRefundBalance(string RequestRefundBalanceID);
        public Task<CANTEEN_RequestRefundBalances?> SetRequestRefundBalance(CANTEEN_RequestRefundBalances Data);
        public Task<CANTEEN_RequestRefundBalances_Resource?> SetRequestRessource(CANTEEN_RequestRefundBalances_Resource Data);
        public Task<CANTEEN_RequestRefundBalances_Status_Log?> SetRequestRefundBalanceStatusLog(CANTEEN_RequestRefundBalances_Status_Log RequestStatusLog);
        public Task<CANTEEN_RequestRefundBalances_Status_Log_Extended?> SetRequestRefundBalanceStatusLogExtended(CANTEEN_RequestRefundBalances_Status_Log_Extended RequestStatusLog);
        public Task<CANTEEN_RequestRefundBalances_Status_Log?> CreateRequestRefundBalanceStatusLogEntry(CANTEEN_RequestRefundBalances Request);
        public Task<CANTEEN_RequestRefundBalances_Status?> GetDefaultRequestRefundBalanceStatus();
        public Task<CANTEEN_RequestRefundBalances_Status?> GetSignedRequestRefundBalanceStatus();
        public Task<List<CANTEEN_RequestRefundBalances_Status>> GetAllRequestRefundBalanceStatus();
        public Task<List<CANTEEN_RequestRefundBalances_Status_Log>> GetAllRequestRefundBalanceStatusLogEntries(Guid CanteenRequestRefundBalancesID, Guid LANG_Language_ID);
        public Task<List<CANTEEN_RequestRefundBalances_Resource>> GetAllRequestRefundBalanceResource(Guid CanteenRequestRefundBalancesID);
        public Task<long> GetLatestProgressivNumberRequestRefundBalances(Guid AUTH_Municipality_ID, int Year);
        public Task<List<V_CANTEEN_RequestRefundBalances>> GetRequestRefundBalances(Guid AUTH_Municipality_ID, Administration_Filter_CanteenRequestRefundBalances? Filter, Guid LANG_Language_ID);
        public Task<List<V_CANTEEN_RequestRefundBalances>> GetRequestRefundBalances(Guid AUTH_Municipality_ID, Guid AUTH_User_ID, Guid LANG_Language_ID);
        public Task<List<V_CANTEEN_RequestRefundBalances_UsersWithRequests>> GetRequestRefundBalancesUsers(Guid AUTH_Municipality_ID);
        public Task<bool> UserHasOpenRequest(Guid AUTH_Municipality_ID, Guid AUTH_User_ID);
        public Task<List<V_CANTEEN_Student>> GetStudents(Guid AUTH_Municipality_ID);
        public Task<LoginResponse?> CreatePOSSession(string Username, string Password);
        public Task<CANTEEN_POS_Session?> GetPOSSession(Guid SessionToken);
        public Task<CANTEEN_POS_Session?> UpdateSession(CANTEEN_POS_Session Data);
        public Task<CANTEEN_POS_Session?> ExpireSession(Guid SessionToken);
        public Task<CardResponse> CheckCard(string CardID, Guid AUTH_Municipality_ID, bool Manual = false);
        //Pos Backend
        public Task<CANTEEN_Subscriber_Card?> GetCurrentSubscriberCard(string taxnumber);
        public Task<List<V_CANTEEN_Subscriber_Movements>> GetTodaysCanteenMovements(Guid canteenId, DateTime? date = null);
        public Task<bool> ManualCheckIn(Guid movementId);
        public Task<bool> ManualCheckOut(Guid movementId);
        public Task<CANTEEN_Canteen?> GetCanteenByAccessToken(Guid token);
        public Task<CANTEEN_Subscriber_Card_Request?> GetUnfinishedCardRequest(string subscriberTaxNumber);
        public Task<bool> SetSubscriberCardRequest(CANTEEN_Subscriber_Card_Request request);
        public Task<CANTEEN_Subscriber_Card_Request?> GetSubscriberCardRequest(Guid requestId);
        public Task<CANTEEN_Subscriber_Card?> SetSubscriberCardRequestPayed(Guid requestId);
        public Task<List<V_CANTEEN_Subscriber_Card>> GetAllCards(Guid municipalityID, Guid languageID);
        public Task<List<V_CANTEEN_Subscriber_Card_Request>> GetPayedCardRequests(Guid municipalityId);
        public Task<List<V_CANTEEN_Subscriber_Card_Request>> GetPayedCardRequests(Guid municipalityId, Administration_Filter_CanteenRequestCardList filter);
        public Task<MSG_Mailer?> GetChildListLinkMsg(CANTEEN_Canteen canteen, string destinationAddress, DateTime expirationTime, Guid languageId, string baseUrl);
        //
        public Task<bool> ArchiveOldSubscribers(Guid? municipalityId = null);
        public Task<bool> SubsToArchiveExist(Guid municipalityId);
        public Task<bool> UpdateNewCardRequestPrice(Guid transactionId, decimal price);
        public Task<string?> GetExternalListQrCode(string url);
    }
}
