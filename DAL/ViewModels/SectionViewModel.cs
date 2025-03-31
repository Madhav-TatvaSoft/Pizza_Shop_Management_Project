using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class SectionViewModel
{
    public long SectionId { get; set; }


    [Required(AllowEmptyStrings = false, ErrorMessage = "Section Name is required")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Section Name contain only alphabets")]
    [StringLength(20, ErrorMessage = "SectionName cannot exceed 20 characters.")]
    public string SectionName { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
    public string? Description { get; set; }
    public bool Isdelete { get; set; }

}