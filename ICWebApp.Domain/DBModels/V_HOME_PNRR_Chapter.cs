﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_HOME_PNRR_Chapter
{
    public Guid ID { get; set; }

    public Guid? HOME_PNRR_ID { get; set; }

    public int SortOrder { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public Guid? LANG_Language_ID { get; set; }
}