using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class ItemsViewModel
{
    public long ItemId { get; set; }


    [Required(ErrorMessage = "Item Name is Required")]
    public string ItemName { get; set; } = null!;

    public long CategoryId { get; set; }

    public long ItemTypeId { get; set; }

    public string TypeImage { get; set; } = null!;

    [Required(ErrorMessage = "Rate is Required")]
    public decimal Rate { get; set; }

    [Required(ErrorMessage = "Quantity is Required")]
    public int? Quantity { get; set; }

    public string? ItemImage { get; set; }

    public bool? Isavailable { get; set; }

    public bool Isdelete { get; set; }
}
