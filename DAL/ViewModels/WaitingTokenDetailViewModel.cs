using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class WaitingTokenDetailViewModel
{
    public long WaitingId { get; set; }

    public long CustomerId { get; set; }

    [Required( ErrorMessage = "Customer Name is required")]
    [RegularExpression(@"\S.*", ErrorMessage = "Only white spaces are not allowed")]
    [StringLength(50, ErrorMessage = "Customer Name cannot exceed 20 characters.")]
    public string CustomerName { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
    public long? PhoneNo { get; set; }

    [Required]
    public string? Email { get; set; }

    [Required(ErrorMessage = "No of Person is required.")]
    [Range(1, 100, ErrorMessage = "No of Person should be between 1 to 100.")]
    public int NoOfPerson { get; set; }
    public long SectionId { get; set; }
    public string SectionName { get; set; }
    public DateTime CreatedAt { get; set; }
    public TimeOnly WaitingTime { get; set; }
}
