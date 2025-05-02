namespace DAL.ViewModels;

public class ItemOrderViewModel
{
    public long ItemId { get; set; }
    public string ItemName { get; set; } = null!;
    public decimal? Rate { get; set; }
    public int? Quantity { get; set; }
    public string? ExtraInstruction { get; set; }
    public decimal TotalItemAmount { get; set; }
    public string status { get; set; }
    public long OrderdetailId { get; set; }

    public List<ModifierorderViewModel> modifierOrderVM { get; set; }

}