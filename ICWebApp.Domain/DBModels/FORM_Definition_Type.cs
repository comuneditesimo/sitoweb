﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class FORM_Definition_Type
{
    [Key]
    public Guid ID { get; set; }

    public string TEXT_SystemText_Code { get; set; }

    public string DescriptionTEXT_SystemText_Code { get; set; }
}