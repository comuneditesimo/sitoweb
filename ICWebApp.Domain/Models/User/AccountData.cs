using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.User
{
    public class AccountData
    {
        [Required] public string? Password { get; set; }
        [Required] public string? ConfirmPassword { get; set; }
    }
}
