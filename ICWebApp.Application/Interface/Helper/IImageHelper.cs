using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface IImageHelper
    {
        public string GetThemePath(V_HOME_Theme Item);
        public string GetFilePath(Guid? FILE_FileInfo_ID, string? Type = null);
        public string GetFilePathThumbnail(Guid FILE_FileInfo_ID);
        public string GetPDFThumbnail(Guid? FILE_FileInfo_ID);
    }
}
