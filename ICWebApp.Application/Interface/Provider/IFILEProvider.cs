using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IFILEProvider
    {
        public Task<FILE_FileInfo?> GetFileInfoAsync(Guid ID);
        public FILE_FileInfo? GetFileInfo(Guid ID);
        public Task<string> GetFileName(Guid id);
        public Task<FILE_FileInfo?> GetFileInfoByAgreementID(string AdobeSign_AgreementID);
        public Task<FILE_FileInfo?> SetFileInfo(FILE_FileInfo Data);
        public Task<bool> RemoveFileInfo(Guid ID, bool force = false);
        public Task<FILE_FileStorage?> GetFileStorageAsync(Guid FILE_FileInfo_ID);
        public FILE_FileStorage? GetFileStorage(Guid FILE_FileInfo_ID);
        public Task<FILE_FileStorage?> SetFileStorage(FILE_FileStorage Data);
        public long GetFileSize(Guid FILE_FileInfo_ID);
        public Task<FILE_Sign_Log?> SetLog(FILE_Sign_Log Data);
        public FILE_Download_Log? SetDownloadLog(FILE_Download_Log Data);
        public FILE_Download_Log? GetDownloadLogByToken(Guid Token);
    }
}
