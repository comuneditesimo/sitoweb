using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ICWebApp.Domain.DBModels;

public partial class CANTEEN_Subscriber_Card_Request
{
    public bool AddressNotFound = false;
    
    [Required][NotMapped]
    public string AddressReq
    {
        get => Address;
        set => Address = value;
    }
    [Required][NotMapped]
    public string MunicipalityReq
    {
        get => Municipality;
        set => Municipality = value;
    }
    [Required][NotMapped]
    public string ProvinceReq
    {
        get => Place;
        set => Place = value;
    }
    [Required][NotMapped]
    public string PLZReq
    {
        get => PLZ;
        set => PLZ = value;
    }

    [NotMapped] public Guid SelectedMunicipality { get; set; }
}