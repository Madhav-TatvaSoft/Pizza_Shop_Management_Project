namespace DAL.ViewModels;

public class ItemsViewModel
{
    public long ItemId { get; set; }

    public string ItemName { get; set; } = null!;

    public long CategoryId { get; set; }

    public long ItemTypeId { get; set; }

    public string TypeImage { get; set; } = null!;

    public decimal Rate { get; set; }

    public int? Quantity { get; set; }

    public string? ItemImage { get; set; }

    public bool? Isavailable { get; set; }

    public bool Isdelete { get; set; }
}
