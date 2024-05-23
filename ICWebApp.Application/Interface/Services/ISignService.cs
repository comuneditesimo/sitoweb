using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface ISignService
    {
        public Task<string?> StartLocalSignProcess(Guid FILE_Info_ID, bool NeedsMunicipalSign = false, SignerItem ? SignerItem = null);
        public Task<string?> StartMultiSignProcess(Guid FILE_Info_ID, List<SignerItem> Signers, bool NeedsMunicipalSign = false);
        public Task<string?> GetSigningUrl(string AgreementID, string TargetMail);
        public Task<string?> GetSignOnURL(string State);
    }
}