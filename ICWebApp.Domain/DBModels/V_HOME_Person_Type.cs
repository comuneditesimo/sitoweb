﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_HOME_Person_Type
{
    public Guid ID { get; set; }

    public string TEXT_System_Text_Code_Male { get; set; }

    public string DescriptionTEXT_System_Text_Code { get; set; }

    public string TEXT_System_Text_Code_Female { get; set; }

    public Guid LANG_LanguagesID { get; set; }

    public string Type_M { get; set; }

    public string Type_F { get; set; }

    public string Description { get; set; }
}