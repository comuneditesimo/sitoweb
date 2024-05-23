using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.User
{
    public class ServiceDataItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string WarningText { get; set; }
        public string StatusIcon { get; set; }
        public string StatusText { get; set; }
        public string StatusCSS { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? LastChangeDate { get; set; }
        public string ProtocollNumber { get; set; }
        public string TaxNumber { get; set; }
        public Guid? File_FileInfo_ID { get; set; }
        public Action? DetailAction { get; set; }
        public string? DetailTextCode { get; set; }
        public Action? CancelAction { get; set; }
        public string? CancelTextCode { get; set; }
        public string? CanteenRequestNewCardTextCode { get; set; }
        public Action? CanteenRequestNewCardAction { get; set; }
        public string? Days { get; set; }
        public string? Rooms { get; set; }
        public Guid? CanteenReportGerman { get; set; }
        public Guid? CanteenReportItalian { get; set; }
        public string? OrgUserTaxNumber { get; set; }
        public string? OrgUserRole { get; set; }
        public DateTime? OrgUserJoinDate { get; set; }
        public Action? OrgUserToggleActive { get; set; }
        public string? OrgUserToggleActiveTextCode { get; set; }
        public Action? OrgUserDelete { get; set; }
        public Action? OrgUserChangeRole { get; set; }
        public Action? OrgUserConfirm { get; set; }
        public bool? MessageIsRead { get; set; }
        public Action? ReadMessage { get; set; }
        public Guid? ServiceItemID { get; set; }
        public bool? HasUnreadChatMessage { get; set; }
        public bool? HasToPay { get; set; }
    }
}
