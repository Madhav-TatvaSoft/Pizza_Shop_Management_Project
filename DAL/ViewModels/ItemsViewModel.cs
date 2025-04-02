
using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class ItemsViewModel
{
    public long ItemId { get; set; }


    [Required( ErrorMessage = "Item Name is required")]
    [RegularExpression(@"\S.*", ErrorMessage = "Only white spaces are not allowed")]
    [StringLength(20, ErrorMessage = "Item Name cannot exceed 20 characters.")]
    public string ItemName { get; set; } = null!;

    public long CategoryId { get; set; }

    public long ItemTypeId { get; set; }

    public string TypeImage { get; set; } = null!;

    [Required(ErrorMessage = "Rate is Required")]
    [Range(0, 999, ErrorMessage = "Rate should be Positive and cannot exceed 3 digit")]
    public decimal Rate { get; set; }

    [Required(ErrorMessage = "Quantity is Required")]
    [Range(0, 999, ErrorMessage = "Quantity should be Positive and cannot exceed 3 digit")]
    public int? Quantity { get; set; }

    public string? ItemImage { get; set; }

    public bool? Isavailable { get; set; }

    public bool Isdelete { get; set; }
}