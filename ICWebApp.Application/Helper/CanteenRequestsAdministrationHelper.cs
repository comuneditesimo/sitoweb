using ICWebApp.Application.Interface.Helper;
using ICWebApp.Domain.Models;

namespace ICWebApp.Application.Helper
{
    public class CanteenRequestsAdministrationHelper : ICanteenRequestsAdministrationHelper
    {
        private Administration_Filter_CanteenRequestRefundBalances? _filter;
        public Administration_Filter_CanteenRequestRefundBalances? Filter { get => _filter; set => _filter = value; }
    }
}
