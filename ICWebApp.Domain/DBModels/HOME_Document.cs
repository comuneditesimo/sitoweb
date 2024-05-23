﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class HOME_Document
{
    [Key]
    public Guid ID { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReleaseDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastChangeDate { get; set; }

    public Guid? Managed_HOME_Person_ID { get; set; }

    public Guid? Managed_AUTH_Authority_ID { get; set; }

    public Guid? HOME_Document_License_ID { get; set; }

    public Guid? HOME_Document_Format_ID { get; set; }

    public bool Highlight { get; set; }

    [InverseProperty("HOME_Document")]
    public virtual ICollection<HOME_Article_Document> HOME_Article_Document { get; set; } = new List<HOME_Article_Document>();

    [InverseProperty("HOME_Document")]
    public virtual ICollection<HOME_Association_Document> HOME_Association_Document { get; set; } = new List<HOME_Association_Document>();

    [InverseProperty("HOME_Document")]
    public virtual ICollection<HOME_Document_Authority> HOME_Document_Authority { get; set; } = new List<HOME_Document_Authority>();

    [InverseProperty("HOME_Document")]
    public virtual ICollection<HOME_Document_Data> HOME_Document_Data { get; set; } = new List<HOME_Document_Data>();

    [InverseProperty("HOME_Document")]
    public virtual ICollection<HOME_Document_Extended> HOME_Document_Extended { get; set; } = new List<HOME_Document_Extended>();

    [InverseProperty("HOME_Document")]
    public virtual ICollection<HOME_Document_Theme> HOME_Document_Theme { get; set; } = new List<HOME_Document_Theme>();

    [InverseProperty("HOME_Document")]
    public virtual ICollection<HOME_Location_Document> HOME_Location_Document { get; set; } = new List<HOME_Location_Document>();

    [InverseProperty("HOME_Document")]
    public virtual ICollection<HOME_Organisation_Document> HOME_Organisation_Document { get; set; } = new List<HOME_Organisation_Document>();

    [InverseProperty("HOME_Document")]
    public virtual ICollection<HOME_Person_Document> HOME_Person_Document { get; set; } = new List<HOME_Person_Document>();

    [InverseProperty("HOME_Document")]
    public virtual ICollection<HOME_Venue_Document> HOME_Venue_Document { get; set; } = new List<HOME_Venue_Document>();
}