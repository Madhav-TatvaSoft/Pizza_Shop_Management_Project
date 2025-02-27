using System.ComponentModel.DataAnnotations;

namespace DAL.ViewModels;

public class ResetPasswordViewModel
{
    public string Email { get; set; }
    
    [MinLength(6,ErrorMessage ="Password must contains at least 6 characters")]
    [MaxLength(20,ErrorMessage ="Password should not exceed 20 characters")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",ErrorMessage ="Password must contain at least one uppercase letter, one number,and one special character.")]
    public string Password { get; set; }

    // [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
}
