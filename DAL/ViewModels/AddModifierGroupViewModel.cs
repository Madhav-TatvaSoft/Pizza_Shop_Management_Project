
using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class AddModifierGroupViewModel
{
    public long ModifierGrpId { get; set; }


    [Required(ErrorMessage = "Modifier Group Name is required")]
    [RegularExpression(@"\S.*", ErrorMessage = "Only white spaces are not allowed")]
    // [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Modifier Group Name must contain only alphabets")]
    [StringLength(20, ErrorMessage = "Modifier Group Name cannot exceed 20 characters.")]
    public string ModifierGrpName { get; set; }

    [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
    public string? Desciption { get; set; }
    public string Temp_Ids { get; set; }
}
