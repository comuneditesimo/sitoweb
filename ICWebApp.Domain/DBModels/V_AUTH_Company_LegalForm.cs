﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_AUTH_Company_LegalForm
{
    public Guid ID { get; set; }

    public string Text_SystemText_Code { get; set; }

    public Guid? AUTH_Company_Type_ID { get; set; }

    public string Text { get; set; }

    public Guid? LANG_LanguagesID { get; set; }

    public bool CanBeBolloFree { get; set; }
}