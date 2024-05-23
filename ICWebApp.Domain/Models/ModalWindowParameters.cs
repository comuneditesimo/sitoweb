using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class ModalWindowParameters
    {
        public string Title { get; set; } = "Optionen";
        public string Width { get; set; } = "50%";
        public string Height { get; set; } = "auto";
        public bool Modal { get; set; } = true;
        public bool ShowClose { get; set; } = true;
        public bool Draggable { get; set; } = false;
    }
}
