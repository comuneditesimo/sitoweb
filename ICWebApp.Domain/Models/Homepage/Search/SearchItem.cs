using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Homepage.Search
{
    public class SearchItem
    {
        public Action OnClick {  get; set; }
        public string Title { get; set; }  
        public Action OnClickType {  get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
