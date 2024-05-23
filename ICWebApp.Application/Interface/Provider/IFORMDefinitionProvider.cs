using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IFORMDefinitionProvider
    {   
        //DEFINITION
        public Task<List<FORM_Definition>> GetDefinitionList(Guid AUTH_Municipality_ID);
        public Task<List<FORM_Definition>> GetDefinitionListByAuthoriy(Guid AUTH_Municipality_ID, Guid AUTH_Authority_ID, bool Municipal = false);
        public Task<List<FORM_Definition>> GetDefinitionListByCategory(Guid AUTH_Municipality_ID, Guid FORM_Definition_Category_ID, bool Municipal = false);
        public Task<List<V_FORM_Definition>> GetVDefinitionListByCategory(Guid AUTH_Municipality_ID, Guid FORM_Definition_Category_ID, Guid LANG_Language_ID);
        public Task<List<FORM_Definition>> GetDefinitionListOnline(Guid AUTH_Municipality_ID, bool Municipal = false);
        public Task<List<V_FORM_Definition>> GetVDefinitionListOnline(Guid AUTH_Municipality_ID, Guid LANG_Language_ID);
        public List<V_FORM_Definition> GetVDefinitionListOnlineSync(Guid AUTH_Municipality_ID, Guid LANG_Language_ID);
        public Task<List<V_FORM_Definition>> GetVDefinitionListOnlineByTheme(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Theme_ID, bool Municipal = false);
        public Task<FORM_Definition?> GetDefinition(Guid ID);
        public Task<V_FORM_Definition?> GetVDefinition(Guid ID, Guid LANG_Language_ID);
        public Task<FORM_Definition?> SetDefinition(FORM_Definition Data, bool setFormCode = false);
        public Task<bool> RemoveDefinition(Guid ID, bool force = false);
        //DEFINITION Extended
        public Task<List<FORM_Definition_Extended>> GetDefinitionExtendedList(Guid FORM_Definition_ID, Guid LANG_Language_ID);
        public Task<FORM_Definition_Extended?> GetDefinitionExtended(Guid ID);
        public Task<FORM_Definition_Extended?> SetDefinitionExtended(FORM_Definition_Extended Data);
        public Task<bool> RemoveDefinitionExtended(Guid ID);
        //Definition Category
        public Task<List<FORM_Definition_Category>> GetDefinitionCategoryList(Guid AUTH_Municipality_ID);
        //Definition Deadlines
        public Task<List<FORM_Definition_Deadlines>> GetDefinitionDeadlinesList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Deadlines?> GetDefinitionDeadlines(Guid ID);
        public Task<FORM_Definition_Deadlines?> SetDefinitionDeadlines(FORM_Definition_Deadlines Data);
        public Task<bool> RemoveDefinitionDeadlines(Guid ID, bool force = false);
        //Definition Deadlines Target
        public Task<List<FORM_Definition_Deadlines_Target>> GetDefinitionDeadlinesTargetList(Guid FORM_Definition_Deadline_ID);
        public Task<FORM_Definition_Deadlines_Target?> GetDefinitionDeadlinesTarget(Guid ID);
        public Task<FORM_Definition_Deadlines_Target?> SetDefinitionDeadlinesTarget(FORM_Definition_Deadlines_Target Data);
        public Task<bool> RemoveDefinitionDeadlinesTarget(Guid ID);
        //Definition Deadlines Time Type
        public Task<List<FORM_Definition_Deadlines_TimeType>> GetDefinitionDeadlinesTimeTypeList();
        //Definition Events
        public Task<List<FORM_Definition_Event>> GetDefinitionEventsList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Event?> GetDefinitionEvents(Guid ID);
        public Task<FORM_Definition_Event?> SetDefinitionEvents(FORM_Definition_Event Data);
        public Task<bool> RemoveDefinitionEvents(Guid ID, bool force = false);
        //Definition Events Extended
        public Task<List<FORM_Definition_Event_Extended>> GetDefinitionEventsExtendedList(Guid FORM_Definition_Event_ID, Guid LANG_Language_ID);
        public Task<FORM_Definition_Event_Extended?> GetDefinitionEventsExtended(Guid ID);
        public Task<FORM_Definition_Event_Extended?> SetDefinitionEventsExtended(FORM_Definition_Event_Extended Data);
        public Task<bool> RemoveDefinitionEventsExtended(Guid ID);
        //Definition Field
        public Task<List<FORM_Definition_Field>> GetDefinitionFieldList(Guid FORM_Definition_ID);
        public Task<List<FORM_Definition_Field>> GetDefinitionFieldListWithPayments(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Field?> GetDefinitionField(Guid ID);
        public Task<FORM_Definition_Field?> SetDefinitionField(FORM_Definition_Field Data);
        public FORM_Definition_Field? CreateDefinitionField(FORM_Definition_Field Data);
        public Task<bool> RemoveDefinitionField(Guid ID);
        //Definition Field
        public Task<List<FORM_Definition_Field_Extended>> GetDefinitionFieldExtendedList(Guid FORM_Definition_Field_ID, Guid? LANG_Language_ID);
        public Task<FORM_Definition_Field_Extended?> GetDefinitionFieldExtended(Guid ID);
        public Task<FORM_Definition_Field_Extended?> SetDefinitionFieldExtended(FORM_Definition_Field_Extended Data);
        public Task<bool?> UpdateDefinitionFieldList(List<FORM_Definition_Field> Data);
        public Task<bool> RemoveDefinitionFieldExtended(Guid ID);
        //Definition Field Option
        public Task<List<FORM_Definition_Field_Option>> GetDefinitionFieldOptionList(Guid FORM_Definition_Field_ID);
        public Task<FORM_Definition_Field_Option?> GetDefinitionFieldOption(Guid ID);
        public Task<FORM_Definition_Field_Option?> SetDefinitionFieldOption(FORM_Definition_Field_Option Data);
        public Task<bool> RemoveDefinitionFieldOption(Guid ID);
        //Definition Field Option Extended
        public Task<List<FORM_Definition_Field_Option_Extended>> GetDefinitionFieldOptionExtendedList(Guid FORM_Definition_Field_Option_ID, Guid? LANG_Language_ID);
        public Task<FORM_Definition_Field_Option_Extended?> GetDefinitionFieldOptionExtended(Guid ID);
        public Task<FORM_Definition_Field_Option_Extended?> SetDefinitionFieldOptionExtended(FORM_Definition_Field_Option_Extended Data);
        public Task<bool> RemoveDefinitionFieldOptionExtended(Guid ID);
        //Definition Field Type
        public List<FORM_Definition_Field_Type> GetDefinitionFieldTypeList();
        public FORM_Definition_Field_Type? GetDefinitionFieldType(Guid ID);
        public FORM_Definition_Field_Type? GetDefinitionFieldTypeByFieldID(Guid FORM_Definition_Field_ID);
        //Definition Field SubType
        public List<FORM_Definition_Field_SubType> GetDefinitionFieldSubTypeList(Guid FORM_Definition_Field_Type_ID);
        public List<FORM_Definition_Field_SubType> GetDefinitionFieldSubTypeList();
        public FORM_Definition_Field_SubType? GetDefinitionFieldSubType(Guid ID);
        //Definition Field Reference
        public Task<List<FORM_Definition_Field_Reference>> GetDefinitionFieldReferenceList(Guid FORM_Definition_Field_ID);
        public Task<List<FORM_Definition_Field_Reference>> GetDefinitionFieldReferenceListByDefinition(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Field_Reference?> GetDefinitionFieldReferenceBySource(Guid FORM_Definition_Field_ID);
        public Task<List<FORM_Definition_Field_Reference>> GetDefinitionFieldReferenceBySourceList(Guid FORM_Definition_Field_ID);
        public Task<FORM_Definition_Field_Reference?> GetDefinitionFieldReference(Guid ID);
        public Task<FORM_Definition_Field_Reference?> SetDefinitionFieldReference(FORM_Definition_Field_Reference Data);
        public Task<bool> RemoveDefinitionFieldReference(Guid ID);
        //Definition Property
        public Task<List<FORM_Definition_Property>> GetDefinitionPropertyList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Property?> GetDefinitionProperty(Guid ID);
        public Task<FORM_Definition_Property?> SetDefinitionProperty(FORM_Definition_Property Data);
        public Task<bool> RemoveDefinitionProperty(Guid ID, bool force = false);
        //Definition Property Extended
        public Task<List<FORM_Definition_Property_Extended>> GetDefinitionPropertyExtendedList(Guid FORM_Definition_Property_ID, Guid LANG_Language_ID);
        public Task<FORM_Definition_Property_Extended?> GetDefinitionPropertyExtended(Guid ID);
        public Task<FORM_Definition_Property_Extended?> SetDefinitionPropertyExtended(FORM_Definition_Property_Extended Data);
        public Task<bool> RemoveDefinitionPropertyExtended(Guid ID);
        //Definition Tasks
        public Task<List<FORM_Definition_Tasks>> GetDefinitionTaskList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Tasks?> GetDefinitionTask(Guid ID);
        public Task<FORM_Definition_Tasks?> SetDefinitionTask(FORM_Definition_Tasks Data);
        public Task<bool> RemoveDefinitionTask(Guid ID, bool force = false);
        //Definition Tasks Extended
        public Task<List<FORM_Definition_Tasks_Extended>> GetDefinitionTaskExtendedList(Guid FORM_Definition_Task_ID, Guid LANG_Language_ID);
        public Task<FORM_Definition_Tasks_Extended?> GetDefinitionTaskExtended(Guid ID);
        public Task<FORM_Definition_Tasks_Extended?> SetDefinitionTaskExtended(FORM_Definition_Tasks_Extended Data);
        public Task<bool> RemoveDefinitionTaskExtended(Guid ID);
        //Definition Tasks Responsible
        public Task<List<FORM_Definition_Tasks_Responsible>> GetDefinitionTaskResponsibleList(Guid FORM_Definition_Task_ID);
        public Task<FORM_Definition_Tasks_Responsible?> GetDefinitionTaskResponsible(Guid ID);
        public Task<FORM_Definition_Tasks_Responsible?> SetDefinitionTaskResponsible(FORM_Definition_Tasks_Responsible Data);
        public Task<bool> RemoveDefinitionTaskResponsible(Guid ID);
        //Definition Upload
        public Task<List<FORM_Definition_Upload>> GetDefinitionUploadList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Upload> GetDefinitionUpload(Guid ID);
        public Task<FORM_Definition_Upload?> SetDefinitionUpload(FORM_Definition_Upload Data);
        public Task<bool> RemoveDefinitionUpload(Guid ID, bool force = false);
        //Definition Upload Extended
        public Task<List<FORM_Definition_Upload_Extended>> GetDefinitionUploadExtendedList(Guid FORM_Definition_Upload_ID, Guid LANG_Language_ID);
        public Task<FORM_Definition_Upload_Extended?> GetDefinitionUploadExtended(Guid ID);
        public Task<FORM_Definition_Upload_Extended?> SetDefinitionUploadExtended(FORM_Definition_Upload_Extended Data);
        public Task<bool> RemoveDefinitionUploadExtended(Guid ID);
        //Definition Ressources
        public Task<List<FORM_Definition_Ressources>> GetDefinitionRessourceList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Ressources?> GetDefinitionRessource(Guid ID);
        public Task<FORM_Definition_Ressources?> SetDefinitionRessource(FORM_Definition_Ressources Data);
        public Task<bool> RemoveDefinitionRessource(Guid ID, bool force = false);
        //Definition Ressources Extended
        public Task<List<FORM_Definition_Ressources_Extended>> GetDefinitionRessourceExtendedList(Guid FORM_Definition_Ressource_ID, Guid LANG_Language_ID);
        public Task<FORM_Definition_Ressources_Extended?> GetDefinitionRessourceExtended(Guid ID);
        public Task<FORM_Definition_Ressources_Extended?> SetDefinitionRessourceExtended(FORM_Definition_Ressources_Extended Data);
        public Task<bool> RemoveDefinitionRessourceExtended(Guid ID);
        //Definition Additional FORM
        public Task<List<FORM_Definition_Additional_FORM>> GetDefinitionAdditionalFORMList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Additional_FORM?> GetDefinitionAdditionalFORM(Guid ID);
        public Task<FORM_Definition_Additional_FORM?> SetDefinitionAdditionalFORM(FORM_Definition_Additional_FORM Data);
        public Task<bool> RemoveDefinitionAdditionalFORM(Guid ID, bool force = false);
        //Definition Reminder
        public Task<List<FORM_Definition_Reminder>> GetDefinitionReminderList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Reminder?> GetDefinitionReminder(Guid ID);
        public Task<FORM_Definition_Reminder?> SetDefinitionReminder(FORM_Definition_Reminder Data);
        public Task<bool> RemoveDefinitionReminder(Guid ID, bool force = false);
        //Definition Reminder Type
        public Task<List<FORM_Definition_Reminder_Type>> GetDefinitionReminderTypeList();
        //Definition Signing
        public Task<List<FORM_Definition_Signings>> GetDefinitionSigningList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Signings?> GetDefinitionSigning(Guid ID);
        public Task<FORM_Definition_Signings?> SetDefinitionSigning(FORM_Definition_Signings Data);
        public Task<bool> RemoveDefinitionSigning(Guid ID, bool force = false);
        //Definition Signing Extended
        public Task<List<FORM_Definition_Signings_Extended>> GetDefinitionSigningExtendedList(Guid FORM_Definition_Signing_ID, Guid LANG_Language_ID);
        public Task<FORM_Definition_Signings_Extended?> GetDefinitionSigningExtended(Guid ID);
        public Task<FORM_Definition_Signings_Extended?> SetDefinitionSigningExtended(FORM_Definition_Signings_Extended Data);
        public Task<bool> RemoveDefinitionSigningExtended(Guid ID);
        //Definition Payment
        public Task<List<FORM_Definition_Payment_Position>> GetDefinitionPaymentList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Payment_Position?> GetDefinitionPayment(Guid ID);
        public Task<FORM_Definition_Payment_Position?> SetDefinitionPayment(FORM_Definition_Payment_Position Data);
        public Task<bool> RemoveDefinitionPayment(Guid ID, bool force = false);
        //Definition Paymenting Extended
        public Task<List<FORM_Definition_Payment_Position_Extended>> GetDefinitionPaymentExtendedList(Guid FORM_Definition_Payment_Position_ID, Guid LANG_Language_ID);
        public Task<FORM_Definition_Payment_Position_Extended?> GetDefinitionPaymentExtended(Guid ID);
        public Task<FORM_Definition_Payment_Position_Extended?> SetDefinitionPaymentExtended(FORM_Definition_Payment_Position_Extended Data);
        public Task<bool> RemoveDefinitionPaymentExtended(Guid ID);
        //Deifintion Bollo Type
        public List<FORM_Definition_Bollo_Type> GetDefinitionBolloList();
        //Statistik
        public Task<List<V_FORM_Definition_Statistik>> GetDefintionStatistik(Guid AUTH_Municipality_ID, Guid AUTH_Authority_ID);
        //Definition Mun Field
        public Task<List<FORM_Definition_Municipal_Field>> GetDefinitionMunFieldList(Guid FORM_Definition_ID);
        public Task<FORM_Definition_Municipal_Field?> GetDefinitionMunField(Guid ID);
        public Task<FORM_Definition_Municipal_Field?> SetDefinitionMunField(FORM_Definition_Municipal_Field Data);
        public Task<bool> RemoveDefinitionMunField(Guid ID);
        //Definition Mun Field Extended
        public Task<List<FORM_Definition_Municipal_Field_Extended>> GetDefinitionMunFieldExtendedList(Guid FORM_Definition_MUN_Field_ID, Guid? LANG_Language_ID);
        public Task<FORM_Definition_Municipal_Field_Extended?> GetDefinitionMunFieldExtended(Guid ID);
        public Task<FORM_Definition_Municipal_Field_Extended?> SetDefinitionMunFieldExtended(FORM_Definition_Municipal_Field_Extended Data);
        public Task<bool> RemoveDefinitionMunFieldExtended(Guid ID);
        //Definition Mun Field Type
        public Task<List<V_FORM_Definition_Municipal_Field>> GetDefinitionMunFieldList(Guid FORM_Definition_ID, Guid LANG_Language_ID);
        public Task<List<V_FORM_Definition_Municipal_Field_Type>> GetDefinitionMunFieldTypeList(Guid LANG_Language_ID);
        public Task<V_FORM_Definition_Municipal_Field_Type?> GetDefinitionMunFieldType(Guid LANG_Language_ID, Guid ID);
        //Definition Eskalation Funktions
        public Task<DateTime?> GetDefinitionEskalationDate(Guid DeadlineTypeID, DateTime? SubmitAt, DateTime? LegalDeadline, DateTime? EstimatedDeadline, long AdditionalDays);
        public Task<List<V_FORM_Definition_Template>> GetDefinitionTemplates(Guid LANG_Language_ID);
        public Task<FORM_Definition_Template?> GetDefinitionTemplate(Guid ID);
        public Task<List<FORM_Definition_Template_Signfields>> GetDefinitionTemplateSignfields(Guid FORM_Definition_Template_ID);
        //THEMES
        public Task<List<FORM_Definition_Theme>> GetThemes(Guid FORM_Definition_ID);
        public Task<List<V_HOME_Theme>> GetVThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid FORM_Definition_ID);
        public Task<bool> RemoveTheme(FORM_Definition_Theme Data);
        public Task<FORM_Definition_Theme?> SetTheme(FORM_Definition_Theme Data);
        public Task<List<V_FORM_Definition_Type>> GetTypes(Guid LANG_Language_ID);
        public List<V_FORM_Definition_Type> GetTypesSync(Guid LANG_Language_ID);
        public Task<V_FORM_Definition_Type?> GetFormType(Guid ID, Guid LANG_Language_ID);
        public Task CopyDefinition(Guid FORM_Definition_ID);
        //PEOPLE
        public Task<List<FORM_Definition_Person>> GetPeople(Guid FORM_Definition_ID);
        public Task<bool> RemovePerson(Guid FORM_Definition_Person_ID);
        public Task<FORM_Definition_Person?> SetPerson(FORM_Definition_Person Data);
    }
}
