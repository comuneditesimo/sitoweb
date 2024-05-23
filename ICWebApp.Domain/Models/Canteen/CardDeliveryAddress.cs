using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Canteen
{
    public class CardDeliveryAddress
    {
        [Required] public string? Name { get; set; }
        [Required] public string? Street { get; set; }
        [Required] public string? PLZ { get; set; }
        [Required] public string? Municipality { get; set; }
    }
}
