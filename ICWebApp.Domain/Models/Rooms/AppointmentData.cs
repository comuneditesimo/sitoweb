using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Rooms
{
    public class AppointmentData
    {
        public int Id { get; set; }
        [Required] public string Subject { get; set; }
        [Required] public DateTime StartTime { get; set; }
        [Required] public DateTime EndTime { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0).AddHours(1);
        [Required] public Guid? RoomID { get; set; }
        public string Description { get; set; }
        public bool IsBlock { get; set; }
        public bool IsReadonly { get; set; }
        public string CssClass { get; set; }
        public bool ManualInput { get; set; }
        public int OwnerID { get; set; }
        public string AppointmentString { get; set; }
        public string RoomName { get; set; }
        public Guid ExternalID { get; set; }
        public Guid? ExternalGroupID { get; set; }
        public Guid? StatusID { get; set; }
        public Guid? AUTH_User_ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public string? MobilePhone { get; set; }
        public bool IsNew { get; set; } = true;
        public bool Accepted { get; set; } = false;
        public bool ToPay { get; set; } = false;
        public bool ToSign { get; set; } = false;
        public Guid? ROOMBookingTypeID { get; set; } = Guid.Parse("0715D3BB-2040-4575-9692-10555BDC06FB");
        public bool? IsWholeDay { get; set; }
    }
}
