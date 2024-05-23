﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class NEWS_Article
{
    [Key]
    public Guid ID { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public Guid? LANG_Languages_ID { get; set; }

    public string Title { get; set; }

    public string Link { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PublishingDate { get; set; }

    public string Image { get; set; }

    public string Content { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeletedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastFeedReadDate { get; set; }

    public string InputType { get; set; }

    public string ShortContent { get; set; }

    public bool? Enabled { get; set; }

    public Guid? FamilyID { get; set; }
}