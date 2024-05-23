﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class AUTH_UserRoles
    {
        [NotMapped] public string RoleName { get; set; }
        [NotMapped] public bool Removed { get; set; } = false;
    }
}
