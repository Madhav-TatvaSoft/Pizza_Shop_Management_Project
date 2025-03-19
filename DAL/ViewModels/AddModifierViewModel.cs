using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class AddModifierViewModel
{

    public long ModifierId { get; set; }

    [Required(ErrorMessage = "Modifier Name is Required")]
    public string ModifierName { get; set; } = null!;

    public long ModifierGrpId { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Unit is Required")]
    public string? Unit { get; set; }

    [Required(ErrorMessage = "Rate is Required")]
    public decimal? Rate { get; set; }

    [Required(ErrorMessage = "Quantity is Required")]
    public int Quantity { get; set; }

    public bool Isdelete { get; set; }

}