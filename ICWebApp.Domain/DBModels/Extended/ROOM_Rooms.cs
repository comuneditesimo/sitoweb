using System.ComponentModel.DataAnnotations.Schema;

namespace ICWebApp.Domain.DBModels
{
    public partial class ROOM_Rooms
    {
        [NotMapped]
        public int QuantityOfSubRooms { get; set; } = 0;
        [NotMapped]
        public int SumSurfaceM2 { get; set; } = 0;
        [NotMapped]
        public int SumCapacity { get; set; } = 0;
        [NotMapped]
        public bool IsSelected { get; set; } = false;
    }
}
