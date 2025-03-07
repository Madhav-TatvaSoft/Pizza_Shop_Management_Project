using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class UserLoginViewModel
{
    public string Email { get; set; }

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [MaxLength(20, ErrorMessage = "Password cannot exceed 20 characters.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.")]
    public string Password { get; set; }
    public bool Remember_me { get; set; }
}