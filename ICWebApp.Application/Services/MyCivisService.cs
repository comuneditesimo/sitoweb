using ICWebApp.Application.Interface.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Services
{
    public class MyCivisService : IMyCivisService
    {
        public bool Enabled { get; set; }
    }
}
