using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DAL.ViewModels;

public class AddItemViewModel
{
    public long ItemId { get; set; }

    [Required(ErrorMessage = "ItemName is required")]
    [RegularExpression(@"\S.*", ErrorMessage = "Only white spaces are not allowed")]
    [StringLength(20, ErrorMessage = "ItemName cannot exceed 20 characters.")]
    public string ItemName { get; set; } = null!;

    public long CategoryId { get; set; }

    [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
    public string? Description { get; set; }

    public long ItemTypeId { get; set; }

    public IFormFile ItemFormImage { get; set; }

    public string ItemImage { get; set; }

    [Range(0, 999, ErrorMessage = "Rate should be Positive and cannot exceed 3 digit")]
    public decimal Rate { get; set; }

    [StringLength(50, ErrorMessage = "ShortCode cannot exceed 50 characters.")]
    public string? ShortCode { get; set; }

    [Range(0, 999, ErrorMessage = "Quantity should be Positive and cannot exceed 3 digit")]
    public int Quantity { get; set; }

    public bool Isavailable { get; set; }

    public string Unit { get; set; }

    public bool Isdefaulttax { get; set; }

    [Range(0, 999, ErrorMessage = "TaxValue should be Positive and cannot exceed 3 digit")]
    public decimal TaxValue { get; set; }

    public bool Isdelete { get; set; }

    public List<ItemModifierViewModel> itemModifiersVM { get; set; }
}
