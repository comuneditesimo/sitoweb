using ICWebApp.Domain.DBModels;
using ICWebApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ICWebApp.Application.Interface.Helper
{
    public interface ISpidHelper
    {
        public string? CheckXml(MemoryStream MemoryStream, bool Localhost = false);
        public AUTH_External_Verification Verification { get; set; }
    }
}
