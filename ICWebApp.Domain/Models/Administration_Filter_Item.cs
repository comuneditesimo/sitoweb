using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class Administration_Filter_Item
    {
        public string? Text { get; set; }
        public bool? EskalatedTasks { get; set; }
        public bool MyTasks { get; set; }
        public bool Archived { get; set; }
        public bool? ManualInput { get; set; }
        public Guid? Auth_User_ID { get; set; }
        public List<Guid>? AUTH_Authority_ID { get; set; }
        public List<Guid>? FORM_Application_Status_ID { get; set; }
        public List<Guid>? FORM_Application_Priority_ID { get; set; }
        public DateTime? SubmittedFrom { get; set; }
        public DateTime? SubmittedTo { get; set; }
        public DateTime? DeadlineFrom { get; set; }
        public DateTime? DeadlineTo { get; set; }
        public bool OnlyMunicipal { get; set; } = false;
        public bool OnlyMunicipalCommittee { get; set; } = false;
        public bool OnlyPublic { get; set; } = false;
        public bool OnlyNewChatMessages { get; set; } = false;
        public bool OnlyToPay { get; set; } = false;
    }
}
