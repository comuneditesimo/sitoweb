using System.ComponentModel.DataAnnotations.Schema;

namespace ICWebApp.Domain.DBModels
{
    public partial class ROOM_Property
    {
        [NotMapped]
        public string? Title { get; set; }
        [NotMapped]
        public string? Description { get; set; }
    }
}
