using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface IPRIVProvider
    {   
        //Defintion Privacy
        public Task<PRIV_Privacy?> GetPrivacy(Guid AUTH_Municipality_ID);
        public Task<PRIV_Privacy?> SetPrivacy(PRIV_Privacy Data);
        //Defintion Privacy Extended
        public Task<PRIV_Privacy_Extended?> GetPrivacyExtended(Guid ID);
        public Task<PRIV_Privacy_Extended?> SetPrivacyExtended(PRIV_Privacy_Extended Data);
        //PRIVACY DOCUMENTS
        public Task<List<PRIV_Backend_Privacy>> GetPrivacyDocuments();
        public Task<PRIV_Backend_Privacy?> SetPrivacyDocuments(PRIV_Backend_Privacy Data);
        public Task<bool> RemovePrivacyDocuments(Guid ID);
    }
}
