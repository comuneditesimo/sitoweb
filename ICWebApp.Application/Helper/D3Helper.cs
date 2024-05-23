using Azure.Core;
using ICWebApp.Application.Interface.Helper;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models.Rooms;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Application.Helper;

public class D3Helper : ID3Helper
{
    private IFILEProvider _fileProvider;
    private IAUTHProvider _authProvider;
    private ITEXTProvider _textProvider;
    private IMailerService _mailerService;
    private IFORMDefinitionProvider _formDefinitionProvider;
    private IUnitOfWork _unitOfWork;

    public D3Helper(IFILEProvider fileProvider, IAUTHProvider authProvider, ITEXTProvider textProvider, IMailerService mailerService, IFORMDefinitionProvider formDefinitionProvider,
                    IUnitOfWork unitOfWork)
    {
        this._fileProvider = fileProvider;
        this._authProvider = authProvider;
        this._textProvider = textProvider;
        this._mailerService = mailerService;
        this._formDefinitionProvider = formDefinitionProvider;
        this._unitOfWork = unitOfWork;
    }

    public async Task ProtocolNewCanteenSubscription(Guid fileInfoId, IEnumerable<CANTEEN_Subscriber> canteenSubscriber)
    {
        try
        {
            var canteenSubscribers = canteenSubscriber.ToList();
            var subscriber = canteenSubscribers.FirstOrDefault();

            if (subscriber == null)
                throw new Exception("Subscriber is null. Canteen Request File Info Id: " + fileInfoId);

            if (subscriber.AUTH_Users_ID == null)
                throw new Exception("Subscriber has no AUTH_Users_ID. Canteen Request File Info Id: " + fileInfoId);

            if (subscriber.AUTH_Municipality_ID == null)
                throw new Exception("Could not get Municipality or protocol email address not set. Canteen Request File Info Id: " + fileInfoId);

            var municipality = await _authProvider.GetMunicipality(subscriber.AUTH_Municipality_ID.Value);

            if (municipality == null || municipality.D3ProtocolEmail == null)
                throw new Exception("Could not get Municipality or protocol email address not set. Canteen Request File Info Id: " + fileInfoId);

            var user = await _authProvider.GetUser(subscriber.AUTH_Users_ID.Value);
            if (user == null)
                throw new Exception("Could not get AUTH_User from DB. Canteen Request File Info Id: " + fileInfoId); ;
            var userAnagraphics = await _authProvider.GetAnagraficByUserID(user.ID);

            var type = _textProvider.Get("D3_CANTEEN_SUBSCRIPTION");
            var protocolNumber = municipality.MensaPNPrefix + "-" + subscriber.ProgressivYear + "-" +
                                 subscriber.ProgressivNumber;

            var fileInfo = await _fileProvider.GetFileInfoAsync(fileInfoId);
            var file = await _fileProvider.GetFileStorageAsync(fileInfoId);

            var attachments = new List<MSG_Mailer_Attachment>();
            if (fileInfo != null && file != null && file.FileImage.Length > 0)
            {
                attachments.Add(new MSG_Mailer_Attachment()
                {
                    FileData = file.FileImage,
                    FileName = fileInfo.FileName + fileInfo.FileExtension
                });
            }

            var applicantText = _textProvider.Get("APPLICANT");
            var uploadedFilesText = _textProvider.Get("UPLOADED_FILES");
            
            var msg = new MSG_Mailer();
            msg.Subject = protocolNumber  + "  " + type + "  " + user.FullnameComposed;
            msg.Body = "";
            msg.ToAdress = municipality.D3ProtocolEmail;
            var body = applicantText + ": <br/>" + user.FullnameComposed + "<br/>" + userAnagraphics?.FiscalNumber;
            var attachedDocs = canteenSubscribers.Count(e => e.FILE_FileInfo_SpecialMenu_ID != null);
            if (attachedDocs > 0)
            {
                body += "<br/><br/>" + uploadedFilesText + ": <br/>";
                foreach (var sub in canteenSubscribers)
                {
                    if (sub.FILE_FileInfo_SpecialMenu_ID != null)
                    {
                        var medicalFileInfo = await _fileProvider.GetFileInfoAsync(sub.FILE_FileInfo_SpecialMenu_ID.Value);
                        if (medicalFileInfo != null)
                        {
                            var storage = await _fileProvider.GetFileStorageAsync(medicalFileInfo.ID);
                            if (storage != null && storage.FileImage.Length > 0)
                            {
                                attachments.Add(new MSG_Mailer_Attachment()
                                {
                                    FileData = storage.FileImage,
                                    FileName = medicalFileInfo.FileName + medicalFileInfo.FileExtension
                                });
                                body += medicalFileInfo.FileName + medicalFileInfo.FileExtension + "<br/>";
                            }
                        }
                    }
                }
            }
            msg.Body = body;

            await _mailerService.SendMail(msg, attachments, municipality.ID, null);
        }
        catch (Exception e)
        {
            var errLog = new D3_Error_Log()
            {
                ProtocolElementId = fileInfoId,
                ErrorMessage = e.Message,
                ProtocolElementType = "CanteenApplicationFileInfoId"
            };
            await _unitOfWork.Repository<D3_Error_Log>().InsertAsync(errLog);
        }
    }
    public async Task ProtocolNewCanteenRequestRefundBalances(CANTEEN_RequestRefundBalances request)
    {

        try
        {
            CANTEEN_RequestRefundBalances? _updatedRequest = await _unitOfWork.Repository<CANTEEN_RequestRefundBalances>().FirstOrDefaultAsync(e => e.ID == request.ID);

            if (_updatedRequest != null)
                request = _updatedRequest;
            else
                throw new Exception("Could not get request refund balances mensa with ID " + request.ID + " from DB.");

            if (request.AUTH_Municipality_ID == null)
                throw new Exception("Could not get Municipality ID From Sessionwrapper. Request refund balances mensa ID: " + request.ID);

            AUTH_Municipality? _municipality = await _authProvider.GetMunicipality(request.AUTH_Municipality_ID.Value);

            if (_municipality == null || _municipality.D3ProtocolEmail == null)
                throw new Exception("Could not get Municipality with ID" + request.AUTH_Municipality_ID.Value + " from DB or protocol email address not set. Request refund balances mensa ID: " + request.ID);

            if (request.AUTH_User_ID == null)
                throw new Exception("Request refund balances mensa with ID: " + request.ID + " does not have AUTH_Users_ID");

            AUTH_Users? _user = await _authProvider.GetUser(request.AUTH_User_ID.Value);

            if (_user == null)
                throw new Exception("Could not get user for request refund balances mensa with ID: " + request.ID);

            AUTH_Users_Anagrafic? _userAnagraphics = await _authProvider.GetAnagraficByUserID(_user.ID);
            if (_userAnagraphics == null)
                throw new Exception("Could not get user Anagraphics from DB. Request refund balances mensa ID: " + request.ID);

            string protocolNumber = _municipality.MensaRefundPNPrefix + "-" + request.ProgressivYear + "-" + request.ProgressivNumber;

            List<MSG_Mailer_Attachment> _attachments = new List<MSG_Mailer_Attachment>();
            if (request.FILE_FileInfoID != null)
            {
                FILE_FileInfo? _formFileInfo = await _fileProvider.GetFileInfoAsync(request.FILE_FileInfoID.Value);
                FILE_FileStorage? _formFile = await _fileProvider.GetFileStorageAsync(request.FILE_FileInfoID.Value);
                if (_formFileInfo != null && _formFile != null && _formFile.FileImage.Length > 0)
                {
                    _attachments.Add(new MSG_Mailer_Attachment()
                    {
                        FileData = _formFile.FileImage,
                        FileName = _formFileInfo.FileName + _formFileInfo.FileExtension
                    });
                }
            }

            // Muss angepasst werden!
            var applicantText = _textProvider.Get("APPLICANT");

            // Muss angepasst werden!
            var type = _textProvider.Get("D3_CANTEEN_REFUND_REQUEST");
            var msg = new MSG_Mailer();
            msg.Subject = protocolNumber + "  " + type + "  " + _user.FullnameComposed;
            msg.Body = "";
            msg.ToAdress = _municipality.D3ProtocolEmail;
            var body = "<b>" + applicantText + ":</b><br/>" + _user.FullnameComposed + "<br/>" + _userAnagraphics?.FiscalNumber;
          
            msg.Body = body;

            await _mailerService.SendMail(msg, _attachments.Count > 0 ? _attachments : null, request.AUTH_Municipality_ID.Value, null);
        }
        catch (Exception e)
        {
            var id = request?.ID;

            var errLog = new D3_Error_Log()
            {
                ProtocolElementId = id,
                ErrorMessage = e.Message,
                ProtocolElementType = "FormApplication"
            };
            await _unitOfWork.Repository<D3_Error_Log>().InsertAsync(errLog);
        }
    }
    public async Task ProtocolNewFormApplication(FORM_Application application)
    {
        try
        {
            if (application.IsMunicipal || (application.IsMunicipalCommittee ?? false))
                return;
            
            var updatedApplication = await _unitOfWork.Repository<FORM_Application>().FirstOrDefaultAsync(e => e.ID == application.ID);

            if (updatedApplication != null)
                application = updatedApplication;
            else
                throw new Exception("Could not get application with ID " + application.ID + " from DB.");

            if (application.AUTH_Municipality_ID == null) 
                throw new Exception("Could not get Municipality ID From Sessionwrapper. Application ID: " + application.ID);
            
            var municipality = await _authProvider.GetMunicipality(application.AUTH_Municipality_ID.Value);

            if (municipality == null || municipality.D3ProtocolEmail == null)
                throw new Exception("Could not get Municipality with ID " + application.AUTH_Municipality_ID.Value + " from DB or protocol email address not set. Application ID: " + application.ID);

            if (application.FORM_Definition_ID == null || application.AUTH_Users_ID == null)
                throw new Exception("Application with ID: " + application.ID + " does not have FROM_Definition_ID or AUTH_Users_ID");

            var formDefinition = await _formDefinitionProvider.GetDefinition(application.FORM_Definition_ID.Value);

            if (formDefinition == null)
                throw new Exception("Form Definition could not be fetched from DB. Application ID: " + application.ID);

            var user = await _authProvider.GetUser(application.AUTH_Users_ID.Value);

            if (user == null) 
                throw new Exception("Could not get user for Application with ID: " + application.ID);

            var userAnagraphics = await _authProvider.GetAnagraficByUserID(user.ID);
            if (userAnagraphics == null)
                throw new Exception("Could not get user Anagraphics from DB. Application ID: " + application.ID);

            var protocolNumber = formDefinition.FormCode + "-" + application.ProgressivYear + "-" +
                                 application.ProgressivNumber;

            var attachments = new List<MSG_Mailer_Attachment>();
            if (application.FILE_Fileinfo_ID != null)
            {
                var formFileInfo = await _fileProvider.GetFileInfoAsync(application.FILE_Fileinfo_ID.Value);
                var formFile = await _fileProvider.GetFileStorageAsync(application.FILE_Fileinfo_ID.Value);
                if (formFileInfo != null && formFile != null && formFile.FileImage.Length > 0)
                {
                    attachments.Add(new MSG_Mailer_Attachment()
                    {
                        FileData = formFile.FileImage,
                        FileName = formFileInfo.FileName + formFileInfo.FileExtension
                    });
                }
            }

            var uploadedFiles = await GetUploadedFilesForApplication(application);
            
            var applicantText = _textProvider.Get("APPLICANT");
            var uploadedFilesText = _textProvider.Get("UPLOADED_FILES");

            var type = formDefinition.FORM_Name ?? "Unkown";
            var msg = new MSG_Mailer();
            msg.Subject = protocolNumber  + "  " + type + "  " + user.FullnameComposed;
            msg.Body = "";
            msg.ToAdress = municipality.D3ProtocolEmail;
            var body = "<b>"+applicantText + ":</b><br/>" + user.FullnameComposed + "<br/>" + userAnagraphics?.FiscalNumber;
            if (application.Mantainance_Title != null)
            {
                var maintenanceTitleText = _textProvider.Get("FORM_MANTAINANCE_TITLE");
                var descriptionText = _textProvider.Get("FORM_MANTAINANCE_DESCRIPTION");
                body += "<br/><br/><b>" + maintenanceTitleText + ":</b><br/>" + application.Mantainance_Title;
                body += "<br/><br/><b>" + descriptionText + ":</b><br/>" + application.Mantainance_Description;
            }
            //if meldung --> add title, description, location?
            if (uploadedFiles.Count > 0)
            {
                body += "<br/><br/><b>" + uploadedFilesText + ": </b><br/>";
                foreach (var fileId in uploadedFiles)
                {
                    if (fileId != null)
                    {
                        var formFileInfo = await _fileProvider.GetFileInfoAsync(fileId);
                        if (formFileInfo != null)
                        {
                            body += formFileInfo.FileName + formFileInfo.FileExtension + "<br/>";
                        }
                    }
                    
                }
            }
            msg.Body = body;

            //add uploaded files to attachments
            foreach (var fileInfoId in uploadedFiles)
            {
                if (fileInfoId != null)
                {
                    var formFileInfo = await _fileProvider.GetFileInfoAsync(fileInfoId);
                    var formFile = await _fileProvider.GetFileStorageAsync(fileInfoId);
                    if (formFileInfo != null && formFile != null && formFile.FileImage.Length > 0)
                    {
                        attachments.Add(new MSG_Mailer_Attachment()
                        {
                            FileData = formFile.FileImage,
                            FileName = formFileInfo.FileName + formFileInfo.FileExtension
                        });
                    }
                }
            }

            await _mailerService.SendMail(msg, attachments.Count > 0 ? attachments : null, application.AUTH_Municipality_ID.Value, null);
        }
        catch (Exception e)
        {
            var id = application?.ID;

            var errLog = new D3_Error_Log()
            {
                ProtocolElementId = id,
                ErrorMessage = e.Message,
                ProtocolElementType = "FormApplication"
            };
            await _unitOfWork.Repository<D3_Error_Log>().InsertAsync(errLog);
        }
    }
    public async Task ProtocolRoomBooking(ROOM_BookingGroup booking)
    {
        try
        {
            if (booking.AUTH_MunicipalityID == null)
                throw new Exception("Could not get Municipality ID From Session Wrapper. Booking Group ID: " + booking.ID);

            var municipality = await _authProvider.GetMunicipality(booking.AUTH_MunicipalityID.Value);
            if (municipality == null || municipality.D3ProtocolEmail == null)
                throw new Exception("Could not get Municipality or protocol email address not set. Booking Group ID: " + booking.ID); ;

            if (booking.AUTH_User_ID == null)
                throw new Exception("Booking Group has no AUTH_User_Id. Booking Group ID: " + booking.ID);

            var user = await _authProvider.GetUser(booking.AUTH_User_ID.Value);
            if (user == null)
                throw new Exception("Could not get User from DB. Booking Group ID: " + booking.ID); ;

            var userAnagraphics = await _authProvider.GetAnagraficByUserID(user.ID);

            var type = _textProvider.Get("D3_ROOM_BOOKING");
            var protocolNumber = municipality.RoomPNPrefix + "-" + booking.ProgressivYear + "-" +
                                 booking.ProgressivNumber;

            var attachments = new List<MSG_Mailer_Attachment>();
            if (booking.FILE_FileInfo_ID != null)
            {
                var fileInfo = await _fileProvider.GetFileInfoAsync(booking.FILE_FileInfo_ID.Value);
                var file = await _fileProvider.GetFileStorageAsync(booking.FILE_FileInfo_ID.Value);
                if (file != null && fileInfo != null && file.FileImage.Length > 0)
                {
                    attachments.Add(new MSG_Mailer_Attachment()
                    {
                        FileData = file.FileImage,
                        FileName = fileInfo.FileName + fileInfo.FileExtension
                    });
                }
            }

            var applicantText = _textProvider.Get("APPLICANT");
            var uploadedFilesText = _textProvider.Get("UPLOADED_FILES");
            
            var msg = new MSG_Mailer();
            msg.Subject = protocolNumber  + "  " + type + "  " + user.FullnameComposed;
            msg.Body = "";
            msg.ToAdress = municipality.D3ProtocolEmail;
            var body = applicantText + ": <br/>" + user.FullnameComposed + "<br/>" + userAnagraphics?.FiscalNumber;
            msg.Body = body;

            await _mailerService.SendMail(msg, attachments, municipality.ID, null);
        }
        catch (Exception e)
        {
            var id = booking?.ID;

            var errLog = new D3_Error_Log()
            {
                ProtocolElementId = id,
                ErrorMessage = e.Message,
                ProtocolElementType = "RoomBookingGroup"
            };
            await _unitOfWork.Repository<D3_Error_Log>().InsertAsync(errLog);
        }
    }
    public async Task ProtocolNewOrganization(ORG_Request request)
    {
        try
        {
            if (request.AUTH_Municipality_ID == null)
                throw new Exception("Could not get Municipality ID From Session Wrapper. Org Request ID: " + request.ID);

            var municipality = await _authProvider.GetMunicipality(request.AUTH_Municipality_ID.Value);

            if (municipality == null || municipality.D3ProtocolEmail == null)
                throw new Exception("Could not get Municipality or protocol email address not set. Org Request ID: " + request.ID);

            if (request.AUTH_Users_ID == null)
                throw new Exception("Org Request has no AUTH_Users_ID. Org Request ID: " + request.ID);

            var user = await _authProvider.GetUser(request.AUTH_Users_ID.Value);
            if (user == null)
                throw new Exception("Could not get User from DB. Org Request ID: " + request.ID);

            var userAnagraphics = await _authProvider.GetAnagraficByUserID(user.ID);

            var type = _textProvider.Get("D3_ORGANIZATION");
            var protocolNumber = municipality.OrgPNPrefix + "-" + request.ProgressivYear + "-" +
                                 request.ProgressivNumber;

            var attachments = new List<MSG_Mailer_Attachment>();
            if (request.FILE_Fileinfo_ID != null)
            {
                var fileInfo = await _fileProvider.GetFileInfoAsync(request.FILE_Fileinfo_ID.Value);
                var file = await _fileProvider.GetFileStorageAsync(request.FILE_Fileinfo_ID.Value);
                if (file != null && file.FileImage.Length > 0)
                {
                    attachments.Add(new MSG_Mailer_Attachment()
                    {
                        FileData = file.FileImage,
                        FileName = fileInfo.FileName + fileInfo.FileExtension
                    });
                }
            }

            var uploadedFiles = await GetUploadedFilesOrganization(request);
            

            var applicantText = _textProvider.Get("APPLICANT");
            var uploadedFilesText = _textProvider.Get("UPLOADED_FILES");
            
            var msg = new MSG_Mailer();
            msg.Subject = protocolNumber  + "  " + type + "  " + user.FullnameComposed;
            msg.Body = "";
            msg.ToAdress = municipality.D3ProtocolEmail;
            var body = applicantText + ": <br/>" + user.FullnameComposed + "<br/>" + userAnagraphics?.FiscalNumber;
            if (uploadedFiles.Count > 0)
            {
                body += "<br/><br/> " + uploadedFilesText + ": <br/>";
                foreach (var fileId in uploadedFiles)
                {
                    if (fileId != null)
                    {
                        var formFileInfo = await _fileProvider.GetFileInfoAsync(fileId.Value);
                        if (formFileInfo != null)
                        {
                            body += formFileInfo.FileName + formFileInfo.FileExtension + "<br/>";
                        }
                    }
                    
                }
            }
            msg.Body = body;

            
            //add uploaded files to attachments
            foreach (var fileInfoId in uploadedFiles)
            {
                if (fileInfoId != null)
                {
                    var formFileInfo = await _fileProvider.GetFileInfoAsync(fileInfoId.Value);
                    var formFile = await _fileProvider.GetFileStorageAsync(fileInfoId.Value);
                    if (formFileInfo != null && formFile != null && formFile.FileImage.Length > 0)
                    {
                        attachments.Add(new MSG_Mailer_Attachment()
                        {
                            FileData = formFile.FileImage,
                            FileName = formFileInfo.FileName + formFileInfo.FileExtension
                        });
                    }
                }
            }
            
            await _mailerService.SendMail(msg, attachments, municipality.ID, null);
        }
        catch (Exception e)
        {
            var id = request?.ID;

            var errLog = new D3_Error_Log()
            {
                ProtocolElementId = id,
                ErrorMessage = e.Message,
                ProtocolElementType = "OrgRequest"
            };
            await _unitOfWork.Repository<D3_Error_Log>().InsertAsync(errLog);
        }
    }

    private async Task<List<Guid>> GetUploadedFilesForApplication(FORM_Application application)
    {
        var uploadfields = await _unitOfWork.Repository<FORM_Application_Upload>().ToListAsync(p => p.FORM_Application_ID == application.ID);
         var filesList = new List<FORM_Application_Upload_File>();
        
         foreach (var up in uploadfields)
         {
             var files = await _unitOfWork.Repository<FORM_Application_Upload_File>()
                 .ToListAsync(p => p.FORM_Application_Upload_ID == up.ID);
             filesList.AddRange(files);
         }
         
         var ids =  filesList.Where(e => e.FILE_FileInfo_ID != null).Select(e => e.FILE_FileInfo_ID!.Value).ToList();
         if (application.FORM_Definition_ID != null)
         {
             var dynamicFiles = await GetApplicationUploadFiles(application.ID, application.FORM_Definition_ID.Value);
             var dynFileIds = dynamicFiles.Select(e => e.ID).ToList();
             ids = ids.Union(dynFileIds).ToList();
         }
         return ids;
    }
    private async Task<List<FILE_FileInfo>> GetApplicationUploadFiles(Guid FORM_Application_ID, Guid FORM_Definition_ID)
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
    private async Task<List<Guid?>> GetUploadedFilesOrganization(ORG_Request request)
    {
        var files = await _unitOfWork.Repository<ORG_Request_Attachment>().Where(e => e.ORG_Request_ID == request.ID).Select(e => e.FILE_FileInfo_ID).ToListAsync();
        return files;
    }
}