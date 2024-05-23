﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class AUTH_Users
{
    [Key]
    public Guid ID { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public bool? EmailConfirmed { get; set; }

    public string EmailConfirmToken { get; set; }

    public string PasswordHash { get; set; }

    public Guid? PasswordResetToken { get; set; }

    public Guid LastLoginToken { get; set; }

    [StringLength(50)]
    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public bool LockoutEnabled { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? RemovedAt { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastLoginTimeStamp { get; set; }

    public string RegistrationMode { get; set; }

    public Guid? Logo_FILE_FileInfo_ID { get; set; }

    public bool VeriffConfirmed { get; set; }

    public string PhoneNumberConfirmToken { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? VeriffStartDate { get; set; }

    public bool IsOrganization { get; set; }

    public Guid? AUTH_Company_Type_ID { get; set; }

    public Guid? AUTH_Company_LegalForm_ID { get; set; }

    public bool ForceEmailVerification { get; set; }

    public bool ForcePhoneVerification { get; set; }

    public bool ForcePasswordReset { get; set; }

    public Guid? ForcePwResetToken { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ForcePwResetTokenExpirationData { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastForceResetMailSent { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PrivacyAccepted { get; set; }

    public Guid? ExternalLoginToken { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExternalLoginDate { get; set; }

    [ForeignKey("AUTH_Municipality_ID")]
    [InverseProperty("AUTH_Users")]
    public virtual AUTH_Municipality AUTH_Municipality { get; set; }

    [InverseProperty("AUTH_Users")]
    public virtual ICollection<AUTH_UserSettings> AUTH_UserSettings { get; set; } = new List<AUTH_UserSettings>();

    [InverseProperty("AUTH_Users")]
    public virtual ICollection<AUTH_Users_Anagrafic> AUTH_Users_Anagrafic { get; set; } = new List<AUTH_Users_Anagrafic>();

    [InverseProperty("AUTH_Users")]
    public virtual ICollection<FORM_Application_Responsible> FORM_Application_Responsible { get; set; } = new List<FORM_Application_Responsible>();

    [InverseProperty("ORG_Request_User")]
    public virtual ICollection<ORG_Request_Status_Log> ORG_Request_Status_Log { get; set; } = new List<ORG_Request_Status_Log>();
}