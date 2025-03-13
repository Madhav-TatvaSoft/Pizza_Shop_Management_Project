namespace DAL.ViewModels;

public class AddModifierViewModel
{

    public long ModifierId { get; set; }

    public string ModifierName { get; set; } = null!;

    public long ModifierGrpId { get; set; }

    public string? Description { get; set; }

    public string? Unit { get; set; }

    public decimal? Rate { get; set; }

    public int Quantity { get; set; }

    public bool Isdelete { get; set; }

}
