using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class FORM_Definition
    {
        [NotMapped]
        public string? FORM_Name { get; set; }
        [NotMapped]
        public string? FORM_Description { get; set; }
        [NotMapped]
        public string? ShortText { get; set; }
        [NotMapped] public string? AmtName { get; set; }
    }
}
