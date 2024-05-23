using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Application_Priority
    {
        [NotMapped] public string? Name { get; set; }
        [NotMapped] public long? Amount { get; set; }
        [NotMapped] public bool Explode { get; set; } = false;
    }
}
