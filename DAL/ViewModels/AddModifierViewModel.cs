using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class AddModifierViewModel
{

    public long ModifierId { get; set; }

    [Required( ErrorMessage = "Modifier Name is required")]
    [RegularExpression(@"\S.*", ErrorMessage = "Only white spaces are not allowed")]
    [StringLength(20, ErrorMessage = "Modifier Name cannot exceed 20 characters.")]
    public string ModifierName { get; set; } = null!;

    public long ModifierGrpId { get; set; }

    [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Unit is Required")]
    public string? Unit { get; set; }

    [Required(ErrorMessage = "Rate is Required")]
    [Range(0, 999, ErrorMessage = "Rate should be Positive and cannot exceed 3 digit")]
    public decimal? Rate { get; set; }

    [Required(ErrorMessage = "Quantity is Required")]
    [Range(0, 999, ErrorMessage = "Quantity should be Positive and cannot exceed 3 digit")]
    public int Quantity { get; set; }

    public bool Isdelete { get; set; }

}