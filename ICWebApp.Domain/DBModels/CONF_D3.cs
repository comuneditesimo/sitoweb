﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels;

public partial class CONF_D3
{
    [Key]
    public Guid ID { get; set; }

    public string Host { get; set; }

    public string Version { get; set; }

    public string User { get; set; }

    public string Password { get; set; }

    public string UserAppend { get; set; }

    public string Arch { get; set; }

    public long? Port { get; set; }
}