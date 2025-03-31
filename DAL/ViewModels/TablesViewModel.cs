using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class TablesViewModel
{
    public long TableId { get; set; }

    public long SectionId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Table Name is required")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Table Name contain only alphabets")]
    [StringLength(20, ErrorMessage = "Table Name cannot exceed 20 characters.")] 
    public string TableName { get; set; } = null!;

    [Required(ErrorMessage = "Capacity is Required")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity should be Positive")]
    public int Capacity { get; set; }

    public bool Status { get; set; }

    public bool Isdelete { get; set; }

}