﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class TASK_Bucket_Default
{
    [Key]
    public Guid ID { get; set; }

    [Unicode(false)]
    public string TEXT_SystemTexts_Code { get; set; }

    public long? SortOrder { get; set; }

    [Unicode(false)]
    public string Icon { get; set; }

    public bool? Default { get; set; }
}