using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class ORG_Request_Status_Log
    {
        [NotMapped] public string? Status { get; set; }
        [NotMapped] public string? StatusIcon { get; set; }
        [NotMapped] public string? User { get; set; }
        [NotMapped] public string? Title { get; set; }
        [NotMapped] public string? Reason { get; set; }
    }
}
