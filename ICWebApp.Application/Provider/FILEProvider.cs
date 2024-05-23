using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.Domain.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ICWebApp.Application.Provider
{
    public class FILEProvider : IFILEProvider
    {
        private readonly IUnitOfWork _unitOfWork;
        public FILEProvider(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }

        public async Task<FILE_FileInfo?> GetFileInfoAsync(Guid ID)
        {
            return await _unitOfWork.Repository<FILE_FileInfo>().FirstOrDefaultAsync(p => p.ID == ID);
        }
        public FILE_FileInfo? GetFileInfo(Guid ID)
        {
            return _unitOfWork.Repository<FILE_FileInfo>().FirstOrDefault(p => p.ID == ID);
        }

        public async Task<string> GetFileName(Guid id)
        {
            var fileInfo = await _unitOfWork.Repository<FILE_FileInfo>().FirstOrDefaultAsync(e => e.ID == id);
            if (fileInfo != null)
            {
                return fileInfo.FileName + fileInfo.FileExtension;
            }

            return "";
        }
        public async Task<FILE_FileStorage?> GetFileStorageAsync(Guid FILE_FileInfo_ID)
        {
            return await _unitOfWork.Repository<FILE_FileStorage>().FirstOrDefaultAsync(p => p.FILE_FileInfo_ID == FILE_FileInfo_ID);
        }
        public FILE_FileStorage? GetFileStorage(Guid FILE_FileInfo_ID)
        {
            return _unitOfWork.Repository<FILE_FileStorage>().FirstOrDefault(p => p.FILE_FileInfo_ID == FILE_FileInfo_ID);
        }
        public async Task<bool> RemoveFileInfo(Guid ID, bool force = false)
        {
            var itemexists = await _unitOfWork.Repository<FILE_FileInfo>().Where(p => p.ID == ID).FirstOrDefaultAsync();

            if (itemexists != null)
            {
                if (force)
                {
                    await _unitOfWork.Repository<FILE_FileInfo>().DeleteAsync(itemexists);
                }
                else
                {
                    itemexists.DeleteDate = DateTime.Now;
                }

                await _unitOfWork.Repository<FILE_FileInfo>().InsertOrUpdateAsync(itemexists);
            }

            return true;
        }
        public async Task<FILE_FileInfo?> SetFileInfo(FILE_FileInfo Data)
        {
            var fi = await _unitOfWork.Repository<FILE_FileInfo>().InsertOrUpdateAsync(Data);

            if (Data != null && Data.FILE_FileStorage != null && Data.FILE_FileStorage.Count() > 0)
            {
                foreach(var fs in Data.FILE_FileStorage)
                {
                    await _unitOfWork.Repository<FILE_FileStorage>().InsertOrUpdateAsync(fs);
                }
            }

            return fi;
        }
        public async Task<FILE_FileStorage?> SetFileStorage(FILE_FileStorage Data)
        {
            return await _unitOfWork.Repository<FILE_FileStorage>().InsertOrUpdateAsync(Data);
        }
        public long GetFileSize(Guid FILE_FileInfo_ID)
        {
            var data = _unitOfWork.Repository<FILE_FileInfo>().GetByID(FILE_FileInfo_ID);

            if(data != null && data.Size != null)
            {
                return data.Size.Value;
            }

            return 0;
        }
        public async Task<FILE_FileInfo?> GetFileInfoByAgreementID(string AdobeSign_AgreementID)
        {
            return await _unitOfWork.Repository<FILE_FileInfo>().FirstOrDefaultAsync(p => p.AdobeSign_Agreement_ID == AdobeSign_AgreementID);
        }
        public async Task<FILE_Sign_Log?> SetLog(FILE_Sign_Log Data)
        {
            return await _unitOfWork.Repository<FILE_Sign_Log>().InsertOrUpdateAsync(Data);
        }
        public FILE_Download_Log? SetDownloadLog(FILE_Download_Log Data)
        {
            return _unitOfWork.Repository<FILE_Download_Log>().InsertOrUpdate(Data);
        }
        public FILE_Download_Log? GetDownloadLogByToken(Guid Token)
        {
            return _unitOfWork.Repository<FILE_Download_Log>().FirstOrDefault(p => p.DownloadToken == Token && p.DownloadExpirationDate >= DateTime.Now);
        }
    }
}
