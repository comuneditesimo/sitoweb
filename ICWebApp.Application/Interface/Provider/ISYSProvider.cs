using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Provider
{
    public interface ISYSProvider
    {
        public Task<bool> SetLog(SYS_Log Data);
    }
}
