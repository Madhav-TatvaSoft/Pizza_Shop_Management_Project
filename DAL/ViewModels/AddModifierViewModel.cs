using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class AddModifierViewModel
{

    public long ModifierId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Modifier Name is required")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Modifier Name contain only alphabets")]
    [StringLength(20, ErrorMessage = "Modifier Name cannot exceed 20 characters.")]
    public string ModifierName { get; set; } = null!;

    public long ModifierGrpId { get; set; }

    [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Unit is Required")]
    [Range(0, int.MaxValue, ErrorMessage = "Unit should be Positive")]
    public string? Unit { get; set; }

    [Required(ErrorMessage = "Rate is Required")]
    [Range(0, int.MaxValue, ErrorMessage = "Rate should be Positive")]
    public decimal? Rate { get; set; }

    [Required(ErrorMessage = "Quantity is Required")]
    [Range(0,int.MaxValue, ErrorMessage = "Quantity should be Positive")]
    public int Quantity { get; set; }

    public bool Isdelete { get; set; }

}