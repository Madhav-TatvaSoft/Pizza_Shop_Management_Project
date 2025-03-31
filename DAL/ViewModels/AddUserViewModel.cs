using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Serialization;

namespace DAL.ViewModels;

public class AddUserViewModel
{
    public long UserId { get; set; }

    public long UserloginId { get; set; }

    public long RoleId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage ="FirstName is required")]
    [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "FirstName must contain only alphabets")]
    [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = null!;

    [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "UserName is required.")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    public string Username { get; set; } = null!;
    
    public string Email { get; set; }

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [MaxLength(20, ErrorMessage = "Password cannot exceed 20 characters.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.")]
    public string Password { get; set; }

    public string? Image { get; set; }

    public IFormFile? ProfileImage { get; set; }

    [Required(ErrorMessage = "Please select a Country.")]
    public long? CountryId { get; set; }

    [Required(ErrorMessage = "Please select a State.")]
    public long? StateId { get; set; }

    [Required(ErrorMessage = "Please select a City.")]
    public long? CityId { get; set; }

    public string? Address { get; set; }

    [Required(ErrorMessage = "Zipcode is required.")]
    [Range(100000, 999999, ErrorMessage = "Zipcode format is Invalid.")]
    public long? Zipcode { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be 10 digits.")]
    public long Phone { get; set; }

    public bool? Status { get; set; }
}