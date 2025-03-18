using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class AddModifierGroupViewModel
{
    public long ModifierGrpId { get; set; }

    [Required(ErrorMessage = "Modifier Group Name is Required")]
    public string ModifierGrpName { get; set; }
    public string? Desciption { get; set; }
    public string Temp_Ids { get; set; }
}