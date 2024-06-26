﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class HOME_Association_Document
{
    [Key]
    public Guid ID { get; set; }

    public Guid? HOME_Association_ID { get; set; }

    public Guid? HOME_Document_ID { get; set; }

    [ForeignKey("HOME_Association_ID")]
    [InverseProperty("HOME_Association_Document")]
    public virtual HOME_Association HOME_Association { get; set; }

    [ForeignKey("HOME_Document_ID")]
    [InverseProperty("HOME_Association_Document")]
    public virtual HOME_Document HOME_Document { get; set; }
}