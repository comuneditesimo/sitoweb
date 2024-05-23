using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class AUTH_Authority
    {
        [NotMapped] public string? Description { get; set; }
        [NotMapped] public string? ShortText { get; set; }
        [NotMapped] public bool EnabledFalse { get; set; } = false;
        [NotMapped] public bool EnabledTrue { get; set; } = true;
    }
}
