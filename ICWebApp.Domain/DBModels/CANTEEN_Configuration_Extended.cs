﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class CANTEEN_Configuration_Extended
{
    [Key]
    public Guid ID { get; set; }

    public Guid? CANTEEN_Configuration_ID { get; set; }

    public Guid? LANG_Language_ID { get; set; }

    public string ForWhom { get; set; }

    public string HowTo { get; set; }

    public string WhatFor { get; set; }

    public string Result { get; set; }

    [ForeignKey("CANTEEN_Configuration_ID")]
    [InverseProperty("CANTEEN_Configuration_Extended")]
    public virtual CANTEEN_Configuration CANTEEN_Configuration { get; set; }
}