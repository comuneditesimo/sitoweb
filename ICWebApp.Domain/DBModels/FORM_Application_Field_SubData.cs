﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class FORM_Application_Field_SubData
{
    [Key]
    public Guid ID { get; set; }

    public Guid? FORM_Application_Field_Data_ID { get; set; }

    public string Description { get; set; }

    public string Value { get; set; }

    [ForeignKey("FORM_Application_Field_Data_ID")]
    [InverseProperty("FORM_Application_Field_SubData")]
    public virtual FORM_Application_Field_Data FORM_Application_Field_Data { get; set; }
}