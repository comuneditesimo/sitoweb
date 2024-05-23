using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IFormApplicationService
    {
        public enum Step
        {
            ToPay,
            PaymentProcessing,
            ToSign,
            InSigning,
            InSigningMultiSign,
            ShowPreview,
            ToCommit,
            Committed,
            Error
        }

        public Task<Step> NextStep(FORM_Application Application, bool ForceCommit = false);
        public Task<FORM_Application> CheckApplication(FORM_Application Application, bool ForceCommit = false);
    }
}
