﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Index("LANG_LanguagesID", Name = "IX_TEXT_SystemTexts_LANG_LanguagesID")]
public partial class TEXT_SystemTexts
{
    [Key]
    public Guid ID { get; set; }

    public Guid LANG_LanguagesID { get; set; }

    public string Code { get; set; }

    public string Text { get; set; }

    public bool? AutoGenerated { get; set; }
}