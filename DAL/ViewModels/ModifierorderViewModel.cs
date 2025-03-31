namespace DAL.ViewModels;

public class ModifierorderViewModel
{
    public long ModifierId { get; set; }
    public string ModifierName { get; set; } = null!;
    public decimal? Rate { get; set; }
    public int Quantity { get; set; }

    public decimal TotalModifierAmount { get; set; }
}