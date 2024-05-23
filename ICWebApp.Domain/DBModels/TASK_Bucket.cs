﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class TASK_Bucket
{
    [Key]
    public Guid ID { get; set; }

    public long? SortOrder { get; set; }

    [Unicode(false)]
    public string Icon { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public bool? Enabled { get; set; }

    public bool? Default { get; set; }

    public long? TASK_Context_ID { get; set; }

    [InverseProperty("TASK_Bucket")]
    public virtual ICollection<TASK_Bucket_Extended> TASK_Bucket_Extended { get; set; } = new List<TASK_Bucket_Extended>();
}