﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class TEXT_Template_Extended
{
    [Key]
    public Guid ID { get; set; }

    public Guid? TEXT_Template_ID { get; set; }

    public Guid? LANG_Languages_ID { get; set; }

    public string Content { get; set; }

    public string Name { get; set; }

    [ForeignKey("TEXT_Template_ID")]
    [InverseProperty("TEXT_Template_Extended")]
    public virtual TEXT_Template TEXT_Template { get; set; }
}