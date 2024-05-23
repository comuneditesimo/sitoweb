using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Application.Interface.Helper
{
    public interface ICalendarHelper
    {
        public string GetFilePath(Guid ID, DateTime DateStart, DateTime DateEnd, string Subject, string? Description = null, string? Location = null);
    }
}
