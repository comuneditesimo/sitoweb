using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class SignerItem
    {
        [Required]
        public string? Mail { get; set; }
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? XPosition { get; set; }
        public string? YPosition { get; set; }
        public string? Width { get; set; }
        public string? Height{ get; set; }
        public string? PageNumber{ get; set; }
        public bool SelfSign { get; set; } = false;
    }
}
