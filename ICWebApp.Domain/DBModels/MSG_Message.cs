﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

[Index("AUTH_User_ID", "FirstReadDate", "ShowInList", Name = "IX_MSG_MEssage_AUTH_User_ID_FirstReadDate_ShowInList")]
[Index("AUTH_User_ID", "ShowInList", Name = "IX_MSG_MEssage_ShowInList")]
[Index("ID", Name = "msg_message_id_uindex", IsUnique = true)]
public partial class MSG_Message
{
    [Key]
    public Guid ID { get; set; }

    public Guid? AUTH_User_ID { get; set; }

    public Guid? MessageTypeID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PlannedSendDate { get; set; }

    public string Subject { get; set; }

    public string Messagetext { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FirstReadDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? DeleteDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RealSendDateSms { get; set; }

    public Guid? SmsID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RealSendDateMail { get; set; }

    public Guid? MailID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RealSendDatePush { get; set; }

    public Guid? PushID { get; set; }

    public Guid? LANG_LanguageID { get; set; }

    public Guid? SenderAuthorityID { get; set; }

    public Guid? SenderUserID { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public Guid? ExternalObjectID { get; set; }

    [StringLength(100)]
    public string ExternalObjectType { get; set; }

    public string FromAdress { get; set; }

    public string DisplayName { get; set; }

    public string Link { get; set; }

    public string ToAddress { get; set; }

    public string PhoneNumber { get; set; }

    public bool? ShowInList { get; set; }

    public string ShortText { get; set; }

    [ForeignKey("MessageTypeID")]
    [InverseProperty("MSG_Message")]
    public virtual MSG_MessageType MessageType { get; set; }
}