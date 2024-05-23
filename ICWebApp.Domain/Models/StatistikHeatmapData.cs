using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models
{
    public class StatistikHeatmapData
    {
        public object? CategoryY { get; set; }
        public object? CategoryX { get; set; }
        public double Value { get; set; }
        public int PriorityOrder { get; set; } = 0;
    }
}
