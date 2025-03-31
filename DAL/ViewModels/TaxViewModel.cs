using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class TaxViewModel
{
    public long TaxId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Tax Name is required")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Tax Name contain only alphabets")]
    [StringLength(20, ErrorMessage = "Tax Name cannot exceed 20 characters.")]
    public string TaxName { get; set; } = null!;

    public string TaxType { get; set; } = null!;

    [Required(ErrorMessage = "Amount is Required")]
    [Range(1, int.MaxValue, ErrorMessage = "Amount should be Positive")]
    public decimal TaxValue { get; set; }

    public bool Isenable { get; set; }

    public bool Isdefault { get; set; }

    public bool Isdelete { get; set; }

}