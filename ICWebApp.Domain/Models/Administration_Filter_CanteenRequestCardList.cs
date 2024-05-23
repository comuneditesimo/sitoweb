namespace ICWebApp.Domain.Models
{
    public class Administration_Filter_CanteenRequestCardList
    {
        public string? Text { get; set; }
        public List<int> CANTEEN_Card_Status_ID { get; set; } = new List<int>();
    }
}
