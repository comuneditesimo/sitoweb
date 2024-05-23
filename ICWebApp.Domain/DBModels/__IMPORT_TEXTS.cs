using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    [Table("__IMPORT_TEXTS", Schema = "dbo")]
    public partial class __IMPORT_TEXTS
    {
        public string? Code { get; set; }
        public string? Ital { get; set; }
        [Key]
        public Guid ID { get; set; }
    }
}
