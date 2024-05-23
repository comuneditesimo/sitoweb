using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Homepage.Request
{
    public class DaySlot
    {
        public DateTime? DateFrom { get; set; }
        public string Day
        {
            get
            {
                if (DateFrom == null)
                    return "";

                return DateFrom.Value.ToString("dddd, dd MMMM yyyy");
            }
        }
        public int DayInt
        {
            get
            {
                if (DateFrom == null)
                    return 0;

                return DateFrom.Value.Day;
            }
        }
    }
}
