﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class CONF_Mailer
{
    [Key]
    public Guid ID { get; set; }

    public string Client { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

    public string MailFrom { get; set; }

    public string DisplayName { get; set; }

    public Guid CONF_Mailer_TypeID { get; set; }

    public Guid? AUTH_Municipality_ID { get; set; }

    public string ClientPort { get; set; }

    public string TemplateID { get; set; }

    public string ApiKey { get; set; }

    public string Tag { get; set; }

    public string TemplateHtml { get; set; }

    public string BaseUrl { get; set; }

    public string ReplyTo { get; set; }
}