using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class BucketView_Search
    {
        public List<Guid> AUTH_Users { get; set; } = new List<Guid>();
        public List<Guid> TASK_Tags { get; set; } = new List<Guid>();
        public Guid? TASK_Priorities { get; set; }
        public Guid? TASK_Status { get; set; }
    }
}
