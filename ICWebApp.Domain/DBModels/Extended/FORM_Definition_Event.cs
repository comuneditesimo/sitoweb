using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Definition_Event
    {
        [NotMapped] public string? Title { get; set; }
        [NotMapped] public string? Description { get; set; }
    }
}
