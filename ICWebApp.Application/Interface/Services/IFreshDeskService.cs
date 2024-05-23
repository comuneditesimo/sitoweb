using Freshdesk;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface IFreshDeskService
    {
        public Task<GetTicketResponse?> CreateTicket(FreshDeskTicket Ticket);
    }
}
