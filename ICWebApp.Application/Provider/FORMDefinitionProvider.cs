using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using Microsoft.EntityFrameworkCore;
using ICWebApp.Domain.DBModels;
using System.Globalization;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ICWebApp.Application.Provider
{
    public class FORMDefinitionProvider : IFORMDefinitionProvider
    {
        private IUnitOfWork _unitOfWork;
        private readonly ISessionWrapper _sessionWrapper;
        private readonly ILANGProvider _langProvider;
        private readonly ITEXTProvider _textProvider;
        public FORMDefinitionProvider(IUnitOfWork _unitOfWork, ISessionWrapper _sessionWrapper, ILANGProvider _langProvider, ITEXTProvider _textProvider)
        {
            this._unitOfWork = _unitOfWork;
            this._sessionWrapper = _sessionWrapper;
            this._langProvider = _langProvider;
            this._textProvider = _textProvider;
        }
        public async Task<FORM_Definition?> GetDefinition(Guid ID)
        {
            var result = await _unitOfWork.Repository<FORM_Definition>().Where(p => p.ID == ID).Include(p => p.AUTH_Authority)
                                                                     .Include(p => p.FORM_Definition_Extended)
                                                                     .FirstOrDefaultAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var languages = await _langProvider.GetAll();

            if (result != null && Language != null) 
            {
                if (result.FORM_Definition_Extended != null && result.FORM_Definition_Extended.Count < languages.Count)
                {
                    foreach (var l in languages)
                    {
                        if (result.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                        {
                            var dataE = new FORM_Definition_Extended()
                            {
                                ID = Guid.NewGuid(),
                                FORM_Definition_ID = result.ID,
                                LANG_Language_ID = l.ID
                            };

                            result.FORM_Definition_Extended.Add(dataE);
                        }
                    }
                }

                var extended = result.FORM_Definition_Extended?.FirstOrDefault(p => p.LANG_Language_ID == Language.ID);

                if (extended != null)
                {
                    result.FORM_Name = extended.Name;
                    result.FORM_Description = extended.Description;
                    result.ShortText = extended.ShortText;
                }

                if (result.AUTH_Authority != null && result.AUTH_Authority.TEXT_SystemText_Code != null)
                {
                    result.AmtName = _textProvider.Get(result.AUTH_Authority.TEXT_SystemText_Code);
                }
            }

            return result;
        }
        public async Task<V_FORM_Definition?> GetVDefinition(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Definition>().Where(p => p.ID == ID && p.LANG_Language_ID == LANG_Language_ID).FirstOrDefaultAsync();
        }
        public async Task<FORM_Definition_Additional_FORM?> GetDefinitionAdditionalFORM(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Additional_FORM>().FirstOrDefaultAsync(p => p.ID == ID);

            if(data != null && data.FORM_Definition_Additional_ID != null)
            {
                var FormDefinition = await GetDefinition(data.FORM_Definition_Additional_ID.Value);

                if (FormDefinition != null)
                {
                    data.DefintionName = FormDefinition.FORM_Name;
                }
            }

            return data;
        }
        public async Task<List<FORM_Definition_Additional_FORM>> GetDefinitionAdditionalFORMList(Guid FORM_Definition_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Additional_FORM>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).ToListAsync();

            foreach (var d in data)
            {
                if (d != null && d.FORM_Definition_Additional_ID != null)
                {
                    var FormDefinition = await GetDefinition(d.FORM_Definition_Additional_ID.Value);

                    if (FormDefinition != null)
                    {
                        d.DefintionName = FormDefinition.FORM_Name;
                    }
                }
            }

            return data;
        }
        public async Task<List<FORM_Definition_Category>> GetDefinitionCategoryList(Guid AUTH_Municipality_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Category>().ToListAsync();

            foreach(var d in data) 
            {
                if (d.TEXT_SystemTexts_Code != null)
                {
                    d.Name = _textProvider.Get(d.TEXT_SystemTexts_Code);
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Deadlines?> GetDefinitionDeadlines(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Deadlines>().Where(p => p.ID == ID).Include(p => p.FORM_Definition_Deadlines_TimeType).FirstOrDefaultAsync();

            if (data != null && data.FORM_Definition_Deadlines_TimeType != null)
            {
                data.TimeType = _textProvider.Get(data.FORM_Definition_Deadlines_TimeType.Text_SystemTexts_Code);
            }

            return data;
        }
        public async Task<List<FORM_Definition_Deadlines>> GetDefinitionDeadlinesList(Guid FORM_Definition_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Deadlines>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).Include(p => p.FORM_Definition_Deadlines_TimeType).ToListAsync();

            foreach (var d in data)
            {
                if (d != null && d.FORM_Definition_Deadlines_TimeType != null)
                {
                    d.TimeType = _textProvider.Get(d.FORM_Definition_Deadlines_TimeType.Text_SystemTexts_Code);
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Deadlines_Target?> GetDefinitionDeadlinesTarget(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Deadlines_Target>().Where(p => p.ID == ID).Include(p => p.AUTH_Municpal_Users).FirstOrDefaultAsync();

            if (data != null && data.AUTH_Municpal_Users != null)
            {
                data.Fullname = data.AUTH_Municpal_Users.Firstname + " " + data.AUTH_Municpal_Users.Lastname;
                data.Email = data.AUTH_Municpal_Users.Email;
            }

            return data;
        }
        public async Task<List<FORM_Definition_Deadlines_Target>> GetDefinitionDeadlinesTargetList(Guid FORM_Definition_Deadline_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Deadlines_Target>().Where(p => p.FORM_Definition_Deadline_ID == FORM_Definition_Deadline_ID).Include(p => p.AUTH_Municpal_Users).ToListAsync();

            foreach (var d in data)
            {
                if (d != null && d.AUTH_Municpal_Users != null)
                {
                    d.Fullname = d.AUTH_Municpal_Users.Firstname + " " + d.AUTH_Municpal_Users.Lastname;
                    d.Email = d.AUTH_Municpal_Users.Email;
                }
            }

            return data;
        }
        public async Task<List<FORM_Definition_Deadlines_TimeType>> GetDefinitionDeadlinesTimeTypeList()
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Deadlines_TimeType>().ToListAsync();

            foreach(var d in data)
            {
                d.Description = _textProvider.Get(d.Text_SystemTexts_Code);
            }

            return data;
        }
        public async Task<FORM_Definition_Event?> GetDefinitionEvents(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Event>().Where(p => p.ID == ID).Include(p => p.FORM_Definition_Event_Extended).FirstOrDefaultAsync();
            
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (data != null && Language != null)
            {
                var extended = data.FORM_Definition_Event_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                if (extended != null)
                {
                    data.Title = extended.Title;
                    data.Description = extended.Description;
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Event_Extended?> GetDefinitionEventsExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Event_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<FORM_Definition_Event_Extended>> GetDefinitionEventsExtendedList(Guid FORM_Definition_Event_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Event_Extended>().Where(p => p.FORM_Definition_Event_ID == FORM_Definition_Event_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<FORM_Definition_Event>> GetDefinitionEventsList(Guid FORM_Definition_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Event>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).Include(p => p.FORM_Definition_Event_Extended).ToListAsync();
            
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var languages = await _langProvider.GetAll();

            foreach (var d in data) 
            {
                if (data != null && Language != null)
                {
                    if (d.FORM_Definition_Event_Extended != null && d.FORM_Definition_Event_Extended.Count < languages.Count)
                    {
                        foreach (var l in languages)
                        {
                            if (d.FORM_Definition_Event_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                            {
                                var dataE = new FORM_Definition_Event_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    FORM_Definition_Event_ID = d.ID,
                                    LANG_Languages_ID = l.ID
                                };

                                d.FORM_Definition_Event_Extended.Add(dataE);
                            }
                        }
                    }

                    var extended = d.FORM_Definition_Event_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                    if (extended != null)
                    {
                        d.Title = extended.Title;
                        d.Description = extended.Description;
                    }
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Extended?> GetDefinitionExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<FORM_Definition_Extended>> GetDefinitionExtendedList(Guid FORM_Definition_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Extended>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<FORM_Definition_Field?> GetDefinitionField(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field>().Where(p => p.ID == ID)
                                                                        .Include(p => p.FORM_Definition_Field_Extended)
                                                                        .Include(p => p.FORM_Definition_Field_Option)
                                                                        .ThenInclude(p => p.FORM_Definition_Field_Option_Extended)
                                                                        .FirstOrDefaultAsync();
        }
        public async Task<FORM_Definition_Field_Extended?> GetDefinitionFieldExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<FORM_Definition_Field_Extended>> GetDefinitionFieldExtendedList(Guid FORM_Definition_Field_ID, Guid? LANG_Language_ID)
        {
            if (LANG_Language_ID != null)
            {
                return await _unitOfWork.Repository<FORM_Definition_Field_Extended>().Where(p => p.FORM_Definition_Field_ID == FORM_Definition_Field_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
            }
            else
            {
                return await _unitOfWork.Repository<FORM_Definition_Field_Extended>().Where(p => p.FORM_Definition_Field_ID == FORM_Definition_Field_ID).ToListAsync();
            }
        }
        public async Task<List<FORM_Definition_Field>> GetDefinitionFieldList(Guid FORM_Definition_ID)
        {
                return await _unitOfWork.Repository<FORM_Definition_Field>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID)
                                                                            .Include(p => p.FORM_Definition_Field_Extended)
                                                                            .Include(p => p.FORM_Definition_Field_Option).ThenInclude(p => p.FORM_Definition_Field_Option_Extended)
                                                                            .Include(p => p.FORM_Definition_Field_ReferenceFORM_Definition_Field)
                                                                            .ToListAsync(); 
        }
        public async Task<List<FORM_Definition_Field>> GetDefinitionFieldListWithPayments(Guid FORM_Definition_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID && p.HasAdditionalCharge == true)
                                                                        .Include(p => p.FORM_Definition_Field_Extended)
                                                                        .Include(p => p.FORM_Definition_Field_Option).ThenInclude(p => p.FORM_Definition_Field_Option_Extended)
                                                                        .ToListAsync();
        }
        public async Task<FORM_Definition_Field_Option?> GetDefinitionFieldOption(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Option>().Where(p => p.ID == ID)
                                                                               .Include(p => p.FORM_Definition_Field_Option_Extended)
                                                                               .FirstOrDefaultAsync();
        }
        public async Task<FORM_Definition_Field_Option_Extended?> GetDefinitionFieldOptionExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Option_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<FORM_Definition_Field_Option_Extended>> GetDefinitionFieldOptionExtendedList(Guid FORM_Definition_Field_Option_ID, Guid? LANG_Language_ID)
        {
            if (LANG_Language_ID != null)
            {
                return await _unitOfWork.Repository<FORM_Definition_Field_Option_Extended>().Where(p => p.FORM_Definition_Field_Option_ID == FORM_Definition_Field_Option_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
            }
            else
            {
                return await _unitOfWork.Repository<FORM_Definition_Field_Option_Extended>().Where(p => p.FORM_Definition_Field_Option_ID == FORM_Definition_Field_Option_ID).ToListAsync();
            }
        }
        public async Task<List<FORM_Definition_Field_Option>> GetDefinitionFieldOptionList(Guid FORM_Definition_Field_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Field_Option>().Where(p => p.FORM_Definition_Field_ID == FORM_Definition_Field_ID)
                                                                                            .Include(p => p.FORM_Definition_Field_Option_Extended).ToListAsync();
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            foreach (var d in data)
            {
                if (data != null && Language != null)
                {
                    var extended = d.FORM_Definition_Field_Option_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                    if (extended != null)
                    {
                        d.Description = extended.Description;
                    }
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Field_Reference?> GetDefinitionFieldReference(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Reference>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<FORM_Definition_Field_Reference?> GetDefinitionFieldReferenceBySource(Guid FORM_Definition_Field_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Reference>().FirstOrDefaultAsync(p => p.FORM_Definition_Field_Source_ID == FORM_Definition_Field_ID);
        }
        public async Task<List<FORM_Definition_Field_Reference>> GetDefinitionFieldReferenceBySourceList(Guid FORM_Definition_Field_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Reference>().ToListAsync(p => p.FORM_Definition_Field_Source_ID == FORM_Definition_Field_ID);
        }
        public async Task<List<FORM_Definition_Field_Reference>> GetDefinitionFieldReferenceList(Guid FORM_Definition_Field_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Reference>().Where(p => p.FORM_Definition_Field_ID == FORM_Definition_Field_ID)
                                                                                  .Include(p => p.FORM_Definition_Field_Source)
                                                                                  .ThenInclude(p => p.FORM_Definition_Field_Extended).ToListAsync();
        }
        public async Task<List<FORM_Definition_Field_Reference>> GetDefinitionFieldReferenceListByDefinition(Guid FORM_Definition_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Reference>().Where(p => p.FORM_Definition_Field != null && p.FORM_Definition_Field.FORM_Definition_ID == FORM_Definition_ID)
                                                                                  .Include(p => p.FORM_Definition_Field)
                                                                                  .Include(p => p.FORM_Definition_Field_Source).ToListAsync();
        }
        public List<FORM_Definition_Field_SubType> GetDefinitionFieldSubTypeList(Guid FORM_Definition_Field_Type_ID)
        {
            var data = _unitOfWork.Repository<FORM_Definition_Field_SubType>().Where(p => p.FORM_Definition_Field_Type_ID == FORM_Definition_Field_Type_ID).ToList();

            foreach(var d in data)
            {
                if (d.TEXT_SystemTEXT_Code != null)
                {
                    d.Name = _textProvider.Get(d.TEXT_SystemTEXT_Code);
                }
            }

            return data;
        }
        public FORM_Definition_Field_SubType? GetDefinitionFieldSubType(Guid ID)
        {
            return _unitOfWork.Repository<FORM_Definition_Field_SubType>().FirstOrDefault(p => p.ID == ID);
        }
        public FORM_Definition_Field_Type? GetDefinitionFieldType(Guid ID)
        {
            var data = _unitOfWork.Repository<FORM_Definition_Field_Type>().Where(p => p.ID == ID).Include(p => p.FORM_Definition_Field_SubType).FirstOrDefault();

            if(data != null && data.TEXT_SystemText_Code != null)
            {
                data.Name = _textProvider.Get(data.TEXT_SystemText_Code);
            }

            return data;
        }
        public FORM_Definition_Field_Type? GetDefinitionFieldTypeByFieldID(Guid FORM_Definition_Field_ID)
        {
            FORM_Definition_Field_Type? data = null;

            var formField = _unitOfWork.Repository<FORM_Definition_Field>().FirstOrDefault(p => p.ID == FORM_Definition_Field_ID);

            if (formField != null)
            {
                data = _unitOfWork.Repository<FORM_Definition_Field_Type>().Where(p => p.ID == formField.FORM_Definition_Fields_Type_ID).Include(p => p.FORM_Definition_Field_SubType).FirstOrDefault();
            }

            if (data != null && data.TEXT_SystemText_Code != null)
            {
                data.Name = _textProvider.Get(data.TEXT_SystemText_Code);
            }

            return data;
        }
        public List<FORM_Definition_Field_Type> GetDefinitionFieldTypeList()
        {
            var data = _unitOfWork.Repository<FORM_Definition_Field_Type>().Where().Include(p => p.FORM_Definition_Field_SubType).ToList();

            foreach (var d in data) {

                if (d != null && d.TEXT_SystemText_Code != null)
                {
                    d.Name = _textProvider.Get(d.TEXT_SystemText_Code);
                }
            }

            return data;
        }
        public async Task<List<FORM_Definition>> GetDefinitionList(Guid AUTH_Municipality_ID)
        {
            var result = await _unitOfWork.Repository<FORM_Definition>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedAt == null)
                                                                        .Include(p => p.FORM_Definition_Extended)
                                                                        .Include(p => p.AUTH_Authority)
                                                                        .ToListAsync();

            foreach (var r in result) 
            {
                var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

                if (r != null && Language != null)
                {
                    var extended = r.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Language.ID);

                    if (extended != null)
                    {
                        r.FORM_Name = extended.Name;
                        r.FORM_Description = extended.Description;
                        r.ShortText = extended.ShortText;
                    }

                    if (r.AUTH_Authority != null && r.AUTH_Authority.TEXT_SystemText_Code != null)
                    {
                        r.AmtName = _textProvider.Get(r.AUTH_Authority.TEXT_SystemText_Code);
                    }
                } 
            }

            return result;
        }
        public async Task<List<FORM_Definition>> GetDefinitionListOnline(Guid AUTH_Municipality_ID, bool Municipal = false)
        {
            var result = await _unitOfWork.Repository<FORM_Definition>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedAt == null && p.Enabled && p.OnlyForMunicipal == Municipal)
                                                                        .Include(p => p.FORM_Definition_Extended)
                                                                        .Include(p => p.AUTH_Authority)
                                                                        .ToListAsync();

            foreach (var r in result)
            {
                var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

                if (r != null && Language != null)
                {
                    var extended = r.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Language.ID);

                    if (extended != null)
                    {
                        r.FORM_Name = extended.Name;
                        r.FORM_Description = extended.Description;  
                        r.ShortText = extended.ShortText;
                    }

                    if (r.AUTH_Authority != null && r.AUTH_Authority.TEXT_SystemText_Code != null)
                    {
                        r.AmtName = _textProvider.Get(r.AUTH_Authority.TEXT_SystemText_Code);
                    }
                }
            }

            return result;
        }
        public async Task<List<V_FORM_Definition>> GetVDefinitionListOnline(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Definition>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedAt == null && p.LANG_Language_ID == LANG_Language_ID && p.Enabled).ToListAsync();
        }
        public List<V_FORM_Definition> GetVDefinitionListOnlineSync(Guid AUTH_Municipality_ID, Guid LANG_Language_ID)
        {
            return _unitOfWork.Repository<V_FORM_Definition>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedAt == null && p.LANG_Language_ID == LANG_Language_ID && p.Enabled).ToList();
        }
        public async Task<List<V_FORM_Definition>> GetVDefinitionListOnlineByTheme(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid HOME_Theme_ID, bool Municipal = false)
        {
            var themeArticles = _unitOfWork.Repository<FORM_Definition_Theme>().Where(p => p.HOME_Theme_ID == HOME_Theme_ID).ToList();

            var data = _unitOfWork.Repository<V_FORM_Definition>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID);

            return await data.Where(p => themeArticles.Select(x => x.FORM_Definition_ID).Contains(p.ID)).ToListAsync();
        }
        public async Task<List<FORM_Definition>> GetDefinitionListByAuthoriy(Guid AUTH_Municipality_ID, Guid AUTH_Authority_ID, bool Municipal = false)
        {
            var result = await _unitOfWork.Repository<FORM_Definition>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedAt == null && 
                                                                                    p.Enabled && p.OnlyForMunicipal == Municipal &&
                                                                                    p.AUTH_Authority_ID == AUTH_Authority_ID)
                                                                        .Include(p => p.FORM_Definition_Extended)
                                                                        .Include(p => p.AUTH_Authority)
                                                                        .ToListAsync();

            foreach (var r in result)
            {
                var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

                if (r != null && Language != null)
                {
                    var extended = r.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Language.ID);

                    if (extended != null)
                    {
                        r.FORM_Name = extended.Name;
                        r.FORM_Description = extended.Description;
                        r.ShortText = extended.ShortText;
                    }

                    if (r.AUTH_Authority != null && r.AUTH_Authority.TEXT_SystemText_Code != null)
                    {
                        r.AmtName = _textProvider.Get(r.AUTH_Authority.TEXT_SystemText_Code);
                    }
                }
            }

            return result;
        }
        public async Task<List<V_FORM_Definition>> GetVDefinitionListByCategory(Guid AUTH_Municipality_ID, Guid FORM_Definition_Category_ID, Guid LANG_Language_ID)
        {
            var result = await _unitOfWork.Repository<V_FORM_Definition>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID && p.FORM_Definition_Category_ID == FORM_Definition_Category_ID &&
                                                                                    p.RemovedAt == null).OrderBy(p => p.FORM_Definition_Category_ID).ThenBy(p => p.AmtName).ThenBy(p => p.Name).ToListAsync();
            return result;
        }
        public async Task<List<FORM_Definition>> GetDefinitionListByCategory(Guid AUTH_Municipality_ID, Guid FORM_Definition_Category_ID, bool Municipal = false)
        {
            var result = await _unitOfWork.Repository<FORM_Definition>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.FORM_Definition_Category_ID == FORM_Definition_Category_ID && 
                                                                                    p.RemovedAt == null && p.OnlyForMunicipal == Municipal)
                                                                        .Include(p => p.FORM_Definition_Extended)
                                                                        .Include(p => p.AUTH_Authority)
                                                                        .ToListAsync();

            foreach (var r in result)
            {
                var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

                if (r != null && Language != null)
                {
                    var extended = r.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Language.ID);

                    if (extended != null)
                    {
                        r.FORM_Name = extended.Name;
                        r.FORM_Description = extended.Description;
                        r.ShortText = extended.ShortText;
                    }

                    if (r.AUTH_Authority != null && r.AUTH_Authority.TEXT_SystemText_Code != null)
                    {
                        r.AmtName = _textProvider.Get(r.AUTH_Authority.TEXT_SystemText_Code);
                    }
                }
            }

            return result;
        }
        public async Task<FORM_Definition_Property?> GetDefinitionProperty(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Property>().Where(p => p.ID == ID).Include(p => p.FORM_Definition_Property_Extended).FirstOrDefaultAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (data != null && Language != null)
            {
                var extended = data.FORM_Definition_Property_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                if (extended != null)
                {
                    data.Title = extended.Title;
                    data.Description = extended.Description;
                }
            }            

            return data;
        }
        public async Task<FORM_Definition_Property_Extended?> GetDefinitionPropertyExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Property_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<FORM_Definition_Property_Extended>> GetDefinitionPropertyExtendedList(Guid FORM_Definition_Property_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Property_Extended>().Where(p => p.FORM_Definition_Property_ID == FORM_Definition_Property_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<FORM_Definition_Property>> GetDefinitionPropertyList(Guid FORM_Definition_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Property>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).Include(p => p.FORM_Definition_Property_Extended).ToListAsync();
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var languages = await _langProvider.GetAll();

            foreach (var d in data)
            {
                if (d != null && Language != null)
                {
                    if (d.FORM_Definition_Property_Extended != null && d.FORM_Definition_Property_Extended.Count < languages.Count)
                    {
                        foreach (var l in languages)
                        {
                            if (d.FORM_Definition_Property_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                            {
                                var dataE = new FORM_Definition_Property_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    FORM_Definition_Property_ID = d.ID,
                                    LANG_Languages_ID = l.ID
                                };

                                d.FORM_Definition_Property_Extended.Add(dataE);
                            }
                        }
                    }

                    var extended = d.FORM_Definition_Property_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                    if (extended != null)
                    {
                        d.Title = extended.Title;
                        d.Description = extended.Description;
                    }
                }
                
            }

            return data;
        }
        public async Task<FORM_Definition_Reminder?> GetDefinitionReminder(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Reminder>().FirstOrDefaultAsync(p => p.ID == ID);

            var types = await GetDefinitionReminderTypeList();

            if (data != null && data.FORM_Definition_Reminder_Type_ID != null)
            {
                var type = types.FirstOrDefault(p => p.ID == data.FORM_Definition_Reminder_Type_ID);

                if (type != null)
                {
                    data.Type = type.Description;
                }
            }            

            return data;
        }
        public async Task<List<FORM_Definition_Reminder>> GetDefinitionReminderList(Guid FORM_Definition_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Reminder>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).ToListAsync();

            var types = await GetDefinitionReminderTypeList();

            foreach (var d in data)
            {
                if(d.FORM_Definition_Reminder_Type_ID != null)
                {
                    var type = types.FirstOrDefault(p => p.ID == d.FORM_Definition_Reminder_Type_ID);

                    if(type != null)
                    {
                        d.Type = type.Description;
                    }
                }
            }

            return data;
        }
        public async Task<List<FORM_Definition_Reminder_Type>> GetDefinitionReminderTypeList()
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Reminder_Type>().ToListAsync();

            foreach (var d in data)
            {
                if (d != null && d.TEXT_SystemText_Code != null)
                {
                    d.Description = _textProvider.Get(d.TEXT_SystemText_Code);
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Ressources?> GetDefinitionRessource(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Ressources>().Where(p => p.ID == ID).Include(p => p.FORM_Definition_Ressources_Extended).FirstOrDefaultAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (data != null && Language != null)
            {
                var extended = data.FORM_Definition_Ressources_Extended.FirstOrDefault(p => p.LANG_Language_ID == Language.ID);

                if (extended != null)
                {
                    data.Description = extended.Description;
                }
            }
            return data;
        }
        public async Task<FORM_Definition_Ressources_Extended?> GetDefinitionRessourceExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Ressources_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<FORM_Definition_Ressources_Extended>> GetDefinitionRessourceExtendedList(Guid FORM_Definition_Ressource_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Ressources_Extended>().Where(p => p.FORM_Definition_Ressources_ID == FORM_Definition_Ressource_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<FORM_Definition_Ressources>> GetDefinitionRessourceList(Guid FORM_Definition_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Ressources>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).Include(p => p.FORM_Definition_Ressources_Extended).ToListAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var languages = await _langProvider.GetAll();

            foreach (var d in data)
            {
                if (d != null && Language != null)
                {
                    if (d.FORM_Definition_Ressources_Extended != null && d.FORM_Definition_Ressources_Extended.Count < languages.Count)
                    {
                        foreach (var l in languages)
                        {
                            if (d.FORM_Definition_Ressources_Extended.FirstOrDefault(p => p.LANG_Language_ID == l.ID) == null)
                            {
                                var dataE = new FORM_Definition_Ressources_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    FORM_Definition_Ressources_ID = d.ID,
                                    LANG_Language_ID = l.ID
                                };

                                d.FORM_Definition_Ressources_Extended.Add(dataE);
                            }
                        }
                    }

                    var extended = d.FORM_Definition_Ressources_Extended.FirstOrDefault(p => p.LANG_Language_ID == Language.ID);

                    if (extended != null)
                    {
                        d.Description = extended.Description;
                    }
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Tasks?> GetDefinitionTask(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Tasks>().Where(p => p.ID == ID).Include(p => p.FORM_Definition_Tasks_Extended).FirstOrDefaultAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (data != null && Language != null)
            {
                var extended = data.FORM_Definition_Tasks_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                if (extended != null)
                {
                    data.Description = extended.Description;
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Tasks_Extended?> GetDefinitionTaskExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Tasks_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<FORM_Definition_Tasks_Extended>> GetDefinitionTaskExtendedList(Guid FORM_Definition_Task_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Tasks_Extended>().Where(p => p.FORM_Definition_Tasks_ID == FORM_Definition_Task_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<FORM_Definition_Tasks>> GetDefinitionTaskList(Guid FORM_Definition_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Tasks>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).Include(p => p.FORM_Definition_Tasks_Extended).ToListAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var languages = await _langProvider.GetAll();

            foreach (var d in data)
            {
                if (data != null && Language != null)
                {
                    if (d.FORM_Definition_Tasks_Extended != null && d.FORM_Definition_Tasks_Extended.Count < languages.Count)
                    {
                        foreach (var l in languages)
                        {
                            if (d.FORM_Definition_Tasks_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                            {
                                var dataE = new FORM_Definition_Tasks_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    FORM_Definition_Tasks_ID = d.ID,
                                    LANG_Languages_ID = l.ID
                                };

                                d.FORM_Definition_Tasks_Extended.Add(dataE);
                            }
                        }
                    }

                    var extended = d.FORM_Definition_Tasks_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                    if (extended != null)
                    {
                        d.Description = extended.Description;
                    }
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Upload> GetDefinitionUpload(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Upload>().Where(p => p.ID == ID).Include(p => p.FORM_Definition_Upload_Extended).FirstOrDefaultAsync(); 

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (data != null && Language != null)
            {
                var extended = data.FORM_Definition_Upload_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                if (extended != null)
                {
                    data.Description = extended.Description;
                }

                if (!string.IsNullOrEmpty(data.AllowedTypes))
                {
                    data.AlloweTypesParsed = data.AllowedTypes.Replace(";", " ");
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Upload_Extended?> GetDefinitionUploadExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Upload_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<FORM_Definition_Upload_Extended>> GetDefinitionUploadExtendedList(Guid FORM_Definition_Upload_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Upload_Extended>().Where(p => p.FORM_Definition_Upload_ID == FORM_Definition_Upload_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<FORM_Definition_Upload>> GetDefinitionUploadList(Guid FORM_Definition_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Upload>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).Include(p => p.FORM_Definition_Upload_Extended).ToListAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var languages = await _langProvider.GetAll();

            foreach (var d in data) 
            {
                if (d != null && Language != null)
                {
                    if (d.FORM_Definition_Upload_Extended != null && d.FORM_Definition_Upload_Extended.Count < languages.Count)
                    {
                        foreach (var l in languages)
                        {
                            if (d.FORM_Definition_Upload_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                            {
                                var dataE = new FORM_Definition_Upload_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    FORM_Definition_Upload_ID = d.ID,
                                    LANG_Languages_ID = l.ID
                                };

                                d.FORM_Definition_Upload_Extended.Add(dataE);
                            }
                        }
                    }

                    var extended = d.FORM_Definition_Upload_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                    if (extended != null)
                    {
                        d.Description = extended.Description;
                    }

                    if (!string.IsNullOrEmpty(d.AllowedTypes))
                    {
                        d.AlloweTypesParsed = d.AllowedTypes.Replace(";", " ");
                    }
                }
            }

            return data;
        }
        public async Task<bool> RemoveDefinition(Guid ID, bool force = false)
        {
            var itemexists = _unitOfWork.Repository<FORM_Definition>().Where(p => p.ID == ID).FirstOrDefault();

            if (itemexists != null)
            {
                if (force)
                {
                    await _unitOfWork.Repository<FORM_Definition>().DeleteAsync(itemexists);
                }
                else
                {
                    itemexists.RemovedAt = DateTime.Now;

                    await _unitOfWork.Repository<FORM_Definition>().InsertOrUpdateAsync(itemexists);
                }
            }

            return true;
        }
        public async Task<bool> RemoveDefinitionAdditionalFORM(Guid ID, bool force = false)
        {
            return await _unitOfWork.Repository<FORM_Definition_Additional_FORM>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionDeadlines(Guid ID, bool force = false)
        {
            return await _unitOfWork.Repository<FORM_Definition_Deadlines>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionDeadlinesTarget(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Deadlines_Target>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionEvents(Guid ID, bool force = false)
        {
            return await _unitOfWork.Repository<FORM_Definition_Event>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionEventsExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Event_Extended>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Extended>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionField(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionFieldExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Extended>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionFieldOption(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Option>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionFieldOptionExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Option_Extended>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionFieldReference(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Reference>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionProperty(Guid ID, bool force = false)
        {
            return await _unitOfWork.Repository<FORM_Definition_Property>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionPropertyExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Property_Extended>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionReminder(Guid ID, bool force = false)
        {
            return await _unitOfWork.Repository<FORM_Definition_Reminder>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionRessource(Guid ID, bool force = false)
        {
            return await _unitOfWork.Repository<FORM_Definition_Ressources>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionRessourceExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Ressources_Extended>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionTask(Guid ID, bool force = false)
        {
            return await _unitOfWork.Repository<FORM_Definition_Tasks>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionTaskExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Tasks_Extended>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionUpload(Guid ID, bool force = false)
        {
            return await _unitOfWork.Repository<FORM_Definition_Upload>().DeleteAsync(ID);
        }
        public async Task<bool> RemoveDefinitionUploadExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Upload_Extended>().DeleteAsync(ID);
        }
        public async Task<FORM_Definition?> SetDefinition(FORM_Definition Data, bool setFormCode = false)
        {
            if (setFormCode)
            {
                if (Data.AUTH_Authority_ID != null)
                {
                    var authority = await _unitOfWork.Repository<AUTH_Authority>().GetByIDAsync(Data.AUTH_Authority_ID);

                    if (authority != null)
                    {
                        if (authority.IdentificationLetters != null && authority.NextFormIndex != null && (Data.FormCode == null || !Data.FormCode.StartsWith(authority.IdentificationLetters)))
                        {
                            Data.FormCode = authority.IdentificationLetters + authority.NextFormIndex.Value.ToString("D2");
                        }

                        authority.NextFormIndex++;

                        await _unitOfWork.Repository<AUTH_Authority>().UpdateAsync(authority);
                    }
                }
            }
            Data.LastModificationDate = DateTime.Now;
            return await _unitOfWork.Repository<FORM_Definition>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Additional_FORM?> SetDefinitionAdditionalFORM(FORM_Definition_Additional_FORM Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Additional_FORM>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Deadlines?> SetDefinitionDeadlines(FORM_Definition_Deadlines Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Deadlines>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Deadlines_Target?> SetDefinitionDeadlinesTarget(FORM_Definition_Deadlines_Target Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Deadlines_Target>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Event?> SetDefinitionEvents(FORM_Definition_Event Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Event>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Event_Extended?> SetDefinitionEventsExtended(FORM_Definition_Event_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Event_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Extended?> SetDefinitionExtended(FORM_Definition_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Field?> SetDefinitionField(FORM_Definition_Field Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool?> UpdateDefinitionFieldList(List<FORM_Definition_Field> Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field>().BulkUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Field_Extended?> SetDefinitionFieldExtended(FORM_Definition_Field_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Field_Option?> SetDefinitionFieldOption(FORM_Definition_Field_Option Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Option>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Field_Option_Extended?> SetDefinitionFieldOptionExtended(FORM_Definition_Field_Option_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Option_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Field_Reference?> SetDefinitionFieldReference(FORM_Definition_Field_Reference Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Field_Reference>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Property?> SetDefinitionProperty(FORM_Definition_Property Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Property>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Property_Extended?> SetDefinitionPropertyExtended(FORM_Definition_Property_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Property_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Reminder?> SetDefinitionReminder(FORM_Definition_Reminder Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Reminder>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Ressources?> SetDefinitionRessource(FORM_Definition_Ressources Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Ressources>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Ressources_Extended?> SetDefinitionRessourceExtended(FORM_Definition_Ressources_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Ressources_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Tasks?> SetDefinitionTask(FORM_Definition_Tasks Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Tasks>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Tasks_Extended?> SetDefinitionTaskExtended(FORM_Definition_Tasks_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Tasks_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Upload?> SetDefinitionUpload(FORM_Definition_Upload Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Upload>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Definition_Upload_Extended?> SetDefinitionUploadExtended(FORM_Definition_Upload_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Upload_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<FORM_Definition_Signings>> GetDefinitionSigningList(Guid FORM_Definition_ID)
        {
            var result = await _unitOfWork.Repository<FORM_Definition_Signings>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).Include(p => p.FORM_Definition_Signings_Extended).ToListAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var languages = await _langProvider.GetAll();

            foreach (var r in result)
            {
                if (r != null && Language != null)
                {
                    if (r.FORM_Definition_Signings_Extended != null && r.FORM_Definition_Signings_Extended.Count < languages.Count)
                    {
                        foreach (var l in languages)
                        {
                            if (r.FORM_Definition_Signings_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                            {
                                var dataE = new FORM_Definition_Signings_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    FORM_Definition_Signings_ID = r.ID,
                                    LANG_Languages_ID = l.ID
                                };

                                r.FORM_Definition_Signings_Extended.Add(dataE);
                            }
                        }
                    }

                    var extended = r.FORM_Definition_Signings_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                    if (extended != null)
                    {
                        r.Description = extended.Description;
                    }
                }
            }

            return result;
        }
        public async Task<FORM_Definition_Signings?> GetDefinitionSigning(Guid ID)
        {
            var result = await _unitOfWork.Repository<FORM_Definition_Signings>().Where(p => p.ID == ID).Include(p => p.FORM_Definition_Signings_Extended).FirstOrDefaultAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (result != null && Language != null)
            {
                var extended = result.FORM_Definition_Signings_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                if (extended != null)
                {
                    result.Description = extended.Description;
                }
            }            

            return result;
        }
        public async Task<FORM_Definition_Signings?> SetDefinitionSigning(FORM_Definition_Signings Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Signings>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveDefinitionSigning(Guid ID, bool force = false)
        {
            return await _unitOfWork.Repository<FORM_Definition_Signings>().DeleteAsync(ID);
        }
        public async Task<List<FORM_Definition_Signings_Extended>> GetDefinitionSigningExtendedList(Guid FORM_Definition_Signing_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Signings_Extended>().Where(p => p.FORM_Definition_Signings_ID == FORM_Definition_Signing_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<FORM_Definition_Signings_Extended?> GetDefinitionSigningExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Signings_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<FORM_Definition_Signings_Extended?> SetDefinitionSigningExtended(FORM_Definition_Signings_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Signings_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveDefinitionSigningExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Signings_Extended>().DeleteAsync(ID);
        }
        public async Task<List<FORM_Definition_Tasks_Responsible>> GetDefinitionTaskResponsibleList(Guid FORM_Definition_Task_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Tasks_Responsible>().Where(p => p.FORM_Definition_Tasks_ID == FORM_Definition_Task_ID).Include(p => p.AUTH_Municipal_Users).ToListAsync();

            foreach (var d in data)
            {
                if (d != null && d.AUTH_Municipal_Users != null)
                {
                    d.Fullname = d.AUTH_Municipal_Users.Firstname + " " + d.AUTH_Municipal_Users.Lastname;
                    d.Email = d.AUTH_Municipal_Users.Email;
                }
            }

            return data;
        }
        public async Task<FORM_Definition_Tasks_Responsible?> GetDefinitionTaskResponsible(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Definition_Tasks_Responsible>().Where(p => p.ID == ID).Include(p => p.AUTH_Municipal_Users).FirstOrDefaultAsync();

            if (data != null && data.AUTH_Municipal_Users != null)
            {
                data.Fullname = data.AUTH_Municipal_Users.Firstname + " " + data.AUTH_Municipal_Users.Lastname;
                data.Email = data.AUTH_Municipal_Users.Email;
            }            

            return data;
        }
        public async Task<FORM_Definition_Tasks_Responsible?> SetDefinitionTaskResponsible(FORM_Definition_Tasks_Responsible Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Tasks_Responsible>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveDefinitionTaskResponsible(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Tasks_Responsible>().DeleteAsync(ID);
        }
        public List<FORM_Definition_Field_SubType> GetDefinitionFieldSubTypeList()
        {
            return _unitOfWork.Repository<FORM_Definition_Field_SubType>().ToList();
        }
        public FORM_Definition_Field? CreateDefinitionField(FORM_Definition_Field Data)
        {
            return _unitOfWork.Repository<FORM_Definition_Field>().Insert(Data);
        }
        public async Task<List<FORM_Definition_Payment_Position>> GetDefinitionPaymentList(Guid FORM_Definition_ID)
        {
            var result = await _unitOfWork.Repository<FORM_Definition_Payment_Position>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).Include(p => p.FORM_Definition_Payment_Position_Extended).ToListAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var languages = await _langProvider.GetAll();

            foreach (var r in result)
            {
                if (r != null && Language != null)
                {
                    if (r.FORM_Definition_Payment_Position_Extended != null && r.FORM_Definition_Payment_Position_Extended.Count < languages.Count)
                    {
                        foreach (var l in languages)
                        {
                            if (r.FORM_Definition_Payment_Position_Extended.FirstOrDefault(p => p.LANG_Languages_ID == l.ID) == null)
                            {
                                var dataE = new FORM_Definition_Payment_Position_Extended()
                                {
                                    ID = Guid.NewGuid(),
                                    FORM_Definition_Payment_Position_ID = r.ID,
                                    LANG_Languages_ID = l.ID
                                };

                                r.FORM_Definition_Payment_Position_Extended.Add(dataE);
                            }
                        }
                    }

                    var extended = r.FORM_Definition_Payment_Position_Extended?.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                    if (extended != null && extended.Description != null)
                    {
                        r.Description = extended.Description;
                    }
                }
            }

            return result;
        }
        public async Task<FORM_Definition_Payment_Position?> GetDefinitionPayment(Guid ID)
        {
            var r = await _unitOfWork.Repository<FORM_Definition_Payment_Position>().Where(p => p.ID == ID).Include(p => p.FORM_Definition_Payment_Position_Extended).FirstOrDefaultAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (r != null && Language != null)
            {
                var extended = r.FORM_Definition_Payment_Position_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                if (extended != null && extended.Description != null)
                {
                    r.Description = extended.Description;
                }
            }

            return r;
        }
        public async Task<FORM_Definition_Payment_Position?> SetDefinitionPayment(FORM_Definition_Payment_Position Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Payment_Position>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveDefinitionPayment(Guid ID, bool force = false)
        {
            return await _unitOfWork.Repository<FORM_Definition_Payment_Position>().DeleteAsync(ID);
        }
        public async Task<List<FORM_Definition_Payment_Position_Extended>> GetDefinitionPaymentExtendedList(Guid FORM_Definition_Payment_Position_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Payment_Position_Extended>().Where(p => p.FORM_Definition_Payment_Position_ID == FORM_Definition_Payment_Position_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<FORM_Definition_Payment_Position_Extended?> GetDefinitionPaymentExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Payment_Position_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<FORM_Definition_Payment_Position_Extended?> SetDefinitionPaymentExtended(FORM_Definition_Payment_Position_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Payment_Position_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveDefinitionPaymentExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Payment_Position_Extended>().DeleteAsync(ID);
        }
        public List<FORM_Definition_Bollo_Type> GetDefinitionBolloList()
        {
            var list = _unitOfWork.Repository<FORM_Definition_Bollo_Type>().ToList();

            foreach (var l in list)
            {
                if (l.TEXT_SytemTexts_Code != null)
                {
                    l.Description = _textProvider.Get(l.TEXT_SytemTexts_Code);
                }
            }

            return list;
        }
        public async Task<List<V_FORM_Definition_Statistik>> GetDefintionStatistik(Guid AUTH_Municipality_ID, Guid AUTH_Authority_ID)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            return await _unitOfWork.Repository<V_FORM_Definition_Statistik>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Authority_ID == AUTH_Authority_ID && p.LANG_Language_ID == Language.ID).ToListAsync();
        }
        public async Task<List<FORM_Definition_Municipal_Field>> GetDefinitionMunFieldList(Guid FORM_Definition_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Municipal_Field>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).Include(p => p.FORM_Definition_Municipal_Field_Extended).ToListAsync();
        }
        public async Task<FORM_Definition_Municipal_Field?> GetDefinitionMunField(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Municipal_Field>().Where(p => p.ID == ID).Include(p => p.FORM_Definition_Municipal_Field_Extended).FirstOrDefaultAsync();
        }
        public async Task<FORM_Definition_Municipal_Field?> SetDefinitionMunField(FORM_Definition_Municipal_Field Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Municipal_Field>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveDefinitionMunField(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Municipal_Field>().DeleteAsync(ID);
        }
        public async Task<List<FORM_Definition_Municipal_Field_Extended>> GetDefinitionMunFieldExtendedList(Guid FORM_Definition_MUN_Field_ID, Guid? LANG_Language_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Municipal_Field_Extended>().Where(p => p.FORM_Definition_Municipal_Field_ID == FORM_Definition_MUN_Field_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<FORM_Definition_Municipal_Field_Extended?> GetDefinitionMunFieldExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Municipal_Field_Extended>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<FORM_Definition_Municipal_Field_Extended?> SetDefinitionMunFieldExtended(FORM_Definition_Municipal_Field_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Municipal_Field_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveDefinitionMunFieldExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Municipal_Field_Extended>().DeleteAsync(ID);
        }
        public async Task<List<V_FORM_Definition_Municipal_Field_Type>> GetDefinitionMunFieldTypeList(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Definition_Municipal_Field_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<V_FORM_Definition_Municipal_Field>> GetDefinitionMunFieldList(Guid FORM_Definition_ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Definition_Municipal_Field>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID && p.LANG_Languages_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<V_FORM_Definition_Municipal_Field_Type?> GetDefinitionMunFieldType(Guid LANG_Language_ID, Guid ID)
        {
            return await _unitOfWork.Repository<V_FORM_Definition_Municipal_Field_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID && p.ID == ID).FirstOrDefaultAsync();
        }
        public async Task<DateTime?> GetDefinitionEskalationDate(Guid DeadlineTypeID, DateTime? SubmitAt, DateTime? LegalDeadline, DateTime? EstimatedDeadline, long AdditionalDays)
        {
            if (DeadlineTypeID == Guid.Parse("0171151b-61de-4834-b311-07d7ee4e5124"))         //x Tage ab Einreichdatum
            {
                if (SubmitAt == null)
                    return null;
                
                return SubmitAt.Value.AddDays(AdditionalDays);
            }
            else if (DeadlineTypeID == Guid.Parse("0318c7c1-5b07-4208-a07d-0fd2f0be0106"))   //x Tage vor gesetzlicher Frist
            {
                if (LegalDeadline == null)
                    return null;

                return LegalDeadline.Value.AddDays(AdditionalDays * -1);                
            }
            else if (DeadlineTypeID == Guid.Parse("1cace041-7df3-4565-ac1e-762b436dfabc"))   //x Tage nach Ablauf durchschittliche Wartezeit
            {
                if (EstimatedDeadline == null)
                    return null;

                return EstimatedDeadline.Value.AddDays(AdditionalDays);
            }
            else if (DeadlineTypeID == Guid.Parse("af119191-de7f-4e91-8e2a-6595d81f31f8"))   //x Tage vor Ablauf durchschittliche Wartezeit
            {
                if (EstimatedDeadline == null)
                    return null;

                return EstimatedDeadline.Value.AddDays(AdditionalDays * -1);
            }

            return null;
        }
        public async Task<List<V_FORM_Definition_Template>> GetDefinitionTemplates(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Definition_Template>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public async Task<FORM_Definition_Template?> GetDefinitionTemplate(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Template>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<FORM_Definition_Template_Signfields>> GetDefinitionTemplateSignfields(Guid FORM_Definition_Template_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Template_Signfields>().ToListAsync(p => p.FORM_Definition_Template_ID == FORM_Definition_Template_ID);
        }
        public async Task<List<FORM_Definition_Theme>> GetThemes(Guid FORM_Definition_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Theme>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).ToListAsync();
        }
        public async Task<List<V_HOME_Theme>> GetVThemes(Guid AUTH_Municipality_ID, Guid LANG_Language_ID, Guid FORM_Definition_ID)
        {
            var existingElements = await _unitOfWork.Repository<FORM_Definition_Theme>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).ToListAsync();

            var themes = await _unitOfWork.Repository<V_HOME_Theme>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();

            return themes.Where(p => existingElements.Select(x => x.HOME_Theme_ID).Contains(p.ID)).ToList();
        }
        public async Task<bool> RemoveTheme(FORM_Definition_Theme Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Theme>().DeleteAsync(Data);
        }
        public async Task<FORM_Definition_Theme?> SetTheme(FORM_Definition_Theme Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Theme>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<V_FORM_Definition_Type>> GetTypes(Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Definition_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToListAsync();
        }
        public List<V_FORM_Definition_Type> GetTypesSync(Guid LANG_Language_ID)
        {
            return _unitOfWork.Repository<V_FORM_Definition_Type>().Where(p => p.LANG_LanguagesID == LANG_Language_ID).ToList();
        }
        public async Task<V_FORM_Definition_Type?> GetFormType(Guid ID, Guid LANG_Language_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Definition_Type>().Where(p => p.ID == ID && p.LANG_LanguagesID == LANG_Language_ID).FirstOrDefaultAsync();
        }
        public async Task CopyDefinition(Guid FORM_Definition_ID)
        {
            if (FORM_Definition_ID != Guid.Empty)
            {
                FormattableString query = $"exec [dbo].[FORM_CopyDataset] {FORM_Definition_ID};";
                await _unitOfWork.SQLQueryAsync(query);
            }
        }
        public async Task<List<FORM_Definition_Person>> GetPeople(Guid FORM_Definition_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Person>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID).ToListAsync();
        }
        public async Task<bool> RemovePerson(Guid FORM_Definition_Person_ID)
        {
            return await _unitOfWork.Repository<FORM_Definition_Person>().DeleteAsync(FORM_Definition_Person_ID);
        }
        public async Task<FORM_Definition_Person?> SetPerson(FORM_Definition_Person Data)
        {
            return await _unitOfWork.Repository<FORM_Definition_Person>().InsertOrUpdateAsync(Data);
        }
    }
}
