using Freshdesk;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class FreshDeskService : IFreshDeskService
    {
        private ICONFProvider ConfProvider;
        public FreshDeskService(ICONFProvider ConfProvider)
        {
            this.ConfProvider = ConfProvider;
        } 
        public async Task<GetTicketResponse?> CreateTicket(FreshDeskTicket Ticket)
        {
            var conf = await ConfProvider.GetFreshDeskConfiguration(null);

            if (conf != null)
            {
                var freshdeskService = new FreshdeskService(conf.APIKey, new Uri("https://innocon.freshdesk.com"));

                GetTicketResponse ticketResponse = freshdeskService.CreateTicket(new CreateTicketRequest()
                {
                    TicketInfo = new CreateTicketInfo()
                    {

                        Email = Ticket.Email,
                        Subject = Ticket.Subject,
                        Description = Ticket.Description,
                        Priority = int.Parse(Ticket.Priority.ToString()),
                        Status = 2,
                    },
                    Options = new CreateTicketOptions()
                    {
                        Tags = conf.Description
                    }
                });

                return ticketResponse;
            }

            return null;
        }
    }
}
