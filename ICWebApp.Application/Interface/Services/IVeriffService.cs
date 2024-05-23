using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IVeriffService
    {
        public Task<bool> InitializeVeriff(Guid AUTH_Users_ID, string DivID);
    }
}
