﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class HOME_Privacy
{
    [Key]
    public Guid ID { get; set; }

    public int SortOrder { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastModificationDate { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    [InverseProperty("HOME_Privacy")]
    public virtual ICollection<HOME_Privacy_Document> HOME_Privacy_Document { get; set; } = new List<HOME_Privacy_Document>();

    [InverseProperty("HOME_Privacy")]
    public virtual ICollection<HOME_Privacy_Extended> HOME_Privacy_Extended { get; set; } = new List<HOME_Privacy_Extended>();
}