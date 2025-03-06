using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DAL.ViewModels;

public class AddItemViewModel
{
    public long ItemId { get; set; }

    public string ItemName { get; set; } = null!;

    public long CategoryId { get; set; }

    public string? Description { get; set; }

    public long ItemTypeId { get; set; }

    public IFormFile ItemFormImage { get; set; }

    public string ItemImage { get; set; }

    [RegularExpression(@"^\d$", ErrorMessage = "Rate cannot be negative")]
    public decimal Rate { get; set; }

    public string? ShortCode { get; set; }
 
    public int Quantity { get; set; }

    public bool Isavailable { get; set; }

    public string Unit { get; set; }

    public bool Isdefaulttax { get; set; }

    public decimal TaxValue { get; set; }

    public bool Isdelete { get; set; }
}
