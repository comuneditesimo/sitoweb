using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Settings;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ICWebApp.Application.Sessionless
{
    public class FORMApplicationSessionless : IFORMApplicationSessionless
    {
        private IUnitOfWork _unitOfWork;
        public FORMApplicationSessionless(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }

        public async Task<bool> CheckApplicationStatus(List<FORM_Application> Applications)
        {
            foreach (var app in Applications)
            {
                if (app != null && app.FILE_Fileinfo_ID != null && app.FORM_Definition_ID != null)
                {
                    var file = await _unitOfWork.Repository<FILE_FileInfo>().GetByIDAsync(app.FILE_Fileinfo_ID.Value);
                    var def = await _unitOfWork.Repository<FORM_Definition>().GetByIDAsync(app.FORM_Definition_ID.Value);

                    if (file != null && file.Signed)
                    {
                        app.FORM_Application_Status_ID = FORMStatus.Comitted;
                        app.SignedAt = DateTime.Now;
                        app.SubmitAt = DateTime.Now;
                        
                        if (def?.EstimateProcessingTime != null)
                        {
                            app.EstimatedDeadline = DateTime.Now.AddDays(def.EstimateProcessingTime.Value);
                        }

                        if (def?.LegalDeadline != null)
                        {
                            app.LegalDeadline = DateTime.Now.AddDays(def.LegalDeadline.Value);
                        }

                        if (app.FORM_Definition_ID != null)
                        {
                            long lastNumber = 0;

                            var number = _unitOfWork.Repository<FORM_Application>()
                                .Where(p => p.FORM_Definition_ID == app.FORM_Definition_ID.Value 
                                            && p.AUTH_Municipality_ID == app.AUTH_Municipality_ID.Value 
                                            && p.ProgressivYear == DateTime.Now.Year).Max(p => p.ProgressivNumber);

                            if (number != null)
                            {
                                lastNumber = number.Value;
                            }

                            if (app.ProgressivNumber == null || app.ProgressivNumber == 0)
                            {
                                app.ProgressivYear = DateTime.Now.Year;

                                app.ProgressivNumber = lastNumber + 1;
                            }
                        }

                        if (def != null && def.LegalDeadline != null)
                        {
                            app.LegalDeadline = app.SignedAt.Value.AddDays(def.LegalDeadline.Value);
                        }
                        if (def != null && def.EstimateProcessingTime != null)
                        {
                            app.EstimatedDeadline = app.SignedAt.Value.AddDays(def.EstimateProcessingTime.Value);
                        }
                        //D3! this sessionless stuff is no longer used anyway
                    }
                    else if (file != null && file.Signed == false && file.AgreementComitted == true)
                    {
                        app.FORM_Application_Status_ID = FORMStatus.InSigning;

                        var status = await _unitOfWork.Repository<FORM_Application_Status>().FirstOrDefaultAsync(p => p.ID == app.FORM_Application_Status_ID);

                        var StatusLog = new FORM_Application_Status_Log();

                        StatusLog.ID = Guid.NewGuid();

                        StatusLog.FORM_Application_ID = app.ID;
                        StatusLog.AUTH_Users_ID = app.AUTH_Users_ID;
                        StatusLog.FORM_Application_Status_ID = app.FORM_Application_Status_ID;
                        StatusLog.ChangeDate = DateTime.Now;

                        await _unitOfWork.Repository<FORM_Application_Status_Log>().InsertOrUpdateAsync(StatusLog);

                        var Languages = await _unitOfWork.Repository<LANG_Languages>().ToListAsync();

                        if (Languages != null)
                        {
                            foreach (var l in Languages)
                            {
                                var dataE = new FORM_Application_Status_Log_Extended()
                                {
                                    FORM_Application_Status_Log_ID = StatusLog.ID,
                                    LANG_Languages_ID = l.ID
                                };

                                if (l.ID == Guid.Parse("b97f0849-fa25-4cd0-8c7b-43f90fbe4075"))  //DE
                                {
                                    dataE.Title = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == status.TEXT_SystemTexts_Code && p.LANG_LanguagesID ==  LanguageSettings.German).Text;
                                }
                                else if (l.ID == Guid.Parse("e450421a-baff-493e-a390-71b49be6485f")) //IT
                                {
                                    dataE.Title = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == status.TEXT_SystemTexts_Code && p.LANG_LanguagesID == LanguageSettings.Italian).Text;
                                }
                                else
                                {
                                    dataE.Title = _unitOfWork.Repository<TEXT_SystemTexts>().FirstOrDefault(p => p.Code == status.TEXT_SystemTexts_Code && p.LANG_LanguagesID == LanguageSettings.German).Text;
                                }

                                await _unitOfWork.Repository<FORM_Application_Status_Log_Extended>().InsertOrUpdateAsync(dataE);
                            }
                        }
                    }

                    await _unitOfWork.Repository<FORM_Application>().InsertOrUpdateAsync(app);
                }
            }

            return true;
        }
    }
}
