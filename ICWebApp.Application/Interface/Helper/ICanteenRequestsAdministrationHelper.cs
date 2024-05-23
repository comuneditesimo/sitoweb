using ICWebApp.Domain.Models;

namespace ICWebApp.Application.Interface.Helper
{
    public interface ICanteenRequestsAdministrationHelper
    {
        public Administration_Filter_CanteenRequestRefundBalances? Filter { get; set; }
    }
}
