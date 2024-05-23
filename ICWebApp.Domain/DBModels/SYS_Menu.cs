using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    [Table("SYS_Menu", Schema = "dbo")]
    public partial class SYS_Menu
    {
        [Key]
        public Guid ID { get; set; }
        [StringLength(350)]
        public string Description { get; set; } = null!;
        [StringLength(250)]
        public string? Icon { get; set; }
        [StringLength(250)]
        public string? AccessRoles { get; set; }
        public int? SortOrder { get; set; }
        [StringLength(250)]
        public string? Application { get; set; }
        [StringLength(250)]
        public string? Area { get; set; }
        [StringLength(250)]
        public string? ClassName { get; set; }
        public bool IsEnabled { get; set; }
        [StringLength(800)]
        public string? Url { get; set; }
        public Guid? ParentID { get; set; }
        [StringLength(500)]
        public string? ParameterName1 { get; set; }
        [StringLength(500)]
        public string? ParameterValue1 { get; set; }
    }
}
