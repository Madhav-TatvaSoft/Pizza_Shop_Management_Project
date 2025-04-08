using System.ComponentModel.DataAnnotations;

namespace DAL.Models;

public partial class Category
{
    public long CategoryId { get; set; }


    [Required(ErrorMessage = "Category Name is required")]
    [RegularExpression(@"\S.*", ErrorMessage = "Only white spaces are not allowed")]
    [StringLength(20, ErrorMessage = "Category Name cannot exceed 20 characters.")]
    public string CategoryName { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool Isdelete { get; set; }

    public long? CreatedBy { get; set; }

    public long? ModifiedBy { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Item> Items { get; } = new List<Item>();

    public virtual User? ModifiedByNavigation { get; set; }
}
