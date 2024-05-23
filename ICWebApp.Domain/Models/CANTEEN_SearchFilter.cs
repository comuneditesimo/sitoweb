using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class CANTEEN_SearchFilter
    {
        public Guid? CurrentSchoolID { get; set; }
        public Guid? CurrentCanteenID { get; set; }
        public Guid? CurrentStatusID { get; set; }
        public Guid? CurrentStatusActionID { get; set; }
        public Guid? CurrentSchoolYearID { get; set; }
    }
}
