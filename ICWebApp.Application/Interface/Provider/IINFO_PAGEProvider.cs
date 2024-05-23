using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IINFO_PAGEProvider
    {
        public Task<List<INFO_Page>> GetPages(Guid menuID, Guid? parentID, String? languageCode ,string pageUrl);

        public Task<List<INFO_Page>> GetInfoPagesList(Guid menuID, Guid? parentID);

        public Task<List<INFO_Template>> GetTemplateList(Guid? externalKey);

        public Task<List<INFO_Template_Type>> GetTemplateTypeList();

        public Task<INFO_Page> GetDetailPage(Guid newspageID);

        public Task<INFO_Template> GetDetailTemplate(Guid newspageID);

        public Task<INFO_Message> GetDetailMessage(Guid newspageID);

        public Task<bool> UpdatePage(INFO_Page item);

        public Task<bool> UpdateTemplate(INFO_Template item);

        public Task<bool> UpdateMessage(INFO_Message item);

        public Task<bool> RemoveTemplate(Guid ID);

    }
}