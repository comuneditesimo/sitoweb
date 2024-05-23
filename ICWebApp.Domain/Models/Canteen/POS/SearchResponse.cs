using ICWebApp.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Canteen.POS
{
    public class SearchResponse
    {
        public List<SearchResponseItem> Results { get; set; }
        public string Code { get; set; }
    }
}
