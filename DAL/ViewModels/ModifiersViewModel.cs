using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class ModifiersViewModel
{
    public long ModifierId { get; set; }

    public string ModifierName { get; set; } = null!;

    public long ModifierGrpId { get; set; }

    public string? Unit { get; set; }

    [Range(0, 999, ErrorMessage = "Rate should be Positive and cannot exceed 3 digit")]
    public decimal? Rate { get; set; }

    public int Quantity { get; set; }

    public bool Isdelete { get; set; }
}
