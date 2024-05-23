using System.ComponentModel.DataAnnotations.Schema;

namespace ICWebApp.Domain.DBModels
{
    public partial class CANTEEN_RequestRefundBalances_Status_Log
    {
        [NotMapped] public string? Status { get; set; }
        [NotMapped] public string? StatusIcon { get; set; }
        [NotMapped] public string? User { get; set; }
        [NotMapped] public string? Title { get; set; }
        [NotMapped] public string? Reason { get; set; }
    }
}
