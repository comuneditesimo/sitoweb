using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class Administration_Filter_CanteenSubscriptions
    {
        public string? Text { get; set; }
        public bool? EskalatedTasks { get; set; }
        public bool MyTasks { get; set; }
        public bool Archived { get; set; }
        public bool? ManualInput { get; set; }
        public Guid? Auth_User_ID { get; set; }
        public List<Guid>? AUTH_Authority_ID { get; set; }
        public List<Guid>? Subscription_Status_ID { get; set; }
        public List<Guid>? Canteen_ID { get; set; }
        public List<Guid>? School_ID { get; set; }
        public List<Guid>? SchoolYear_ID { get; set; }

        public List<Guid>? Menu_ID { get; set; }
        public DateTime? SubmittedFrom { get; set; }
        public DateTime? SubmittedTo { get; set; }
        public DateTime? DeadlineFrom { get; set; }
        public DateTime? DeadlineTo { get; set; }
        public bool? NegagiveBalance { get; set; }
        public Int32? MaxDistnanceFromSchool { get; set; }

        public Int32? MinDistnanceFromSchool { get; set; }
    }
}
