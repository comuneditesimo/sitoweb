using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.User
{
    public class KontaktData
    {
        [Required] public string? Input { get; set; }
    }
}
