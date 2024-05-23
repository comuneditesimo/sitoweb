using ICWebApp.Application.Interface.Helper;
using ICWebApp.Domain.Models;

namespace ICWebApp.Application.Helper
{
    public class CanteenRequestCardListHelper : ICanteenRequestCardListHelper
    {
        private Administration_Filter_CanteenRequestCardList? _filter;
        public Administration_Filter_CanteenRequestCardList? Filter { get => _filter; set => _filter = value; }
    }
}
