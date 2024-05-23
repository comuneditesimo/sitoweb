using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class PAY_Transaction
    {
        [NotMapped] public string? Type { get; set; }
        [NotMapped] public string? User { get; set; }
    }
}
