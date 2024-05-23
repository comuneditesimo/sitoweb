using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Definition_Tasks_Responsible
    {
        [NotMapped] public string? Fullname { get; set; }
        [NotMapped] public string? Email { get; set; }
    }
}
