using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class V_AUTH_Organizations
    {
        [NotMapped] public string? RequestMessage { get; set; }
        [NotMapped] public string? RequestErrorMessage { get; set; }
    }
}
