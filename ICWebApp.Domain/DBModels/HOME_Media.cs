﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class HOME_Media
{
    [Key]
    public Guid ID { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    public string Copyright { get; set; }

    public int? SortOrder { get; set; }

    public Guid? FILE_FileInfo_ID { get; set; }

    [InverseProperty("HOME_Media")]
    public virtual ICollection<HOME_Media_Extended> HOME_Media_Extended { get; set; } = new List<HOME_Media_Extended>();
}