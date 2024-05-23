using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class Administration_Filter_Request
    {
        public string? Text { get; set; }
        public Guid? Auth_User_ID { get; set; }
        public List<Guid>? Request_Status_ID { get; set; }
        public List<Guid>? Company_Type_ID { get; set; }
        public DateTime? SubmittedFrom { get; set; }
        public DateTime? SubmittedTo { get; set; }
        public bool Archived { get; set; } = false;
        public bool OnlyNewChatMessages { get; set; } = false;
    }
}
