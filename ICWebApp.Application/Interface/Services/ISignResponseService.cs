using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface ISignResponseService
    {
        public Task<bool> SetSignedAgreement(string AgreementID);
        public Task<bool> SetAgreementCreated(string AgreementID);
        public Task<bool> SetAgreementComitted(string AgreementID, string UserMail);
    }
}