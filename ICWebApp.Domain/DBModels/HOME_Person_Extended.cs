﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class HOME_Person_Extended
{
    [Key]
    public Guid ID { get; set; }

    public Guid? HOME_Person_ID { get; set; }

    public Guid? LANG_Language_ID { get; set; }

    public string EMail { get; set; }

    public string PECEmail { get; set; }

    public string Tasks { get; set; }

    public string AreaOfWork { get; set; }

    public string Description { get; set; }

    public string DescriptionShort { get; set; }

    public string Room { get; set; }

    public string Address { get; set; }

    public string EMail2 { get; set; }

    [ForeignKey("HOME_Person_ID")]
    [InverseProperty("HOME_Person_Extended")]
    public virtual HOME_Person HOME_Person { get; set; }
}