using ICWebApp.Domain.Models;

namespace ICWebApp.Application.Interface.Helper
{
    public interface ICanteenRequestCardListHelper
    {
        public Administration_Filter_CanteenRequestCardList? Filter { get; set; }
    }
}
