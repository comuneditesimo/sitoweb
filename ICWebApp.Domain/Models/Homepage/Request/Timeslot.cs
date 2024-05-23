using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Homepage.Request
{
    public class Timeslot
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set;}
        public string Month 
        { 
            get
            {
                if (DateFrom == null)
                    return "";

                return DateFrom.Value.ToString("yyyy MMMM");
            } 
        }
        public int MonthInt
        {
            get
            {
                if (DateFrom == null)
                    return 0;

                return DateFrom.Value.Month;
            }
        }
        public bool Selected { get; set; } = false;
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
