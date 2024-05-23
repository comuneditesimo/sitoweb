using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.User
{
    public class DomicileData
    {
        [Required] public string? ReqDomicileStreetAddress { get; set; }
        [Required] public string? ReqDomicilePostalCode { get; set; }
        [Required] public string? ReqDomicileMunicipality { get; set; }
        [Required] public string? ReqDomicileProvince { get; set; }
        [Required] public string? ReqDomicileNation { get; set; }
        [Required] public Guid? SelectedMunicipality { get; set; }
    }
}
