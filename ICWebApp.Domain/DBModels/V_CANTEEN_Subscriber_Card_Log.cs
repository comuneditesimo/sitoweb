﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_CANTEEN_Subscriber_Card_Log
{
    public Guid ID { get; set; }

    public Guid? CANTEEN_Subscriber_Card_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    public string TitleCode { get; set; }

    public string MessageCode { get; set; }

    public string Message { get; set; }

    public string Title { get; set; }

    public Guid LANG_LanguagesID { get; set; }
}