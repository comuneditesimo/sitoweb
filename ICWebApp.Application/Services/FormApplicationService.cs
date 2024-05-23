using ICWebApp.Application.Helper;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Settings;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using SQLitePCL;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static ICWebApp.Application.Interface.Services.IFormApplicationService;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ICWebApp.Application.Services
{
    public class FormApplicationService : IFormApplicationService
    {
        private IMunicipalityHelper _MunicipalityHelper;
        private IFORMApplicationProvider _FormApplicationProvider;
        private IFORMDefinitionProvider _FormDefinitionProvider;
        private IAUTHProvider _AuthProvider;
        private ITASKService _TaskService;
        private IPAYProvider _PayProvider;
        private ITEXTProvider _TextProvider;
        private ILANGProvider _LangProvider;
        private IFILEProvider _fileProvider;
        private IMessageService _messageService;
        private ID3Helper _d3Helper;

        public FormApplicationService(IMunicipalityHelper _MunicipalityHelper, IFORMApplicationProvider _FormApplicationProvider, IFORMDefinitionProvider _FormDefinitionProvider,
                                      IAUTHProvider _AuthProvider, ITASKService _TaskService, IPAYProvider _PayProvider, ITEXTProvider _TextProvider, ILANGProvider _LangProvider,
                                      IFILEProvider _fileProvider, IMessageService _messageService, ID3Helper _d3Helper)
        {
            this._MunicipalityHelper = _MunicipalityHelper;
            this._FormApplicationProvider = _FormApplicationProvider;
            this._FormDefinitionProvider = _FormDefinitionProvider;
            this._AuthProvider = _AuthProvider;
            this._TaskService = _TaskService;
            this._PayProvider = _PayProvider;
            this._TextProvider = _TextProvider;
            this._LangProvider = _LangProvider;
            this._fileProvider = _fileProvider;
            this._messageService = _messageService;
            this._d3Helper = _d3Helper;
        }

        public async Task<Step> NextStep(FORM_Application Application, bool ForceCommit = false)
        {
            if (Application.FORM_Definition_ID == null)
                return Step.Error;

            if (Application.SubmitAt != null)
                return Step.Committed;

            var definition = await _FormDefinitionProvider.GetDefinition(Application.FORM_Definition_ID.Value);

            if (definition != null)
            {
                if (definition.HasPayment && Application.PayedAt == null)
                {
                    var appTransactions = await _FormApplicationProvider.GetApplicationTransactionList(Application.ID);

                    if(appTransactions != null)
                    {
                        foreach (var trans in appTransactions)
                        {
                            if (trans.PAY_Transaction_ID != null)
                            {
                                var payment = await _PayProvider.GetTransaction(trans.PAY_Transaction_ID.Value);

                                if (payment != null && payment.PagoPANotificationDate != null && payment.PagoPANotificationValue != "OK" && payment.PagoPANotificationValue != "KO" && payment.PagoPANotificationValue != "UK")
                                {
                                    return Step.PaymentProcessing;
                                }
                            }
                        }
                    }

                    return Step.ToPay;
                }
                else if (definition.HasBollo && Application.PayedAt == null)
                {
                    var appTransactions = await _FormApplicationProvider.GetApplicationTransactionList(Application.ID);

                    if (appTransactions != null)
                    {
                        foreach (var trans in appTransactions)
                        {
                            if (trans.PAY_Transaction_ID != null)
                            {
                                var payment = await _PayProvider.GetTransaction(trans.PAY_Transaction_ID.Value);

                                if (payment != null && payment.PagoPANotificationDate != null && payment.PagoPANotificationValue != "OK" && payment.PagoPANotificationValue != "KO" && payment.PagoPANotificationValue != "UK")
                                {
                                    return Step.PaymentProcessing;
                                }
                            }
                        }
                    }

                    return Step.ToPay;
                }
                else if ((definition.HasSigning || await HasAdditionalSignings(Application)) && Application.SignedAt == null && !ForceCommit)
                {
                    if (Application.FILE_Fileinfo_ID != null)
                    {
                        var file = await _fileProvider.GetFileInfoAsync(Application.FILE_Fileinfo_ID.Value);

                        if (file != null && file.Signed == false && file.AgreementComitted == true)
                        {
                            return Step.InSigning;
                        }
                    }

                    return Step.ToSign;
                }
                else if ((definition.HasMultiSigning || await HasAdditionalSignings(Application)) && Application.SignedAt == null)
                {
                    if (Application.FILE_Fileinfo_ID != null)
                    {
                        var file = await _fileProvider.GetFileInfoAsync(Application.FILE_Fileinfo_ID.Value);

                        if (file != null && file.Signed == false && file.AgreementComitted == true)
                        {
                            return Step.InSigningMultiSign;
                        }
                    }

                    return Step.ToSign;
                }
                else if (Application.PreviewSeenAt == null)
                {
                    return Step.ShowPreview;
                }
                else
                {
                    return Step.ToCommit;
                }
            }

            return Step.Error;
        }
        public async Task<FORM_Application> CheckApplication(FORM_Application Application, bool ForceCommit = false)
        {
            await SetApplicationLog(Application, "Check called");

            var nextStep = await NextStep(Application, ForceCommit);

            if (nextStep == Step.ToPay)
            {
                var payed = await IsPayed(Application);

                if (payed == true)
                {
                    await SetApplicationLog(Application, "Payed");

                    Application.PayedAt = DateTime.Now;
                    Application.PreviewSeenAt = DateTime.Now;

                    await _FormApplicationProvider.SetApplication(Application);

                    return await CheckApplication(Application);
                }
                else
                {
                    var transactions = await CreateOrGetTransactions(Application);

                    if (transactions != null && transactions.Count() > 0)
                    {
                        Application.FORM_Application_Status_ID = FORMStatus.ToPay;

                        await SetApplicationLog(Application, "ToPay set");

                        await _FormApplicationProvider.SetApplication(Application);

                        return Application;
                    }
                    else
                    {
                        await SetApplicationLog(Application, "Payed");

                        Application.PayedAt = DateTime.Now;
                        Application.PreviewSeenAt = DateTime.Now;

                        await _FormApplicationProvider.SetApplication(Application);

                        return await CheckApplication(Application);
                    }
                }
            }
            else if(nextStep == Step.PaymentProcessing)
            {
                await SetApplicationLog(Application, "Payment Processing");

                Application.FORM_Application_Status_ID = FORMStatus.PayProcessing;

                await _FormApplicationProvider.SetApplication(Application);

                return Application;
            }
            else if(nextStep == Step.InSigningMultiSign)
            {
                await SetApplicationLog(Application, "In Signing");

                Application.FORM_Application_Status_ID = FORMStatus.InSigning;

                await _FormApplicationProvider.SetApplication(Application);

                return Application;
            }
            else if (nextStep == Step.ToSign && ForceCommit == false)
            {
                var signed = await IsSigned(Application);

                if (signed == true)
                {
                    await SetApplicationLog(Application, "Signed");

                    Application.SignedAt = DateTime.Now;
                    Application.PreviewSeenAt = DateTime.Now;

                    await _FormApplicationProvider.SetApplication(Application);

                    return await CheckApplication(Application);
                }
                else
                {
                    Application.FORM_Application_Status_ID = FORMStatus.ToSign;

                    await SetApplicationLog(Application, "ToSign set");

                    await _FormApplicationProvider.SetApplication(Application);

                    return Application;
                }
            } else if (nextStep == Step.InSigning && ForceCommit == false)
            {
                var signed = await IsSigned(Application);
                if (signed == true)
                {
                    await SetApplicationLog(Application, "Signed");

                    Application.SignedAt = DateTime.Now;
                    Application.PreviewSeenAt = DateTime.Now;

                    await _FormApplicationProvider.SetApplication(Application);

                    return await CheckApplication(Application);
                }
                else
                {
                    Application.FORM_Application_Status_ID = FORMStatus.SignatureProcessing;

                    await SetApplicationLog(Application, "SignatureProcessing set");

                    await _FormApplicationProvider.SetApplication(Application);

                    return Application;
                }
            }
            else if (nextStep == Step.ToCommit || ForceCommit)
            {
                await SetApplicationLog(Application, "Comitted");

                if (Application.PayedAt == null)
                {
                    Application.PayedAt = DateTime.Now;
                }
                if (Application.SignedAt == null)
                {
                    Application.SignedAt = DateTime.Now;
                }

                var definition = await _FormDefinitionProvider.GetDefinition(Application.FORM_Definition_ID.Value);

                if (definition != null)
                {
                    if (definition.EstimateProcessingTime != null)
                    {
                        Application.EstimatedDeadline = DateTime.Now.AddDays(definition.EstimateProcessingTime.Value);
                    }

                    if (definition.LegalDeadline != null)
                    {
                        Application.LegalDeadline = DateTime.Now.AddDays(definition.LegalDeadline.Value);
                    }
                }

                if (Application.FORM_Definition_ID != null)
                {
                    var lastNumber = _FormApplicationProvider.GetLatestProgressivNumber(Application.FORM_Definition_ID.Value, Application.AUTH_Municipality_ID.Value, DateTime.Now.Year);

                    if (Application.ProgressivNumber == null || Application.ProgressivNumber == 0)
                    {
                        Application.ProgressivYear = DateTime.Now.Year;

                        Application.ProgressivNumber = lastNumber + 1;
                    }
                }

                Application.SubmitAt = DateTime.Now;

                Application.FORM_Application_Status_ID = FORMStatus.Comitted;

                await SetApplicationLog(Application, "Comitted set");

                await _FormApplicationProvider.SetApplication(Application);

                await _d3Helper.ProtocolNewFormApplication(Application);

                await SetApplicationLog(Application, "D3 Mail sent");

                await SendUserMessage(Application);
                await SendAuthorityMessages(Application);
                await CreateTasks(Application);
            }

            return Application;
        }
        private async Task<List<FORM_Application_Transactions>?> CreateOrGetTransactions(FORM_Application Application)
        {
            await _FormApplicationProvider.SetApplication(Application);

            var definition = await _FormDefinitionProvider.GetDefinition(Application.FORM_Definition_ID.Value);

            if (definition != null && Application.PayedAt == null && Application.PaymentStarted == null)
            {                
                await SetApplicationLog(Application, "Create transactions");

                await _FormApplicationProvider.RemoveApplicationTransactions(Application.ID);

                PAY_Transaction trans = new PAY_Transaction();

                trans.ID = Guid.NewGuid();
                trans.AUTH_Users_ID = Application.AUTH_Users_ID;
                trans.AUTH_Municipality_ID = Application.AUTH_Municipality_ID;
                trans.CreationDate = DateTime.Now;
                trans.Description = definition.FORM_Name;
                trans.PAY_Type_ID = Guid.Parse("b16d8119-e050-46c8-bb81-a1d08891f298"); //STANDARD

                decimal TotalAmount = 0;

                await _PayProvider.SetTransaction(trans);

                if (Application.AUTH_Users_ID != null)
                {
                    AUTH_Users_Anagrafic? anagrafic = await _AuthProvider.GetAnagraficByUserID(Application.AUTH_Users_ID.Value);

                    var isBolloFree = await _FormApplicationProvider.IsBolloFree(Application.ID, definition.ID);

                    if (definition.HasBollo && (anagrafic == null || anagrafic.BolloFree != true) && !isBolloFree)
                    {
                        PAY_Transaction_Position transPos = new PAY_Transaction_Position();

                        var BolloTypeList = _FormDefinitionProvider.GetDefinitionBolloList();

                        var bollo = BolloTypeList.FirstOrDefault(p => p.ID == definition.FORM_Defintion_Bollo_Type_ID);

                        if (bollo != null && definition.Bollo_Amount > 0)
                        {
                            for (int i = 0; i < definition.Bollo_Amount; i++)
                            {
                                transPos.ID = Guid.NewGuid();
                                transPos.PAY_Transaction_ID = trans.ID;
                                transPos.BolloNumber = DateTime.Now.Second + "" + DateTime.Now.Day + "" + DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Millisecond + "" + DateTime.Now.Minute + "" + DateTime.Now.Hour;
                                transPos.Description = _TextProvider.Get("FORM_PAYMENT_BOLLO_DESCRIPTION") + " - " + transPos.BolloNumber + " - " + bollo.Description;
                                transPos.BolloCreationDate = DateTime.Now;
                                transPos.IsBollo = true;

                                var pagoPaIdentifier = await _PayProvider.GetPagoPaApplicaitonsIdentifier();

                                transPos.PagoPA_Identification = pagoPaIdentifier?.PagoPA_Identifier;
                                transPos.TipologiaServizio = pagoPaIdentifier?.TipologiaServizio;
                                transPos.BolloFILE_FileInfo_ID = Application.FILE_Fileinfo_ID;
                                transPos.BolloHashType = 0;
                                transPos.Amount = bollo.Amount;

                                if (transPos.Amount != null)
                                {
                                    TotalAmount += transPos.Amount.Value;
                                }

                                await _PayProvider.SetTransactionPosition(transPos);
                            }
                        }
                    }

                    if (definition.HasPayment)
                    {
                        var PaymentList = await _FormDefinitionProvider.GetDefinitionPaymentList(definition.ID);

                        if (PaymentList != null && PaymentList.Count > 0)
                        {
                            foreach (var payment in PaymentList)
                            {
                                PAY_Transaction_Position transPos = new PAY_Transaction_Position();

                                transPos.ID = Guid.NewGuid();
                                transPos.PAY_Transaction_ID = trans.ID;

                                transPos.Description = payment.Description;
                                transPos.Amount = payment.Amount;

                                var pagoPaIdentifier = await _PayProvider.GetPagoPaApplicaitonsIdentifier();

                                transPos.PagoPA_Identification = pagoPaIdentifier?.PagoPA_Identifier;
                                transPos.TipologiaServizio = pagoPaIdentifier?.TipologiaServizio;

                                if (transPos.Amount != null)
                                {
                                    TotalAmount += transPos.Amount.Value;
                                }

                                await _PayProvider.SetTransactionPosition(transPos);
                            }
                        }

                        var elementsWithPayment = await _FormDefinitionProvider.GetDefinitionFieldListWithPayments(definition.ID);
                        var appFields = await _FormApplicationProvider.GetApplicationFieldDataList(Application.ID);

                        if (appFields != null)
                        {
                            foreach (var element in elementsWithPayment)
                            {
                                var appField = appFields.FirstOrDefault(p => p.FORM_Definition_Field_ID == element.ID);

                                if (appField != null)
                                {
                                    if (element.FORM_Definition_Fields_Type_ID == FORMElements.Dropdown ||
                                        element.FORM_Definition_Fields_Type_ID == FORMElements.Radiobutton)
                                    {
                                        if (appField.GuidValue != null)
                                        {
                                            var option = await _FormDefinitionProvider.GetDefinitionFieldOption(appField.GuidValue.Value);

                                            if (option != null && option.AdditionalCharge != 0 && option.AdditionalCharge != null)
                                            {
                                                PAY_Transaction_Position transPos = new PAY_Transaction_Position();

                                                transPos.ID = Guid.NewGuid();
                                                transPos.PAY_Transaction_ID = trans.ID;

                                                var pagoPaIdentifier = await _PayProvider.GetPagoPaApplicaitonsIdentifier();

                                                transPos.PagoPA_Identification = pagoPaIdentifier?.PagoPA_Identifier;
                                                transPos.TipologiaServizio = pagoPaIdentifier?.TipologiaServizio;

                                                if (element.FORM_Definition_Field_Extended != null && element.FORM_Definition_Field_Extended.Count() > 1)
                                                {
                                                    transPos.Description = element.FORM_Definition_Field_Extended.FirstOrDefault(p => p.LANG_Languages_ID == _LangProvider.GetCurrentLanguageID()).Name;
                                                }

                                                if (option.FORM_Definition_Field_Option_Extended != null && option.FORM_Definition_Field_Option_Extended.Count() > 1)
                                                {
                                                    if (transPos.Description != null)
                                                    {
                                                        transPos.Description += " - " + option.FORM_Definition_Field_Option_Extended.FirstOrDefault(p => p.LANG_Languages_ID == _LangProvider.GetCurrentLanguageID()).Description;
                                                    }
                                                    else
                                                    {
                                                        transPos.Description = option.FORM_Definition_Field_Option_Extended.FirstOrDefault(p => p.LANG_Languages_ID == _LangProvider.GetCurrentLanguageID()).Description;
                                                    }
                                                }

                                                transPos.Amount = option.AdditionalCharge;

                                                if (transPos.Amount != null)
                                                {
                                                    TotalAmount += transPos.Amount.Value;
                                                }

                                                await _PayProvider.SetTransactionPosition(transPos);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (element.FORM_Definition_Fields_Type_ID == FORMElements.Checkbox)
                                        {
                                            if (appField.BoolValue == true)
                                            {
                                                PAY_Transaction_Position transPos = new PAY_Transaction_Position();

                                                transPos.ID = Guid.NewGuid();
                                                transPos.PAY_Transaction_ID = trans.ID;
                                                var pagoPaIdentifier = await _PayProvider.GetPagoPaApplicaitonsIdentifier();
                                                transPos.PagoPA_Identification = pagoPaIdentifier?.PagoPA_Identifier;
                                                transPos.TipologiaServizio = pagoPaIdentifier?.TipologiaServizio;

                                                if (element.FORM_Definition_Field_Extended != null && element.FORM_Definition_Field_Extended.Count() > 1)
                                                {
                                                    transPos.Description = element.FORM_Definition_Field_Extended.FirstOrDefault(p => p.LANG_Languages_ID == _LangProvider.GetCurrentLanguageID()).Name;
                                                }

                                                transPos.Amount = element.AdditionalCharge;

                                                if (transPos.Amount != null)
                                                {
                                                    TotalAmount += transPos.Amount.Value;
                                                }

                                                await _PayProvider.SetTransactionPosition(transPos);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    trans.TotalAmount = TotalAmount;

                    if (trans.TotalAmount == 0)
                    {
                        await SetApplicationLog(Application, "Transactions Amount is Zero");
                        await _PayProvider.RemoveTransaction(trans.ID);
                    }
                    else
                    {
                        await SetApplicationLog(Application, "Transactions created");

                        await _PayProvider.SetTransaction(trans);
                        FORM_Application_Transactions appTrans = new FORM_Application_Transactions();

                        appTrans.ID = Guid.NewGuid();
                        appTrans.PAY_Transaction_ID = trans.ID;
                        appTrans.FORM_Application_ID = Application.ID;

                        await _FormApplicationProvider.SetApplicationTransaction(appTrans);

                        return new List<FORM_Application_Transactions>() { appTrans };
                    }
                }

                return null;
            }
            else
            {
                return await _FormApplicationProvider.GetApplicationTransactionList(Application.ID);
            }
        }
        private async Task<bool> IsPayed(FORM_Application Application)
        {
            await SetApplicationLog(Application, "Check Payment");

            var applicationTransactions = await _FormApplicationProvider.GetApplicationTransactionList(Application.ID);

            if (applicationTransactions != null)
            {
                bool Payed = false;

                foreach (var appTrans in applicationTransactions)
                {
                    if (appTrans.PAY_Transaction_ID != null)
                    {
                        var payTrans = await _PayProvider.GetTransaction(appTrans.PAY_Transaction_ID.Value);

                        if (payTrans != null && payTrans.PaymentDate != null)
                        {
                            Payed = true;
                        }
                        else
                        {
                            Payed = false;
                            break;
                        }
                    }
                }

                if (Payed)
                {
                    await SetApplicationLog(Application, "Payment Ok");
                }
                else
                {
                    await SetApplicationLog(Application, "Payment Not Ok");
                }

                return Payed;
            }

            return false;
        }
        private async Task<bool> IsSigned(FORM_Application Application)
        {
            await SetApplicationLog(Application, "Check Signing");

            if (Application != null && Application.FILE_Fileinfo_ID != null)
            {
                var file = await _fileProvider.GetFileInfoAsync(Application.FILE_Fileinfo_ID.Value);

                if (file != null && file.Signed)
                {
                    await SetApplicationLog(Application, "Signing Ok");
                    return true;
                }
            }

            await SetApplicationLog(Application, "Signing Not Ok");

            return false;
        }
        private async Task<bool> SendUserMessage(FORM_Application Application)
        {
            if (Application.FORM_Definition_ID != null)
            {
                var definition = await _FormDefinitionProvider.GetDefinition(Application.FORM_Definition_ID.Value);

                if (definition != null && Application.AUTH_Users_ID != null && Application.AUTH_Municipality_ID != null)
                {
                    var msg = await _messageService.GetMessage(Application.AUTH_Users_ID.Value, Application.AUTH_Municipality_ID.Value, "APPLICATION_COMITTED_MAIL_TEXT", "APPLICATION_COMITTED_MAIL_SHORTTEXT", "APPLICATION_COMITTED_MAIL_TITLE", Guid.Parse("7d03e491-5826-4131-a6a1-06c99be991c9"), true);

                    if (msg != null)
                    {
                        msg.Messagetext = msg.Messagetext.Replace("{FormName}", definition.FORM_Name);
                        msg.Messagetext = msg.Messagetext.Replace("{ProtocolNumber}", definition.FormCode + "-" + Application.ProgressivYear + "-" + Application.ProgressivNumber);

                        await _messageService.SendMessage(msg, await _MunicipalityHelper.GetMunicipalBasePath(Application.AUTH_Municipality_ID.Value) + "/Form/Application/UserDetails/" + Application.ID);

                        await SetApplicationLog(Application, "User Message sent");

                        return true;
                    }
                }
            }

            return false;
        }
        private async Task<bool> SendAuthorityMessages(FORM_Application Application)
        {
            if (Application.FORM_Definition_ID != null)
            {
                var definition = await _FormDefinitionProvider.GetDefinition(Application.FORM_Definition_ID.Value);

                if (definition != null && definition.AUTH_Authority_ID != null && Application.AUTH_Users_ID != null && Application.AUTH_Municipality_ID != null)
                {
                    var msg = await _messageService.GetMessage(Application.AUTH_Users_ID.Value, Application.AUTH_Municipality_ID.Value, "MUN_APPLICATION_COMITTED_MAIL_TEXT", "MUN_APPLICATION_COMITTED_MAIL_SHORTTEXT", "MUN_APPLICATION_COMITTED_MAIL_TITLE", Guid.Parse("7d03e491-5826-4131-a6a1-06c99be991c9"), true);

                    if (msg != null)
                    {
                        msg.Subject = msg.Subject.Replace("{FormName}", definition.FORM_Name);
                        msg.Messagetext = msg.Messagetext.Replace("{FormName}", definition.FORM_Name);
                        msg.Messagetext = msg.Messagetext.Replace("{ProtocolNumber}", definition.FormCode + "-" + Application.ProgressivYear + "-" + Application.ProgressivNumber);
                        msg.Messagetext = msg.Messagetext.Replace("{ApplicantName}", Application.FirstName + " " + Application.LastName);
                        msg.Messagetext = msg.Messagetext.Replace("{ApplicantTaxNumber}", Application.FiscalNumber);

                        await _messageService.SendMessageToAuthority(definition.AUTH_Authority_ID.Value, msg, await _MunicipalityHelper.GetMunicipalBasePath(Application.AUTH_Municipality_ID.Value) + "/Backend/Form/Detail/" + Application.ID);

                        await SetApplicationLog(Application, "Authority Message sent");

                        return true;
                    }
                }
            }

            return false;
        }
        private async Task<bool> SetApplicationLog(FORM_Application Application, string Action)
        {
            FORM_Application_Log log = new FORM_Application_Log();

            log.ID = Guid.NewGuid();
            log.Action = Action;
            log.FORM_Application_ID = Application.ID;
            log.AUTH_Users_ID = Application.AUTH_Users_ID;
            log.CreationDate = DateTime.Now;

            await _FormApplicationProvider.SetApplicationLog(log);

            return true;
        }
        private async Task<bool> CreateTasks(FORM_Application Application)
        {
            await SetApplicationLog(Application, "Tasks called");

            if (Application.FORM_Definition_ID == null)
                return false;

            if (Application.AUTH_Municipality_ID == null)
                return false;


            var definition = await _FormDefinitionProvider.GetDefinition(Application.FORM_Definition_ID.Value);

            if (definition != null)
            {
                var tasksToCreate = await _FormDefinitionProvider.GetDefinitionTaskList(Application.FORM_Definition_ID.Value);

                if (tasksToCreate != null)
                {
                    foreach (var task in tasksToCreate)
                    {
                        var responsible = await _FormDefinitionProvider.GetDefinitionTaskResponsibleList(task.ID);

                        var authUsers = new List<AUTH_Municipal_Users>();

                        if (responsible != null)
                        {
                            foreach (var resp in responsible)
                            {
                                if (resp != null)
                                {
                                    var user = await _AuthProvider.GetMunicipalUser(resp.AUTH_Municipal_Users_ID.Value);

                                    if (user != null)
                                    {
                                        authUsers.Add(user);
                                    }
                                }
                            }
                        }

                        DateTime? deadline = null;

                        if (task.DeadlineDays != null)
                        {
                            deadline = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                            deadline.Value.AddDays(task.DeadlineDays.Value);
                        }

                        var newTask = await _TaskService.CreateTask(1, Application.ID.ToString(), null, false, task.ID.ToString(), task.Description, await _MunicipalityHelper.GetMunicipalBasePath(Application.AUTH_Municipality_ID.Value) + "/Backend/Form/Detail/" + Application.ID, deadline, authUsers);
                        var eskalations = await _FormDefinitionProvider.GetDefinitionDeadlinesList(Application.FORM_Definition_ID.Value);

                        if (newTask != null && eskalations != null && eskalations.Count() > 0)
                        {
                            foreach (var esk in eskalations)
                            {
                                if (esk.FORM_Definition_Deadlines_TimeType_ID != null && esk.AdditionalDays != null)
                                {
                                    var defTargets = await _FormDefinitionProvider.GetDefinitionDeadlinesTargetList(esk.ID);
                                    var resultDate = await _FormDefinitionProvider.GetDefinitionEskalationDate(esk.FORM_Definition_Deadlines_TimeType_ID.Value, Application.SubmitAt, Application.LegalDeadline, Application.EstimatedDeadline, esk.AdditionalDays.Value);

                                    if (resultDate != null && defTargets != null && defTargets.Count() > 0)
                                    {
                                        await _TaskService.CreateEskalation(newTask.ID, resultDate.Value, defTargets.Where(p => p.AUTH_Municpal_Users_ID != null).Select(p => p.AUTH_Municpal_Users_ID.Value).ToList());
                                    }
                                }
                            }
                        }
                    }

                    await SetApplicationLog(Application, "Tasks created");
                }
            }

            return true;
        }
        private async Task<bool> HasAdditionalSignings(FORM_Application Application)
        {
            if (Application.FORM_Definition_ID == null)
                return false;

            var definition = await _FormDefinitionProvider.GetDefinition(Application.FORM_Definition_ID.Value);

            if (definition != null && definition.FORM_Definition_Template_ID != null)
            {
                var signings = await _FormDefinitionProvider.GetDefinitionTemplateSignfields(definition.FORM_Definition_Template_ID.Value);

                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(Application.TemplateJsonData);

                if (data != null)
                {

                    foreach (var sign in signings)
                    {
                        string? Name = null;
                        string? Email = null;

                        if (!string.IsNullOrEmpty(sign.FirstnameFieldName))
                        {
                            var item = data.FirstOrDefault(p => p.Key == sign.FirstnameFieldName);

                            Name = item.Value;
                        }
                        if (!string.IsNullOrEmpty(sign.LastnameFieldName))
                        {
                            var item = data.FirstOrDefault(p => p.Key == sign.LastnameFieldName);

                            Name += " " + item.Value;
                        }
                        if (!string.IsNullOrEmpty(sign.EmailFieldName))
                        {
                            var item = data.FirstOrDefault(p => p.Key == sign.EmailFieldName);

                            Email += " " + item.Value;
                        }

                        if (!string.IsNullOrEmpty(Name))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
