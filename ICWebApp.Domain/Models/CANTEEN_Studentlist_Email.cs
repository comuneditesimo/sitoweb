using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class CANTEEN_Studentlist_Email
    {
        [Required]
        public string? Email { get; set; }
    }
}
