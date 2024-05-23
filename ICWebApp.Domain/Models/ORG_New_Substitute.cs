using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class ORG_New_Substitute
    {
        [Required][RegularExpression(@"^[A-Z]{6}[0-9]{2}[A-Z][0-9]{2}[A-Z][0-9]{3}[A-Z]$", ErrorMessage = "VALIDATION_REGEX")]
        public string? FiscalNumber { get; set; }
        public string? Error { get; set; }
    }
}
