using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Helper;
using Microsoft.AspNetCore.Components;
using ICWebApp.Application.Settings;
using Microsoft.EntityFrameworkCore;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System.Globalization;
using System.Data;

namespace ICWebApp.Application.Provider
{
    public class FORMApplicationProvider : IFORMApplicationProvider
    {
        private IUnitOfWork _unitOfWork;
        private ISessionWrapper _sessionWrapper;
        private ILANGProvider _langProvider;
        private ITEXTProvider _textProvider;
        private IFILEProvider _fileProvider;
        private IAUTHProvider _authProvider;
        private IFORM_ReportPrintHelper _formPrintHelper;
        private NavigationManager NavManager;
        private ID3Helper _d3Helper;

        public FORMApplicationProvider(IUnitOfWork _unitOfWork, ISessionWrapper _sessionWrapper, ILANGProvider _langProvider, ITEXTProvider _textProvider, IFILEProvider _fileProvider,
                                       IAUTHProvider _authProvider, NavigationManager NavManager, IFORM_ReportPrintHelper _formPrintHelper, ID3Helper d3Helper)
        {
            this._unitOfWork = _unitOfWork;
            this._sessionWrapper = _sessionWrapper;
            this._langProvider = _langProvider;
            this._textProvider = _textProvider;
            this._fileProvider = _fileProvider;
            this._authProvider = _authProvider;
            this.NavManager = NavManager;
            this._formPrintHelper = _formPrintHelper;
            this._d3Helper = d3Helper;
        }

        public async Task<FORM_Application?> GetApplication(Guid ID)
        {
            var app = await _unitOfWork.Repository<FORM_Application>().FirstOrDefaultAsync(p => p.ID == ID);

            return app;
        }
        public async Task<FORM_Application?> GetApplicationByFileID(Guid FILE_Fileinfo_ID)
        {
            var app = await _unitOfWork.Repository<FORM_Application>().FirstOrDefaultAsync(p => p.FILE_Fileinfo_ID == FILE_Fileinfo_ID);

            return app;
        }
        public async Task<FORM_Application_Field_Data?> GetApplicationFieldData(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Field_Data>().Where(p => p.ID == ID).Include(p => p.FORM_Application_Field_SubData).FirstOrDefaultAsync();
        }
        public async Task<List<FORM_Application_Field_Data>> GetApplicationFieldDataList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Field_Data>().Where(p => p.FORM_Application_ID == FORM_Application_ID).Include(p => p.FORM_Application_Field_SubData).ToListAsync();
        }
        public async Task<List<V_FORM_Application_Field>> GetVApplicationFieldDataList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Field>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<List<FORM_Application_Field_Data>> GetApplicationFieldDataMunicipalList(Guid FORM_Application_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Application_Field_Data>().Where(p => p.FORM_Application_ID == FORM_Application_ID).Include(p => p.FORM_Application_Field_SubData).ToListAsync();

            var definitionIDs = data.Select(p => p.FORM_Definition_Field_ID).ToList();

            var definitions = await _unitOfWork.Repository<FORM_Definition_Field>().Where(p => p.ShowOnMunicipalSite == true && definitionIDs.Contains(p.ID)).ToListAsync();

            var types = await _unitOfWork.Repository<FORM_Definition_Field_Type>().ToListAsync();

            List<FORM_Application_Field_Data> result = new List<FORM_Application_Field_Data>();

            foreach (var def in definitions.OrderBy(p => p.SortOrder))
            {
                var app = data.FirstOrDefault(p => p.FORM_Definition_Field_ID == def.ID);
                var type = types.FirstOrDefault(p => p.ID == def.FORM_Definition_Fields_Type_ID);

                if (app != null && !result.Select(p => p.ID).Contains(app.ID))
                {
                    if (!string.IsNullOrEmpty(app.Value) || (type != null && type.ReadOnly == true))
                    {
                        result.Add(app);
                    }
                }

                var children = definitions.Where(p => p.FORM_Definition_Field_Parent_ID == def.FORM_Definition_Field_Parent_ID && p.ID != def.ID).ToList();

                if (children != null && children.Count > 0)
                {
                    foreach (var child in children.OrderBy(p => p.ColumnPos))
                    {
                        var Childapp = data.FirstOrDefault(p => p.FORM_Definition_Field_ID == child.ID);

                        if (Childapp != null && !result.Select(p => p.ID).Contains(Childapp.ID))
                        {
                            if (!string.IsNullOrEmpty(Childapp.Value) || (type != null && type.ReadOnly == true))
                            {
                                result.Add(Childapp);
                            }
                        }
                    }
                }
            }

            return result;
        }
        public async Task<FORM_Application_Field_SubData?> GetApplicationFieldSubData(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Field_SubData>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<List<FORM_Application_Field_SubData>> GetApplicationFieldSubDataList(Guid FORM_Application_Field_Data_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Field_SubData>().Where(p => p.FORM_Application_Field_Data_ID == FORM_Application_Field_Data_ID).ToListAsync();
        }
        public async Task<List<V_FORM_Application_ResponsibleMunicipalUser>> GetApplicationListOfActualMunicipalUser()
        {
            if (_sessionWrapper.CurrentMunicipalUser == null)
            {
                return new List<V_FORM_Application_ResponsibleMunicipalUser>();
            }

            var data = await _unitOfWork.Repository<V_FORM_Application_ResponsibleMunicipalUser>().Where(p => p.Responsible_Municipal_User_ID == _sessionWrapper.CurrentMunicipalUser.ID && p.AUTH_Municipality_ID == _sessionWrapper.CurrentMunicipalUser.AUTH_Municipality_ID).ToListAsync();

            return data;
        }
        public async Task<List<FORM_Application>> GetApplicationListByDefinition(Guid FORM_Definition_ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                if (_sessionWrapper.AUTH_Municipality_ID == null)
                {
                    _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
                }
            }

            var data = await _unitOfWork.Repository<FORM_Application>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value).ToListAsync();

            return data;
        }
        public async Task<List<FORM_Application>> GetApplicationListByUser(Guid AUTH_Users_ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                if (_sessionWrapper.AUTH_Municipality_ID == null)
                {
                    _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
                }
            }

            var data = await _unitOfWork.Repository<FORM_Application>().Where(p => p.AUTH_Users_ID == AUTH_Users_ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value).ToListAsync();

            return data;
        }
        public async Task<List<FORM_Application>> GetApplicationListByUser(Guid AUTH_Users_ID, Guid FORM_Definition_ID)
        {
            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                if (_sessionWrapper.AUTH_Municipality_ID == null)
                {
                    _sessionWrapper.AUTH_Municipality_ID = await _sessionWrapper.GetMunicipalityID();
                }
            }

            var data = await _unitOfWork.Repository<FORM_Application>().Where(p => p.AUTH_Users_ID == AUTH_Users_ID && p.FORM_Definition_ID == FORM_Definition_ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value).ToListAsync();

            return data;
        }
        public async Task<FORM_Application_Responsible?> GetApplicationResponsible(Guid ID)
        {
            var data = await _unitOfWork.Repository<FORM_Application_Responsible>().Where(p => p.ID == ID).Include(p => p.AUTH_Users).FirstOrDefaultAsync();

            if (data != null && data.AUTH_Users != null)
            {
                data.Fullname = data.AUTH_Users.Firstname + " " + data.AUTH_Users.Lastname;
                data.Email = data.AUTH_Users.Email;
            }

            return data;
        }
        public async Task<List<FORM_Application_Responsible>> GetApplicationResponsibleListByApplication(Guid FORM_Application_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Application_Responsible>().Where(p => p.FORM_Application_ID == FORM_Application_ID).Include(p => p.AUTH_Users).ToListAsync();

            foreach (var d in data)
            {
                if (d != null && d.AUTH_Users != null)
                {
                    d.Fullname = d.AUTH_Users.Firstname + " " + d.AUTH_Users.Lastname;
                    d.Email = d.AUTH_Users.Email;
                }
            }

            return data;
        }
        public async Task<List<FORM_Application_Responsible>> GetApplicationResponsibleListByUser(Guid AUTH_Users_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Application_Responsible>().Where(p => p.AUTH_Users_ID == AUTH_Users_ID).Include(p => p.AUTH_Users).ToListAsync();

            foreach (var d in data)
            {
                if (d != null && d.AUTH_Users != null)
                {
                    d.Fullname = d.AUTH_Users.Firstname + " " + d.AUTH_Users.Lastname;
                    d.Email = d.AUTH_Users.Email;
                }
            }

            return data;
        }
        public async Task<List<V_FORM_Application>> GetApplications(Guid AUTH_Municipality_ID, Guid Current_AUTH_Users_ID, List<Guid> AllowedAuthorities, Administration_Filter_Item? Filter)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var result = new List<V_FORM_Application>();

            bool filtered = false;

            if (Filter != null && Filter.AUTH_Authority_ID != null && Filter.AUTH_Authority_ID.Count() > 0)
            {
                AllowedAuthorities = AllowedAuthorities.Where(p => Filter.AUTH_Authority_ID.Contains(p)).ToList();
                filtered = true;
            }

            var applications = _unitOfWork.Repository<V_FORM_Application>().Where(p => p.LANG_Language_ID == Language.ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID &&
                                                                                       (p.ResponsibleIDList.Contains(Current_AUTH_Users_ID.ToString()) ||
                                                                                       (p.AUTH_Authority_ID != null && AllowedAuthorities.Contains(p.AUTH_Authority_ID.Value))));

            var hasCommitteeRights = _authProvider.HasUserRole(AuthRoles.Committee);

            var committeeRightsApplications = applications.Where(e => e.IsMunicipalCommittee == true);

            if (!hasCommitteeRights)
            {
                applications = applications.Where(p => p.IsMunicipalCommittee != true);

                var additionalApplications = new List<V_FORM_Application>();

                foreach (var application in committeeRightsApplications)
                {
                    if(await IsResponsibleForTask(Current_AUTH_Users_ID, application.ID))
                        additionalApplications.Add(application);
                }

                result = result.Union(additionalApplications).ToList();
            }

            if (Filter != null && Filter.EskalatedTasks != null)
            {
                applications = applications.Where(p => p.IsEskalated == Filter.EskalatedTasks);
                filtered = true;
            }
            if (Filter != null && Filter.ManualInput != null)
            {
                applications = applications.Where(p => p.IsManualInput == Filter.ManualInput);
                filtered = true;
            }
            if (Filter != null && Filter.DeadlineFrom != null)
            {
                applications = applications.Where(p => p.LegalDeadline >= Filter.DeadlineFrom);
                filtered = true;
            }
            if (Filter != null && Filter.DeadlineTo != null)
            {
                applications = applications.Where(p => p.LegalDeadline <= Filter.DeadlineTo);
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
            if (Filter != null && Filter.Text != null && Filter.Text != "")
            {
                applications = applications.Where(p => p.SearchBox.ToLower().Contains(Filter.Text.ToLower()));
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
            if (Filter != null && Filter.MyTasks == true)
            {
                applications = applications.Where(p => p.ResponsibleIDList.Contains(Current_AUTH_Users_ID.ToString()));
                filtered = true;
            }
            if (Filter != null && Filter.OnlyToPay == true)
            {
                applications = applications.Where(p => p.OpenPayments > 0);
                filtered = true;
            }

            var items = await applications.ToListAsync();

            result = result.UnionBy(items, p => p.ID).ToList();

            if (Filter != null && Filter.FORM_Application_Status_ID != null && Filter.FORM_Application_Status_ID.Count() > 0)
            {
                result = result.Where(p => p.FORM_Application_Status_ID != null && Filter.FORM_Application_Status_ID.Contains(p.FORM_Application_Status_ID.Value)).ToList();
                filtered = true;
            }

            if (Filter != null && Filter.FORM_Application_Priority_ID != null && Filter.FORM_Application_Priority_ID.Count() > 0)
            {
                result = result.Where(p => p.FORM_Application_Priority_ID != null && (Filter.FORM_Application_Priority_ID.Contains(p.FORM_Application_Priority_ID.Value))).ToList();
                filtered = true;
            }

            if(Filter != null && Filter.OnlyPublic)
            {
                result = result.Where(p => p.IsMunicipal != true && p.IsMunicipalCommittee != true).ToList();
                filtered = true;
            }

            if (Filter != null && Filter.OnlyMunicipal == true)
            {
                result = result.Where(p => p.IsMunicipal == true).ToList();
                filtered = true;
            }

            if (Filter != null && Filter.OnlyMunicipalCommittee == true)
            {
                result = result.Where(p => p.IsMunicipalCommittee == true).ToList();
                filtered = true;
            }

            if (filtered)
                return result;

            result = result.OrderByDescending(e => e.SubmitAt).Take(100).ToList();

            return result;
        }
        public async Task<List<V_FORM_Application>> GetApplications(Guid AUTH_Authority_ID, Guid AUTH_Municipality_ID, int Amount = 6)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            var data = await _unitOfWork.Repository<V_FORM_Application>().Where(p => p.AUTH_Authority_ID == AUTH_Authority_ID
                                                                                      && p.AUTH_Municipality_ID == AUTH_Municipality_ID
                                                                                      && p.LANG_Language_ID == Language.ID
                                                                                      && p.FORM_Application_Status_ID == FORMStatus.Comitted)
                                                              .OrderByDescending(p => p.SubmitAt).Take(Amount).ToListAsync();

            return data;
        }
        public async Task<bool> CheckApplicationOpenPaymentsPersonalArea(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID, Guid LANG_Language_ID, Guid FORM_Definition_Category_ID)
        {
            V_FORM_Application_Personal_Area? result =  await _unitOfWork.Repository<V_FORM_Application_Personal_Area>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID &&
                                                                                                                                                  p.AUTH_Users_ID == AUTH_Users_ID &&
                                                                                                                                                  p.FORM_Definition_Category_ID == FORM_Definition_Category_ID &&
                                                                                                                                                  p.LANG_Language_ID == LANG_Language_ID &&
                                                                                                                                                  (
                                                                                                                                                      p.FORM_Application_Status_ID == FORMStatus.ToPay ||
                                                                                                                                                      p.OpenPayments > 0
                                                                                                                                                  ));
            return result != null;
        }
        public async Task<List<V_FORM_Application_Personal_Area>> GetApplicationsPersonalArea(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID, Guid LANG_Language_ID, Guid FORM_Definition_Category_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Personal_Area>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Users_ID == AUTH_Users_ID
                                                                                            && p.FORM_Definition_Category_ID == FORM_Definition_Category_ID
                                                                                            && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<V_FORM_Application>> GetApplications(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            Guid category = FORMCategories.Applications;

            var data = await _unitOfWork.Repository<V_FORM_Application>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Users_ID == AUTH_Users_ID
                                                                                    && p.FORM_Definition_Category_ID == category
                                                                                    && p.LANG_Language_ID == Language.ID).ToListAsync();

            var Authorities = await _authProvider.GetAuthorityList(AUTH_Municipality_ID, true);
            var statusList = GetStatusList();
            var formList = await _unitOfWork.Repository<FORM_Definition>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedAt == null)
                                                                          .Include(p => p.FORM_Definition_Extended)
                                                                          .Include(p => p.AUTH_Authority)
                                                                          .ToListAsync();

            foreach (var d in data)
            {
                var auth = Authorities.FirstOrDefault(p => p.ID == d.AUTH_Authority_ID);

                if (auth != null)
                {
                    d.Authority = auth.Description;
                    d.AuthorityIcon = auth.Icon;
                }

                var status = statusList.FirstOrDefault(p => p.ID == d.FORM_Application_Status_ID);

                if (status != null)
                {
                    d.Status = status.Name;
                    d.StatusIcon = status.Icon;
                }

                if (d.PriorityTextCode != null)
                {
                    d.PriorityStatus = _textProvider.Get(d.PriorityTextCode);
                }

                var formName = formList.FirstOrDefault(p => p.ID == d.FORM_Definition_ID);

                if (formName != null && formName.FORM_Definition_Extended != null && formName.FORM_Definition_Extended.Count() > 0)
                {
                    d.FormName = formName.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Language.ID).Name;
                }
            }


            return data;
        }
        public async Task<bool> CheckMantainancesOpenPaymentsPersonalArea(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID, Guid LANG_Language_ID, Guid FORM_Definition_Category_ID)
        {
            V_FORM_Application_Personal_Area? result = await _unitOfWork.Repository<V_FORM_Application_Personal_Area>().FirstOrDefaultAsync(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID &&
                                                                                                                                                  p.AUTH_Users_ID == AUTH_Users_ID &&
                                                                                                                                                  p.FORM_Definition_Category_ID == FORM_Definition_Category_ID &&
                                                                                                                                                  p.LANG_Language_ID == LANG_Language_ID &&
                                                                                                                                                  (
                                                                                                                                                      p.FORM_Application_Status_ID == FORMStatus.ToPay ||
                                                                                                                                                      p.OpenPayments > 0
                                                                                                                                                  ));
            return result != null;
        }
        public async Task<List<V_FORM_Application_Personal_Area>> GetMantainancesPersonalArea(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID, Guid LANG_Language_ID, Guid FORM_Definition_Category_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Personal_Area>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Users_ID == AUTH_Users_ID
                                                                                            && p.FORM_Definition_Category_ID == FORM_Definition_Category_ID
                                                                                            && p.LANG_Language_ID == LANG_Language_ID).ToListAsync();
        }
        public async Task<List<V_FORM_Application>> GetMantainances(Guid AUTH_Municipality_ID, Guid AUTH_Users_ID)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            Guid category = FORMCategories.Maintenance;

            var data = await _unitOfWork.Repository<V_FORM_Application>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID
                                                                                  && p.AUTH_Users_ID == AUTH_Users_ID && p.FORM_Definition_Category_ID == category
                                                                                  && p.LANG_Language_ID == Language.ID).ToListAsync();

            var Authorities = await _authProvider.GetAuthorityList(AUTH_Municipality_ID, false);
            var statusList = GetStatusList();
            var formList = await _unitOfWork.Repository<FORM_Definition>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.RemovedAt == null)
                                                                          .Include(p => p.FORM_Definition_Extended)
                                                                          .Include(p => p.AUTH_Authority)
                                                                          .ToListAsync();

            foreach (var d in data)
            {
                var auth = Authorities.FirstOrDefault(p => p.ID == d.AUTH_Authority_ID);

                if (auth != null)
                {
                    d.Authority = auth.Description;
                    d.AuthorityIcon = auth.Icon;
                }

                var status = statusList.FirstOrDefault(p => p.ID == d.FORM_Application_Status_ID);

                if (status != null)
                {
                    d.Status = status.Name;
                    d.StatusIcon = status.Icon;
                }

                if (d.PriorityTextCode != null)
                {
                    d.PriorityStatus = _textProvider.Get(d.PriorityTextCode);
                }

                var formName = formList.FirstOrDefault(p => p.ID == d.FORM_Definition_ID);

                if (formName != null && formName.FORM_Definition_Extended != null && formName.FORM_Definition_Extended.Count() > 0)
                {
                    d.FormName = formName.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Language.ID).Name;
                }
            }

            return data;
        }
        public async Task<FORM_Application_Upload?> GetApplicationUpload(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Upload>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<FORM_Application_Upload?> GetApplicationUploadByDefinition(Guid FORM_Definition_Upload_ID, Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Upload>().FirstOrDefaultAsync(p => p.FORM_Definition_Upload_ID == FORM_Definition_Upload_ID && p.FORM_Application_ID == FORM_Application_ID);
        }
        public async Task<FORM_Application_Upload_File?> GetApplicationUploadFile(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Upload_File>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<FORM_Application_Upload_File?> GetApplicationUploadFileByFileID(Guid FILE_FileInfo_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Upload_File>().FirstOrDefaultAsync(p => p.FILE_FileInfo_ID == FILE_FileInfo_ID);
        }
        public async Task<List<FORM_Application_Upload_File>> GetApplicationUploadFileList(Guid FORM_Application_Upload_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Upload_File>().Where(p => p.FORM_Application_Upload_ID == FORM_Application_Upload_ID).ToListAsync();
        }
        public async Task<List<FORM_Application_Upload>> GetApplicationUploadList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Upload>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<List<V_FORM_Application_Users>> GetApplicationUsers(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Users>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();
        }
        public async Task<List<FORM_Application_Field_Data>> GetOrCreateApplicationFieldData(FORM_Application FORM_Application)
        {
            List<FORM_Application_Field_Data>? result = new List<FORM_Application_Field_Data>();

            if (FORM_Application != null && FORM_Application.FORM_Definition_ID != null)
            {
                var existingItems = await _unitOfWork.Repository<FORM_Application_Field_Data>().Where(p => p.FORM_Application_ID == FORM_Application.ID)
                                                                                               .Include(p => p.FORM_Application_Field_SubData).ToListAsync();

                if (existingItems != null && existingItems.Count() > 0)
                {
                    return existingItems;
                }

                var definitionItems = await _unitOfWork.Repository<FORM_Definition_Field>().Where(p => p.FORM_Definition_ID == FORM_Application.FORM_Definition_ID.Value)
                                                                                           .Include(p => p.FORM_Definition_Field_Extended)
                                                                                           .Include(p => p.FORM_Definition_Field_Option).ThenInclude(p => p.FORM_Definition_Field_Option_Extended)
                                                                                           .ToListAsync();

                var resultlist = new List<FORM_Application_Field_Data>();

                foreach (var d in definitionItems.Where(p => p.FORM_Definition_Field_Parent_ID == null).OrderBy(p => p.SortOrder))
                {
                    if (d.FORM_Definition_Fields_Type_ID == FORMElements.ColumnContainer)  //CONTAINER
                    {
                        var containerResult = CreateContainerFields(FORM_Application, d, FORM_Application.ID, existingItems, definitionItems);

                        resultlist.AddRange(containerResult);
                    }
                    else
                    {
                        var existingItem = existingItems.FirstOrDefault(p => p.FORM_Definition_Field_ID == d.ID);

                        if (existingItem == null)
                        {
                            var newItem = new FORM_Application_Field_Data();

                            newItem.ID = Guid.NewGuid();
                            newItem.FORM_Application_ID = FORM_Application.ID;
                            newItem.FORM_Definition_Field_ID = d.ID;
                            newItem.RepetitionParentID = FORM_Application.ID;
                            newItem.RepetitionCount = 1;
                            newItem.SortOrder = d.SortOrder;

                            resultlist.Add(newItem);
                        }
                        else
                        {
                            resultlist.Add(existingItem);
                        }
                    }
                }

                result = resultlist;
            }

            return result;
        }
        private List<FORM_Application_Field_Data> CreateContainerFields(FORM_Application FORM_Application, FORM_Definition_Field DefinitionField, Guid ParentID, List<FORM_Application_Field_Data> ExistingItems, List<FORM_Definition_Field> DefinitionItems)
        {
            var resultlist = new List<FORM_Application_Field_Data>();

            var existingItemList = ExistingItems.Where(p => p.FORM_Definition_Field_ID == DefinitionField.ID).OrderBy(p => p.RepetitionCount).ToList();

            if (existingItemList == null || existingItemList.Count == 0)
            {
                var newItem = new FORM_Application_Field_Data();

                newItem.ID = Guid.NewGuid();
                newItem.FORM_Application_ID = FORM_Application.ID;
                newItem.FORM_Definition_Field_ID = DefinitionField.ID;
                newItem.RepetitionParentID = ParentID;
                newItem.RepetitionCount = 1;
                newItem.SortOrder = DefinitionField.SortOrder;

                resultlist.Add(newItem);

                foreach (var subd in DefinitionItems.Where(p => p.FORM_Definition_Field_Parent_ID == DefinitionField.ID).OrderBy(p => p.SortOrder).ToList())
                {
                    if (subd.FORM_Definition_Fields_Type_ID == FORMElements.ColumnContainer)  //CONTAINER
                    {
                        var result = CreateContainerFields(FORM_Application, subd, newItem.ID, ExistingItems, DefinitionItems);

                        resultlist.AddRange(result);
                    }
                    else
                    {
                        var existingSubItem = ExistingItems.FirstOrDefault(p => p.FORM_Definition_Field_ID == subd.ID && p.RepetitionParentID == newItem.ID);

                        if (existingSubItem == null)
                        {
                            var newSubItem = new FORM_Application_Field_Data();

                            newSubItem.ID = Guid.NewGuid();
                            newSubItem.FORM_Application_ID = FORM_Application.ID;
                            newSubItem.FORM_Definition_Field_ID = subd.ID;
                            newSubItem.RepetitionParentID = newItem.ID;
                            newSubItem.RepetitionCount = 1;
                            newSubItem.SortOrder = subd.SortOrder;

                            resultlist.Add(newSubItem);
                        }
                        else
                        {
                            resultlist.Add(existingSubItem);
                        }
                    }
                }
            }

            return resultlist;
        }
        public async Task<List<FORM_Application_Field_Data>> GetAdditionalContainerFieldData(Guid FORM_Application_Field_ID, FORM_Definition_Field FORM_Defintion_Field, FORM_Application FORM_Application, long? NextRepetitionCount)
        {
            var resultlist = new List<FORM_Application_Field_Data>();

            if (FORM_Application != null && FORM_Application.FORM_Definition_ID != null)
            {
                var definitionItems = await _unitOfWork.Repository<FORM_Definition_Field>().Where(p => p.FORM_Definition_ID == FORM_Application.FORM_Definition_ID.Value)
                                                                                           .Include(p => p.FORM_Definition_Field_Extended)
                                                                                           .Include(p => p.FORM_Definition_Field_Option).ThenInclude(p => p.FORM_Definition_Field_Option_Extended)
                                                                                           .ToListAsync();

                if (FORM_Defintion_Field.FORM_Definition_Fields_Type_ID == FORMElements.ColumnContainer)  //CONTAINER
                {
                    var newItem = new FORM_Application_Field_Data();

                    newItem.ID = Guid.NewGuid();
                    newItem.FORM_Application_ID = FORM_Application.ID;
                    newItem.FORM_Definition_Field_ID = FORM_Defintion_Field.ID;
                    newItem.RepetitionCount = NextRepetitionCount;
                    newItem.RepetitionParentID = FORM_Application_Field_ID;
                    newItem.SortOrder = FORM_Defintion_Field.SortOrder;

                    resultlist.Add(newItem);

                    foreach (var subd in definitionItems.Where(p => p.FORM_Definition_Field_Parent_ID == FORM_Defintion_Field.ID).OrderBy(p => p.SortOrder).ToList())
                    {
                        var newSubItem = new FORM_Application_Field_Data();

                        newSubItem.ID = Guid.NewGuid();
                        newSubItem.FORM_Application_ID = FORM_Application.ID;
                        newSubItem.FORM_Definition_Field_ID = subd.ID;
                        newSubItem.RepetitionParentID = newItem.ID;
                        newSubItem.RepetitionCount = 1;
                        newSubItem.SortOrder = subd.SortOrder;

                        resultlist.Add(newSubItem);

                        var result = AddSubContainers(FORM_Application, newSubItem.ID, subd.ID, definitionItems);

                        resultlist.AddRange(result);
                    }
                }
            }

            return resultlist;
        }
        private List<FORM_Application_Field_Data> AddSubContainers(FORM_Application FORM_Application, Guid FORM_Application_Parent_ID, Guid FORM_Definition_Parent_ID, List<FORM_Definition_Field> Fields)
        {
            var resultlist = new List<FORM_Application_Field_Data>();

            foreach (var subd in Fields.Where(p => p.FORM_Definition_Field_Parent_ID == FORM_Definition_Parent_ID).OrderBy(p => p.SortOrder).ToList())
            {
                var newSubItem = new FORM_Application_Field_Data();

                newSubItem.ID = Guid.NewGuid();
                newSubItem.FORM_Application_ID = FORM_Application.ID;
                newSubItem.FORM_Definition_Field_ID = subd.ID;
                newSubItem.RepetitionParentID = FORM_Application_Parent_ID;
                newSubItem.RepetitionCount = 1;
                newSubItem.SortOrder = subd.SortOrder;

                resultlist.Add(newSubItem);

                var result = AddSubContainers(FORM_Application, newSubItem.ID, subd.ID, Fields);
                resultlist.AddRange(result);
            }

            return resultlist;
        }
        public async Task<FILE_FileInfo?> GetOrCreateFileInfo(Guid FORM_Application_ID, string ReportName)
        {
            var app = await GetApplication(FORM_Application_ID);

            if (app != null && app.FORM_Definition_ID != null)
            {
                var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
                var def = await _unitOfWork.Repository<FORM_Definition>().Where(p => p.ID == app.FORM_Definition_ID.Value).Include(p => p.AUTH_Authority)
                                                                         .Include(p => p.FORM_Definition_Extended)
                                                                         .FirstOrDefaultAsync();

                if (def != null && Language != null)
                {
                    var extended = def.FORM_Definition_Extended.FirstOrDefault(p => p.LANG_Language_ID == Language.ID);

                    if (extended != null)
                    {
                        def.FORM_Name = extended.Name;
                        def.FORM_Description = extended.Description;
                    }

                    if (def.AUTH_Authority != null && def.AUTH_Authority.TEXT_SystemText_Code != null)
                    {
                        def.AmtName = _textProvider.Get(def.AUTH_Authority.TEXT_SystemText_Code);
                    }
                }

                if (app.FILE_Fileinfo_ID == null)
                {
                    if (def != null && _sessionWrapper.AUTH_Municipality_ID != null && Language != null)
                    {
                        bool HasDynamicSection = true;

                        var FieldData = await GetApplicationFieldDataList(app.ID);

                        if (FieldData == null || FieldData.Count() == 0)
                        {
                            HasDynamicSection = false;
                        }

                        var ms = _formPrintHelper.GetPDF(ReportName, Language.ID.ToString(), _sessionWrapper.AUTH_Municipality_ID.Value.ToString(), FORM_Application_ID.ToString(), HasDynamicSection);

                        var fi = new FILE_FileInfo();

                        fi.ID = Guid.NewGuid();
                        fi.FileName = def.FORM_Name;
                        fi.FileExtension = ".pdf";
                        fi.CreationDate = DateTime.Now;
                        fi.AUTH_Users_ID = app.AUTH_Users_ID;
                        fi.Size = ms.Length;

                        var storage = new FILE_FileStorage();

                        storage.ID = Guid.NewGuid();
                        storage.FILE_FileInfo_ID = fi.ID;
                        storage.FileImage = ms.ToArray();
                        storage.CreationDate = DateTime.Now;

                        fi = await _fileProvider.SetFileInfo(fi);
                        storage = await _fileProvider.SetFileStorage(storage);

                        if (fi != null)
                        {
                            app.FILE_Fileinfo_ID = fi.ID;

                            await SetApplication(app);

                            return fi;
                        }
                    }
                }
                else
                {
                    var fstorage = await _fileProvider.GetFileStorageAsync(app.FILE_Fileinfo_ID.Value);

                    if (fstorage != null)
                    {
                        if (def != null && _sessionWrapper.AUTH_Municipality_ID != null && Language != null)
                        {
                            bool HasDynamicSection = true;

                            var FieldData = await GetApplicationFieldDataList(app.ID);

                            if (FieldData == null || FieldData.Count() == 0)
                            {
                                HasDynamicSection = false;
                            }

                            var ms = _formPrintHelper.GetPDF(ReportName, Language.ID.ToString(), _sessionWrapper.AUTH_Municipality_ID.Value.ToString(), FORM_Application_ID.ToString(), HasDynamicSection);

                            var fi = await _fileProvider.GetFileInfoAsync(app.FILE_Fileinfo_ID.Value);

                            if (fi != null)
                            {
                                fstorage.FileImage = ms.ToArray();
                                fi.Size = ms.Length;

                                fi = await _fileProvider.SetFileInfo(fi);
                                fstorage = await _fileProvider.SetFileStorage(fstorage);
                            }

                            return fi;
                        }
                    }
                }
            }

            return null;
        }
        public FORM_Application_Status? GetStatus(Guid FORM_Application_Status_ID)
        {
            var d = _unitOfWork.Repository<FORM_Application_Status>().FirstOrDefault(p => p.ID == FORM_Application_Status_ID);

            if (d != null && d.TEXT_SystemTexts_Code != null)
            {
                d.Name = _textProvider.Get(d.TEXT_SystemTexts_Code);
            }

            return d;
        }
        public List<FORM_Application_Status> GetStatusList()
        {
            var data = _unitOfWork.Repository<FORM_Application_Status>().ToList();

            foreach (var d in data)
            {
                if (d.TEXT_SystemTexts_Code != null)
                {
                    d.Name = _textProvider.Get(d.TEXT_SystemTexts_Code);
                }
            }

            return data;
        }
        public async Task<List<FORM_Application_Priority>> GetPriorities()
        {
            var data = await _unitOfWork.Repository<FORM_Application_Priority>().ToListAsync();

            foreach (var d in data)
            {
                if (d.TEXT_SystemText_Code != null)
                {
                    d.Name = _textProvider.Get(d.TEXT_SystemText_Code);
                }
            }

            return data;
        }
        public async Task<bool> RemoveApplication(Guid ID, bool force = false)
        {
            var itemexists = _unitOfWork.Repository<FORM_Application>().FirstOrDefault(p => p.ID == ID);

            if (itemexists != null)
            {
                if (force)
                {
                    await _unitOfWork.Repository<FORM_Application>().DeleteAsync(ID);
                }
                else
                {
                    itemexists.RemovedAt = DateTime.Now;

                    await _unitOfWork.Repository<FORM_Application>().InsertOrUpdateAsync(itemexists);
                }

            }

            return true;
        }
        public async Task<bool> RemoveApplicationFieldData(Guid ID, bool force = false)
        {
            var itemexists = _unitOfWork.Repository<FORM_Application_Field_Data>().FirstOrDefault(p => p.ID == ID);

            if (itemexists != null)
            {
                if (force)
                {
                    await _unitOfWork.Repository<FORM_Application_Field_Data>().DeleteAsync(ID);
                }
                else
                {
                    await _unitOfWork.Repository<FORM_Application_Field_Data>().DeleteAsync(ID);
                }
            }

            return true;
        }
        public async Task<bool> RemoveApplicationFieldSubData(Guid ID, bool force = false)
        {
            var itemexists = _unitOfWork.Repository<FORM_Application_Field_SubData>().FirstOrDefault(p => p.ID == ID);

            if (itemexists != null)
            {
                if (force)
                {
                    await _unitOfWork.Repository<FORM_Application_Field_SubData>().DeleteAsync(ID);
                }
                else
                {
                    await _unitOfWork.Repository<FORM_Application_Field_SubData>().DeleteAsync(ID);
                }
            }

            return true;
        }
        public async Task<bool> RemoveApplicationResponsible(Guid ID, bool force = false)
        {
            var itemexists = _unitOfWork.Repository<FORM_Application_Responsible>().FirstOrDefault(p => p.ID == ID);

            if (itemexists != null)
            {
                if (force)
                {
                    await _unitOfWork.Repository<FORM_Application_Responsible>().DeleteAsync(ID);
                }
                else
                {
                    await _unitOfWork.Repository<FORM_Application_Responsible>().DeleteAsync(ID);
                }
            }

            return true;
        }
        public async Task<bool> RemoveApplicationUpload(Guid ID, bool force = false)
        {
            var itemexists = _unitOfWork.Repository<FORM_Application_Upload>().FirstOrDefault(p => p.ID == ID);

            if (itemexists != null)
            {
                if (force)
                {
                    await _unitOfWork.Repository<FORM_Application_Upload>().DeleteAsync(ID);
                }
                else
                {
                    await _unitOfWork.Repository<FORM_Application_Upload>().DeleteAsync(ID);
                }
            }

            return true;
        }
        public async Task<bool> RemoveApplicationUploadFile(Guid ID, bool force = false)
        {
            var itemexists = _unitOfWork.Repository<FORM_Application_Upload_File>().FirstOrDefault(p => p.ID == ID);

            if (itemexists != null)
            {
                if (force)
                {
                    await _unitOfWork.Repository<FORM_Application_Upload_File>().DeleteAsync(ID);
                }
                else
                {
                    await _unitOfWork.Repository<FORM_Application_Upload_File>().DeleteAsync(ID);
                }
            }

            return true;
        }
        public async Task<FORM_Application?> SetApplication(FORM_Application Data)
        {
            return await _unitOfWork.Repository<FORM_Application>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Application_Field_Data?> SetApplicationFieldData(FORM_Application_Field_Data Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Field_Data>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Application_Field_SubData?> SetApplicationFieldSubData(FORM_Application_Field_SubData Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Field_SubData>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveApplicationFieldSubData(Guid fieldDataId)
        {
            var subDatas = await GetApplicationFieldSubDataList(fieldDataId);
            foreach(var subData in subDatas)
            {
                await _unitOfWork.Repository<FORM_Application_Field_SubData>().DeleteAsync(subData);
            }
            return true;
        }
        public async Task<FORM_Application_Responsible?> SetApplicationResponsible(FORM_Application_Responsible Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Responsible>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Application_Upload?> SetApplicationUpload(FORM_Application_Upload Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Upload>().InsertOrUpdateAsync(Data);
        }
        public async Task<FORM_Application_Upload_File?> SetApplicationUploadFile(FORM_Application_Upload_File Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Upload_File>().InsertOrUpdateAsync(Data);
        }
        public List<FORM_Application_Status> GetStatusListByMunicipality(Guid AUTH_Municipality_ID)
        {
            var data = GetStatusList();
            var municipalStatusList = _unitOfWork.Repository<FORM_Application_Status_Municipal>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToList();

            data = data.Where(p => p.Selectable == false || municipalStatusList.Where(p => p.Enabled == true).Select(x => x.FORM_Application_Status_ID).Contains(p.ID)).ToList();

            foreach (var d in data)
            {
                if (d.TEXT_SystemTexts_Code != null)
                {
                    d.Name = _textProvider.Get(d.TEXT_SystemTexts_Code);
                }
            }

            return data;
        }
        public List<FORM_Application_Status_Municipal> GetOrCreateMunicipalStatusList(Guid AUTH_Municipality_ID)
        {
            var defaultStatusList = GetStatusList();
            var municipalStatusList = _unitOfWork.Repository<FORM_Application_Status_Municipal>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToList();

            var StatusToAdd = defaultStatusList.Where(p => p.Selectable == true && !municipalStatusList.Select(x => x.FORM_Application_Status_ID).Contains(p.ID)).ToList();

            foreach (var status in StatusToAdd)
            {
                var newItem = new FORM_Application_Status_Municipal();

                newItem.ID = Guid.NewGuid();
                newItem.FORM_Application_Status_ID = status.ID;
                newItem.Enabled = true;
                newItem.SortOrder = status.SortOrder;
                newItem.AUTH_Municipality_ID = AUTH_Municipality_ID;
                newItem.GeneratesPDF = status.GeneratesPDF;


                _unitOfWork.Repository<FORM_Application_Status_Municipal>().InsertOrUpdateAsync(newItem);
            }

            return _unitOfWork.Repository<FORM_Application_Status_Municipal>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToList();
        }
        public async Task<FORM_Application_Status_Municipal?> GetMunicipalStatus(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Status_Municipal>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public FORM_Application_Status_Municipal? SetMunicipalStatus(FORM_Application_Status_Municipal Data)
        {
            return _unitOfWork.Repository<FORM_Application_Status_Municipal>().InsertOrUpdate(Data);
        }
        public async Task<List<FORM_Application_Status_Log>> GetApplicationStatusLogList(Guid FORM_Application_ID)
        {
            var data = await _unitOfWork.Repository<FORM_Application_Status_Log>().Where(p => p.FORM_Application_ID == FORM_Application_ID).Include(p => p.FORM_Application_Status_Log_Extended).ToListAsync();

            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var statusList = GetStatusList();

            foreach (var d in data)
            {
                if (d != null && d.FORM_Application_Status_ID != null)
                {
                    var status = statusList.FirstOrDefault(p => p.ID == d.FORM_Application_Status_ID.Value);

                    if (status != null)
                    {
                        d.Status = status.Name;
                        d.StatusIcon = status.Icon;
                    }

                    if (d != null && d.AUTH_Users_ID != null)
                    {
                        var user = await _authProvider.GetUser(d.AUTH_Users_ID.Value);

                        if (user != null)
                        {
                            d.User = user.Firstname + " " + user.Lastname;
                        }
                    }

                    if (d != null && Language != null)
                    {
                        var extended = d.FORM_Application_Status_Log_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                        if (extended != null && extended.Reason != null && extended.Title != null)
                        {
                            d.Reason = extended.Reason;
                            d.Title = extended.Title;
                        }
                        else
                        {
                            extended = d.FORM_Application_Status_Log_Extended.FirstOrDefault(p => p.LANG_Languages_ID != Language.ID);

                            if (extended != null && extended.Reason != null && extended.Title != null)
                            {
                                d.Reason = extended.Reason;
                                d.Title = extended.Title;
                            }
                        }
                    }
                }
            }

            return data;
        }
        public async Task<FORM_Application_Status_Log?> GetApplicationStatusLog(Guid ID)
        {
            var d = await _unitOfWork.Repository<FORM_Application_Status_Log>().Where(p => p.ID == ID).Include(p => p.FORM_Application_Status_Log_Extended).FirstOrDefaultAsync();
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (d != null && d.FORM_Application_Status_ID != null)
            {
                var status = GetStatus(d.FORM_Application_Status_ID.Value);

                if (status != null)
                {
                    d.Status = status.Name;
                    d.StatusIcon = status.Icon;
                }
            }

            if (d != null && d.AUTH_Users_ID != null)
            {
                var user = await _authProvider.GetUser(d.AUTH_Users_ID.Value);

                if (user != null)
                {
                    d.User = user.Firstname + " " + user.Lastname;
                }
            }

            if (d != null && Language != null)
            {
                var extended = d.FORM_Application_Status_Log_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                if (extended != null && extended.Reason != null && extended.Title != null)
                {
                    d.Reason = extended.Reason;
                    d.Title = extended.Title;
                }
                else
                {
                    extended = d.FORM_Application_Status_Log_Extended.FirstOrDefault(p => p.LANG_Languages_ID != Language.ID);

                    if (extended != null)
                    {
                        d.Reason = extended.Reason;
                        d.Title = extended.Title;
                    }
                }
            }

            return d;
        }
        public async Task<FORM_Application_Status_Log?> SetApplicationStatusLog(FORM_Application_Status_Log Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Status_Log>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<FORM_Application_Transactions>> GetApplicationTransactionList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Transactions>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync(); 
        }
        public async Task<FORM_Application_Transactions?> SetApplicationTransaction(FORM_Application_Transactions Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Transactions>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<FORM_Application_Chat>> GetApplicationChatList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Chat>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<bool> RemoveApplicationTransactions(Guid applicationID)
        {
            var transactions = await GetApplicationTransactionList(applicationID);
            foreach (var transaction in transactions)
            {
                await _unitOfWork.Repository<FORM_Application_Transactions>().DeleteAsync(transaction.ID);
                if(transaction.PAY_Transaction_ID != null)
                    await _unitOfWork.Repository<PAY_Transaction>().DeleteAsync(transaction.PAY_Transaction_ID);
            }
            return true;
        }
        public async Task<FORM_Application_Chat?> GetApplicationChat(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Chat>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<FORM_Application_Chat?> SetApplicationChat(FORM_Application_Chat Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Chat>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveApplicationChat(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Chat>().DeleteAsync(ID);
        }
        public async Task<List<FORM_Application_Chat_Dokument>> GetApplicationChatDokumentList(Guid FORM_Application_Chat_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Chat_Dokument>().Where(p => p.FORM_Application_Chat_ID == FORM_Application_Chat_ID).ToListAsync();
        }
        public async Task<FORM_Application_Chat_Dokument?> GetApplicationDokumentChat(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Chat_Dokument>().GetByIDAsync(ID);
        }
        public async Task<FORM_Application_Chat_Dokument?> SetApplicationDokumentChat(FORM_Application_Chat_Dokument Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Chat_Dokument>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveApplicationDokumentChat(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Chat_Dokument>().DeleteAsync(ID);
        }
        public async Task<List<V_FORM_Application_Chat>> GetViewApplicationChatList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Chat>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<List<FORM_Application_Chat_Dokument>> GetApplicationChatDokumentListByApplication(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Chat_Dokument>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<List<V_FORM_Application_Chat_Dokument>> GetViewApplicationChatDokumentListByApplication(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Chat_Dokument>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<List<FORM_Application_Ressource>> GetApplicationRessourceList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Ressource>().Where(p => p.FORM_Application_ID == FORM_Application_ID && p.RemovedAt == null).ToListAsync();
        }
        public async Task<List<V_FORM_Application_Ressource>> GetVApplicationRessourceList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Ressource>().Where(p => p.FORM_Application_ID == FORM_Application_ID && p.RemovedAt == null).ToListAsync();
        }
        public async Task<FORM_Application_Ressource?> GetApplicationRessource(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Ressource>().FirstOrDefaultAsync(p => p.ID == ID && p.RemovedAt == null);
        }
        public async Task<FORM_Application_Ressource?> SetApplicationRessource(FORM_Application_Ressource Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Ressource>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveApplicationRessource(Guid ID, bool force = false)
        {
            var itemexists = _unitOfWork.Repository<FORM_Application_Ressource>().FirstOrDefault(p => p.ID == ID);

            if (itemexists != null)
            {
                if (force)
                {
                    await _unitOfWork.Repository<FORM_Application_Ressource>().DeleteAsync(ID);
                }
                else
                {
                    itemexists.RemovedAt = DateTime.Now;

                    await _unitOfWork.Repository<FORM_Application_Ressource>().InsertOrUpdateAsync(itemexists);
                }
            }

            return true;
        }
        public async Task<List<V_FORM_Application_Priority_Statistik>> GetPriorityStatistik(Guid AUTH_Municipality_ID, Guid AUTH_Authority_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Priority_Statistik>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Authority_ID == AUTH_Authority_ID).ToListAsync();
        }
        public async Task<List<V_FORM_Application_Status_Statistik>> GetStatusStatistik(Guid AUTH_Municipality_ID, Guid AUTH_Authority_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Status_Statistik>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.AUTH_Authority_ID == AUTH_Authority_ID).ToListAsync();
        }
        public async Task<FORM_Application_Status_Log_Extended?> GetApplicationStatusLogExtended(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Status_Log_Extended>().GetByIDAsync(ID);
        }
        public async Task<FORM_Application_Status_Log_Extended?> SetApplicationStatusLogExtended(FORM_Application_Status_Log_Extended Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Status_Log_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<FORM_Application_Note>> GetApplicationNoteList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Note>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<FORM_Application_Note?> GetApplicationNote(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Note>().GetByIDAsync(ID);
        }
        public async Task<FORM_Application_Note?> SetApplicationNote(FORM_Application_Note Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Note>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveApplicationNote(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Note>().DeleteAsync(ID);
        }
        public async Task<List<FORM_Application_Note_Dokument>> GetApplicationNoteDokumentList(Guid FORM_Application_Note_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Note_Dokument>().Where(p => p.FORM_Application_Note_ID == FORM_Application_Note_ID).ToListAsync();
        }
        public async Task<FORM_Application_Note_Dokument?> GetApplicationDokumentNote(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Note_Dokument>().GetByIDAsync(ID);
        }
        public async Task<FORM_Application_Note_Dokument?> SetApplicationDokumentNote(FORM_Application_Note_Dokument Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Note_Dokument>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveApplicationDokumentNote(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Note_Dokument>().DeleteAsync(ID);
        }
        public async Task<List<V_FORM_Application_Note>> GetViewApplicationNoteList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Note>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<List<V_FORM_Application_Note_Dokument>> GetViewApplicationNoteDokumentList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<V_FORM_Application_Note_Dokument>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<List<FORM_Application_Municipal_Field_Data>> GetFormApplicationMunicipalFieldList(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Municipal_Field_Data>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<FORM_Application_Municipal_Field_Data?> GetFormApplicationMunicipalField(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Municipal_Field_Data>().GetByIDAsync(ID);
        }
        public async Task<FORM_Application_Municipal_Field_Data?> SetFormApplicationMunicipalField(FORM_Application_Municipal_Field_Data Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Municipal_Field_Data>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveFormApplicationMunicipalField(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Municipal_Field_Data>().DeleteAsync(ID);
        }
        public long GetLatestProgressivNumber(Guid Form_Definition_ID, Guid AUTH_Municipality_ID, int Year)
        {
            var number = _unitOfWork.Repository<FORM_Application>().Where(p => p.FORM_Definition_ID == Form_Definition_ID && p.AUTH_Municipality_ID == AUTH_Municipality_ID && p.ProgressivYear == Year).Max(p => p.ProgressivNumber);

            if (number != null)
            {
                return number.Value;
            }

            return 0;
        }
        public async Task<string> ReplaceKeywords(Guid FORM_Application_ID, Guid LANG_Language_ID, string Text, Guid? PreviousStatus_ID = null)
        {
            var application = await _unitOfWork.Repository<V_FORM_Application>().FirstOrDefaultAsync(p => p.ID == FORM_Application_ID && p.LANG_Language_ID == LANG_Language_ID);

            if (application != null)
            {
                Text = Text.Replace("{Protokollnummer}", application.ProgressivNumber);
                Text = Text.Replace("{Numero di protocollo}", application.ProgressivNumber);
                Text = Text.Replace("{Name Antrag}", application.FormName);
                Text = Text.Replace("{Nome richiesta}", application.FormName);
                Text = Text.Replace("{Amt}", application.Authority);
                Text = Text.Replace("{Ufficio}", application.Authority);
                Text = Text.Replace("{Verantwortliche Mitarbeiter}", application.Responsible);
                Text = Text.Replace("{Dipendenti responsabili}", application.Responsible);

                var StatusList = GetStatusList();

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

                var newStatus = StatusList.FirstOrDefault(p => p.ID == application.FORM_Application_Status_ID);

                if (newStatus != null)
                {
                    var item = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == newStatus.TEXT_SystemTexts_Code && p.LANG_LanguagesID == LANG_Language_ID);

                    if (item != null)
                    {
                        Text = Text.Replace("{Neuer Status}", item.Text);
                        Text = Text.Replace("{Nuovo stato}", item.Text);
                    }
                }

                if (application.SubmitAt != null)
                {
                    Text = Text.Replace("{Einreichdatum}", application.SubmitAt.Value.ToString("dd.MM.yyyy HH:mm"));
                    Text = Text.Replace("{Data di inserimento}", application.SubmitAt.Value.ToString("dd.MM.yyyy HH:mm"));
                }


                Text = Text.Replace("{Titel Meldung}", application.Mantainance_Title);
                Text = Text.Replace("{Titolo segnalazione}", application.Mantainance_Title);
                Text = Text.Replace("{Beschreibung Meldung}", application.Mantainance_Description);
                Text = Text.Replace("{Descritione segnalazione}", application.Mantainance_Description);
            }

            return Text;
        }
        public async Task<List<SignerItem>> GetSigningFields(Guid FORM_Application_ID)
        {
            var fields = await _unitOfWork.Repository<FORM_Application_Field_Data>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();

            List<SignerItem> result = new List<SignerItem>();

            foreach(var f in fields)
            {
                var defField = await _unitOfWork.Repository<FORM_Definition_Field>().Where(p => p.ID == f.FORM_Definition_Field_ID).Include(p => p.FORM_Definition_Field_Extended).FirstOrDefaultAsync();

                if(defField != null && defField.FORM_Definition_Fields_Type_ID == FORMElements.Signature)
                {
                    FORM_Application_Field_Data? nameRef = null; 
                    FORM_Application_Field_Data? mailRef = null;

                    if (defField.Ref_Signature_Name != null && defField.Ref_Signature_Mail != null && f.RepetitionParentID != null)
                    {
                        nameRef = await GetSubFieldByType(f.RepetitionParentID.Value, fields, defField.Ref_Signature_Name.Value);
                        mailRef = await GetSubFieldByType(f.RepetitionParentID.Value, fields, defField.Ref_Signature_Mail.Value);

                        if(nameRef == null)
                            nameRef = await GetSubFieldByType(FORM_Application_ID, fields, defField.Ref_Signature_Name.Value);

                        if(mailRef == null)
                            mailRef = await GetSubFieldByType(FORM_Application_ID, fields, defField.Ref_Signature_Mail.Value);

                        if (nameRef != null && !string.IsNullOrEmpty(nameRef.Value) && mailRef != null && !string.IsNullOrEmpty(mailRef.Value))
                        {
                            var newItem = new SignerItem()
                            {
                                Name = nameRef.Value,
                                Mail = mailRef.Value,
                            };

                            var defFieldExt = defField.FORM_Definition_Field_Extended.FirstOrDefault(p => p.LANG_Languages_ID == _langProvider.GetCurrentLanguageID());

                            if (defFieldExt != null)
                            {
                                newItem.Description = defFieldExt.Name;
                            }

                            result.Add(newItem);
                        }
                    }
                }
            }

            return result;
        }
        private async Task<FORM_Application_Field_Data?> GetSubFieldByType(Guid StartItem_ID, List<FORM_Application_Field_Data> Fields, Guid Target_Definition_Field_ID)
        {           
            foreach(var item in Fields.Where(p => p.RepetitionParentID == StartItem_ID).ToList())
            {
                var defField = await _unitOfWork.Repository<FORM_Definition_Field>().Where(p => p.ID == item.FORM_Definition_Field_ID).FirstOrDefaultAsync();

                if (defField != null && defField.ID == Target_Definition_Field_ID)
                {
                    return item;
                }
                else
                {
                    var result = await GetSubFieldByType(item.ID, Fields, Target_Definition_Field_ID);

                    if(result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
        public async Task<List<FILE_FileInfo>> GetApplicationUploadFiles(Guid FORM_Application_ID, Guid FORM_Definition_ID)
        {
            var result = new List<FILE_FileInfo>();

            var dynamicUploadFields = await _unitOfWork.Repository<FORM_Definition_Field>().ToListAsync(p => p.FORM_Definition_ID == FORM_Definition_ID && p.FORM_Definition_Fields_Type_ID == FORMElements.FileUpload);

            var fields = await _unitOfWork.Repository<FORM_Application_Field_Data>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();

            foreach(var f in fields.Where(p => p.FORM_Definition_Field_ID != null && dynamicUploadFields.Select(x => x.ID).Distinct().Contains(p.FORM_Definition_Field_ID.Value)).ToList())
            {
                if(!string.IsNullOrEmpty(f.Value))
                {
                    var ids = f.Value.Split(";");

                    foreach (var id in ids.Where(p => !string.IsNullOrEmpty(p)).ToList())
                    {
                        var guid = Guid.Parse(id);

                        var fi = await _unitOfWork.Repository<FILE_FileInfo>().FirstAsync(p => p.ID == guid);

                        if (fi != null && result.FirstOrDefault(p => p.ID == fi.ID) == null)
                        {
                            result.Add(fi);
                        }
                    }
                }
            }

            return result;
        }
        public async Task<List<FORM_Application_Archive>> GetArchive(Guid FORM_Application_ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Archive>().Where(p => p.FORM_Application_ID == FORM_Application_ID).ToListAsync();
        }
        public async Task<FORM_Application_Archive?> SetArchive(FORM_Application_Archive Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Archive>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveArchive(Guid ID)
        {
            return await _unitOfWork.Repository<FORM_Application_Archive>().DeleteAsync(ID);
        }
        public async Task<bool> IsBolloFree(Guid FORM_Application_ID, Guid FORM_Definition_ID)
        {
            var bolloFreeFields = await _unitOfWork.Repository<FORM_Definition_Field>().Where(p => p.FORM_Definition_ID == FORM_Definition_ID && (p.BolloFree == true || p.FORM_Definition_Field_Option.FirstOrDefault(x => x.BolloFree == true) != null))
                                                   .Include(p => p.FORM_Definition_Field_Option).ToListAsync();

            foreach(var f in bolloFreeFields)
            {
                var data = await _unitOfWork.Repository<FORM_Application_Field_Data>().Where(p => p.FORM_Definition_Field_ID == f.ID && p.FORM_Application_ID == FORM_Application_ID).ToListAsync();

                foreach(var el in data)
                {
                    if(f.FORM_Definition_Fields_Type_ID == FORMElements.Checkbox)
                    {
                        if (el.BoolValue == true)
                            return true;
                    }
                    else if (f.FORM_Definition_Fields_Type_ID == FORMElements.Radiobutton || f.FORM_Definition_Fields_Type_ID == FORMElements.Dropdown)
                    {
                        foreach(var opt in f.FORM_Definition_Field_Option.Where(p => p.BolloFree == true))
                        {
                            if(el.GuidValue == opt.ID)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public async Task<bool> IsResponsibleForTask(Guid userId, Guid applicationId)
        {
            var tasks = await _unitOfWork.Repository<TASK_Task>()
                .ToListAsync(e => e.ContextElementID == applicationId.ToString());
            foreach (var task in tasks)
            {
                var responsible = await _unitOfWork.Repository<TASK_Task_Responsible>()
                    .FirstOrDefaultAsync(e => e.AUTH_Municipal_Users_ID == userId && e.TASK_Task_ID == task.ID);
                if (responsible != null)
                    return true;
            }
            return false;
        }
        public async Task<FORM_Application_Log> SetApplicationLog(FORM_Application_Log Data)
        {
            return await _unitOfWork.Repository<FORM_Application_Log>().InsertOrUpdateAsync(Data);
        }
    }
}
