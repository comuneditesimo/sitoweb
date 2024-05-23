using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Services
{
    public interface INEWSService
    {
        public Task<bool> ReadRSSFeed(Guid AUTH_Municipality_ID);
    }
}
