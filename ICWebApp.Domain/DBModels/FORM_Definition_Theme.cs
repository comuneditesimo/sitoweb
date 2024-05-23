﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class FORM_Definition_Theme
{
    [Key]
    public Guid ID { get; set; }

    public Guid? FORM_Definition_ID { get; set; }

    public Guid? HOME_Theme_ID { get; set; }

    [ForeignKey("FORM_Definition_ID")]
    [InverseProperty("FORM_Definition_Theme")]
    public virtual FORM_Definition FORM_Definition { get; set; }

    [ForeignKey("HOME_Theme_ID")]
    [InverseProperty("FORM_Definition_Theme")]
    public virtual HOME_Theme HOME_Theme { get; set; }
}