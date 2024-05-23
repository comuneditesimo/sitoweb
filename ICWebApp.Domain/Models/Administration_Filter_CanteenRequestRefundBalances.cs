using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class Administration_Filter_CanteenRequestRefundBalances
    {
        public string? Text { get; set; }
        public List<Guid> CANTEEN_RequestRefundBalances_Status_ID { get; set; } = new List<Guid>();
        public Guid? Auth_User_ID { get; set; }
        public DateTime? SignedFrom { get; set; }
        public DateTime? SignedTo { get; set; }
    }
}
