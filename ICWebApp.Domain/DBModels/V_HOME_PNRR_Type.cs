﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_HOME_PNRR_Type
{
    public Guid ID { get; set; }

    public string TEXT_SystemTexts_Code { get; set; }

    public int SortOrder { get; set; }

    public string Type { get; set; }

    public Guid LANG_LanguagesID { get; set; }
}