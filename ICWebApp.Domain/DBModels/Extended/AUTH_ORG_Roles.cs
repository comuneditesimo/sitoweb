﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class AUTH_ORG_Roles
    {
        [NotMapped] public string? Description { get; set; }
    }
}
