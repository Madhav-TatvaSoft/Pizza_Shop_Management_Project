using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class SectionViewModel
{
    public long SectionId { get; set; }

    [Required(ErrorMessage = "Section Name is Required")]
    public string SectionName { get; set; } = null!;
    public string? Description { get; set; }
    public bool Isdelete { get; set; }

}