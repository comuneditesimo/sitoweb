using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Homepage.Services
{
    public class DeadlineItem
    {
        public DateTime Date {  get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
