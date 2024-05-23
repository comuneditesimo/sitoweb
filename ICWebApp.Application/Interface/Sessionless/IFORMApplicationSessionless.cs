using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Sessionless
{
    public interface IFORMApplicationSessionless
    {        
        public Task<bool> CheckApplicationStatus(List<FORM_Application> Applications);
    }
}
