using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class TablesViewModel
{
    public long TableId { get; set; }

    public long SectionId { get; set; }

    [Required( ErrorMessage = "Table Name is required")]
    [RegularExpression(@"\S.*", ErrorMessage = "Only white spaces are not allowed")]
    [StringLength(20, ErrorMessage = "Table Name cannot exceed 20 characters.")] 
    public string TableName { get; set; } = null!;

    [Required(ErrorMessage = "Capacity is Required")]
    [Range(1, 20, ErrorMessage = "Capacity should be Positive and cannot exceed 20")]
    public int Capacity { get; set; }

    public string Status { get; set; } = null!;

    public bool Isdelete { get; set; }

}