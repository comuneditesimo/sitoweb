﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Keyless]
public partial class V_CHAT_Messages_Responsible
{
    public Guid ID { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string ContextElementId { get; set; }

    [Required]
    [StringLength(50)]
    public string ContextType { get; set; }

    public Guid AUTH_Users_ID { get; set; }

    [Required]
    [StringLength(13)]
    [Unicode(false)]
    public string AUTH_User_Type { get; set; }

    public Guid? AUTH_Frontend_Users_ID { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public Guid? ResponsibleMunicipalUserID { get; set; }

    [Unicode(false)]
    public string Message { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SendDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RemovedDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ReadDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NotificationSendDate { get; set; }

    public string LinkUrl { get; set; }

    public string LinkFaviconUrl { get; set; }

    public string LinkName { get; set; }

    public string Firstname_Frontend_User { get; set; }

    public string Lastname_Frontend_User { get; set; }

    public string Firstname_Responsible_Municipal_User { get; set; }

    public string Lastname_Responsible_Municipal_User { get; set; }

    public string FormName { get; set; }

    public Guid? LANG_Language_ID { get; set; }

    [Required]
    [StringLength(5)]
    [Unicode(false)]
    public string Type { get; set; }
}