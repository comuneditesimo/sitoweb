using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICWebApp.Domain.Models.Searchbar
{
    public class SearchbarItem
    {
        public string? Url { get; set; }
        public string? SubTitle { get; set; }
        public string? SubTitleUrl { get; set; }
        public string? Title { get; set; }
        public string? ShortText { get; set; }
        public string? Themes { get; set; }
        public SearchBarItemType? ItemType { get; set; }
    }

    public enum SearchBarItemType
    {
        Organiations = 0,
        Events = 1,
        Venues = 2,
        Documents = 3,
        News = 4,
        Services = 5,
        People = 6,
    }
}
