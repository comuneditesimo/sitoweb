using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class AUTH_Municipal_Users_Authority
    {
        [NotMapped] public string AuthorityName { get; set; }
        [NotMapped] public bool Removed { get; set; } = false;
    }
}
