using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Reporting;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ICWebApp.Application.Provider
{
    public class ORGProvider : IORGProvider
    {
        private IUnitOfWork _unitOfWork;
        private ISessionWrapper _sessionWrapper;
        private ILANGProvider _langProvider;
        private ITEXTProvider _textProvider;
        private IAUTHProvider _authProvider;
        private IFILEProvider _fileProvider;
        private NavigationManager _navManager;

        private string BasePath = @"D:\Comunix\Reports\";

        public ORGProvider(IUnitOfWork _unitOfWork, ISessionWrapper _sessionWrapper, ILANGProvider _langProvider, ITEXTProvider _textProvider, IAUTHProvider _authProvider, IFILEProvider _fileProvider, NavigationManager _navManager)
        {
            this._unitOfWork = _unitOfWork;
            this._sessionWrapper = _sessionWrapper;
            this._langProvider = _langProvider;
            this._textProvider = _textProvider;
            this._authProvider = _authProvider;
            this._fileProvider = _fileProvider;
            this._navManager = _navManager;

            if (_navManager.BaseUri.Contains("localhost"))
            {
                BasePath = @"C:\VisualStudioProjects\Comunix\ICWebApp.Reporting\wwwroot\Reports\";
            }
        }
        public async Task<ORG_Request?> GetRequest(Guid ID)
        {
            return await _unitOfWork.Repository<ORG_Request>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public async Task<V_ORG_Requests?> GetVRequest(Guid ID)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            return await _unitOfWork.Repository<V_ORG_Requests>().Where(p => p.ID == ID && p.LANG_LanguagesID == Language.ID).FirstOrDefaultAsync();
        }
        public async Task<ORG_Request_Attachment?> GetRequestAttachment(Guid ID)
        {
            return await _unitOfWork.Repository<ORG_Request_Attachment>().Where(p => p.ID == ID).FirstOrDefaultAsync();
        }
        public async Task<List<ORG_Request_Attachment>?> GetRequestAttachments(Guid ORG_Request_ID)
        {
            return await _unitOfWork.Repository<ORG_Request_Attachment>().Where(p => p.ORG_Request_ID == ORG_Request_ID).ToListAsync();
        }
        public async Task<List<V_ORG_Requests>?> GetRequestList(Guid AUTH_Users_ID)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            return await _unitOfWork.Repository<V_ORG_Requests>().Where(p => p.LANG_LanguagesID == Language.ID && p.AUTH_Users_ID == AUTH_Users_ID).ToListAsync();
        }
        public async Task<List<V_ORG_Requests>?> GetRequestList(Administration_Filter_Request Filter)
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if(_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<V_ORG_Requests>();
            }

            var data = _unitOfWork.Repository<V_ORG_Requests>().Where(p => p.LANG_LanguagesID == Language.ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);

            if (Filter != null && Filter.Archived == true)
            {
                data = data.Where(p => p.Archived != null);
            }
            else
            {
                data = data.Where(p => p.Archived == null);
            }

            if (Filter != null && Filter.Auth_User_ID != null)
            {
                data = data.Where(p => p.AUTH_Users_ID == Filter.Auth_User_ID);
            }
            if (Filter != null && Filter.Request_Status_ID != null && Filter.Request_Status_ID.Count() > 0)
            {
                data = data.Where(p => p.ORG_Request_Status_ID != null && Filter.Request_Status_ID.Contains(p.ORG_Request_Status_ID.Value));
            }
            if (Filter != null && Filter.SubmittedFrom != null)
            {
                data = data.Where(p => p.SubmitAt >= Filter.SubmittedFrom);
            }
            if (Filter != null && Filter.SubmittedTo != null)
            {
                data = data.Where(p => p.SubmitAt <= Filter.SubmittedTo);
            }
            if (Filter != null && Filter.Company_Type_ID != null && Filter.Company_Type_ID.Count() > 0)
            {
                data = data.Where(p => p.AUTH_Company_Type_ID != null && Filter.Company_Type_ID.Contains(p.AUTH_Company_Type_ID.Value));
            }
            if (Filter != null && !string.IsNullOrEmpty(Filter.Text))
            {
                data = data.Where(p => p.SearchText != null && p.SearchText.ToUpper().Contains(Filter.Text.ToUpper()));
            }

            return await data.OrderByDescending(o => o.CreationDate).ToListAsync();
        }
        public async Task<List<V_ORG_Requests>?> GetRequestList()
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            if (_sessionWrapper.AUTH_Municipality_ID == null)
            {
                return new List<V_ORG_Requests>();
            }

            var data = _unitOfWork.Repository<V_ORG_Requests>().Where(p => p.LANG_LanguagesID == Language.ID && p.AUTH_Municipality_ID == _sessionWrapper.AUTH_Municipality_ID.Value);


            return await data.OrderByDescending(o => o.CreationDate).ToListAsync();
        }
        public async Task<List<ORG_Request>?> GetRequests(Guid AUTH_Users_ID)
        {
            return await _unitOfWork.Repository<ORG_Request>().Where(p => p.AUTH_Users_ID == AUTH_Users_ID).ToListAsync();
        }
        public async Task<ORG_Request_Status_Log?> GetRequestStatusLog(Guid ID)
        {
            return await _unitOfWork.Repository<ORG_Request_Status_Log>().Where(p => p.ID == ID).Include(p => p.ORG_Request_Status_Log_Extended).FirstOrDefaultAsync();
        }
        public async Task<ORG_Request_Status_Log_Extended?> GetRequestStatusLogExtended(Guid ID)
        {
            return await _unitOfWork.Repository<ORG_Request_Status_Log_Extended>().Where(p => p.ID == ID).FirstOrDefaultAsync();
        }
        public async Task<List<ORG_Request_Status_Log>> GetRequestStatusLogListByRequest(Guid ORG_Request_ID)
        {
            var data = await _unitOfWork.Repository<ORG_Request_Status_Log>().Where(p => p.ORG_Request_ID == ORG_Request_ID).Include(p => p.ORG_Request_Status_Log_Extended).ToListAsync();
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);
            var statusList = await GetStatusList();

            if (statusList != null)
            {
                foreach (var d in data)
                {
                    if (d != null && d.ORG_Request_Status_ID != null)
                    {
                        var status = statusList.FirstOrDefault(p => p.ID == d.ORG_Request_Status_ID.Value);

                        if (status != null && !string.IsNullOrEmpty(status.TEXT_SystemText_Code))
                        {
                            d.Status = _textProvider.Get(status.TEXT_SystemText_Code);
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
                            var extended = d.ORG_Request_Status_Log_Extended.FirstOrDefault(p => p.LANG_Languages_ID == Language.ID);

                            if (extended != null && extended.Reason != null && extended.Title != null)
                            {
                                d.Reason = extended.Reason;
                                d.Title = extended.Title;
                            }
                            else
                            {
                                extended = d.ORG_Request_Status_Log_Extended.FirstOrDefault(p => p.LANG_Languages_ID != Language.ID);

                                if (extended != null && extended.Reason != null && extended.Title != null)
                                {
                                    d.Reason = extended.Reason;
                                    d.Title = extended.Title;
                                }
                            }
                        }
                    }
                }
            }

            return data;
        }
        public async Task<List<ORG_Request_Status_Log>> GetRequestStatusLogListByRequestUser(Guid ORG_Request_User_ID)
        {
            return await _unitOfWork.Repository<ORG_Request_Status_Log>().Where(p => p.ORG_Request_User_ID == ORG_Request_User_ID).Include(p => p.ORG_Request_Status_Log_Extended).ToListAsync();
        }
        public async Task<ORG_Request_User?> GetRequestUser(Guid ID)
        {
            return await _unitOfWork.Repository<ORG_Request_User>().Where(p => p.ID == ID).FirstOrDefaultAsync();
        }
        public async Task<List<ORG_Request_User>?> GetRequestUsers(Guid ORG_AUTH_Users_ID)
        {
            return await _unitOfWork.Repository<ORG_Request_User>().Where(p => p.ORG_AUTH_Users_ID == ORG_AUTH_Users_ID).ToListAsync();
        }
        public async Task<List<V_ORG_Request_Status>?> GetStatusList()
        {
            var Language = _langProvider.GetLanguageByCode(CultureInfo.CurrentCulture.Name);

            return await _unitOfWork.Repository<V_ORG_Request_Status>().Where(p => p.LANG_LanguagesID == Language.ID).ToListAsync();
        }
        public async Task<List<V_ORG_Request_Users>> GetRequestUserList(Guid AUTH_Municipality_ID)
        {
            return await _unitOfWork.Repository<V_ORG_Request_Users>().Where(p => p.AUTH_Municipality_ID == AUTH_Municipality_ID).ToListAsync();
        }
        public async Task<ORG_Request?> SetRequest(ORG_Request Data)
        {
            return await _unitOfWork.Repository<ORG_Request>().InsertOrUpdateAsync(Data);
        }
        public async Task<ORG_Request_Attachment?> SetRequestAttachment(ORG_Request_Attachment Data)
        {
            return await _unitOfWork.Repository<ORG_Request_Attachment>().InsertOrUpdateAsync(Data);
        }
        public async Task<ORG_Request_Status_Log?> SetRequestStatusLog(ORG_Request_Status_Log Data)
        {
            return await _unitOfWork.Repository<ORG_Request_Status_Log>().InsertOrUpdateAsync(Data);
        }
        public async Task<ORG_Request_Status_Log_Extended?> SetRequestStatusLogExtended(ORG_Request_Status_Log_Extended Data)
        {
            return await _unitOfWork.Repository<ORG_Request_Status_Log_Extended>().InsertOrUpdateAsync(Data);
        }
        public async Task<ORG_Request_User?> SetRequestUser(ORG_Request_User Data)
        {
            return await _unitOfWork.Repository<ORG_Request_User>().InsertOrUpdateAsync(Data);
        }
        public async Task<List<ORG_Request_Ressource>> GetRequestRessourceList(Guid ORG_Request_ID)
        {
            return await _unitOfWork.Repository<ORG_Request_Ressource>().Where(p => p.ORG_Request_ID == ORG_Request_ID && p.RemovedAt == null).ToListAsync();
        }
        public async Task<ORG_Request_Ressource?> GetRequestRessource(Guid ID)
        {
            return await _unitOfWork.Repository<ORG_Request_Ressource>().FirstOrDefaultAsync(p => p.ID == ID && p.RemovedAt == null);
        }
        public async Task<ORG_Request_Ressource?> SetRequestRessource(ORG_Request_Ressource Data)
        {
            return await _unitOfWork.Repository<ORG_Request_Ressource>().InsertOrUpdateAsync(Data);
        }
        public async Task<bool> RemoveRequestRessource(Guid ID, bool force = false)
        {
            var itemexists = _unitOfWork.Repository<ORG_Request_Ressource>().Where(p => p.ID == ID).FirstOrDefault();

            if (itemexists != null)
            {
                itemexists.RemovedAt = DateTime.Now;

                await _unitOfWork.Repository<ORG_Request_Ressource>().InsertOrUpdateAsync(itemexists);
            }

            return true;
        }
        public async Task<FILE_FileInfo?> CreateFile(ORG_Request Data)
        {
            if (Data != null)
            {
                var ms = CreatePDF(Data);

                FILE_FileInfo? fi = null;
                FILE_FileStorage storage = null;

                if (Data.FILE_Fileinfo_ID != null)
                {
                    await _fileProvider.RemoveFileInfo(Data.FILE_Fileinfo_ID.Value, true);
                }

                if (fi == null)
                {
                    fi = new FILE_FileInfo();
                    fi.ID = Guid.NewGuid();
                    fi.FileName = _textProvider.Get("ORG_REQUEST_FILE_NAME");
                    fi.FileExtension = ".pdf";
                    fi.CreationDate = DateTime.Now;
                    fi.AUTH_Users_ID = Data.AUTH_Users_ID;
                }

                if (fi.FILE_FileStorage != null && fi.FILE_FileStorage.Count() > 0)
                {
                    storage = _fileProvider.GetFileStorage(fi.ID);
                }

                if (storage == null)
                {
                    storage = new FILE_FileStorage();

                    storage.ID = Guid.NewGuid();
                }

                if (ms != null)
                {
                    storage.FILE_FileInfo_ID = fi.ID;
                    storage.FileImage = ms.ToArray();
                    storage.CreationDate = DateTime.Now;

                    fi.Size = ms.Length;
                }

                fi = await _fileProvider.SetFileInfo(fi);
                await _fileProvider.SetFileStorage(storage);

                if (fi != null)
                {
                    Data.FILE_Fileinfo_ID = fi.ID;

                    await SetRequest(Data);

                    return fi;
                }
            }

            return null;
        }
        public MemoryStream? CreatePDF(ORG_Request Data)
        {
            if (Data != null)
            {
                var reportPackager = new ReportPackager();
                var reportSource = new InstanceReportSource();

                using (var sourceStream = System.IO.File.OpenRead(BasePath + "Organization.trdp"))
                {
                    var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);

                    if (report.ReportParameters["LanguageID"] != null)
                    {
                        report.ReportParameters["LanguageID"].Value = _langProvider.GetCurrentLanguageID().ToString();
                    }
                    else
                    {
                        report.ReportParameters.Add(new ReportParameter("LanguageID", ReportParameterType.String, _langProvider.GetCurrentLanguageID().ToString()));
                    }

                    if (report.ReportParameters["MunicipalityID"] != null)
                    {
                        report.ReportParameters["MunicipalityID"].Value = Data.AUTH_Municipality_ID.ToString();
                    }
                    else
                    {
                        report.ReportParameters.Add(new ReportParameter("MunicipalityID", ReportParameterType.String, Data.AUTH_Municipality_ID.ToString()));
                    }

                    if (report.ReportParameters["RequestID"] != null)
                    {
                        report.ReportParameters["RequestID"].Value = Data.ID.ToString();
                    }
                    else
                    {
                        report.ReportParameters.Add(new ReportParameter("RequestID", ReportParameterType.String, Data.ID.ToString()));
                    }

                    reportSource.ReportDocument = report;

                    var reportProcessor = new Telerik.Reporting.Processing.ReportProcessor();
                    var deviceInfo = new System.Collections.Hashtable();

                    deviceInfo.Add("ComplianceLevel", "PDF/A-2b");

                    Telerik.Reporting.Processing.RenderingResult result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);

                    return new MemoryStream(result.DocumentBytes);
                }
            }

            return null;
        }
        public MemoryStream GetResponsePDF(string LanguageID, string MunicipalityID, string RequestID)
        {
            var reportPackager = new ReportPackager();
            var reportSource = new InstanceReportSource();

            using (var sourceStream = System.IO.File.OpenRead(BasePath + "Organization_ResponseTemplate.trdp"))
            {
                var report = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(sourceStream);

                if (report.ReportParameters["LanguageID"] != null)
                {
                    report.ReportParameters["LanguageID"].Value = LanguageID;
                }
                else
                {
                    report.ReportParameters.Add(new ReportParameter("LanguageID", ReportParameterType.String, LanguageID));
                }

                if (report.ReportParameters["MunicipalityID"] != null)
                {
                    report.ReportParameters["MunicipalityID"].Value = MunicipalityID;
                }
                else
                {
                    report.ReportParameters.Add(new ReportParameter("MunicipalityID", ReportParameterType.String, MunicipalityID));
                }

                if (report.ReportParameters["RequestID"] != null)
                {
                    report.ReportParameters["RequestID"].Value = RequestID;
                }
                else
                {
                    report.ReportParameters.Add(new ReportParameter("RequestID", ReportParameterType.String, RequestID));
                }

                reportSource.ReportDocument = report;

                var reportProcessor = new Telerik.Reporting.Processing.ReportProcessor();
                var deviceInfo = new System.Collections.Hashtable();

                deviceInfo.Add("ComplianceLevel", "PDF/A-2b");

                Telerik.Reporting.Processing.RenderingResult result = reportProcessor.RenderReport("PDF", reportSource, deviceInfo);

                return new MemoryStream(result.DocumentBytes);
            }
        }
        public long GetLatestProgressivNumber(Guid AUTH_Municipality_ID, int Year)
        {
            var number = _unitOfWork.Repository<ORG_Request>().Where(p => p.ProgressivYear == Year && p.AUTH_Municipality_ID == AUTH_Municipality_ID).Max(p => p.ProgressivNumber);

            if (number != null)
            {
                return number.Value;
            }

            return 0;
        }
        public async Task<string> ReplaceKeywords(Guid ORG_Request_ID, Guid LANG_Language_ID, string Text, Guid? PreviousStatus_ID = null)
        {
            var orgRequest = await _unitOfWork.Repository<V_ORG_Requests>().FirstOrDefaultAsync(p => p.ID == ORG_Request_ID && p.LANG_LanguagesID == LANG_Language_ID);

            if (orgRequest != null)
            {
                if (orgRequest.ProgressivNumber != null)
                {
                    Text = Text.Replace("{Protokollnummer}", orgRequest.ProgressivNumber);
                    Text = Text.Replace("{Numero di protocollo}", orgRequest.ProgressivNumber);
                }

                var StatusList = await GetStatusList();

                if (StatusList != null)
                {
                    if (PreviousStatus_ID != null)
                    {
                        var prevStatus = StatusList.FirstOrDefault(p => p.ID == PreviousStatus_ID);

                        if (prevStatus != null)
                        {
                            var item = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == prevStatus.TEXT_SystemText_Code && p.LANG_LanguagesID == LANG_Language_ID);

                            if (item != null)
                            {
                                Text = Text.Replace("{Bisheriger Status}", item.Text);
                                Text = Text.Replace("{Stato precedente}", item.Text);
                            }
                        }
                    }

                    var newStatus = StatusList.FirstOrDefault(p => p.ID == orgRequest.ORG_Request_Status_ID);

                    if (newStatus != null)
                    {
                        var item = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == newStatus.TEXT_SystemText_Code && p.LANG_LanguagesID == LANG_Language_ID);

                        if (item != null)
                        {
                            Text = Text.Replace("{Neuer Status}", item.Text);
                            Text = Text.Replace("{Nuovo stato}", item.Text);
                        }
                    }
                }
            }

            return Text;
        }
    }
}
