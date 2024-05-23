using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.MyCivis
{
    public class PositionDto
    {
        public string? ServiceUid { get; set; }
        public string? ServiceName { get; set; }
        public string? LogoUrl { get; set; }
        public string? ServiceUrl { get; set; }
        public string? ContentHtml { get; set; }
        public DateTime? LastUpdate { get; set; }
        public NotificationDto? Notification { get; set; }
    }
}
