using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ICWebApp.Domain.DBModels
{
    public partial class CONF_MainMenu
    {
        [NotMapped] public string? DynamicName { get; set; }
        [NotMapped] public bool NewNotification { get; set; } = false;
    }
}
