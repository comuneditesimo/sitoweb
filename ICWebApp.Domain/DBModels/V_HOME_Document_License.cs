﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_HOME_Document_License
{
    public Guid ID { get; set; }

    public string TEXT_System_Text_Code { get; set; }

    public Guid LANG_LanguagesID { get; set; }

    public string License { get; set; }
}