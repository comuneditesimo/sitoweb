using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IMailerService
    {
        public Task<bool> SendMail(MSG_Mailer Mail, List<MSG_Mailer_Attachment>? Mail_Attachments,  Guid AUTH_Municipality_ID, Guid? CONF_Mailer_ID = null);
    }
}
