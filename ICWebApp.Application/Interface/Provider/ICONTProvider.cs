using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface ICONTProvider
    {
        public Task<List<V_CONT_Contact>> GetContacts(Guid AUTH_Municipality_ID);
        public Task<CONT_Contact?> GetContact(Guid ID);
        public Task<CONT_Contact?> SetContact(CONT_Contact Data);
        public Task<bool> RemoveContact(Guid ID);
    }
}
