using System.ComponentModel.DataAnnotations;

namespace ICWebApp.Domain.Models.User;

public class GeneralData
{
    [Required] public string? Firstname { get; set; }
    [Required] public string? Lastname { get; set; }
}